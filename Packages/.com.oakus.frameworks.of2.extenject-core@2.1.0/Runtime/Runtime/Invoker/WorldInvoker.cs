using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Zenject
{
    public class MonoUpdater : MonoBehaviour
    {
        public Action<float> OnUnityUpdate;

        private void Update()
        {
            if (OnUnityUpdate != null)
            {
                OnUnityUpdate(Time.deltaTime);
            }
        }
    }
 
    /// <summary>
    /// Wrapper over Manual invoker that allows for setting ignore pause for tasks.
    /// </summary>
    public class WorldInvoker : IInvoker, IInitializable, IPausable
    {
        private List<ManualInvoker.InvokerTaskInfo> ignorePauseTasks = new List<ManualInvoker.InvokerTaskInfo>();
        private List<int> removedTasks = new List<int>(); // contains indices into ignorePauseTasks
        
        private readonly MonoUpdater monoUpdater;
        private readonly ManualInvoker manualInvoker;

        public bool Paused { get; set; } = false;

        public WorldInvoker(
            ManualInvoker manualInvoker,
            MonoUpdater monoUpdater)
        {
            this.monoUpdater = monoUpdater;
            this.manualInvoker = manualInvoker;
        }

        public void Initialize()
        {
            monoUpdater.OnUnityUpdate += OnUnityUpdate;
        }

        private void OnUnityUpdate(float dt)
        {
            foreach (var ignorePauseTask in ignorePauseTasks)
            {
                if (ignorePauseTask.paused && ignorePauseTask.unpauseTime > 0 && Time.time > ignorePauseTask.unpauseTime)
                {
                    ignorePauseTask.paused = false;
                    ignorePauseTask.unpauseTime = 0;
                }
                
                if (ignorePauseTask.paused)
                    continue;
                
                var remove = manualInvoker.UpdateTask(ignorePauseTask, dt);
                if (removedTasks.Count == 0)
                    removedTasks.Clear();
                
                if (remove)
                    removedTasks.Add(ignorePauseTasks.IndexOf(ignorePauseTask));
            }

            if (removedTasks.Count > 0)
            {
                foreach (var removedTask in removedTasks)
                {
                    ignorePauseTasks.RemoveAt(removedTask);
                }

                removedTasks.Clear();
            }
            
            if (!Paused)
                manualInvoker.Update(dt);
        }

        public int InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0, bool ignoreWorldPause = false)
        {
            int taskId = 0;
            
            if (ignoreWorldPause)
            {
                var task = manualInvoker.BorrowTask(task_, delay_, interval_, cancelTime);
                taskId = task.id;
                ignorePauseTasks.Add(task);
            }
            else
            {
                taskId = manualInvoker.InvokeRepeating(task_, delay_, interval_, cancelTime);
            }

            return taskId;
        }

        public int Invoke(InvokerTask task_, float delay_, bool ignorePause)
        {
            int taskId = 0;
            
            if (ignorePause)
            {
                var task = manualInvoker.BorrowTask(task_, delay_, 0, delay_);
                taskId = task.id;
                ignorePauseTasks.Add(task);
            }
            else
            {
                taskId = manualInvoker.Invoke(task_, delay_);
            }

            return taskId;
        }

        public void StopInvoke(int taskId, float delay)
        {
            manualInvoker.StopInvoke(taskId, 0);
            for (var index = 0; index < ignorePauseTasks.Count; index++)
            {
                var ignorePauseTask = ignorePauseTasks[index];
                if (ignorePauseTask.id == taskId)
                {
                    ignorePauseTasks.RemoveAt(index);
                    break;
                }
            }
        }

        public bool HasTask(int taskId)
        {
            var ignoreTask = FindIgnoreTaskById(taskId);
            if (ignoreTask != null)
                return true;

            return manualInvoker.HasTask(taskId);
        }

        public void Pause(int taskId)
        {
            var ignoreTask = FindIgnoreTaskById(taskId);
            if (ignoreTask != null)
            {
                ignoreTask.paused = true;
                ignoreTask.unpauseTime = 0;
            }
            else
            {
                manualInvoker.Pause(taskId);   
            }
        }

        public void PauseFor(int taskId, float time)
        {
            var ignoreTask = FindIgnoreTaskById(taskId);
            if (ignoreTask != null)
            {
                ignoreTask.paused = true;
                ignoreTask.unpauseTime = time > 0f ? Time.time + time : 0f;
            }
            else
            {
                manualInvoker.PauseFor(taskId, time);   
            }            
        }

        public bool IsPaused(int taskId)
        {
            var pausedTask = FindIgnoreTaskById(taskId);
            if (pausedTask != null)
            {
                return pausedTask.paused;
            }

            return manualInvoker.IsPaused(taskId);
        }

        public void Resume(int taskId)
        {
            var ignoreTask = FindIgnoreTaskById(taskId);
            if (ignoreTask != null)
            {
                ignoreTask.paused = false;
                ignoreTask.unpauseTime = 0;
            }
            else
            {
                manualInvoker.Resume(taskId);   
            }              
        }

        private ManualInvoker.InvokerTaskInfo FindIgnoreTaskById(int taskId)
        {
            for (var index = 0; index < ignorePauseTasks.Count; index++)
            {
                var ignorePauseTask = ignorePauseTasks[index];
                if (ignorePauseTask.id == taskId)
                {
                    return ignorePauseTask;
                }
            }

            return null;
        }
    }
}
//  <author>Pavel Fadrhonc</author>
//  <email>pavel.fadrhonc@gmail.com</email>
//  <summary> Allows to InvokeOnce or InvokeRepeating methods that are not part of MonoBehaviours.
// You have to call Update method yourself with delta time allowing for more control than WorldInvoker.</summary>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zenject
{
    public class ManualInvoker
    {
        public class InvokerTaskInfo
        {
            private static int _lastTaskId = 0;
            
            public int id;
            public InvokerTask Task;
            public float Delay;
            public float Interval;
            public float LastInvokeTime; // this is time incremented by dt when the task was actually ran
            public float NextInvokeTime; // this is multiples of Interval
            public float TotalRunTime;
            public bool started;
            public float cancelTime;
            public bool active;
            public bool paused;
            public float unpauseTime;

            public InvokerTaskInfo()
            {
                Reset();
            }

            public void Reset()
            {
                id = _lastTaskId++;
                Task = null;
                Delay = 0;
                Interval = 0;
                LastInvokeTime = 0;
                NextInvokeTime = 0;
                started = false;
                cancelTime = 0;
                TotalRunTime = 0;
                active = true;
            }
        }

        private class InvokeTasksCache
        {
            private List<InvokerTaskInfo> _invokeTasks = new List<InvokerTaskInfo>();

            public InvokerTaskInfo GetCleanTask()
            {
                InvokerTaskInfo taskInfo;
                if ((taskInfo = _invokeTasks.FirstOrDefault(t => t.active == false)) == null)
                {
                    taskInfo = new InvokerTaskInfo();
                    _invokeTasks.Add(taskInfo);
                }
                else
                {
                    taskInfo.Reset();
                }

                return taskInfo;
            }

            public void StopTask(InvokerTaskInfo taskInfo)
            {
                taskInfo.active = false;
            }
        }
        
        private Dictionary<int, InvokerTaskInfo> _tasksById = new Dictionary<int, InvokerTaskInfo>();
        private InvokeTasksCache _tasksCache = new InvokeTasksCache();
        private List<int> _removedTasks = new List<int>();

        public void Update(float dt)
        {
            _removedTasks.Clear();

            foreach (var invokerTaskInfoPair in _tasksById)
            {
                var invokeTaskId = invokerTaskInfoPair.Key;

                if (UpdateTask(invokeTaskId, dt))
                    _removedTasks.Add(invokeTaskId);                
            }
            
            // removing has to be at the end for Invoke to work properly
            for (int index = 0; index < _removedTasks.Count; index++)
            {
                var removedTask = _removedTasks[index];
                StopTask(removedTask);
                _tasksById.Remove(removedTask);
            }
        }

        #region PUBLIC METHODS

        public bool UpdateTask(int taskId, float dt)
        {
            var invokerTaskInfo = _tasksById[taskId];

            return UpdateTask(invokerTaskInfo, dt);
        }
        
        public bool UpdateTask(InvokerTaskInfo invokerTaskInfo, float dt)
        {
            bool removed = false;

            invokerTaskInfo.TotalRunTime += dt;
            
            if (!invokerTaskInfo.started)
            {
                if (invokerTaskInfo.TotalRunTime >= invokerTaskInfo.Delay)
                {
                    invokerTaskInfo.started = true;
                    RunTask(invokerTaskInfo);
                    invokerTaskInfo.TotalRunTime -= invokerTaskInfo.Delay;
                    invokerTaskInfo.LastInvokeTime = invokerTaskInfo.TotalRunTime;
                    invokerTaskInfo.NextInvokeTime = invokerTaskInfo.Interval;
                }
            }
            else
            {
                if (invokerTaskInfo.TotalRunTime > invokerTaskInfo.NextInvokeTime)
                {
                    RunTask(invokerTaskInfo);
                    invokerTaskInfo.LastInvokeTime = invokerTaskInfo.TotalRunTime;
                    invokerTaskInfo.NextInvokeTime += invokerTaskInfo.Interval;
                }
            }

            if ((invokerTaskInfo.TotalRunTime >= invokerTaskInfo.cancelTime) &&
                (invokerTaskInfo.cancelTime > 0 && invokerTaskInfo.Interval != 0 || invokerTaskInfo.Interval == 0))
                removed = true;
            
            // task was stopped during it's execution
            if (!invokerTaskInfo.active)
                removed = true;

            return removed;
        }        

        public InvokerTaskInfo BorrowTask()
        {
            return _tasksCache.GetCleanTask();
        }
        
        /// <summary>
        /// You can borrow task here and then use this class UpdateTask function to update it with dt.
        /// This class won't be part of tasks that belong to this class so you can still update the whole class using Update
        /// </summary>
        public InvokerTaskInfo BorrowTask(InvokerTask task_, float delay_, float interval_, float cancelTime = 0)
        {
            var invokeTask = _tasksCache.GetCleanTask();
            invokeTask.Delay = delay_;
            invokeTask.Interval = interval_;
            invokeTask.Task = task_;
            invokeTask.started = false;
            invokeTask.cancelTime = cancelTime;

            return invokeTask;
        }

        public int InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0)
        {
            var task = BorrowTask(task_, delay_, interval_, cancelTime);
            _tasksById.Add(task.id, task);

            return task.id;
        }

        /// <summary>
        /// Invokes just once
        /// </summary>
        public int Invoke(InvokerTask task_, float delay_)
        {
            // using cancelTime = delay can work because record are remove at the end of Update loop
            return InvokeRepeating(task_, delay_, 0, delay_);
        }

        /// <summary>
        /// Wont break if task is not actually Invoking
        /// </summary>
        public void StopInvoke(int taskId, float delay)
        {
            if (!_tasksById.ContainsKey(taskId)) return;
            if (delay == 0)
                StopTask(taskId);
            else
                _tasksById[taskId].cancelTime = _tasksById[taskId].TotalRunTime + delay;
        }

        public bool HasTask(int taskId)
        {
            return _tasksById.ContainsKey(taskId);
        }

        public void Pause(int taskId)
        {
            Pause(taskId, 0f);
        }
        
        public void PauseFor(int taskId, float time)
        {
            Pause(taskId, time);
        }

        public bool IsPaused(int taskId)
        {
            if (!_tasksById.ContainsKey(taskId)) return false;

            return _tasksById[taskId].paused;
        }
        
        public void Resume(int taskId)
        {
            if (!_tasksById.ContainsKey(taskId)) return;
            
            _tasksById[taskId].paused = false;
            _tasksById[taskId].unpauseTime = 0;
        }

        #endregion

        #region PRIVATE / PROTECTED METHODS

        private void Pause(int taskId, float time = 0)
        {
            if (!_tasksById.ContainsKey(taskId)) return;

            _tasksById[taskId].paused = true;

            if (time > 0f)
                _tasksById[taskId].unpauseTime = Time.time + time;
        }

        private void RunTask(InvokerTaskInfo taskInfo)
        {
            if (taskInfo.paused && taskInfo.unpauseTime > 0f && Time.time > taskInfo.unpauseTime)
            {
                taskInfo.paused = false;
                taskInfo.unpauseTime = 0;
            }

            if (taskInfo.paused)
                return;
            
            taskInfo.Task(taskInfo.TotalRunTime - taskInfo.LastInvokeTime);
        }
        
        private void StopTask(int taskId)
        {
            _tasksCache.StopTask(_tasksById[taskId]);
        }

        #endregion
    }
    

}


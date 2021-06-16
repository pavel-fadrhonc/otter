using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public abstract class Command<T> : IPoolable<IMemoryPool, T>, IDisposable, ITickable
    {
        private IMemoryPool _pool;
        private bool _active;
        
        private List<DelayedAction> emptyDelayedActions = new List<DelayedAction>();
        private List<DelayedAction> delayedActions = new List<DelayedAction>();
        
        protected T signal;

        public void ExecuteCommand()
        {
            Execute();
            
            if (delayedActions.Count == 0)
                Dispose();
        }

        protected abstract void Execute();

        protected void RunDelayed(float delay, Action action)
        {
            DelayedAction delayedAction;
            if (emptyDelayedActions.Count > 0)
            {
                delayedAction = emptyDelayedActions[0];
                emptyDelayedActions.RemoveAt(0);
            }
            else
            {
                delayedAction = new DelayedAction();
            }

            delayedAction.action = action;
            delayedAction.delay = delay;
            delayedAction.elapsed = 0;
            
            delayedActions.Add(delayedAction);
        }

        public void Tick()
        {
            if (!_active)
                return;
            
            for (var index = 0; index < delayedActions.Count; index++)
            {
                var delayedAction = delayedActions[index];
                
                delayedAction.elapsed += Time.deltaTime;

                if (delayedAction.elapsed > delayedAction.delay)
                {
                    delayedAction.action();
                    delayedActions.Remove(delayedAction);
                    emptyDelayedActions.Add(delayedAction);
                    index--;
                }
            }
            
            if (delayedActions.Count == 0)
                Dispose();
        }

        public void OnDespawned()
        {
            _pool = null;
            _active = false;
        }

        public void OnSpawned(IMemoryPool pool, T signal_)
        {
            this.signal = signal_;
            _pool = pool;

            emptyDelayedActions.AddRange(delayedActions);
            delayedActions.Clear();

            _active = true;
        }

        public void Dispose()
        {
            _pool?.Despawn(this);
        }

        private class DelayedAction
        {
            public Action action;
            public float delay;
            public float elapsed;
        }
    }
    
    public class Pool<TCommand, TSignal> : PoolableMemoryPool<IMemoryPool, TSignal, TCommand> where TCommand : Command<TSignal>
    {
    }    
}
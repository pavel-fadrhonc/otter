using System;
using System.Collections;
using UnityEngine;

namespace Zenject
{
    public class ofCoroutine : IEnumerator, IPoolable<IEnumerator, IMemoryPool>, IDisposable
    {
        public event Action CoroutineFinished;
        
        private readonly CoroutineRunner _coroutineRunner;

        public ofCoroutine(
            CoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Has this been initialized with coroutine that it is currently running? (Even though it might be paused). 
        /// </summary>
        public bool IsValid => _pool != null;
        
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                if (!_paused)
                    _runTime = 0f;
            }
        }

        private IEnumerator _coroutine;
        private float _runTime;

        public void PauseFor(float delay)
        {
            Paused = true;
            _runTime = Time.time + delay;
        }

        #region IEnumerator

        public bool MoveNext()
        {
            if (Paused && _runTime > 0f && Time.time > _runTime)
            {
                Paused = false;
                _runTime = 0f;
            }

            if (Paused)
                return true;
            
            var result = _coroutine.MoveNext();
            if (!result)
                _coroutineRunner.StopCoroutine(this);

            return result;
        }

        public void Reset() { }
        
        public object Current =>  null;

        #endregion
        
        #region POOLABLE

        protected IMemoryPool _pool;
        private bool _paused;

        public void OnDespawned()
        {
            _pool = null;
            _coroutine = null;
        }

        public void OnSpawned(IEnumerator coroutine, IMemoryPool pool)
        {
            CoroutineFinished?.Invoke();
            
            _coroutine = coroutine;
            _pool = pool;
        }

        public void Dispose()
        {
            _pool?.Despawn(this);
        }

        #endregion

        public class Factory : PlaceholderFactory<IEnumerator, ofCoroutine>  { }
    }
}
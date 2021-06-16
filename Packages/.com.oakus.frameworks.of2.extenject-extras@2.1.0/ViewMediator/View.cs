using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public abstract class ViewBase : MonoBehaviour
    {
        protected virtual void Initialize() { }
    }

    public abstract class ViewPooled : ViewBase, IDisposable
    {
        private IMemoryPool _pool;

        public virtual void OnDespawned()
        {
            _pool = null;
        }

        public virtual void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
            
            Initialize();
        }

        public virtual void Dispose()
        {
            _pool.Despawn(this);
        }
    }  
    
    public abstract class ViewNotPooled : ViewBase
    {
        private bool _initialized;

        private List<IMediator<ViewNotPooled>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<ViewNotPooled>> mediators)
        {
            _mediators = mediators;
        }

        protected virtual void Initialize()
        {
            _initialized = true;
        }

        private void Start()
        {
            if (!_initialized)
            {
                Initialize();
                
                foreach (var mediator in _mediators)
                {
                    mediator.OnEnable();
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }
    } 
    
    public abstract class ViewNoParams : ViewPooled, IPoolable<IMemoryPool>
    {
        private List<IMediator<ViewNoParams>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<ViewNoParams>> mediators)
        {
            _mediators = mediators;
        }

        public override void OnDespawned()
        {
            base.OnDespawned();   

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public override void OnSpawned(IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }    
    
    public abstract class View<TParam> : ViewPooled, IPoolable<TParam, IMemoryPool>
    {
        private List<IMediator<View<TParam>, TParam>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam>, TParam>> mediators)
        {
            _mediators = mediators;
        }
        
        private TParam _param;
        
        public override void OnDespawned()
        {
            base.OnDespawned();

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam param, IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            _param = param;
            
            foreach (var m in _mediators) m.SetParam(param);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }
    
    public abstract class View<TParam1, TParam2> : ViewPooled, IPoolable<TParam1, TParam2, IMemoryPool>
    {
        private List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> _mediators;

        [Inject]
        public void Construct(
            List<IMediator<View<TParam1, TParam2>, TParam1, TParam2>> mediators)
        {
            _mediators = mediators;
        }
        
        private TParam1 _param1;
        private TParam2 _param2;
        private IMemoryPool _pool;
        
        public override void OnDespawned()
        {
            base.OnDespawned();

            foreach (var mediator in _mediators)
            {
                mediator.OnDisable();
            }
        }

        public void OnSpawned(TParam1 param1, TParam2 param2, IMemoryPool pool)
        {
            base.OnSpawned(pool);
            
            _param1 = param1;
            _param2 = param2;
            
            foreach (var m in _mediators) m.SetParams(param1, param2);
            
            foreach (var mediator in _mediators)
            {
                mediator.OnEnable();
            }
        }
    }    
}
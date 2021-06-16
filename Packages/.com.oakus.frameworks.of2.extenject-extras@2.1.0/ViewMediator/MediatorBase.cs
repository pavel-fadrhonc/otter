using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public abstract class MediatorBase<TView> : IMediator<TView> 
        where TView : ViewBase
    {
        [Inject] protected TView _view;
        [Inject] protected SignalBus _signalBus;
        
        [Inject]
        public MediatorBase() {}

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }
    }

    public abstract class MediatorBase<TView, TParam> : IMediator<TView, TParam> 
        where TView : View<TParam>
    {
        [Inject] protected TView _view;
        [Inject] protected SignalBus _signalBus;

        protected TParam param;
        
        [Inject]
        public MediatorBase() {}

        public void SetParam(TParam param)
        {
            this.param = param;
        }

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }
    }
    
    public abstract class MediatorBase<TView, TParam1, TParam2> : IMediator<TView, TParam1, TParam2> 
        where TView : View<TParam1, TParam2>
    {
        [Inject] protected TView _view;
        [Inject] protected SignalBus _signalBus;

        protected TParam1 param1;
        protected TParam2 param2;
        
        [Inject]
        public MediatorBase() {}

        public void SetParams(TParam1 param1, TParam2 param2)
        {
            this.param1 = param1;
            this.param2 = param2;
        }

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }
    }    

}
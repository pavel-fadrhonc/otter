namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public interface IMediator<out TView> where TView : ViewBase
    {
        void OnEnable();

        void OnDisable();
    }

    public interface IMediator<out TView, in TParam> where TView : View<TParam>
    {
        void SetParam(TParam param);

        void OnEnable();

        void OnDisable();
    }
    
    public interface IMediator<out TView, in TParam1, in TParam2> where TView : View<TParam1, TParam2>
    {
        void SetParams(TParam1 param1, TParam2 param2);

        void OnEnable();

        void OnDisable();
    }
}
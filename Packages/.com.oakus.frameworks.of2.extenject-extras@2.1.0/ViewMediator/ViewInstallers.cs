using System.Linq;
using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public class ViewInstallerNotPooledNoParams<TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<ViewNotPooled>
        where TView : ViewNotPooled
    {
        public override void InstallBindings()
        {
            Container.Bind(new [] {typeof(IMediator<ViewNotPooled>), typeof(TMediator1) }.Concat(typeof(TMediator1).GetInterfaces())).To<TMediator1>().AsSingle();
            Container.BindInstance(GetComponent<ViewNoParams>()).WhenInjectedInto<TMediator1>();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstallerNotPooledNoParams<TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<ViewNotPooled>
        where TMediator2 : IMediator<ViewNotPooled>
        where TView : ViewNotPooled
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<ViewNotPooled>>().To<TMediator1>().AsSingle();
            Container.Bind<IMediator<ViewNotPooled>>().To<TMediator2>().AsSingle();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstallerPooledNoParams<TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<ViewNoParams>
        where TView : ViewNoParams
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<ViewNoParams>>().To<TMediator1>().AsSingle();
            Container.BindInstance(GetComponent<ViewNoParams>()).WhenInjectedInto<TMediator1>();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstallerPooledNoParams<TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<ViewNoParams>
        where TMediator2 : IMediator<ViewNoParams>
        where TView : ViewNoParams
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<ViewNoParams>>().To<TMediator1>().AsSingle();
            Container.Bind<IMediator<ViewNoParams>>().To<TMediator2>().AsSingle();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller1Param1Mediator<TParam1, TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1>, TParam1>
        where TView : View<TParam1>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1>, TParam1>>().To(typeof(TMediator1)).AsSingle();
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller1Param2Mediators<TParam1, TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1>, TParam1>
        where TMediator2 : IMediator<View<TParam1>, TParam1>
        where TView : View<TParam1>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1>, TParam1>>().To(typeof(TMediator1), typeof(TMediator2)).AsSingle();

            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller2Param1Mediator<TParam1, TParam2, TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1, TParam2>, TParam1, TParam2>
        where TView : View<TParam1, TParam2>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1, TParam2>, TParam1, TParam2>>().To(typeof(TMediator1)).AsSingle();
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller2Params2Mediators<TParam1, TParam2, TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1, TParam2>, TParam1, TParam2>
        where TMediator2 : IMediator<View<TParam1, TParam2>, TParam1, TParam2>
        where TView : View<TParam1, TParam2>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1, TParam2>, TParam1, TParam2>>().To(typeof(TMediator1), typeof(TMediator2)).AsSingle();
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }    
}
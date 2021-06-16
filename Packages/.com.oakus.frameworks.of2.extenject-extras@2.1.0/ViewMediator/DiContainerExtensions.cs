using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public static class DiContainerExtensions
    {
        public static void BindViewFactory<TParam1, TView, TPlaceHolderFactory>(this DiContainer container, TView viewPrefab)
            where TView : View<TParam1>
            where TPlaceHolderFactory : PlaceholderFactory<TParam1, PrefabFactorySpawnParams, TView>
        {
            container.BindFactory<TParam1, PrefabFactorySpawnParams, TView, TPlaceHolderFactory>()
                .FromFactory<ViewFactory<TParam1, TView>>();

            container.BindInstance(viewPrefab).WhenInjectedInto<ViewFactory<TParam1, TView>>();
                
           container.BindFactory<UnityEngine.Object, 
                    ViewFactory<TParam1, TView>.Pool,
                    ViewFactory<TParam1, TView>.PoolFactory>()
                .FromSubContainerResolve()
                .ByInstaller<ViewFactory<TParam1, TView>.PoolInstaller>();                
        }
        
        public static void BindViewFactory<TParam1, TParam2, TView, TPlaceHolderFactory>(this DiContainer container, TView viewPrefab)
            where TView : View<TParam1, TParam2>
            where TPlaceHolderFactory : PlaceholderFactory<TParam1, TParam2, PrefabFactorySpawnParams, TView>
        {
            container.BindFactory<TParam1, TParam2, PrefabFactorySpawnParams, TView, TPlaceHolderFactory>()
                .FromFactory<ViewFactory<TParam1, TParam2, TView>>();

            container.BindInstance(viewPrefab).WhenInjectedInto<ViewFactory<TParam1, TParam2, TView>>();
                
            container.BindFactory<UnityEngine.Object, 
                    ViewFactory<TParam1, TParam2, TView>.Pool,
                    ViewFactory<TParam1, TParam2, TView>.PoolFactory>()
                .FromSubContainerResolve()
                .ByInstaller<ViewFactory<TParam1, TParam2, TView>.PoolInstaller>();                
        }           
    }
}
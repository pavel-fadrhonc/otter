using System.ComponentModel;
using UnityEngine;
using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public class ViewFactory<TParam, TView> : IFactory<TParam, PrefabFactorySpawnParams, TView>
        where TView : View<TParam>
    {
        [Inject] 
        private TView _prefab;
        
        [Inject]
        PoolFactory _factory;
        
        private Pool _pool;
        
        [Inject]
        public ViewFactory() {}

        public TView Create(TParam param, PrefabFactorySpawnParams spawnParams = null)
        {
            if (_pool == null)
            {
                _pool = _factory.Create(_prefab.gameObject);
            }
            
            var instance = _pool.Spawn();
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param, _pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TView>
        {
            [Inject] public Pool() {}
            
            protected override void OnDespawned(TView item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }
        }

        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool>
        {
            [Inject]
            public PoolFactory() {}
        }

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] public PoolInstaller() { }

            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TView, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TView, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }
    
    public class ViewFactory<TParam1, TParam2, TView> : IFactory<TParam1, TParam2, PrefabFactorySpawnParams, TView>
        where TView : View<TParam1, TParam2>
    {
        [Inject] 
        private TView _prefab;
        
        [Inject]
        PoolFactory _factory;
        
        private Pool _pool;
        
        [Inject]
        public ViewFactory() {}

        public TView Create(TParam1 param1, TParam2 param2, PrefabFactorySpawnParams spawnParams = null)
        {
            if (_pool == null)
            {
                _pool = _factory.Create(_prefab.gameObject);
            }
            
            var instance = _pool.Spawn();
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param1, param2, _pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TView>
        {
            [Inject] public Pool() {}
            
            protected override void OnDespawned(TView item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }
        }

        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool>
        {
            [Inject]
            public PoolFactory() {}
        }

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] public PoolInstaller() { }
            
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TView, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TView, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }    
}
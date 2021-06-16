using System.Collections.Generic;
using ModestTree;
using UnityEngine;

namespace Zenject
{
    /// <summary>
    /// Same as PrefabFactoryNameBased except the prefab is not passed in runtime in Create method but rather IPrefaFactoryPooledPrefabResolver
    /// class is injected in installer when binding the factory which will determine the prefab based on the TParam passed in Create
    /// </summary>
    /// <typeparam name="TContract"></typeparam>
    
    public class PrefabFactoryPoolable<TContract> : IFactory< 
        PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<IMemoryPool> 
    {
        public interface IPrefaFactoryPooledPrefabResolver
        {
            TContract ResolvePrefab();
        }
        
        [Inject]
        PoolFactory _factory;

        [Inject] // this needs to be bound externally
        private IPrefaFactoryPooledPrefabResolver _resolver;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryPoolable() {}

        public TContract Create(PrefabFactorySpawnParams spawnParams = null)
        {
            TContract instance = null;
            Pool pool = null;

            var prefab = _resolver.ResolvePrefab();
            
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];

            instance = pool.Spawn();
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TContract>
        {
            [Inject] public Pool() {}
            
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);
                
                item.OnDespawned();
            }
        }

        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool>
        {
            [Inject] public PoolFactory() {}
        }

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] public PoolInstaller() { }
            
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }
    
    public class PrefabFactoryPoolable<TParam, TContract> : IFactory<TParam, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<TParam, IMemoryPool> 
    {
        public interface IPrefaFactoryPooledPrefabResolver
        {
            TContract ResolvePrefab(TParam param);
        }

        [Inject]
        PoolFactory _factory;

        [Inject] 
        private IPrefaFactoryPooledPrefabResolver _resolver;

        private Dictionary<string, Pool> _prefabPool = new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryPoolable() {}

        public TContract Create(TParam param1, PrefabFactorySpawnParams spawnParams = null)
        {
            var prefab = _resolver.ResolvePrefab(param1);
            
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1);
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param1, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam, TContract>
        {
            [Inject] public Pool() {}
            
            protected override void OnDespawned(TContract item)
            {
                base.OnDespawned(item);

                item.OnDespawned();
            }
        }

        public class PoolFactory : PlaceholderFactory<UnityEngine.Object, Pool>
        {
            [Inject] public PoolFactory() {}
        }

        public class PoolInstaller : Installer<PoolInstaller>
        {
            [Inject] public PoolInstaller() { }
            
            [Inject] private UnityEngine.Object prefab;

            public override void InstallBindings()
            {
                if (prefab == null)
                { // just to shut the validator
                    Container.BindMemoryPool<TContract, Pool>().FromNewComponentOnNewGameObject();
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
            }
        }
    }            
}
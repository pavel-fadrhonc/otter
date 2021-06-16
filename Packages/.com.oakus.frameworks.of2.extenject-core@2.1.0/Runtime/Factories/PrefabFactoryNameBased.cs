#pragma warning disable 649

using System;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Zenject
{
    /// <summary>
    /// PrefabFactoryNameBased is able to pool instances of prefab by the name of the prefab.
    /// It takes the prefab as the parameter and resolves Component TContract on it.
    /// The prefab can or doesn't have to have GameObjectContext on it.
    /// If the version where TContract is a component is used that it searches for the component on gameObject so it can or doesn't have to have it.
    /// If the version where TContract is just a class is used than it resolves the subcontainer therefore is has to have GameObjectContext and Tcontract bound on it.
    ///
    /// the PrefabFactorySpawnParams purpose is to setup GO transform before OnSpawned gets called so it can safely assume it is on the right position.
    /// </summary>

    public class PrefabFactorySpawnParams
    {
        public Vector3? position;
        public Quaternion? rotation;
        public Vector3? scale;
        public Transform parent;
    }

    public class PrefabFactoryNameBasedHelper
    {
        public static void SetupGameObject(GameObject go, PrefabFactorySpawnParams spawnParams)
        {
            if (spawnParams.position != null)
                go.transform.position = spawnParams.position.Value;

            if (spawnParams.rotation != null)
                go.transform.rotation = spawnParams.rotation.Value;

            if (spawnParams.scale != null)
                go.transform.localScale = spawnParams.scale.Value;
            
            if (spawnParams.parent != null)
                go.transform.SetParent(spawnParams.parent);
        }
    }
    
    public class PrefabFactoryNameBasedNonComp<TContract> : IFactory<Object, PrefabFactorySpawnParams, TContract>
        where TContract : class, IPoolable<IMemoryPool>  
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBasedNonComp() {}
        
        public TContract Create(Object prefab, PrefabFactorySpawnParams spawnParams = null)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];

            instance = pool.Spawn();
            
            // if (spawnParams != null)
            //     PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(pool);

            return instance;
        }

        public class Pool : MemoryPool<TContract>
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
                    if (typeof(TContract).DerivesFrom(typeof(Component)))
                    {
                        Container.BindMemoryPool<TContract, Pool>().FromSubContainerResolve().ByMethod(container => container.Bind<TContract>().FromNewComponentOnRoot().AsSingle());
                    }
                    else
                    {
                        Container.BindMemoryPool<TContract, Pool>().FromSubContainerResolve().ByMethod(container => container.Bind<TContract>().AsSingle());    
                    }
                    
                    Debug.LogWarning("No prefab injected in PoolInstaller.");
                    return;
                }
                
                //Container.BindMemoryPool<TContract, Pool>().FromComponentInNewPrefab(prefab);
                Container.BindMemoryPool<TContract, Pool>().FromSubContainerResolve().ByNewContextPrefab(prefab);
            }
        }
    }    
    
    public class PrefabFactoryNameBased<TContract> : IFactory<Object, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBased() {}

        public TContract Create(Object prefab, PrefabFactorySpawnParams spawnParams = null)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
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

    public class PrefabFactoryNameBased<TParam, TContract> : IFactory<Object, TParam, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<TParam, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool = new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBased() {}

        public TContract Create(Object prefab, TParam param1, PrefabFactorySpawnParams spawnParams = null)
        {
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

    public class PrefabFactoryNameBased<TParam1, TParam2, TContract> : IFactory<Object, TParam1, TParam2, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBased() {}

        public TContract Create(Object prefab, TParam1 param1, TParam2 param2, PrefabFactorySpawnParams spawnParams = null)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2);
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param1, param2, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TContract>
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

    public class PrefabFactoryNameBased<TParam1, TParam2, TParam3, TContract> : IFactory<Object, TParam1, TParam2, TParam3, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, TParam3, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBased() {}

        public TContract Create(Object prefab, TParam1 param1, TParam2 param2, TParam3 param3, PrefabFactorySpawnParams spawnParams = null)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2, param3);
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param1, param2, param3, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TParam3, TContract>
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
    
    public class PrefabFactoryNameBased<TParam1, TParam2, TParam3, TParam4, TContract> : IFactory<Object, TParam1, TParam2, TParam3, TParam4, PrefabFactorySpawnParams, TContract>
        where TContract : Component, IPoolable<TParam1, TParam2, TParam3, TParam4, IMemoryPool> 
    {
        [Inject]
        PoolFactory _factory;

        private Dictionary<string, Pool> _prefabPool =new Dictionary<string, Pool>();
        
        [Inject]
        public PrefabFactoryNameBased() {}

        public TContract Create(Object prefab, TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, PrefabFactorySpawnParams spawnParams = null)
        {
            Assert.That(prefab != null,
                "Null prefab given to factory create method when instantiating object with type '{0}'.", typeof(TContract));

            TContract instance = null;
            Pool pool = null;
            if (!_prefabPool.ContainsKey(prefab.name))
            {
                _prefabPool[prefab.name] = _factory.Create(prefab);
            }
            
            pool = _prefabPool[prefab.name];
            
            instance = pool.Spawn(param1, param2, param3, param4);
            
            if (spawnParams != null)
                PrefabFactoryNameBasedHelper.SetupGameObject(instance.gameObject, spawnParams);

            instance.OnSpawned(param1, param2, param3, param4, pool);

            return instance;
        }

        public class Pool : MonoMemoryPool<TParam1, TParam2, TParam3, TParam4, TContract>
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
﻿using System.Collections.Generic;
using UnityEngine;
using OakFramework2.BaseMono;

namespace of2.Pool
{
    /// <summary>
    /// Object pool manager for managing multiple pools. One per prefab.
    /// </summary>
    public class PoolManager : of2GameObject
    {
        public struct TemporaryObject
        {
            public GameObject rentedObject;
            public float remainingTime;
        }

        [SerializeField]
        private ObjectPool[] m_ObjectPools = new ObjectPool[0];
        
        protected Dictionary<GameObject, ObjectPool> m_ObjectPoolDictionary = new Dictionary<GameObject, ObjectPool>();
        protected Dictionary<GameObject, List<GameObject>> m_RentedObjects = new Dictionary<GameObject, List<GameObject>>();
        protected List<TemporaryObject> m_TemporaryObjects = new List<TemporaryObject>();

        protected override void Awake()
        {
            base.Awake();

            // Clean-up in case something added objects to the pool before Awake
            if (m_ObjectPoolDictionary != null)
            {
                foreach (KeyValuePair<GameObject, ObjectPool> pair in m_ObjectPoolDictionary)
                {
                    pair.Value.ReturnAll();
                    //Destroy(pair.Value.cachedGameObject);
                }
            }

            // Create a dictionary of objects pools. One for each prefab. We use the prefab as key, to identify it later.
            m_ObjectPoolDictionary = new Dictionary<GameObject, ObjectPool>();
            // Create a dictionary of all rented out objects. The key is the prefab that the rented out object was created from.
            m_RentedObjects = new Dictionary<GameObject, List<GameObject>>();

            InitPools();
        }

        protected virtual void Update()
        {
            for (int i = m_TemporaryObjects.Count - 1; i >= 0; i--)
            {
                TemporaryObject to = m_TemporaryObjects[i];
                to.remainingTime -= Time.deltaTime;
                if (to.remainingTime <= 0f)
                {
                    ReturnObject(m_TemporaryObjects[i].rentedObject);
                }
                else
                {
                    m_TemporaryObjects[i] = to;
                }
            }
        }

        private void InitPools()
        {
            if (m_ObjectPools.Length == m_ObjectPoolDictionary.Count)
                return;

            // Add all manually defined object pools to the object pool dictionary.
            for (int i = 0; i < m_ObjectPools.Length; i++)
            {
                if (!m_ObjectPoolDictionary.ContainsKey(m_ObjectPools[i].GetObjectPrefab()))
                {
                    m_ObjectPoolDictionary.Add(m_ObjectPools[i].GetObjectPrefab(), m_ObjectPools[i]);
                }
            }
        }

        public void CreatePoolFromPrefab(GameObject prefab, int initialPoolSize, bool worldPositionStays = true)
        {
            if (prefab == null)
            {
                return;
            }

            InitPools();

            if (!m_ObjectPoolDictionary.ContainsKey(prefab))
            {
                // Create an object pool for the current game object...
                GameObject objectPoolGO = new GameObject(prefab.name + "_ObjectPool");
                objectPoolGO.transform.SetParent(cachedTransform, worldPositionStays);
                ObjectPool pool = objectPoolGO.AddComponent<ObjectPool>();
                pool.SetInitialObjectCount(initialPoolSize);
                // Setting the pool's object prefab will automatically cause it to (re)initialize. Thus the initial object count should be set prior to this.
                pool.SetObjectPrefab(prefab);
                // Add dictionary entry for this prefab.
                m_ObjectPoolDictionary.Add(prefab, pool);
            }
        }

        private void RecreatePoolFromPrefab(GameObject prefab, int initialPoolSize)
        {
            if (prefab == null)
            {
                return;
            }

            if (!m_ObjectPoolDictionary.ContainsKey(prefab))
            {
                Debug.LogWarning("No pool exist for prefab '" + prefab.name + "'! Creating a clean one.");
                CreatePoolFromPrefab(prefab, initialPoolSize);
            }
            else
            { 
                if (m_ObjectPoolDictionary[prefab] != null)
                {
                    m_ObjectPoolDictionary[prefab].ReturnAll();
                    Destroy(m_ObjectPoolDictionary[prefab].cachedGameObject);
                }
                m_ObjectPoolDictionary.Remove(prefab);
                CreatePoolFromPrefab(prefab, initialPoolSize);
            }
        }

        public void PullBackAllRentedObjects()
        {
            // Go through the dictionary of all rented out objects...
            foreach (KeyValuePair<GameObject, List<GameObject>> pair in m_RentedObjects)
            {
                if (pair.Value != null && pair.Value.Count > 0)
                {
                    for (int i = 0; i < pair.Value.Count; i++)
                    {
                        if (pair.Value[i] != null && m_ObjectPoolDictionary != null && m_ObjectPoolDictionary.ContainsKey(pair.Key))
                        {
                            // Return the object to the correct pool.
                            m_ObjectPoolDictionary[pair.Key].ReturnObject(pair.Value[i]);
                        }
                    }
                }
            }

            m_RentedObjects.Clear();
        }

        /// <summary>
        /// Rent an object of the given prefab. If duration is set to a positive value, the object will be returned after that amount of seconds.
        /// </summary>
        /// <param name="prefab">Reference to the prefab you want to get an instance of.</param>
        /// <param name="duration">How long the instance should be active. If value is positive, the object will be automatically returned after that amount of seconds.</param>
        /// <returns>Rented object instance.</returns>
        public GameObject RentObject(GameObject prefab, float duration = -1)
        {
            return RentObject(prefab, Vector3.zero, Quaternion.identity, null, duration);
        }

        /// <summary>
        /// Rent an object of the given prefab. If duration is set to a positive value, the object will be returned after that amount of seconds.
        /// </summary>
        /// <param name="prefab">Reference to the prefab you want to get an instance of.</param>
        /// <param name="position">The position that the object should be activated at. Position will be changed before activating the object, so OnEnable calls will know about the updated position.</param>
        /// <param name="rotation">The rotation that the object should be activated at. Rotation will be changed before activating the object, so OnEnable calls will know about the updated rotation.</param>
        /// <param name="parent">The parent transform that the object will be a child of. Parent will be changed before activating the object, so OnEnable calls will know about the updated parent.</param>
        /// <param name="duration">How long the instance should be active. If value is positive, the object will be automatically returned after that amount of seconds.</param>
        /// <returns>Rented object instance.</returns>
        public GameObject RentObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, float duration = -1)
        {
            if (m_ObjectPoolDictionary.ContainsKey(prefab))
            {
                if (m_ObjectPoolDictionary[prefab] == null)
                {
                    RecreatePoolFromPrefab(prefab, 3);
                }
                // Get an instance of the requested prefab
                GameObject rentedObject = m_ObjectPoolDictionary[prefab].RentObject(position, rotation, parent);
                if (rentedObject == null)
                    return null;
                if (!m_RentedObjects.ContainsKey(prefab))
                {
                    // If an entry for this prefab doesn't already exists in the dictionary of rented out objects, we create it.
                    List<GameObject> newList = new List<GameObject>();
                    newList.Add(rentedObject);
                    m_RentedObjects.Add(prefab, newList);
                }
                else
                {
                    m_RentedObjects[prefab].Add(rentedObject);
                }

                // If rented object is set to have a specific duration, add it to list of temporary rented objects, so it'll be returned automatically
                if (duration >= 0f)
                {
                    TemporaryObject to = new TemporaryObject();
                    to.rentedObject = rentedObject;
                    to.remainingTime = duration;
                    m_TemporaryObjects.Add(to);
                }

                PoolObject pgo = rentedObject.GetComponent<PoolObject>();
                if (pgo != null)
                {
                    pgo.SetOwningPoolManager(this);
                }

                return rentedObject;
            }

            return null;
        }

        public bool ReturnObject(GameObject objectInstance)
        {
            // If object was part of the temporary rented objects, remove it from there
            m_TemporaryObjects.RemoveAll(to => to.rentedObject == objectInstance);
            // Find out which pool the instance belongs to.
            foreach (KeyValuePair<GameObject, List<GameObject>> pair in m_RentedObjects)
            {
                if (pair.Value.Contains(objectInstance))
                {
                    // Return object to correct pool
                    m_ObjectPoolDictionary[pair.Key].ReturnObject(objectInstance);
                    pair.Value.Remove(objectInstance);
                    return true;
                }
            }

            return false;
        }
    }
}
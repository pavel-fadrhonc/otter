﻿
using OakFramework2.BaseMono;

namespace of2.Pool
{
    /// <summary>
    /// Script added to every pooled object.
    /// Contains methods and info about the pool it is used in.
    /// </summary>
    public class PoolObject : of2GameObject
    {

        private ObjectPool m_OwningPool;
        private PoolManager m_OwningPoolManager;

        /// <summary>
        /// Set the pool that this object was created from. This is also the pool that the object will be returned to.
        /// </summary>
        /// <param name="owningPool">The pool that created this object.</param>
        public void SetOwningPool(ObjectPool owningPool)
        {
            m_OwningPool = owningPool;
        }

        /// <summary>
        /// If the object was created through a pool manager, set the pool manager reference. When returning the object, we'll let the manager handle returning it to the correct pool.
        /// </summary>
        /// <param name="owningPoolManager">The pool manager that rented out this object.</param>
        public void SetOwningPoolManager(PoolManager owningPoolManager)
        {
            m_OwningPoolManager = owningPoolManager;
        }

        /// <summary>
        /// When you don't need the object anymore, call this method. If the object was created by a pool and that pool still exist, the object will be returned correctly. Otherwise it gets destroyed.
        /// </summary>
        public void ReturnOrDestroy()
        {
            // If a pool manager is set, let the pool handle returning the object.
            if (m_OwningPoolManager != null)
            {
                m_OwningPoolManager.ReturnObject(cachedGameObject);
            }
            // If there is no pool manager set, but an object pool is set, return to that object pool
            else if (m_OwningPool != null)
            {
                m_OwningPool.ReturnObject(cachedGameObject);
            }
            // If no pools are set, just destroy the game object.
            else
            {
                Destroy(cachedGameObject);
            }
        }

        /// <summary>
        /// Returns true if this object has been returned to its pool and is thus no longer active in the game world. False otherwise.
        /// </summary>
        /// <returns>Whether or not the object has been returned to pool.</returns>
        public bool HasObjectBeenReturnedToPool()
        {
            if (m_OwningPool != null)
            {
                return m_OwningPool.IsObjectAvailableInPool(cachedGameObject);
            }

            return false;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.U2D;
using Zenject;

namespace of2.AssetManagement
{
    public interface IAssetManager
    {
        /// <summary>
        /// Preloads asset and saves it to cache
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <returns></returns>
        void PreloadAsset(string path);

        /// <summary>
        /// Checks if given asset is in cache.
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <returns></returns>
        bool IsAssetPreloaded(string path);

        /// <summary>
        /// Get Asset by path,
        /// Takes object from cache if available.
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Wanted asset of type T</returns>
        T GetAsset<T>(string path,  bool saveToCache = true)
            where T : Object;

        /// <summary>
        /// Get Sprite by path,
        /// Takes object from cache if available.
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <returns>Sprite</returns>
        Sprite GetSprite(string path, bool saveToCache = true);

        /// <summary>
        /// Get Sprite from Spite Atlas,
        /// Takes object from cache if available.
        ///
        /// </summary>
        /// <param name="path">Sprite path relative to sprites root</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <returns>Sprite</returns>
        Sprite GetSpriteFromAtlas(string path, string sprite, bool saveToCache = true);

        /// <summary>
        /// Get Texture2D by path,
        /// Takes object from cache if available.
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <returns>Texture2D</returns>
        Texture2D GetTexture(string path, bool saveToCache = true);

		/// <summary>
		/// Get Material by path,
		/// Takes object from cache if available.
		///
		/// </summary>
		/// <param name="path">Absolute path to asset</param>
		/// <param name="saveToCache">If Asset should be saved to cache</param>
		/// <returns>Sprite</returns>
		Material GetMaterial(string path, bool saveToCache = true);
        
        /// <summary>
        /// Get AudioClip by path,
        /// Takes object from cache if available.
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <returns>AudioClip</returns>
        AudioClip GetAudioClip(string path, bool saveToCache = true);

        /// <summary>
        /// Get prefab by path,
        /// Takes object from cache if available.
        /// Make sure you instantiate the prefab before you use it.
        ///
        /// </summary>
        /// <param name="path">Absolute path to asset</param>
        /// <param name="saveToCache">If Asset should be saved to cache</param>
        /// <returns>Gameobject</returns>
        GameObject GetPrefab(string path, bool saveToCache = true);
        
        void Destroy();
    }

    [type: Preserve]
    public class AssetManager : IAssetManager
    {
        private Dictionary<string, Object> cachedObjects = new Dictionary<string, Object>();
            
        [Inject]
        public AssetManager(){}            
            
        public void PreloadAsset(string path)
        {
            if (cachedObjects.ContainsKey(path))
                return;

            var loadedObj = Resources.Load(path);
            cachedObjects[path] = loadedObj;
        }

        public bool IsAssetPreloaded(string path)
        {
            return cachedObjects.ContainsKey(path);
        }

        public T GetAsset<T>(string path, bool saveToCache = true) where T : Object
        {
            Object loadedObj = TryGetFromCache<T>(path, saveToCache);

            if (loadedObj == null)
            {
                Debug.LogErrorFormat("AssetManager.GetAsset: Asset in path {0} does not exist !" , path);
                return null;
            }
            
            return (T) loadedObj;
        }

		public Material GetMaterial(string path, bool saveToCache = true)
		{
			return TryGetFromCache<Material>(path, saveToCache);
		}

        public AudioClip GetAudioClip(string path, bool saveToCache = true)
        {
            return TryGetFromCache<AudioClip>(path, saveToCache);
        }

        public GameObject GetPrefab(string path, bool saveToCache = true)
        {
            return TryGetFromCache<GameObject>(path, saveToCache);
        }

        public virtual Sprite GetSprite(string path, bool saveToCache = true)
        {
            return TryGetFromCache<Sprite>(path, saveToCache);
        }

        public Sprite GetSpriteFromAtlas(string path, string sprite, bool saveToCache = true)
        {
            var atlas = TryGetFromCache<SpriteAtlas>(path, saveToCache);
            
            var loadedSprite = atlas.GetSprite(sprite);
            if (loadedSprite == null)
            {
                Debug.LogErrorFormat("AssetManager.GetSpriteFromAtlas: There is no Sprite in atlas {0} with name {1}", path, sprite );
            }

            return loadedSprite;
        }

        public Texture2D GetTexture(string path, bool saveToCache = true)
        {
            return TryGetFromCache<Texture2D>(path, saveToCache);
        }

        public void Destroy()
        {
            cachedObjects = null;
        }

        protected T TryGetFromCache<T>(string path, bool saveToCache) where T : Object 
        {
            T loadedObject;

            if (cachedObjects.ContainsKey(path))
            {
                loadedObject = cachedObjects[path] as T;
            }
            else
            {
                
                loadedObject = Resources.Load<T>(path);
                if (saveToCache)
                    cachedObjects[path] = loadedObject;
            }

            if (loadedObject == null)
            {
                Debug.LogErrorFormat("AssetManager.TryGetFromCache: There is no Asset in path {0} or its not of type {1} ", path, typeof(T));
                return null;
            }

            return loadedObject;
        }
    }
}
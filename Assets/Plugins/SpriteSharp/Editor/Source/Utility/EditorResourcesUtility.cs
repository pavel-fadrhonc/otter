using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    internal static class EditorResourcesUtility {
        public static T GetIncludedResource<T>(string resourcePath, string resourceName, bool deepSearchIfNeeded = true)
            where T : UnityEngine.Object {
            T resource = Resources.Load<T>(Path.Combine(resourcePath, resourceName));
            if (deepSearchIfNeeded && resource == null) {
                IEnumerable<T> resources =
                    AssetDatabase.FindAssets(resourceName + " t:" + typeof(T).Name)
                        .Select<string, string>(AssetDatabase.GUIDToAssetPath)
                        .Select<string, T>(s => (T) AssetDatabase.LoadAssetAtPath(s, typeof(T)));

                resource = resources.FirstOrDefault(r => r.name == resourceName);
            }

            return resource;
        }
    }
}
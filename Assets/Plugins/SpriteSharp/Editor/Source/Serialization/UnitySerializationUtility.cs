using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LostPolygon.SpriteSharp.Serialization.Internal {
    internal static class UnitySerializationUtility {
        public static Object DeserializeAssetDatabaseObject(string guid, int localIdentifier) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (String.IsNullOrEmpty(assetPath))
                return null;

            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (objects == null || objects.Length == 0)
                return null;

            for (int i = 0; i < objects.Length; i++) {
                Object obj = objects[i];
                int objectLocalId = obj.GetLocalIdentifierInFile();
                if (objectLocalId == localIdentifier)
                    return obj;
            }

            return null;
        }

        public static bool GetUnityObjectReferenceData(Object unityObject, out string path, out string guid, out int localIdentifier) {
            path = null;
            guid = null;
            localIdentifier = 0;

            if (unityObject == null)
                return false;

            path = AssetDatabase.GetAssetPath(unityObject);
            if (path == null)
                return false;

            guid = AssetDatabase.AssetPathToGUID(path);
            if (guid == null)
                return false;

            localIdentifier = unityObject.GetLocalIdentifierInFile();
            return true;
        }

        public static Dictionary<string, object> GetUnityObjectReferenceJson(string guid, int localIdentifier, string path) {
            return new Dictionary<string, object> {
                { "Guid", guid },
                { "LocalIdentifier", localIdentifier },
                { "LastPath", path },
            };
        }

        public static TLazyReferenceType CreateLazyReference<TObjectType, TLazyReferenceType>(TObjectType unityObject, Func<String, int, int, TLazyReferenceType> createFunc)
            where TObjectType : Object where TLazyReferenceType : UnityEngineObjectLazyReference<TObjectType> {
            string path, guid;
            int localIdentifier;
            bool exists = GetUnityObjectReferenceData(unityObject, out path, out guid, out localIdentifier);
            if (!exists)
                return null;

            TLazyReferenceType lazyReference = createFunc(guid, localIdentifier, unityObject.GetInstanceID());
            return lazyReference;
        }
    }
}
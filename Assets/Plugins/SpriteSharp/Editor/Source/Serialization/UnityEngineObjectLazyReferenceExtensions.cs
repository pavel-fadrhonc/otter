using System;
using Object = UnityEngine.Object;

namespace LostPolygon.SpriteSharp.Serialization.Internal {
    internal static class UnityEngineObjectLazyReferenceExtensions {
        public static bool IsNull<T>(this UnityEngineObjectLazyReference<T> unityEngineObjectLazyReference, bool checkInstance = true) where T : Object {
            return
                unityEngineObjectLazyReference == null ||
                String.IsNullOrEmpty(unityEngineObjectLazyReference.Guid) ||
                (checkInstance && unityEngineObjectLazyReference.Instance == null);
        }
    }
}
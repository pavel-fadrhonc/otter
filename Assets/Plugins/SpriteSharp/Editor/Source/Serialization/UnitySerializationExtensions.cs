using UnityEditor;
using Object = UnityEngine.Object;

namespace LostPolygon.SpriteSharp.Serialization.Internal {
    internal static class UnitySerializationExtensions {
        public static int GetLocalIdentifierInFile(this Object unityObject) {
            if (unityObject == null)
                return 0;

            // Undocumented API
            return Unsupported.GetLocalIdentifierInFile(unityObject.GetInstanceID());
        }
    }
}
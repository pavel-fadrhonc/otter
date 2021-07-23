using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using LostPolygon.SpriteSharp.fastJSON;
using LostPolygon.SpriteSharp.Internal;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;

namespace LostPolygon.SpriteSharp.Json.Internal {
    /// <summary>
    /// Sets up fastJSON for Unity objects serialization, and sets up the serialization parameters.
    /// </summary>
    internal static class fastJSONUnitySetup {
        private static bool _hasRegisteredTypes;

        static fastJSONUnitySetup() {
            RegisterUnityTypesSerialization();

            JSON.Parameters.UseEscapedUnicode = false;
            JSON.Parameters.SerializeNullValues = false;
            JSON.Parameters.ShowReadOnlyProperties = false;
            JSON.Parameters.UsingGlobalTypes = false;
            JSON.Parameters.UseExtensions = false;
            JSON.Parameters.UseValuesOfEnums = false;
            JSON.Parameters.SerializeToLowerCaseNames = false;
        }

        public static void RegisterUnityTypesSerialization() {
            if (_hasRegisteredTypes)
                return;

            _hasRegisteredTypes = true;

            // Check is SpriteSharp assembly is loaded
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return;

            JSON.RegisterCustomType(
                typeof(SpriteLazyReference),
                SpriteLazyReferenceSerializer,
                SpriteLazyReferenceDeserializer
                );

            JSON.RegisterCustomType(
                typeof(BuildTargetGroup),
                BuildTargetGroupSerializer,
                BuildTargetGroupDeserializer
            );
        }

        private static object BuildTargetGroupDeserializer(object data) {
            return Enum.Parse(typeof(BuildTargetGroup), (string) data);
        }

        private static object BuildTargetGroupSerializer(object data) {
            BuildTargetGroup buildTargetGroup = (BuildTargetGroup) data;
            switch (buildTargetGroup) {
                case BuildTargetGroup.iOS:
                    return "iOS";
                case BuildTargetGroup.WSA:
                    return "WSA";
            }

            return data.ToString();
        }

        private static object SpriteLazyReferenceDeserializer(object data) {
            Dictionary<string, object> descriptor = data as Dictionary<string, object>;
            if (descriptor == null)
                return null;

            object tempObject;
            if (!descriptor.TryGetValue("Guid", out tempObject))
                return null;

            string assetGuid = (string) tempObject;
            if (!descriptor.TryGetValue("LocalIdentifier", out tempObject))
                return null;

            int assetLocalId = Convert.ToInt32(tempObject);
            SpriteLazyReference spriteLazyReference =
                new SpriteLazyReference(
                    assetGuid,
                    assetLocalId
                );

            return spriteLazyReference;
        }

        private static object SpriteLazyReferenceSerializer(object data) {
            SpriteLazyReference spriteLazyReference = data as SpriteLazyReference;
            if (spriteLazyReference == null)
                return null;

            if (String.IsNullOrEmpty(spriteLazyReference.Guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(spriteLazyReference.Guid);
            if (String.IsNullOrEmpty(path))
                return null;

            Dictionary<string, object> unityObjectReferenceJson =
                UnitySerializationUtility.GetUnityObjectReferenceJson(
                    spriteLazyReference.Guid,
                    spriteLazyReference.LocalIdentifier,
                    path
                );

            return unityObjectReferenceJson;
        }
    }
}
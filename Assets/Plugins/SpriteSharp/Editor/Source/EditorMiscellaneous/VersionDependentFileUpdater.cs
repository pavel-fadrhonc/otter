using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Internal {
    internal class VersionDependentFileUpdate {
#if !SS_DISABLE_FILE_VERSIONING
        [InitializeOnLoadMethod]
        private static void UpdateVersionDependentFilesOnLoad() {
            EditorApplication.delayCall += UpdateVersionDependentFiles;
        }
#endif

        public static void UpdateVersionDependentFiles() {
            UpdateFile<Shader>("Shaders/", "Sprites-Opaque");
            UpdateFile<Shader>("Shaders/", "Sprites-OpaqueDiffuse");
        }

        private static void UpdateFile<T>(string resourceDirectoryName, string resourceName) where T : UnityEngine.Object {
            try {
                T resource = EditorResourcesUtility.GetIncludedResource<T>(resourceDirectoryName, resourceName);
                string resourcePath = AssetDatabase.GetAssetPath(resource);
                string resourceDirectory = Path.GetDirectoryName(resourcePath);
                string resourceVersionsRootDirectory = Path.Combine(resourceDirectory, "VersionDependent");
                VersionDependentFile versionDependentFile = new VersionDependentFile(resourceVersionsRootDirectory, resourcePath);
                versionDependentFile.Update();
#if !SS_TRACE && false
            } catch {
#else
            } catch (Exception e) {
                Debug.Log(e);
#endif
            }
        }
    }
}
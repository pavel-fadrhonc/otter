using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Helper utilities for working with Textures.
    /// </summary>
    public static class EditorTextureUtility {
        public static void ReimportDistinctTextures(IEnumerable<Texture2D> textures) {
            Texture2D[] distinctTextures = textures.Distinct().ToArray();
            if (distinctTextures.Length == 0)
                return;

            ReimportTextures(distinctTextures);
        }

        public static void ReimportTextures(IEnumerable<Texture2D> textures) {
            try {
                AssetDatabase.StartAssetEditing();
                foreach (Texture2D texture in textures) {
                    AssetDatabase.ImportAsset(
                        AssetDatabase.GetAssetPath(texture),
                        ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer
                    );
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static TextureImporter GetTextureImporter(string texturePath) {
            return AssetImporter.GetAtPath(texturePath) as TextureImporter;
        }
    }
}
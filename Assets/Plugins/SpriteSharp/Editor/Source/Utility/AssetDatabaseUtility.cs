using System.Collections.Generic;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Helper utilities for working with Textures.
    /// </summary>
    public static class AssetDatabaseUtility {
        public static void ReimportAssets(ICollection<string> paths) {
            try {
                AssetDatabase.StartAssetEditing();
                foreach (string path in paths) {
                    AssetDatabase.ImportAsset(
                        path,
                        ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer
                    );
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}
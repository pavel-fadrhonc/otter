using System;
using System.Collections;
using UnityEngine;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing.Internal {
    /// <summary>
    /// Triggers SpriteImportManager.Instance.ExecuteDelayedImportOperations during "Reimport All".
    /// </summary>
    internal class SpriteImportManagerHelperAssetModificationProcessor : UnityEditor.AssetModificationProcessor {
        public static string[] OnWillSaveAssets(string[] paths) {
            if (ReimportAllDetector.IsDatabaseAvailable()) {
                if (((IList)paths).Contains("ProjectSettings/ProjectSettings.asset")) {
                    DatabaseProxy.Instance.SaveToFile();
                }
            }

            try {
                SpriteImportManager.Instance.ExecuteDelayedImportOperations(false, false);
            } catch (Exception e) {
                Debug.LogException(e);
            }

            return paths;
        }
    }
}
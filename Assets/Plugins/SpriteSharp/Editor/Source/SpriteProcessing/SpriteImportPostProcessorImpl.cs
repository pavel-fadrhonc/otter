using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Internal;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing.Internal {
    /// <summary>
    /// The actual implementation of SpriteImportPostProcessor.
    /// </summary>
    internal static class SpriteImportPostProcessorImpl {
        internal static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return;

#if SS_TRACE
            Debug.Log("importedAssets[]: " + String.Join("\n", importedAssets));
#endif
            if (importedAssets.Length == 0 && movedAssets.Length == 0 && SpriteImportManager.Instance.TexturesImportedInWrongState.Count == 0)
                return;

            SpriteImportManager.Instance.ExecuteDelayedImportOperations(true, true);
            SpriteImportManager.Instance.ProcessMovedAssets(movedAssets);
        }

        internal static async void OnPostprocessSprites(AssetPostprocessor postprocessor, Texture2D texture, Sprite[] sprites)
        {
            //await Task.Delay(1);
            
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return;

#if SS_TRACE
            Debug.Log("sprites[]: " + String.Join("\n", sprites.Select(s => s.name).ToArray()));
#endif
            Texture2D originalTexture = texture;
            texture = AssetDatabase.LoadMainAssetAtPath(postprocessor.assetPath) as Texture2D;
            if (texture != null && !texture.IsTightSpriteMesh())
                return;

            // Only delay the texture import if AssetDatabase is not available
            if (texture == null && !ReimportAllDetector.IsDatabaseAvailable()) {
                SpriteImportManager.Instance.TexturesImportedInWrongState.Add(postprocessor.assetPath);
                return;
            }

            if (texture == null) {
                texture = originalTexture;
            }

            DatabaseProxy database = DatabaseProxy.Instance;
            if (database.IsProcessingDisabled)
                return;

            // var spriteObjs = AssetDatabase.LoadAllAssetsAtPath(postprocessor.assetPath).Where(ass => ass is Sprite).ToArray();
            // Sprite[] loadedSprites = new Sprite[spriteObjs.Length];
            // for (var index = 0; index < spriteObjs.Length; index++)
            // {
            //     var spriteObj = spriteObjs[index];
            //     
            //     loadedSprites[index] = spriteObj as Sprite;
            // }

            SpriteImportManager.ProcessSprites(texture, sprites);
        }
    }
}

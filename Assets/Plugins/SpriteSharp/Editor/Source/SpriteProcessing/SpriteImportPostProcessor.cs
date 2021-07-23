using UnityEngine;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Processing.Internal {
    /// <summary>
    /// Main texture and sprite postprocessor.
    /// </summary>
    internal class SpriteImportPostProcessor : AssetPostprocessor {
        public override uint GetVersion() {
            return 2;
        }

        public override int GetPostprocessOrder() {
            return 2;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets, 
            string[] movedFromAssetPaths) {
            SpriteImportPostProcessorImpl.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
        }

        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites) {
            SpriteImportPostProcessorImpl.OnPostprocessSprites(this, texture, sprites);
        }
    }
}

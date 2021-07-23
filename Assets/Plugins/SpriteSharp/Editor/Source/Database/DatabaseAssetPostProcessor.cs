using System.Linq;
using UnityEditor;
using LostPolygon.SpriteSharp.Internal;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Database.Internal {
    /// <summary>
    /// Manages the SpriteSharp database asset.
    /// </summary>
    internal class DatabaseAssetPostProcessor : AssetPostprocessor {
        public override int GetPostprocessOrder() {
            return 0;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths) {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return;

            if (!ReimportAllDetector.IsDatabaseAvailable())
                return;

            if (!PersistentMarker.IsExists)
                return;

            string databasePath = DatabaseAssetsManager.Instance.GetDatabasePath(false);
            bool isDatabaseImported = importedAssets.FirstOrDefault(importedAssetPath => importedAssetPath == databasePath) != null;
            if (isDatabaseImported) {
                if (DatabaseProxy.Instance.LoadFromFile(true)) {
                    if (DatabaseProxy.Instance.ReimportOnMismatch) {
                        DatabaseDiffProcessor.ProcessDiff(databasePath, DatabaseAssetsManager.Instance.GetDatabaseDiffCopyPath());
                    }
                }
            }
        }

    }
}
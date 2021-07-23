using UnityEngine;
using LostPolygon.SpriteSharp.Database.Internal;
using LostPolygon.SpriteSharp.Internal;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Detects whether "Reimport All" is going on.
    /// Checking whether AssetDatabase is initialized is done by attempting to load
    /// a known "marker" asset that should always be available. Failing to load this asset
    /// means that AssetDatabase is not initialized.
    /// </summary>
    internal static class ReimportAllDetector {
        private static bool _isMarkerLoaded;

        public static bool IsDatabaseAvailable(bool forceCheck = false) {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return false;

            if (!forceCheck && _isMarkerLoaded)
                return true;

            TextAsset markerTextAsset = Resources.Load<TextAsset>(PersistentMarker.kFileName);
            _isMarkerLoaded = markerTextAsset != null;
            if (markerTextAsset != null) {
                Resources.UnloadAsset(markerTextAsset);
            }

            return _isMarkerLoaded;
        }
    }
}
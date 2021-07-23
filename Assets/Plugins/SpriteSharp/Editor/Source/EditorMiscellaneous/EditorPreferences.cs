using UnityEditor;

namespace LostPolygon.SpriteSharp.Internal {
    /// <summary>
    /// Contains and persists preferences that are shared between projects.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorPreferences {
        private const string kKeyPrefix = "LostPolygon." + SpriteSharp.Internal.Constants.kAssetNameShort + ".";
        private const string kLastSelectedPlatformIndexKey = kKeyPrefix + "LastSelectedPlatformIndex";

        private static int _lastSelectedPlatformIndex;

        public static int LastSelectedPlatformIndex {
            get {
                return _lastSelectedPlatformIndex;
            }
            set {
                if (_lastSelectedPlatformIndex == value)
                    return;

                _lastSelectedPlatformIndex = value;
                EditorPrefs.SetInt(kLastSelectedPlatformIndexKey, _lastSelectedPlatformIndex);
            }
        }

        static EditorPreferences() {
            _lastSelectedPlatformIndex = EditorPrefs.GetInt(kLastSelectedPlatformIndexKey, -1);
        }
    }
}

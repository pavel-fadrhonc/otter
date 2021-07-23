using UnityEditor;
using LostPolygon.SpriteSharp.Utility;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    public static class SpriteTightMeshSettingsExtensions {
        public static BuildTargetGroup GetBestPlatform(
            this SpriteTightMeshSettings spriteTightMeshSettings, BuildTargetGroup desiredBuildTargetGroup
            ) {
            if (desiredBuildTargetGroup == BuildPlatformsUtility.GetDefaultBuildTargetGroup()) {
                return desiredBuildTargetGroup;
            }

            SinglePlatformSpriteTightMeshSettings settings;
            if (spriteTightMeshSettings.PerPlatformTightMeshSettings.TryGetValue(desiredBuildTargetGroup, out settings)) {
                if (settings.IsOverriding)
                    return desiredBuildTargetGroup;
            }

            return BuildPlatformsUtility.GetDefaultBuildTargetGroup();
        }

        public static SinglePlatformSpriteTightMeshSettings GetBestSingleSpriteTightMeshSettingsForPlatform(
            this SpriteTightMeshSettings spriteTightMeshSettings, BuildTargetGroup desiredBuildTargetGroup
            ) {
            return spriteTightMeshSettings[GetBestPlatform(spriteTightMeshSettings, desiredBuildTargetGroup)];
        }
    }
}
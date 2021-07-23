using System.IO;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Helper utilities for working with sprites.
    /// </summary>
    internal static class EditorSpriteUtility {
        public static bool IsSpritePackerAtlasCacheEmpty() {
            string[] directories = GetSpritePackerAtlasCacheDirectories();

            return directories == null || directories.Length == 0;
        }

        public static void ClearSpritePackerAtlasCache() {
            string[] directories = GetSpritePackerAtlasCacheDirectories();
            if (directories == null)
                return;

            foreach (string directory in directories) {
                Directory.Delete(directory, true);
            }
        }

        private static string[] GetSpritePackerAtlasCacheDirectories() {
            string atlasCachePath = GetAtlasCachePath();

            if (!Directory.Exists(atlasCachePath))
                return null;

            string[] directories = Directory.GetDirectories(atlasCachePath);
            return directories;
        }

        private static string GetAtlasCachePath() {
            string atlasCachePath = Path.Combine("Library", "AtlasCache");
            atlasCachePath = Path.GetFullPath(atlasCachePath);
            return atlasCachePath;
        }
    }
}
using System;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Utilities for working with file paths.
    /// </summary>
    internal static class FilePathUtility {
        public static string MakeRelativePath(string path, string referencePath) {
            Uri fileUri = new Uri(path);
            Uri referenceUri = new Uri(referencePath);
            return FixSlashes(Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()));
        }

        public static string FixSlashes(string path) {
            return path.Replace('\\', '/');
        }
    }
}
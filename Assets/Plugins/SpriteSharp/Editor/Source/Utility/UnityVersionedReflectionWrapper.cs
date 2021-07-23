using System;
using System.Reflection;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Utilities for handling Unity API version differences.
    /// </summary>
    internal static class UnityVersionedReflectionWrapper {
        internal static class TextureImporter {
            private static readonly Type kType = typeof(UnityEditor.TextureImporter);
            private static readonly PropertyInfo _alphaSourceProperty;
            private static readonly PropertyInfo _grayscaleToAlphaProperty;

            static TextureImporter() {
                _alphaSourceProperty = kType.GetProperty("alphaSource");
                _grayscaleToAlphaProperty = kType.GetProperty("grayscaleToAlpha");
            }

            public static TextureImporterAlphaSource GetAlphaSource(UnityEditor.TextureImporter textureImporter) {
                if (_alphaSourceProperty != null)
                    return (TextureImporterAlphaSource) _alphaSourceProperty.GetValue(textureImporter, null);

                bool grayscaleToAlpha = (bool) _grayscaleToAlphaProperty.GetValue(textureImporter, null);
                return grayscaleToAlpha ? TextureImporterAlphaSource.FromGrayScale : TextureImporterAlphaSource.FromInput;
            }

            public enum TextureImporterAlphaSource {
                None,
                FromInput,
                FromGrayScale,
            }
        }
    }
}

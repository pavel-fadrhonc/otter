using System;
using System.Reflection;
using LostPolygon.SpriteSharp.Processing;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// <see cref="TextureImporter"/> helper extensions.
    /// </summary>
    public static class EditorTextureImporterExtensions {
        private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
        private static readonly GetWidthAndHeight _getWidthAndHeightDelegate;

#if !UNITY_5_5_OR_NEWER
        private static readonly bool kIsTextureImporterTypeAdvancedDeprecated;
#endif

        static EditorTextureImporterExtensions() {
            MethodInfo method =
                typeof(TextureImporter)
                    .GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            _getWidthAndHeightDelegate =
                (GetWidthAndHeight) Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method);

#if !UNITY_5_5_OR_NEWER
            // Check if Advanced import type is obsolete (Unity 5.5)
            kIsTextureImporterTypeAdvancedDeprecated = false;
            if (!Enum.IsDefined(typeof(TextureImporterType), TextureImporterType.Advanced)) {
                kIsTextureImporterTypeAdvancedDeprecated = true;
                return;
            }

            FieldInfo fieldInfo = typeof(TextureImporterType).GetField(TextureImporterType.Advanced.ToString());
            if (fieldInfo == null) {
                kIsTextureImporterTypeAdvancedDeprecated = true;
                return;
            }

            if (fieldInfo.IsDefined(typeof(ObsoleteAttribute), true)) {
                kIsTextureImporterTypeAdvancedDeprecated = true;
            }
#endif
        }

        public static bool DoesTextureHaveAlpha(this TextureImporter textureImporter) {
            return
                textureImporter.DoesSourceTextureHaveAlpha() ||
                UnityVersionedReflectionWrapper.TextureImporter.GetAlphaSource(textureImporter) !=
                UnityVersionedReflectionWrapper.TextureImporter.TextureImporterAlphaSource.None ||
                textureImporter.alphaIsTransparency;
        }

        public static IntVector2 GetOriginalTextureSize(this TextureImporter textureImporter) {
            int width = -1;
            int height = -1;
            _getWidthAndHeightDelegate(textureImporter, ref width, ref height);

            return new IntVector2(width, height);
        }

        public static bool IsTextureImporterInSpriteMode(this TextureImporter textureImporter) {
            if (textureImporter.textureType == TextureImporterType.Sprite)
                return true;

#if !UNITY_5_5_OR_NEWER
            if (!kIsTextureImporterTypeAdvancedDeprecated &&
                textureImporter.textureType == TextureImporterType.Advanced &&
                textureImporter.spriteImportMode != SpriteImportMode.None)
                return true;
#endif

            return false;
        }

        public static bool IsTextureImporterWithEditableSpriteMode(this TextureImporter textureImporter) {
            if (textureImporter == null)
                return false;

            SpriteImportMode polygonSpriteImportMode = (SpriteImportMode) 3;
            return
                (textureImporter.textureType == TextureImporterType.Sprite
#if !UNITY_5_5_OR_NEWER
                    || (!kIsTextureImporterTypeAdvancedDeprecated && textureImporter.textureType == TextureImporterType.Advanced)
#endif
                ) &&
                textureImporter.spriteImportMode != polygonSpriteImportMode;
        }
    }
}
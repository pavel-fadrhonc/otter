using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// <see cref="Sprite"/> helper extensions.
    /// </summary>
    public static class EditorSpriteExtensions {
        public static bool IsTextureTightMesh(this Sprite sprite) {
            if (sprite.packed)
                return
                    sprite.packingMode == SpritePackingMode.Tight ||
                    sprite.texture.IsTightSpriteMesh();

            return sprite.texture.IsTightSpriteMesh();
        }

        public static bool IsSpriteTextureInSpriteImportMode(this Sprite sprite) {
            if (sprite == null)
                return false;

            Texture2D texture = sprite.texture;
            if (texture == null)
                return false;

            TextureImporter textureImporter = texture.GetTextureImporter();
            if (textureImporter.IsTextureImporterInSpriteMode())
                return true;

            return false;
        }

        public static bool IsMatchingSpriteProperties(this Sprite a, Sprite b) {
            float aRatio = a.rect.height == 0f ? 0f : a.rect.width / a.rect.height;
            float bRatio = b.rect.height == 0f ? 0f : b.rect.width / b.rect.height;
            Vector2 normalizedAPivot = a.pivot;
            normalizedAPivot.x /= a.texture.width;
            normalizedAPivot.y /= a.texture.height;
            Vector2 normalizedBPivot = b.pivot;
            normalizedBPivot.x /= b.texture.width;
            normalizedBPivot.y /= b.texture.height;

            return
                a.border == b.border &&
                normalizedAPivot == normalizedBPivot &&
                Mathf.Abs(aRatio - bRatio) < Vector3.kEpsilon &&
                a.pixelsPerUnit == a.pixelsPerUnit;
        }
    }
}
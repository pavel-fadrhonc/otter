using UnityEngine;

namespace LostPolygon.SpriteSharp.Serialization.Internal {
    internal static class SpriteLazyReferenceExtensions {
        public static SpriteLazyReference ToSpriteLazyReference(this Sprite sprite) {
            return SpriteLazyReference.FromSprite(sprite);
        }

        public static Sprite GetSpriteInstance(this SpriteLazyReference spriteLazyReference) {
            if (spriteLazyReference == null)
                return null;

            return spriteLazyReference.Instance;
        }
    }
}
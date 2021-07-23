using System;
using UnityEngine;
using LostPolygon.SpriteSharp.Serialization.Internal;

namespace LostPolygon.SpriteSharp.Serialization {
    /// <summary>
    /// <see cref=" UnityEngineObjectLazyReference{T}"/>
    /// </summary>
    [Serializable]
    public class SpriteLazyReference : UnityEngineObjectLazyReference<Sprite> {
        public SpriteLazyReference(string guid, int localIdentifier, int instanceId = 0)
            : base(guid, localIdentifier, instanceId) {
        }

        public static SpriteLazyReference FromSprite(Sprite sprite) {
            return UnitySerializationUtility
                .CreateLazyReference(
                    sprite,
                    (guid, localIdentifier, instanceId) => new SpriteLazyReference(guid, localIdentifier, instanceId));
        }

        public static SpriteLazyReference DeepCopy(SpriteLazyReference source) {
            if (source == null)
                return null;

            SpriteLazyReference spriteLazyReference = new SpriteLazyReference(source.Guid, source.LocalIdentifier);
            return spriteLazyReference;
        }
    }
}

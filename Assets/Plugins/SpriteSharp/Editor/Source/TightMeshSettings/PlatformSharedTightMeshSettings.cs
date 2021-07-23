using System;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.fastJSON;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class PlatformSharedTightMeshSettings : IJSONSerializationCallbackReceiver, IEquatable<PlatformSharedTightMeshSettings> {
        [SerializeField]
        private SpriteLazyReference _alphaSprite;

        public SpriteLazyReference AlphaSprite {
            get { return _alphaSprite; }
            set { _alphaSprite = value; }
        }

        public void CopyTo(PlatformSharedTightMeshSettings destination) {
            destination._alphaSprite = SpriteLazyReference.DeepCopy(_alphaSprite);
        }

        #region Equality

        public bool Equals(PlatformSharedTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_alphaSprite, other._alphaSprite);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlatformSharedTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            return (_alphaSprite != null ? _alphaSprite.GetHashCode() : 0);
        }

        #endregion

        #region IJSONSerializationCallbackReceiver

        public void OnBeforeSerialize() {
            if (_alphaSprite.IsNull(false)) {
                _alphaSprite = null;
            } else {
                if (String.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(_alphaSprite.Guid))) {
                    _alphaSprite = null;
                }
            }
        }

        public void OnAfterDeserialize() {
        }

        #endregion
    }
}
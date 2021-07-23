using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class SharedTightMeshSettings : IEquatable<SharedTightMeshSettings> {
        [SerializeField]
        [Range(Vector3.kEpsilon, 1f)]
        private float _detail = 0.3f;

        [SerializeField]
        [Range(0, 254)]
        private byte _alphaTolerance = 10;

        [SerializeField]
        private SpriteAlphaSourceChannel _alphaSourceChannel = SpriteAlphaSourceChannel.Alpha;

        public float Detail {
            get { return _detail; }
            set {
                if (value < Vector3.kEpsilon)
                    throw new ArgumentOutOfRangeException("value");

                if (value > 1f)
                    throw new ArgumentOutOfRangeException("value");

                _detail = value;
            }
        }

        public byte AlphaTolerance {
            get { return _alphaTolerance; }
            set {
                if (value > 254)
                    throw new ArgumentOutOfRangeException("value");

                _alphaTolerance = value;
            }
        }

        public SpriteAlphaSourceChannel AlphaSourceChannel {
            get { return _alphaSourceChannel; }
            set { _alphaSourceChannel = value; }
        }

        public void CopyTo(SharedTightMeshSettings destination) {
            destination._alphaTolerance = _alphaTolerance;
            destination._detail = _detail;
            destination._alphaSourceChannel = _alphaSourceChannel;
        }

        #region Equality

        public bool Equals(SharedTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _detail.Equals(other._detail) && _alphaTolerance == other._alphaTolerance && _alphaSourceChannel == other._alphaSourceChannel;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SharedTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = _detail.GetHashCode();
                hashCode = (hashCode * 397) ^ _alphaTolerance.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _alphaSourceChannel;
                return hashCode;
            }
        }

        #endregion
    }
}

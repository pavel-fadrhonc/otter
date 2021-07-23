#if !SS_ADVANCED_METHODS_DISABLED

using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class AlphaSeparationTightMeshSettings : IEquatable<AlphaSeparationTightMeshSettings> {
        [SerializeField]
        [Range(0, 30)]
        private byte _opaqueVertexMergeDistance = 2;

        [SerializeField]
        private byte _opaqueNegativeExtrude = 3;

        [SerializeField]
        private bool _reduceAlphaBleed = false;

        [SerializeField]
        [Range(0, 254)]
        private byte _opaqueAlphaTolerance = 254;

        public byte OpaqueVertexMergeDistance {
            get { return _opaqueVertexMergeDistance; }
            set { _opaqueVertexMergeDistance = value; }
        }

        public byte OpaqueNegativeExtrude {
            get {
                return _opaqueNegativeExtrude;
            }
            set {
                if (value > 20)
                    throw new ArgumentOutOfRangeException("value");

                _opaqueNegativeExtrude = value;
            }
        }

        public bool ReduceAlphaBleed {
            get { return _reduceAlphaBleed; }
            set { _reduceAlphaBleed = value; }
        }

        public byte OpaqueAlphaTolerance {
            get { return _opaqueAlphaTolerance; }
            set {
                if (value > 254)
                    throw new ArgumentOutOfRangeException("value");

                _opaqueAlphaTolerance = value;
            }
        }

        public void CopyTo(AlphaSeparationTightMeshSettings destination) {
            destination._opaqueVertexMergeDistance = _opaqueVertexMergeDistance;
            destination._opaqueNegativeExtrude = _opaqueNegativeExtrude;
            destination._reduceAlphaBleed = _reduceAlphaBleed;
            destination._opaqueAlphaTolerance = _opaqueAlphaTolerance;
        }

        #region Equality

        public bool Equals(AlphaSeparationTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                _opaqueVertexMergeDistance == other._opaqueVertexMergeDistance &&
                _opaqueNegativeExtrude == other._opaqueNegativeExtrude &&
                _reduceAlphaBleed == other._reduceAlphaBleed &&
                _opaqueAlphaTolerance == other._opaqueAlphaTolerance;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AlphaSeparationTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = _opaqueVertexMergeDistance.GetHashCode();
                hashCode = (hashCode * 397) ^ _opaqueNegativeExtrude.GetHashCode();
                hashCode = (hashCode * 397) ^ _reduceAlphaBleed.GetHashCode();
                hashCode = (hashCode * 397) ^ _opaqueAlphaTolerance.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}

#endif // !SS_ADVANCED_METHODS_DISABLED
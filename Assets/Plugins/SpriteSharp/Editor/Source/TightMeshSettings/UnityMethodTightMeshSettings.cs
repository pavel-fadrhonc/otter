#if !SS_ADVANCED_METHODS_DISABLED

using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class UnityMethodTightMeshSettings : IEquatable<UnityMethodTightMeshSettings> {
        [SerializeField]
        [Range(0, 30)]
        private byte _vertexMergeDistance = 3;

        [SerializeField]
        private bool _detectHoles = true;

        public byte VertexMergeDistance {
            get { return _vertexMergeDistance; }
            set { _vertexMergeDistance = value; }
        }

        public bool DetectHoles {
            get { return _detectHoles; }
            set { _detectHoles = value; }
        }

        public void CopyTo(UnityMethodTightMeshSettings destination) {
            destination._vertexMergeDistance = _vertexMergeDistance;
            destination._detectHoles = _detectHoles;
        }

        #region Equality

        public bool Equals(UnityMethodTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _vertexMergeDistance == other._vertexMergeDistance && _detectHoles == other._detectHoles;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UnityMethodTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_vertexMergeDistance.GetHashCode() * 397) ^ _detectHoles.GetHashCode();
            }
        }

        #endregion
    }
}

#endif // !SS_ADVANCED_METHODS_DISABLED
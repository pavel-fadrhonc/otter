#if !SS_ADVANCED_METHODS_DISABLED

using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class PreciseTightMeshSettings : IEquatable<PreciseTightMeshSettings> {
        [SerializeField]
        private byte _edgeInflation = 2;

        public byte EdgeInflation {
            get { return _edgeInflation; }
            set { _edgeInflation = value; }
        }

        public void CopyTo(PreciseTightMeshSettings destination) {
            destination._edgeInflation = _edgeInflation;
        }

        #region Equality

        public bool Equals(PreciseTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _edgeInflation == other._edgeInflation;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PreciseTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            return _edgeInflation.GetHashCode();
        }

        #endregion
    }
}

#endif // !SS_ADVANCED_METHODS_DISABLED
using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class RectGridTightMeshSettings : IEquatable<RectGridTightMeshSettings> {
        [SerializeField]
        private int _xSubdivisions = 5;

        [SerializeField]
        private int _ySubdivisions = 5;

        [SerializeField]
        private bool _cullByBoundingBox = true;

        [SerializeField]
        private bool _removeEmptyCells = true;

        [SerializeField]
        private byte _scaleAroundCenter;

        public int XSubdivisions {
            get { return _xSubdivisions; }
            set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");

                _xSubdivisions = value;
            }
        }

        public int YSubdivisions {
            get { return _ySubdivisions; }
            set {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value");

                _ySubdivisions = value;
            }
        }

        public bool CullByBoundingBox {
            get { return _cullByBoundingBox; }
            set { _cullByBoundingBox = value; }
        }

        public bool RemoveEmptyCells {
            get { return _removeEmptyCells; }
            set { _removeEmptyCells = value; }
        }

        public byte ScaleAroundCenter {
            get { return _scaleAroundCenter; }
            set { _scaleAroundCenter = value; }
        }

        public void CopyTo(RectGridTightMeshSettings destination) {
            destination._xSubdivisions = _xSubdivisions;
            destination._ySubdivisions = _ySubdivisions;
            destination._cullByBoundingBox = _cullByBoundingBox;
            destination._removeEmptyCells = _removeEmptyCells;
            destination._scaleAroundCenter = _scaleAroundCenter;
        }

        #region Equality

        public bool Equals(RectGridTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                _xSubdivisions == other._xSubdivisions &&
                _ySubdivisions == other._ySubdivisions &&
                _cullByBoundingBox == other._cullByBoundingBox &&
                _removeEmptyCells == other._removeEmptyCells &&
                _scaleAroundCenter == other._scaleAroundCenter;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RectGridTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = _xSubdivisions;
                hashCode = (hashCode * 397) ^ _ySubdivisions;
                hashCode = (hashCode * 397) ^ _cullByBoundingBox.GetHashCode();
                hashCode = (hashCode * 397) ^ _removeEmptyCells.GetHashCode();
                hashCode = (hashCode * 397) ^ _scaleAroundCenter.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}

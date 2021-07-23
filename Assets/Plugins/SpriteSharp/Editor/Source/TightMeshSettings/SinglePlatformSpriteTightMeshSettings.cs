using System;
using UnityEngine;
using LostPolygon.SpriteSharp.Processing;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class SinglePlatformSpriteTightMeshSettings : IEquatable<SinglePlatformSpriteTightMeshSettings> {
        [SerializeField]
        private bool _isOverriding;

        [SerializeField]
        private SpriteProcessingMethod _processingMethod =
#if !SS_ADVANCED_METHODS_DISABLED
            SpriteProcessingMethod.Normal;
#else
            SpriteProcessingMethod.RectGrid;
#endif // !SS_ADVANCED_METHODS_DISABLED

        [SerializeField]
        private SharedTightMeshSettings _sharedTightMeshSettings = new SharedTightMeshSettings();

#if !SS_ADVANCED_METHODS_DISABLED
        [SerializeField]
        private UnityMethodTightMeshSettings _unityMethodTightMeshSettings = new UnityMethodTightMeshSettings();

        [SerializeField]
        private AlphaSeparationTightMeshSettings _alphaSeparationTightMeshSettings = new AlphaSeparationTightMeshSettings();

        [SerializeField]
        private PreciseTightMeshSettings _preciseTightMeshSettings = new PreciseTightMeshSettings();
#endif // !SS_ADVANCED_METHODS_DISABLED

        [SerializeField]
        private RectGridTightMeshSettings _rectGridTightMeshSettings = new RectGridTightMeshSettings();

        public bool IsOverriding {
            get { return _isOverriding; }
            set { _isOverriding = value; }
        }

        public SpriteProcessingMethod ProcessingMethod {
            get { return _processingMethod; }
            set { _processingMethod = value; }
        }

        public SharedTightMeshSettings SharedTightMeshSettings {
            get { return _sharedTightMeshSettings; }
            set { _sharedTightMeshSettings = value; }
        }

#if !SS_ADVANCED_METHODS_DISABLED
        public UnityMethodTightMeshSettings UnityMethodTightMeshSettings {
            get { return _unityMethodTightMeshSettings; }
            set { _unityMethodTightMeshSettings = value; }
        }

        public AlphaSeparationTightMeshSettings AlphaSeparationTightMeshSettings {
            get { return _alphaSeparationTightMeshSettings; }
            set { _alphaSeparationTightMeshSettings = value; }
        }

        public PreciseTightMeshSettings PreciseTightMeshSettings {
            get { return _preciseTightMeshSettings; }
            set { _preciseTightMeshSettings = value; }
        }
#endif // !SS_ADVANCED_METHODS_DISABLED

        public RectGridTightMeshSettings RectGridTightMeshSettings {
            get { return _rectGridTightMeshSettings; }
            set { _rectGridTightMeshSettings = value; }
        }

        public void CopyTo(SinglePlatformSpriteTightMeshSettings destination) {
            destination._isOverriding = _isOverriding;
            destination._processingMethod = _processingMethod;

            _sharedTightMeshSettings.CopyTo(destination._sharedTightMeshSettings);
#if !SS_ADVANCED_METHODS_DISABLED
            _unityMethodTightMeshSettings.CopyTo(destination._unityMethodTightMeshSettings);
            _alphaSeparationTightMeshSettings.CopyTo(destination._alphaSeparationTightMeshSettings);
            _preciseTightMeshSettings.CopyTo(destination._preciseTightMeshSettings);
#endif // !SS_ADVANCED_METHODS_DISABLED
            _rectGridTightMeshSettings.CopyTo(destination._rectGridTightMeshSettings);
        }

        #region Equality

        public bool Equals(SinglePlatformSpriteTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                _isOverriding == other._isOverriding &&
                _processingMethod == other._processingMethod &&
                Equals(_sharedTightMeshSettings, other._sharedTightMeshSettings) &&
                Equals(_unityMethodTightMeshSettings, other._unityMethodTightMeshSettings) &&
                Equals(_alphaSeparationTightMeshSettings, other._alphaSeparationTightMeshSettings) &&
                Equals(_preciseTightMeshSettings, other._preciseTightMeshSettings) &&
                Equals(_rectGridTightMeshSettings, other._rectGridTightMeshSettings);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SinglePlatformSpriteTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = _isOverriding.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _processingMethod;
                hashCode = (hashCode * 397) ^ (_sharedTightMeshSettings != null ? _sharedTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_unityMethodTightMeshSettings != null ? _unityMethodTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_alphaSeparationTightMeshSettings != null ? _alphaSeparationTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_preciseTightMeshSettings != null ? _preciseTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_rectGridTightMeshSettings != null ? _rectGridTightMeshSettings.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
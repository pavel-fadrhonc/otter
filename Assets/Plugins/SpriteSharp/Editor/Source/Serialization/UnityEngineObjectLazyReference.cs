using System;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Serialization.Internal;

namespace LostPolygon.SpriteSharp.Serialization {
    /// <summary>
    /// Lazy reference to the on-disk UnityEngine.Object instance. GUID and LocalIdentifier are stored,
    /// the actual object is only loaded lazily when needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class UnityEngineObjectLazyReference<T> : IEquatable<UnityEngineObjectLazyReference<T>>
        where T : UnityEngine.Object
    {
        private readonly string _guid;
        private readonly int _localIdentifier;

        [SerializeField]
        private int _cachedInstanceId;

        public string Guid {
            get { return _guid; }
        }

        public int LocalIdentifier {
            get { return _localIdentifier; }
        }

        public T Instance {
            get {
                T instance;
                if (_cachedInstanceId == 0) {
                    instance = Deserialize();
                    _cachedInstanceId = instance == null ? 0 : instance.GetInstanceID();
                    return instance;
                }

                instance = EditorUtility.InstanceIDToObject(_cachedInstanceId) as T;
                if (instance == null) {
                    instance = Deserialize();
                    _cachedInstanceId = instance == null ? 0 : instance.GetInstanceID();
                }

                return instance;
            }
        }

        public UnityEngineObjectLazyReference(string guid, int localIdentifier, int instanceId = 0) {
            _guid = guid;
            _localIdentifier = localIdentifier;
            _cachedInstanceId = instanceId;
        }

        protected virtual T Deserialize() {
            return UnitySerializationUtility.DeserializeAssetDatabaseObject(_guid, _localIdentifier) as T;
        }

        #region Equality

        public bool Equals(UnityEngineObjectLazyReference<T> other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return String.Equals(_guid, other._guid) && _localIdentifier == other._localIdentifier;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UnityEngineObjectLazyReference<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_guid != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_guid) : 0) * 397) ^ _localIdentifier;
            }
        }

        public static bool operator ==(UnityEngineObjectLazyReference<T> left, UnityEngineObjectLazyReference<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(UnityEngineObjectLazyReference<T> left, UnityEngineObjectLazyReference<T> right) {
            return !Equals(left, right);
        }

        #endregion
    }
}

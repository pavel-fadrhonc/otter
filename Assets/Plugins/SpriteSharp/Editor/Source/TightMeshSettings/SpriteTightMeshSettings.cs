using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.TightMeshSettings {
    [Serializable]
    public class SpriteTightMeshSettings : IEquatable<SpriteTightMeshSettings> {
        [SerializeField]
        private SinglePlatformSpriteTightMeshSettings _defaultTightMeshSettings = new SinglePlatformSpriteTightMeshSettings();

        [SerializeField]
        private PlatformSharedTightMeshSettings _platformSharedTightMeshSettings = new PlatformSharedTightMeshSettings();

        [SerializeField]
        private BuildTargetGroupToSinglePlatformSpriteTightMeshSettingsDictionary _perPlatformTightMeshSettings =
            new BuildTargetGroupToSinglePlatformSpriteTightMeshSettingsDictionary();

        public PlatformSharedTightMeshSettings PlatformSharedTightMeshSettings {
            get { return _platformSharedTightMeshSettings; }
            set { _platformSharedTightMeshSettings = value; }
        }

        public BuildTargetGroupToSinglePlatformSpriteTightMeshSettingsDictionary PerPlatformTightMeshSettings {
            get { return _perPlatformTightMeshSettings; }
            set { _perPlatformTightMeshSettings = value; }
        }

        public SinglePlatformSpriteTightMeshSettings DefaultTightMeshSettings {
            get { return _defaultTightMeshSettings; }
            set { _defaultTightMeshSettings = value; }
        }

        public SinglePlatformSpriteTightMeshSettings this[BuildTargetGroup buildTargetGroup] {
            get {
                if (buildTargetGroup == BuildPlatformsUtility.GetDefaultBuildTargetGroup()) {
                    return _defaultTightMeshSettings;
                }

                SinglePlatformSpriteTightMeshSettings settings;
                if (!_perPlatformTightMeshSettings.TryGetValue(buildTargetGroup, out settings)) {
                    settings = new SinglePlatformSpriteTightMeshSettings();
                    _perPlatformTightMeshSettings.Add(buildTargetGroup, settings);
                }

                return settings;
            }
        }

        public IEnumerable<SpriteLazyReference> GetAllReferencedSprites() {
            if (!PlatformSharedTightMeshSettings.AlphaSprite.IsNull())
                yield return PlatformSharedTightMeshSettings.AlphaSprite;
        }

        public void CopyTo(SpriteTightMeshSettings destination) {
            _platformSharedTightMeshSettings.CopyTo(destination._platformSharedTightMeshSettings);
            _defaultTightMeshSettings.CopyTo(destination._defaultTightMeshSettings);

            destination._perPlatformTightMeshSettings.Clear();
            foreach (KeyValuePair<BuildTargetGroup, SinglePlatformSpriteTightMeshSettings> pair in _perPlatformTightMeshSettings) {
                SinglePlatformSpriteTightMeshSettings copy = new SinglePlatformSpriteTightMeshSettings();
                pair.Value.CopyTo(copy);
                destination._perPlatformTightMeshSettings.Add(pair.Key, copy);
            }
        }

        public SpriteTightMeshSettings DeepCopy() {
            SpriteTightMeshSettings clone = new SpriteTightMeshSettings();
            CopyTo(clone);

            return clone;
        }

        #region Equality

        public bool Equals(SpriteTightMeshSettings other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Equals(_defaultTightMeshSettings, other._defaultTightMeshSettings) &&
                Equals(_platformSharedTightMeshSettings, other._platformSharedTightMeshSettings) &&
                _perPlatformTightMeshSettings.Compare(other._perPlatformTightMeshSettings);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpriteTightMeshSettings) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = (_defaultTightMeshSettings != null ? _defaultTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_platformSharedTightMeshSettings != null ? _platformSharedTightMeshSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HashCodeUtility.CalculateItemsHashCode(_perPlatformTightMeshSettings);
                return hashCode;
            }
        }

        #endregion

        [Serializable]
        public class BuildTargetGroupToSinglePlatformSpriteTightMeshSettingsDictionary :
            SerializableEditorDictionary<BuildTargetGroup, SinglePlatformSpriteTightMeshSettings> {
            protected override bool AreValuesValidated {
                get { return true; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Database.Internal {
    /// <summary>
    /// The database container that gets de- and serialized to JSON.
    /// </summary>
    internal sealed class DatabaseContainer {
        public const int kVersion = 2;

        private int _version = kVersion;
        private SpritesToTightMeshSettingsDictionary _spriteMeshSettings = new SpritesToTightMeshSettingsDictionary();
        private SpriteToSpriteDictionary _alphaSpriteToOpaqueSpriteDictionary = new SpriteToSpriteDictionary();
        private ProductPreferences _preferences = new ProductPreferences();

        public int Version {
            get { return _version; }
            set { _version = value; }
        }

        public SpriteToSpriteDictionary AlphaSpriteToOpaqueSpriteDictionary {
            get { return _alphaSpriteToOpaqueSpriteDictionary; }
            set { _alphaSpriteToOpaqueSpriteDictionary = value; }
        }

        public SpritesToTightMeshSettingsDictionary SpriteMeshSettings {
            get { return _spriteMeshSettings; }
            set { _spriteMeshSettings = value; }
        }

        public ProductPreferences Preferences {
            get { return _preferences; }
            set { _preferences = value; }
        }

        public sealed class ProductPreferences {
            private bool _isDisabled;
            private bool _skipTextureSpriteExtrude;
            private bool _workaroundSpriteAtlasRepacking = true;
            private bool _reimportChangedSpritesOnDatabaseMismatch = true;
            private SpriteTightMeshSettings _defaultSettings = new SpriteTightMeshSettings();

            public bool IsDisabled {
                get { return _isDisabled; }
                set { _isDisabled = value; }
            }

            public bool SkipTextureSpriteExtrude {
                get { return _skipTextureSpriteExtrude; }
                set { _skipTextureSpriteExtrude = value; }
            }

            public bool WorkaroundSpriteAtlasRepacking {
                get {
                    return _workaroundSpriteAtlasRepacking;
                }
                set {
                    _workaroundSpriteAtlasRepacking = value;
                }
            }

            public bool ReimportChangedSpritesOnDatabaseMismatch {
                get { return _reimportChangedSpritesOnDatabaseMismatch; }
                set { _reimportChangedSpritesOnDatabaseMismatch = value; }
            }

            public SpriteTightMeshSettings DefaultSettings {
                get { return _defaultSettings; }
                set { _defaultSettings = value; }
            }
        }

        public sealed class SpritesToTightMeshSettingsDictionary : ValidatedSerializationDictionary<SpriteLazyReference, SpriteTightMeshSettings> {
            protected override bool AreValuesValidated {
                get { return true; }
            }

            protected override bool AdditionalSerializationValidationPredicate(KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> pair) {
                return !String.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(pair.Key.Guid));
            }

            protected override bool AdditionalDeserializationValidationPredicate(KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> pair) {
                return AdditionalSerializationValidationPredicate(pair);
            }
        }

        public sealed class SpriteToSpriteDictionary : ValidatedSerializationDictionary<SpriteLazyReference, SpriteLazyReference> {
            protected override bool AreValuesValidated {
                get { return true; }
            }

            protected override bool AdditionalSerializationValidationPredicate(KeyValuePair<SpriteLazyReference, SpriteLazyReference> pair) {
                return !String.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(pair.Key.Guid));
            }

            protected override bool AdditionalDeserializationValidationPredicate(KeyValuePair<SpriteLazyReference, SpriteLazyReference> pair) {
                return AdditionalSerializationValidationPredicate(pair);
            }
        }
    }
}
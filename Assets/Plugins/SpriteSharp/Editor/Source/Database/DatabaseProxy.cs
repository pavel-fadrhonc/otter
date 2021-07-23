using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LostPolygon.SpriteSharp.Database.Internal;
using LostPolygon.SpriteSharp.fastJSON;
using LostPolygon.SpriteSharp.Internal;
using LostPolygon.SpriteSharp.Json.Internal;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Database {
    /// <summary>
    /// Manages saving and loading the database, provides useful methods to retrieve data from it.
    /// </summary>
    public class DatabaseProxy : ScriptableObject {
        private static DatabaseProxy _instance;
        private readonly List<SpriteLazyReference> _spriteListCache = new List<SpriteLazyReference>();

#if !SS_ADVANCED_METHODS_DISABLED
        [SerializeField]
        private List<AlphaSpriteLink> _alphaSpriteLinkData = new List<AlphaSpriteLink>();
#endif

        [NonSerialized]
        private DatabaseContainer _container = new DatabaseContainer();

        [NonSerialized]
        private bool _isLoadedFromJson;

        [NonSerialized]
        private bool _isDatabaseInvalid;

        [NonSerialized]
        private bool _isDatabaseSavedBeforeUnload;

        /// <summary>
        /// Gets the instance of <see cref="DatabaseProxy"/>.
        /// Makes an attempt to recreate the database asset in case it is missing.
        /// </summary>
        public static DatabaseProxy Instance {
            get {
                if (_instance != null)
                    return _instance;

                if (!ReimportAllDetector.IsDatabaseAvailable())
                    throw new InvalidOperationException("Attempt to load " + typeof(DatabaseProxy) + " while AssetDatabase is not initialized");

                // Load at default path
                _instance = Resources.Load<DatabaseProxy>(DatabaseAssetsManager.kDatabaseProxyName);
                if (_instance != null)
                    return _instance;

                // If no database proxy is found at default path, try searching for it in the project
                // and create a new one at the default location if none were found.
                DatabaseProxy database;
                string[] databases = AssetDatabase.FindAssets("t:" + typeof(DatabaseProxy).Name);
                if (databases.Length > 0) {
                    string databasePath = AssetDatabase.GUIDToAssetPath(databases[0]);
                    database = AssetDatabase.LoadMainAssetAtPath(databasePath) as DatabaseProxy;
                    if (database == null) {
                        database = DatabaseAssetsManager.Instance.CreateDatabaseProxy();

                        // To avoid the dialog from appearing constantly
                        Selection.activeObject = null;
                        EditorUtility.FocusProjectWindow();
                    }
                } else {
                    //Debug.LogWarning(SpriteSharp.Internal.Constants.kAssetName + " database proxy could not be found, creating one in default location");
                    database = DatabaseAssetsManager.Instance.CreateDatabaseProxy();
                }

                _instance = database;
                return _instance;
            }
        }

        public IDictionary<SpriteLazyReference, SpriteTightMeshSettings> SpriteSettings {
            get { return _container.SpriteMeshSettings; }
        }

        public IDictionary<SpriteLazyReference, SpriteLazyReference> AlphaSpriteToOpaqueSprite {
            get { return _container.AlphaSpriteToOpaqueSpriteDictionary; }
        }

        public SpriteTightMeshSettings DefaultSpriteSettings {
            get { return _container.Preferences.DefaultSettings; }
        }

        public bool IsProcessingDisabled {
            get { return _container.Preferences.IsDisabled; }
            set { _container.Preferences.IsDisabled = value; }
        }

        public bool SkipTextureSpriteExtrude {
            get { return _container.Preferences.SkipTextureSpriteExtrude; }
            set { _container.Preferences.SkipTextureSpriteExtrude = value; }
        }

        public bool WorkaroundSpriteAtlasRepacking {
            get { return _container.Preferences.WorkaroundSpriteAtlasRepacking; }
            set { _container.Preferences.WorkaroundSpriteAtlasRepacking = value; }
        }

        public bool ReimportOnMismatch {
            get { return _container.Preferences.ReimportChangedSpritesOnDatabaseMismatch; }
            set { _container.Preferences.ReimportChangedSpritesOnDatabaseMismatch = value; }
        }

#if !SS_ADVANCED_METHODS_DISABLED
        internal List<AlphaSpriteLink> AlphaSpriteLinkData {
            get { return _alphaSpriteLinkData; }
        }
#endif

        internal bool IsLoadedFromJson {
            get { return _isLoadedFromJson; }
        }

        /// <summary>
        /// Returns "real" sprite that is an sub-asset of <paramref name="spriteTexture"/> based on
        /// the pre-processed sprite that is not an asset yet.
        /// </summary>
        /// <param name="sprite">
        /// The pre-processed sprite.
        /// </param>
        /// <param name="spriteTexture">
        /// The parent texture of the <paramref name="sprite"/>.
        /// </param>
        /// <returns>
        /// The <see cref="Sprite"/>.
        /// </returns>
        public Sprite GetMatchingSprite(Sprite sprite, Texture2D spriteTexture) {
            return GetMatchingSpriteInternal(sprite, spriteTexture, _container.SpriteMeshSettings);
        }

#if !SS_ADVANCED_METHODS_DISABLED
        public Sprite GetMatchingOpaqueSprite(Sprite sprite, Texture2D spriteTexture) {
            return GetMatchingSpriteInternal(sprite, spriteTexture, _container.AlphaSpriteToOpaqueSpriteDictionary);
        }
#endif

        /// <summary>
        /// Retrieves matching <see cref="SpriteTightMeshSettings"/> for the <paramref name="sprite"/>
        /// from the database.
        /// </summary>
        /// <param name="sprite">
        /// The sprite.
        /// </param>
        /// <param name="createIfMissing">
        /// Whether to create a new record in the database if <paramref name="sprite"/> is not registered.
        /// </param>
        /// <returns>
        /// Matching <see cref="SpriteTightMeshSettings"/> for the <paramref name="sprite"/>.
        /// returns null, if <paramref name="createIfMissing"/> is false and no matching <see cref="SpriteTightMeshSettings"/>
        /// was found.
        /// </returns>
        public SpriteTightMeshSettings GetTightMeshSettings(Sprite sprite, bool createIfMissing = true) {
            SpriteLazyReference spriteLazyReference = sprite.ToSpriteLazyReference();
            SpriteTightMeshSettings spriteTightMeshSettings;
            if (!_container.SpriteMeshSettings.TryGetValue(spriteLazyReference, out spriteTightMeshSettings) && createIfMissing) {
                spriteTightMeshSettings = new SpriteTightMeshSettings();
                _container.SpriteMeshSettings.Add(spriteLazyReference, spriteTightMeshSettings);
            }

            return spriteTightMeshSettings;
        }

        public Sprite GetOpaqueSpriteOfAlphaSprite(Sprite alphaSprite) {
            SpriteLazyReference opaqueSprite;
            _container.AlphaSpriteToOpaqueSpriteDictionary.TryGetValue(alphaSprite.ToSpriteLazyReference(), out opaqueSprite);

            return opaqueSprite.GetSpriteInstance();
        }

        internal bool LoadFromFile(bool forceReload = false) {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return false;

#if SS_TRACE
            Stopwatch sw = Stopwatch.StartNew();
#endif

            if (_isLoadedFromJson && !forceReload || !ReimportAllDetector.IsDatabaseAvailable(true))
                return false;

            string databasePath = DatabaseAssetsManager.Instance.GetDatabasePath(false);
            string databaseJson = DatabaseAssetsManager.kEmptyJsonContents;
            if (!File.Exists(databasePath)) {
                DatabaseAssetsManager.Instance.CreateDatabase();
            } else {
                databaseJson = File.ReadAllText(databasePath);
            }

            try {
                _container = LoadFromFileRaw(databaseJson);
            } catch {
                _isDatabaseInvalid = true;
                throw;
            }

            _isLoadedFromJson = true;

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("DB JSON load took {0} ms", sw.ElapsedMilliseconds);
#endif

            return true;
        }

        internal static DatabaseContainer LoadFromFileRaw(string databaseJson) {
            fastJSONUnitySetup.RegisterUnityTypesSerialization();
            Dictionary<string, object> decodedJson = JSON.Parse(databaseJson) as Dictionary<string, object>;
            if (decodedJson == null)
                throw new InvalidOperationException("Unable to parse the " + SpriteSharp.Internal.Constants.kAssetName + " database JSON.");

            int serializedContainerVersion = decodedJson.ContainsKey("Version") ? Convert.ToInt32(decodedJson["Version"]) : -1;
            if (serializedContainerVersion == -1)
                throw new InvalidOperationException("SpriteSharp database is broken or invalid. Please repair or delete the database file.");

            if (serializedContainerVersion != DatabaseContainer.kVersion) {
                decodedJson = MigrateContainerVersion(serializedContainerVersion, decodedJson);
            }

            DatabaseContainer container = new DatabaseContainer();
            try {
                JSON.FillObject(container, decodedJson);
            } catch (Exception e) {
                Debug.LogErrorFormat("Exception when filling " + SpriteSharp.Internal.Constants.kAssetName + "Database with JSON data\n{0}", e);
                throw;
            }

            return container;
        }

        internal void SaveToFile() {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase)
                return;

            if (!ReimportAllDetector.IsDatabaseAvailable())
                return;

#if SS_TRACE
            Stopwatch sw = Stopwatch.StartNew();
#endif

            fastJSONUnitySetup.RegisterUnityTypesSerialization();
            string databasePath = DatabaseAssetsManager.Instance.GetDatabasePath(true);
            if (!File.Exists(databasePath)) {
                DatabaseAssetsManager.Instance.CreateDatabase();
            }

            if (!_isDatabaseInvalid) {
                string databaseJson;
                if (_container != null) {
                    databaseJson = JSON.ToNiceJSON(_container, JSON.Parameters);
                } else {
                    databaseJson = DatabaseAssetsManager.kEmptyJsonContents;
                }
                File.WriteAllText(databasePath, databaseJson);

                if (_container != null && _container.Preferences.ReimportChangedSpritesOnDatabaseMismatch) {
                    File.WriteAllText(DatabaseAssetsManager.Instance.GetDatabaseDiffCopyPath(), databaseJson);
                }
            }

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("DB JSON save took {0} ms", sw.ElapsedMilliseconds);
#endif
        }

        #region ScriptableObject messages

        private void OnEnable() {
            LoadFromFile();

            // OnDisable is not called when Editor is closed, so we have to resort to a hack.
            // This is safe - DomainUnload event seems to be called on the main thread.
            // From 2017.1, OnDisable() is called.
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
                SaveDatabaseOnUnload();
            };
        }

        private void OnDisable() {
            SaveDatabaseOnUnload();
        }

        private void OnDestroy() {
            SaveDatabaseOnUnload();
        }

        private void SaveDatabaseOnUnload() {
            if (_isDatabaseSavedBeforeUnload)
                return;

            try {
                SaveToFile();
            } catch (Exception e) {
                Debug.LogException(e);
            }

            _isDatabaseSavedBeforeUnload = true;
        }

        #endregion

        private static Dictionary<string, object> MigrateContainerVersion(int serializedContainerVersion, Dictionary<string, object> decodedJson) {
            if (serializedContainerVersion > DatabaseContainer.kVersion) {
                throw new InvalidOperationException(
                    "Serialized " + SpriteSharp.Internal.Constants.kAssetName + " database version is newer than " +
                    "of the current " + SpriteSharp.Internal.Constants.kAssetName + " version. Please update SpriteSharp.");
            }

            switch (serializedContainerVersion) {
                case 1:
                    decodedJson = new DatabaseVersion1To2Upgrader(decodedJson).Upgrade();
                    break;
                default:
                    throw new InvalidOperationException(
                        String.Format("Unknown " + SpriteSharp.Internal.Constants.kAssetName + " database version {0}, can't migrate", serializedContainerVersion)
                        );
            }

            return decodedJson;
        }

        private Sprite GetMatchingSpriteInternal<T>(Sprite sprite, Texture2D spriteTexture, IDictionary<SpriteLazyReference, T> dictionary) {
            string tempSpriteName = sprite.name;

            // Construct list of destroyed C++ wrappers that mimic a null object
            // and remove them from dictionary
            _spriteListCache.Clear();
            foreach (SpriteLazyReference key in dictionary.Keys) {
                if (!ReferenceEquals(key, null) && key.Equals(null)) {
                    _spriteListCache.Add(key);
                }
            }

            _spriteListCache.ForEach(invalidSprite => dictionary.Remove(invalidSprite));
            _spriteListCache.Clear();

            // The matching sprite is determined by checking
            // if the sprite texture and sprite name are the same.
            // This is hacky, but there doesn't seems to be any other way
            // to do this. However, it is safe enough since sprite names in one texture
            // can't be the same since Unity 5
            string spriteTextureGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteTexture));
            foreach (SpriteLazyReference key in dictionary.Keys) {
                if (key.Guid != spriteTextureGuid)
                    continue;

                Sprite keyInstance = key.Instance;
                if (keyInstance != null && keyInstance.texture == spriteTexture && keyInstance.name == tempSpriteName)
                    return keyInstance;
            }

            return null;
        }

        [DidReloadScripts]
        private static void CheckDatabase() {
            if (!ReimportAllDetector.IsDatabaseAvailable())
                return;

            DatabaseProxy database = Instance;
            if (database == null) {
                Debug.LogError(SpriteSharp.Internal.Constants.kAssetName + " database could not be loaded. Please re-import the package.");
            }
        }

#if !SS_ADVANCED_METHODS_DISABLED
        [Serializable]
        internal class AlphaSpriteLink {
            public string AlphaSpriteName;
            public Sprite OpaqueSprite;
            public Texture2D OpaqueSpriteTexture;
        }
#endif

        private abstract class DatabaseUpgrader {
            protected readonly Dictionary<string, object> _database;

            protected DatabaseUpgrader(Dictionary<string, object> database) {
                _database = database;
            }

            public abstract Dictionary<string, object> Upgrade();

            protected static bool TryGetValue<T>(Dictionary<string, object> database, string key, out T value) where T : class {
                value = default(T);
                object temp;
                if (database.TryGetValue(key, out temp)) {
                    value = temp as T;
                    return value != null;
                }

                return false;
            }
        }

        private class DatabaseVersion1To2Upgrader : DatabaseUpgrader {
            public DatabaseVersion1To2Upgrader(Dictionary<string, object> database) : base(database) {
            }

            public override Dictionary<string, object> Upgrade() {
                Dictionary<string, object> preferences;
                ICollection<object> spriteTightMeshSettingsList;

                _database["Version"] = (long) DatabaseContainer.kVersion;

                if (TryGetValue(_database, "SpriteMeshSettings", out spriteTightMeshSettingsList)) {
                    foreach (object item in spriteTightMeshSettingsList) {
                        Dictionary<string, object> spriteTightMeshSettingsListItem = item as Dictionary<string, object>;
                        if (spriteTightMeshSettingsListItem != null && spriteTightMeshSettingsListItem.ContainsKey("v")) {
                            UpgradeSpriteTightMeshSettings((Dictionary<string, object>) spriteTightMeshSettingsListItem["v"]);
                        }
                    }
                }

                if (TryGetValue(_database, "Preferences", out preferences)) {
                    Dictionary<string, object> spriteTightMeshSettings;
                    if (TryGetValue(preferences, "DefaultSettings", out spriteTightMeshSettings)) {
                        UpgradeSpriteTightMeshSettings(spriteTightMeshSettings);
                    }
                }

                return _database;
            }

            private void UpgradeSpriteTightMeshSettings(Dictionary<string, object> spriteTightMeshSettings) {
                ICollection<object> perPlatformTightMeshSettingsList;
                if (TryGetValue(spriteTightMeshSettings, "PerPlatformTightMeshSettings", out perPlatformTightMeshSettingsList)) {
                    foreach (object item in perPlatformTightMeshSettingsList) {
                        Dictionary<string, object> perPlatformTightMeshSettingsListItem = item as Dictionary<string, object>;
                        if (perPlatformTightMeshSettingsListItem != null && perPlatformTightMeshSettingsListItem.ContainsKey("v")) {
                            UpgradeSinglePlatformSpriteTightMeshSettings((Dictionary<string, object>) perPlatformTightMeshSettingsListItem["v"]);
                        }
                    }
                }

                Dictionary<string, object> defaultTightMeshSettings;
                if (TryGetValue(spriteTightMeshSettings, "DefaultTightMeshSettings", out defaultTightMeshSettings)) {
                    UpgradeSinglePlatformSpriteTightMeshSettings(defaultTightMeshSettings);
                }
            }

            private void UpgradeSinglePlatformSpriteTightMeshSettings(Dictionary<string, object> singlePlatformSpriteTightMeshSettings) {
                Dictionary<string, object> rectGridTightMeshSettings;
                if (TryGetValue(singlePlatformSpriteTightMeshSettings, "RectGridTightMeshSettings", out rectGridTightMeshSettings)) {
                    if (rectGridTightMeshSettings.ContainsKey("EdgeInflation")) {
                        rectGridTightMeshSettings.Add("ScaleAroundCenter", rectGridTightMeshSettings["EdgeInflation"]);
                        rectGridTightMeshSettings.Remove("EdgeInflation");
                    }
                }
            }
        }
    }
}
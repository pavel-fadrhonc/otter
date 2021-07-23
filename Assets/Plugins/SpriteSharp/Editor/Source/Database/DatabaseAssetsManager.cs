using System;
using System.IO;
using LostPolygon.SpriteSharp.Gui.Internal;
using UnityEngine;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Database.Internal {
    /// <summary>
    /// Manages the database asset and the <see cref="DatabaseProxy"/> singleton instance.
    /// </summary>
    internal class DatabaseAssetsManager {
        public const string kDatabaseProxyName = SpriteSharp.Internal.Constants.kAssetNameShort + "DatabaseProxy";
        public static readonly string kEmptyJsonContents = String.Format("{{\"Version\" : {0}}}", DatabaseContainer.kVersion);

        private const string kDefaultEditorDirectoryPath = "Assets/" + SpriteSharp.Internal.Constants.kAssetDirectoryName + "/Editor";

        private const string kDatabaseProxyAssetName = kDatabaseProxyName + ".asset";
        private const string kDefaultDatabaseProxyDirectoryPath = kDefaultEditorDirectoryPath + "/" + SpriteSharpEditorResources.kEditorResourcesDirectoryName;
        private const string kDefaultDatabaseProxyPath = kDefaultDatabaseProxyDirectoryPath + "/" + kDatabaseProxyAssetName;

        private const string kDatabaseDirectoryName = "Database";
        private const string kDatabaseName = SpriteSharp.Internal.Constants.kAssetNameShort + "Database";
        private const string kDatabaseAssetName = kDatabaseName + ".json";
        private const string kDefaultDatabaseDirectoryPath = kDefaultEditorDirectoryPath + "/" + kDatabaseDirectoryName;

        private const string kDefaultDatabasePath = kDefaultDatabaseDirectoryPath + "/" + kDatabaseAssetName;

        private static DatabaseAssetsManager _instance;
        private string _lastDatabasePath;

        public static DatabaseAssetsManager Instance {
            get { return _instance ?? (_instance = new DatabaseAssetsManager()); }
        }

        public string GetDatabaseDiffCopyPath() {
            return Path.Combine("Library", "SpriteSharpDatabaseDiff.json");
        }

        public string GetDatabasePath(bool cached) {
            if (cached && _lastDatabasePath != null)
                return _lastDatabasePath;

            _lastDatabasePath = SpriteSharpEditorResources.PersistentDataPath + "/" + kDatabaseDirectoryName + "/" + kDatabaseAssetName;
            return _lastDatabasePath;
        }

        public DatabaseProxy CreateDatabaseProxy() {
            DatabaseProxy database = ScriptableObject.CreateInstance<DatabaseProxy>();
            try {
                if (!Directory.Exists(kDefaultDatabaseProxyDirectoryPath))
                    throw new DirectoryNotFoundException(kDefaultDatabaseProxyDirectoryPath);

                AssetDatabaseCreateAssetOverwrite(database, kDefaultDatabaseProxyPath);
            } catch (Exception e) {
                Debug.LogWarning(
                    "Creating " + SpriteSharp.Internal.Constants.kAssetName + " database proxy at '" +
                    kDefaultDatabaseProxyPath +
                    "' failed, attempting to find root.\n" +
                    e
                    );

                string databasePath = "";
                try {
                    databasePath = SpriteSharpEditorResources.PersistentEditorResourcesPath + "/" + kDatabaseProxyAssetName;
                    AssetDatabaseCreateAssetOverwrite(database, databasePath);
                } catch (Exception e2) {
                    Selection.activeObject = null;
                    UnityEngine.Object.DestroyImmediate(database);

                    throw new UnityException(
                        "Creating  " + SpriteSharp.Internal.Constants.kAssetName + " database proxy at '" +
                        databasePath +
                        "' failed, check if the directory exists.\n" +
                        e2,
                        e2
                        );
                }
            }

            return database;
        }

        public void CreateDatabase() {
            string databasePath;
            try {
                databasePath = GetDatabasePath(false);
                Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
                File.WriteAllText(databasePath, kEmptyJsonContents);
            } catch (Exception e) {
                Debug.LogWarning(
                    "Creating " + SpriteSharp.Internal.Constants.kAssetName + " database at '" +
                    kDefaultDatabasePath +
                    "' failed, check if the directory exists.\n" +
                    e
                );

                try {
                    databasePath = kDefaultDatabasePath;
                    Directory.CreateDirectory(Path.GetDirectoryName(databasePath));
                    File.WriteAllText(databasePath, kEmptyJsonContents);
                } catch (Exception e2) {
                    Selection.activeObject = null;

                    throw new Exception(
                        "Creating " + SpriteSharp.Internal.Constants.kAssetName + " database at '" +
                        kDefaultDatabasePath +
                        "' failed, attempting to find root.\n" +
                        e2,
                        e2
                    );
                }
            }

            Debug.Log(SpriteSharp.Internal.Constants.kAssetName + " database created successfully.");
        }

        private static void AssetDatabaseCreateAssetOverwrite(UnityEngine.Object asset, string path) {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
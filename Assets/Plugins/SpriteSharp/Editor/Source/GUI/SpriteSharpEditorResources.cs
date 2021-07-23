using System.IO;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Database.Internal;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Gui.Internal {
    /// <summary>
    /// External resources used in the editor.
    /// </summary>
    internal static class SpriteSharpEditorResources {
        public const string kEditorResourcesDirectoryName = "EditorResources";

        private static Texture2D _linkIcon;
        private static Texture2D _linkBreakIcon;
        private static Texture2D _diceIcon;
        private static Texture2D _meshIcon;
        private static Texture2D _spriteIcon;

        public static Texture2D LinkIcon {
            get { return LoadPersistentEditorResource("Images/spritesharp-link.png", ref _linkIcon); }
        }

        public static Texture2D LinkBreakIcon {
            get { return LoadPersistentEditorResource("Images/spritesharp-link-break.png", ref _linkBreakIcon); }
        }

        public static Texture2D DiceIcon {
            get { return LoadPersistentEditorResource("Images/spritesharp-dice.png", ref _diceIcon); }
        }

        public static Texture2D MeshIcon {
            get { return LoadPersistentEditorResource("Images/spritesharp-mesh.png", ref _meshIcon); }
        }

        public static Texture2D SpriteIcon {
            get { return LoadPersistentEditorResource("Images/spritesharp-sprite.png", ref _spriteIcon); }
        }

        public static string PersistentDataPath {
            get {
                if (!PersistentMarker.IsExists)
                    throw new FileNotFoundException(PersistentMarker.kFileName + " not found. Please re-import SpriteSharp.");

                string markerPath = PersistentMarker.Path;
                DirectoryInfo persistentDataDirectoryInfo = Directory.GetParent(markerPath).Parent;
                string relativePath = FilePathUtility.MakeRelativePath(persistentDataDirectoryInfo.FullName, Application.dataPath);
                return relativePath;
            }
        }

        public static string PersistentEditorResourcesPath {
            get {
                return PersistentDataPath + "/" + kEditorResourcesDirectoryName;
            }
        }

        private static T LoadPersistentEditorResource<T>(string path, ref T var) where T : Object {
            if (var != null)
                return var;

            var = LoadPersistentEditorResource<T>(path);
            return var;
        }

        private static T LoadPersistentEditorResource<T>(string path) where T : Object {
            string fullPath = PersistentEditorResourcesPath + "/" + path;
            return (T) AssetDatabase.LoadAssetAtPath(fullPath, typeof(T));
        }
    }
}
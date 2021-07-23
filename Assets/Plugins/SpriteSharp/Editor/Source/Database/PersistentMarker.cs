using System;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Database.Internal {
    internal static class PersistentMarker {
        public const string kFileName = SpriteSharp.Internal.Constants.kAssetNameShort + "PersistentMarker";

        public static bool IsExists {
            get {
                return Asset != null;
            }
        }

        public static string Path {
            get {
                TextAsset textAsset = Asset;
                if (textAsset == null)
                    throw new InvalidOperationException("Check IsExists first");

                return AssetDatabase.GetAssetPath(textAsset);
            }
        }

        private static TextAsset Asset {
            get {
                return Resources.Load<TextAsset>(kFileName);
            }
        }
    }
}
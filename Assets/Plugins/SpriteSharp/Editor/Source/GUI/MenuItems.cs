using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Experimental;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Internal {
    /// <summary>
    /// Additional Unity main menu items.
    /// </summary>
    internal static class MenuItems {
        private const string kMenuItemRoot = "Tools/Lost Polygon/" + Constants.kAssetName + "/";

        [MenuItem(kMenuItemRoot + "Reimport Affected Sprites", priority = 1)]
        private static void ReimportAffectedSprites() {
            bool result =
                EditorUtility.DisplayDialog(
                    "Reimport Affected Sprites",
                    "Reimport all sprites in the " + Constants.kAssetName + " database? " +
                    "This process might take significant time, depending on your project size.",
                    "Reimport",
                    "Cancel"
                    );

            if (!result)
                return;

            HashSet<Texture2D> affectedTextures = SpriteSharpUtility.GetAffectedTextures();
            if (affectedTextures.Count == 0) {
                Debug.Log("No affected sprites found.");
                return;
            }

            EditorTextureUtility.ReimportTextures(affectedTextures);
        }

        [MenuItem(kMenuItemRoot + "Remove Non-overriden Sprites From Database", priority = 2)]
        private static void ReimportNonOverridenSprites() {
            bool result =
                EditorUtility.DisplayDialog(
                    "Remove non-overriden sprites from database",
                    "This will remove all sprites that don't have any overrides set from " + Constants.kAssetName + " database. " +
                    "This operation can not be reverted.",
                    "Remove",
                    "Cancel"
                    );

            if (!result)
                return;

            int removeNonOverridenSprites = SpriteSharpUtility.RemoveNonOverridenSprites();
            Debug.LogFormat("Removed {0} sprites from SpriteSharp database", removeNonOverridenSprites);

            DatabaseProxy.Instance.SaveToFile();
        }

        [MenuItem(kMenuItemRoot + "Attach Alpha Sprites To Selection", priority = 3)]
        private static void AttachAlphaSprites() {
            GameObject[] gameObjects =
                Selection
                    .gameObjects
                    .Where(go => go.GetComponent<SpriteRenderer>() != null)
                    .ToArray();

            int processedSpriteCount = SpriteSharpUtility.AttachAlphaSprites(gameObjects, SpriteSharpUtility.GetOpaqueSpriteMaterial());
            if (processedSpriteCount == 0) {
                if (EditorUtility.DisplayDialog(
                    "Search all ",
                    "No objects in selection have sprites with no alpha sprites attached. Search and attach over whole hierarchy?",
                    "Search and Attach", "" +
                    "Cancel")) {
                    gameObjects =
                        Object.FindObjectsOfType<SpriteRenderer>()
                            .Select(spriteRenderer => spriteRenderer.gameObject)
                            .ToArray();

                    processedSpriteCount = SpriteSharpUtility.AttachAlphaSprites(gameObjects, SpriteSharpUtility.GetOpaqueSpriteMaterial());
                }
            }

            if (processedSpriteCount == 0) {
                Debug.Log("Found no objects that have sprites with no alpha sprites attached");
            } else {
                Debug.LogFormat("Attached {0} alpha sprites", processedSpriteCount);
            }
        }

        [MenuItem(kMenuItemRoot + "Clear Sprite Packer Atlas Cache", priority = 10)]
        private static void ClearSpritePackerAtlasCache() {
            bool result =
                EditorUtility.DisplayDialog(
                "Clear Sprite Packer Atlas Cache?",
                "This will clear the Sprite Packer atlas cache and restart Unity.\n\n" +
                "Use this only if you seeing strange behaviour when using Sprite Packer with SpriteSharp " +
                "(like sprite meshes reverting to original Unity-generated meshes).",
                "Clear and Restart",
                "Cancel"
                );

            if (!result)
                return;

            // Close all Sprite Editor windows to prevent crash
            Type spriteUtilityWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SpriteUtilityWindow");
            Object[] spriteUtilityWindows = Resources.FindObjectsOfTypeAll(spriteUtilityWindowType);
            foreach (EditorWindow spriteUtilityWindow in spriteUtilityWindows) {
                spriteUtilityWindow.Close();
            }

            // Clear cache and reopen project
            EditorSpriteUtility.ClearSpritePackerAtlasCache();
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }

        [MenuItem(kMenuItemRoot + "Clear Sprite Packer Atlas Cache", true)]
        private static bool ClearSpritePackerAtlasCacheValidate() {
            return !EditorSpriteUtility.IsSpritePackerAtlasCacheEmpty();
        }

        [MenuItem(kMenuItemRoot + "Sprite Dicing (Experimental)", priority = 10000)]
        private static void ShowSpriteDicingWindow() {
            EditorWindow.GetWindow<SpriteDicingWindow>(true);
        }
    }
}
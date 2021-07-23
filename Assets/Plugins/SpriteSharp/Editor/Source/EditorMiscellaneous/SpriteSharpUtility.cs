using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Processing.Internal;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;
using LostPolygon.SpriteSharp.TightMeshSettings;

namespace LostPolygon.SpriteSharp {
    /// <summary>
    /// Helper functions, mainly for better integration with user build scripts.
    /// </summary>
    public static class SpriteSharpUtility {
        /// <summary>
        /// Removes sprites without any override set from the database.
        /// </summary>
        /// <returns>Number of removed sprites.</returns>
        public static int RemoveNonOverridenSprites() {
            DatabaseProxy databaseProxy = DatabaseProxy.Instance;
            List<SpriteLazyReference> nonOverridenSprites = new List<SpriteLazyReference>();
            List<SpriteLazyReference> danglingAlphaSprites = new List<SpriteLazyReference>();
            foreach (KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> pair in databaseProxy.SpriteSettings) {
                if (pair.Value.DefaultTightMeshSettings.IsOverriding)
                    continue;

                bool isOverriding = false;
                foreach (KeyValuePair<BuildTargetGroup, SinglePlatformSpriteTightMeshSettings> platformTightMeshSetting in pair.Value.PerPlatformTightMeshSettings) {
                    if (platformTightMeshSetting.Value.IsOverriding) {
                        isOverriding = true;
                        break;
                    }
                }

                if (isOverriding)
                    continue;

                if (!pair.Value.PlatformSharedTightMeshSettings.AlphaSprite.IsNull(false)) {
                    danglingAlphaSprites.Add(pair.Value.PlatformSharedTightMeshSettings.AlphaSprite);
                }
                nonOverridenSprites.Add(pair.Key);
            }

            foreach (SpriteLazyReference nonOverridenSprite in nonOverridenSprites) {
                databaseProxy.SpriteSettings.Remove(nonOverridenSprite);
            }

            // Remove dangling alpha sprites
            foreach (SpriteLazyReference danglingAlphaSprite in danglingAlphaSprites) {
                databaseProxy.AlphaSpriteToOpaqueSprite.Remove(danglingAlphaSprite);
            }

            EditorUtility.SetDirty(databaseProxy);

            return nonOverridenSprites.Count;
        }

        public static HashSet<Texture2D> GetAffectedTextures() {
            DatabaseProxy database = DatabaseProxy.Instance;
            HashSet<Texture2D> affectedTextures = new HashSet<Texture2D>();

            foreach (KeyValuePair<SpriteLazyReference, SpriteLazyReference> pair in database.AlphaSpriteToOpaqueSprite) {
                affectedTextures.Add(GuidToTexture2D(pair.Key.Guid));
                affectedTextures.Add(GuidToTexture2D(pair.Value.Guid));
            }

            foreach (KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> pair in database.SpriteSettings) {
                affectedTextures.Add(GuidToTexture2D(pair.Key.Guid));
                affectedTextures.UnionWith(pair.Value.GetAllReferencedSprites().Select(sprite => GuidToTexture2D(sprite.Guid)));
            }

            return affectedTextures;
        }

        public static Mesh CreateMeshFromSprite(Sprite sprite) {
            Mesh mesh = new Mesh();
            mesh.name = sprite.name;

            Vector2[] spriteVertices = sprite.vertices;
            Vector3[] meshVertices = new Vector3[spriteVertices.Length];
            Vector3[] meshNormals = new Vector3[spriteVertices.Length];
            Vector3 backFacingNormal = new Vector3(0f, 0f, -1f);
            for (int i = 0; i < spriteVertices.Length; i++) {
                meshVertices[i] = spriteVertices[i];
                meshNormals[i] = backFacingNormal;
            }

            ushort[] spriteTriangles = sprite.triangles;
            int[] meshTriangles = new int[spriteTriangles.Length];
            for (int i = 0; i < spriteTriangles.Length; i++) {
                meshTriangles[i] = spriteTriangles[i];
            }

            mesh.vertices = meshVertices;
            mesh.normals = meshNormals;
            mesh.uv = sprite.uv;
            mesh.triangles = meshTriangles;

            mesh.RecalculateBounds();

            return mesh;
        }

        public static Material GetOpaqueSpriteMaterial(bool deepSearchIfNeeded = true) {
            // Get Sprites-Opaque material
            const string materialName = "Sprites-Opaque";

            Material opaqueSpriteMaterial = Resources.Load<Material>("Materials/" + materialName);
            if (deepSearchIfNeeded && opaqueSpriteMaterial == null) {
                IEnumerable<Material> opaqueSpriteMaterials =
                    AssetDatabase.FindAssets(materialName + " t:Material")
                        .Select<string, string>(AssetDatabase.GUIDToAssetPath)
                        .Select<string, Material>(s => (Material) AssetDatabase.LoadAssetAtPath(s, typeof(Material)));

                opaqueSpriteMaterial = opaqueSpriteMaterials.FirstOrDefault(material => material.name == materialName);
            }

            return opaqueSpriteMaterial;
        }

        public static int AttachAlphaSprites(GameObject[] gameObjects, Material opaqueSpriteMaterial) {
            int processedSpriteCount = 0;
            foreach (GameObject gameObject in gameObjects) {
                SpriteRenderer opaqueSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (opaqueSpriteRenderer.sprite == null)
                    continue;

                SpriteTightMeshSettings spriteTightMeshSettings = DatabaseProxy.Instance.GetTightMeshSettings(opaqueSpriteRenderer.sprite, false);
                if (spriteTightMeshSettings == null)
                    continue;

                Sprite alphaSprite = spriteTightMeshSettings.PlatformSharedTightMeshSettings.AlphaSprite.Instance;
                if (alphaSprite == null || alphaSprite == opaqueSpriteRenderer.sprite)
                    continue;

                // Search for direct children with alpha sprites
                bool childWithAlphaSpriteFound = false;
                foreach (Transform childTransform in gameObject.transform) {
                    SpriteRenderer childSpriteRenderer = childTransform.GetComponent<SpriteRenderer>();
                    if (childSpriteRenderer == null)
                        continue;

                    if (childSpriteRenderer.sprite ==  alphaSprite) {
                        childWithAlphaSpriteFound = true;
                        break;
                    }
                }

                if (childWithAlphaSpriteFound)
                    continue;

                processedSpriteCount++;

                Undo.RegisterFullObjectHierarchyUndo(gameObject, "Attach Alpha Sprites");

                if (opaqueSpriteRenderer.sharedMaterial.name == "Sprites-Default") {
                    opaqueSpriteRenderer.sharedMaterial = opaqueSpriteMaterial;
                }

                // Create alpha sprite object
                GameObject alphaGameObject = new GameObject(alphaSprite.name);
                alphaGameObject.layer = gameObject.layer;
                alphaGameObject.transform.SetParent(gameObject.transform);
                alphaGameObject.transform.localPosition = Vector3.zero;
                alphaGameObject.transform.localRotation = Quaternion.identity;
                alphaGameObject.transform.localScale = Vector3.one;
                SpriteRenderer alphaSpriteRenderer = alphaGameObject.AddComponent<SpriteRenderer>();
                alphaSpriteRenderer.sprite = alphaSprite;
                alphaSpriteRenderer.sortingLayerID = opaqueSpriteRenderer.sortingLayerID;
                alphaSpriteRenderer.sortingOrder = opaqueSpriteRenderer.sortingOrder;
                alphaSpriteRenderer.color = opaqueSpriteRenderer.color;
            }

            return processedSpriteCount;
        }

        public static class AssetPostprocessor {
            public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
                SpriteImportPostProcessorImpl.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            }

            public static void OnPostprocessSprites(UnityEditor.AssetPostprocessor postprocessor, Texture2D texture, Sprite[] sprites) {
                SpriteImportPostProcessorImpl.OnPostprocessSprites(postprocessor, texture, sprites);
            }
        }

        private static Texture2D GuidToTexture2D(string guid) {
            String assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (String.IsNullOrEmpty(assetPath))
                return null;

            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
        }
    }
}
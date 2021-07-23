using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;
using Tuple = LostPolygon.SpriteSharp.Utility.Internal.Tuple;

namespace LostPolygon.SpriteSharp.Processing.Internal {
    /// <remarks>
    /// There are two major different cases of texture import we must handle.
    /// 1. Normal situation, AssetDatabase is loaded, and a texture is RE-imported.
    /// In this case, we can load SpriteSharp database normally, and most job is done
    /// in OnPostprocessSprites().
    /// 2. A "Reimport All" is in progress.
    /// In this case, AssetDatabase is not fully initialized, so we can't use it,
    /// and we can't load the SpriteSharp database.
    ///
    /// So instead, the process is deferred. OnPostprocessSprites() stores paths to
    /// textures that are being reloaded. Each time OnPostprocessAllAssets() is triggered,
    /// it checks whether AssetDatabase is initialized, and when it finally is, reimports
    /// all the textures with affected sprites that need to be reloaded.
    /// This effectively doubles the import time for affected textures, but there seems to be
    /// no other workaround, since there is no way to load the SpriteSharp database until
    /// AssetDatabase is loaded as well.
    /// </remarks>
    internal class SpriteImportManager {
        private static SpriteImportManager _instance;
        private readonly HashSet<string> _texturesImportedInWrongState = new HashSet<string>();
        private readonly HashSet<string> _justReimportedTextures = new HashSet<string>();

        public static SpriteImportManager Instance {
            get { return _instance ?? (_instance = new SpriteImportManager()); }
        }

        public HashSet<string> TexturesImportedInWrongState {
            get {
                return _texturesImportedInWrongState;
            }
        }

        public void ExecuteDelayedImportOperations(bool forceCheck, bool allowUpdateSpriteLinks) {
            if (!ReimportAllDetector.IsDatabaseAvailable(forceCheck))
                return;

            DatabaseProxy database = DatabaseProxy.Instance;

            // Make sure database is loaded
            database.LoadFromFile();
            if (database.IsProcessingDisabled)
                return;

            BuildTargetGroup buildTargetGroup = BuildPlatformsUtility.ActiveBuildTargetGroup;

            if (_texturesImportedInWrongState.Count > 0) {
#if SS_TRACE
                Debug.LogFormat("Reimporting {0} textures that were imported in wrong state:\n{1}", _texturesImportedInWrongState.Count, String.Join("\n", _texturesImportedInWrongState.ToArray()));
#endif
                List<string> texturesToReimport;
                string[] databaseSpritesTexturePaths =
                    database.SpriteSettings.Keys
                        .Concat(
                            database
                            .SpriteSettings
                            .Where(pair => pair.Value.GetBestSingleSpriteTightMeshSettingsForPlatform(buildTargetGroup).IsOverriding)
                            .SelectMany(pair => pair.Value.GetAllReferencedSprites()))
                        .Where(sprite => sprite != null && !String.IsNullOrEmpty(sprite.Guid))
                        .Select(sprite => AssetDatabase.GUIDToAssetPath(sprite.Guid))
                        .Distinct()
                        .Where(path => !String.IsNullOrEmpty(path))
                        .ToArray();
                if (database.DefaultSpriteSettings.DefaultTightMeshSettings.IsOverriding) {
                    texturesToReimport = _texturesImportedInWrongState
                        .Select(texturePath => (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)))
                        .Select(texture => Tuple.New(texture, texture.GetTextureImporter()))
                        .Where(tuple => tuple.First != null && tuple.Second.IsTextureImporterInSpriteMode())
                        .Select(tuple => tuple.Second.assetPath)
                        .Concat(databaseSpritesTexturePaths.Intersect(_texturesImportedInWrongState))
                        .Distinct()
                        .ToList();
                } else {
                    texturesToReimport =
                        databaseSpritesTexturePaths
                            .Intersect(_texturesImportedInWrongState)
                            .Except(_justReimportedTextures)
                            .ToList();
                }

                // Add reimported textures to the temporary exclusion list
                // to avoid recursive reimport of the same assets
                _justReimportedTextures.Clear();
                texturesToReimport.ForEach(path => _justReimportedTextures.Add(path));

                if (texturesToReimport.Count > 0) {
                    _texturesImportedInWrongState.Clear();
#if SS_TRACE
                    Debug.LogFormat("Importing {0} textures...:\n{1}", texturesToReimport.Count, String.Join("\n", texturesToReimport.ToArray()));
#endif
                    AssetDatabase.StartAssetEditing();
#if SS_TRACE
                    Stopwatch sw = Stopwatch.StartNew();
#endif
                    foreach (string texturePath in texturesToReimport) {
                        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                        //ProcessTextureSprites(texturePath);
                    }
#if SS_TRACE
                    sw.Stop();
                    Debug.LogFormat("Reimporting textures imported in wrong state took {0} ms", sw.ElapsedMilliseconds);
#endif
                    AssetDatabase.StopAssetEditing();
                }
            }
#if !SS_ADVANCED_METHODS_DISABLED
            if (allowUpdateSpriteLinks) {
                // Link opaque and alpha sprites after import is done
                UpdateAlphaSpriteLinks();
            }
#endif // !SS_ADVANCED_METHODS_DISABLED
        }

        public void ProcessMovedAssets(string[] movedAssets) {
            // Reimport moved assets
            if (movedAssets.Length == 0)
                return;

            DatabaseProxy database = DatabaseProxy.Instance;
            if (database.IsProcessingDisabled)
                return;

            string[] movedTextureAssets =
                movedAssets
                    .Select(path => AssetImporter.GetAtPath(path))
                    .OfType<TextureImporter>()
                    .Select(importer => importer.assetPath)
                    .ToArray();

            if (movedTextureAssets.Length == 0)
                return;

            AssetDatabase.StartAssetEditing();
            foreach (string movedTextureAsset in movedTextureAssets) {
                AssetDatabase.ImportAsset(movedTextureAsset);
            }
            AssetDatabase.StopAssetEditing();
        }

        public static void ProcessSprites(Texture2D texture, Sprite[] sprites) {
            // Sort sprites to guarantee they will be processed in same order
            sprites = sprites.OrderBy(sprite => sprite.name).ToArray();
            foreach (Sprite sprite in sprites) {
                ProcessSingleSprite(texture, sprite);
            }
        }

        private static bool ProcessSingleSprite(Texture2D texture, Sprite sprite) {
            TextureImporter textureImporter = texture.GetTextureImporter();
            if (!textureImporter.IsTextureImporterWithEditableSpriteMode())
                return false;
            
            DatabaseProxy database = DatabaseProxy.Instance;
            BuildTargetGroup buildTargetGroup = BuildPlatformsUtility.ActiveBuildTargetGroup;
            SpriteProcessor.AlphaSpriteMode alphaSpriteMode =
#if !SS_ADVANCED_METHODS_DISABLED
                SpriteProcessor.AlphaSpriteMode.Normal;
#else
                SpriteProcessor.AlphaSpriteMode.RectGrid;
#endif // !SS_ADVANCED_METHODS_DISABLED

            Sprite existingSprite = database.GetMatchingSprite(sprite, texture);
#if !SS_ADVANCED_METHODS_DISABLED
            Sprite tempOpaqueSprite = database.GetMatchingOpaqueSprite(sprite, texture);
            Sprite opaqueSprite = null;
            if (tempOpaqueSprite != null) {
                SpriteLazyReference opaqueSpriteLazyReference;
                database.AlphaSpriteToOpaqueSprite.TryGetValue(tempOpaqueSprite.ToSpriteLazyReference(), out opaqueSpriteLazyReference);
                opaqueSprite = opaqueSpriteLazyReference.GetSpriteInstance();
            }

            if (existingSprite == null || tempOpaqueSprite != null) {
                if (opaqueSprite == null && !database.DefaultSpriteSettings.DefaultTightMeshSettings.IsOverriding)
                    return false;

                existingSprite = opaqueSprite;
                alphaSpriteMode = SpriteProcessor.AlphaSpriteMode.NonOpaque;
            }
#endif // !SS_ADVANCED_METHODS_DISABLED

            // In case a texture was just added and override is active in Preferences
            if (existingSprite == null) {
                existingSprite = sprite;
                alphaSpriteMode =
#if !SS_ADVANCED_METHODS_DISABLED
                    SpriteProcessor.AlphaSpriteMode.Normal;
#else
                    SpriteProcessor.AlphaSpriteMode.RectGrid;
#endif // !SS_ADVANCED_METHODS_DISABLED
            }

            SpriteTightMeshSettings spriteTightMeshSettings = database.GetTightMeshSettings(existingSprite, false);
            if ((spriteTightMeshSettings == null || !spriteTightMeshSettings.GetBestSingleSpriteTightMeshSettingsForPlatform(buildTargetGroup).IsOverriding)
#if !SS_ADVANCED_METHODS_DISABLED
                && alphaSpriteMode != SpriteProcessor.AlphaSpriteMode.NonOpaque
#endif // !SS_ADVANCED_METHODS_DISABLED
                ) {
                if (!database.DefaultSpriteSettings.DefaultTightMeshSettings.IsOverriding)
                    return false;

                spriteTightMeshSettings = database.DefaultSpriteSettings.DeepCopy();
                buildTargetGroup = BuildPlatformsUtility.GetDefaultBuildTargetGroup();
            }

            if (spriteTightMeshSettings == null)
                throw new InvalidOperationException(String.Format("Invalid state, spriteTightMeshSettings == null, alphaSpriteMode = {0}, existingSprite = {1}", alphaSpriteMode, existingSprite));

#if !SS_ADVANCED_METHODS_DISABLED
            if (alphaSpriteMode != SpriteProcessor.AlphaSpriteMode.NonOpaque && spriteTightMeshSettings.PlatformSharedTightMeshSettings.AlphaSprite.GetSpriteInstance() != null) {
                alphaSpriteMode = SpriteProcessor.AlphaSpriteMode.Opaque;
            }
#endif // !SS_ADVANCED_METHODS_DISABLED

            SpriteProcessor.AlphaSpriteMode spriteMode;
            buildTargetGroup = spriteTightMeshSettings.GetBestPlatform(buildTargetGroup);
            switch (spriteTightMeshSettings[buildTargetGroup].ProcessingMethod) {
#if !SS_ADVANCED_METHODS_DISABLED
                case SpriteProcessingMethod.Normal:
                    // Revert to default
                    spriteMode = SpriteProcessor.AlphaSpriteMode.Normal;
                    break;
                case SpriteProcessingMethod.AlphaSeparation:
                    if (opaqueSprite != null && !EditorSpriteExtensions.IsMatchingSpriteProperties(sprite, opaqueSprite)) {
                        Debug.LogErrorFormat(opaqueSprite, "Alpha sprite '{0}' and opaque sprite '{1}' have different properties. " + "Please re-link the alpha sprite.", sprite.name, opaqueSprite.name);

                        return false;
                    }

                    spriteMode = alphaSpriteMode;
                    break;
                case SpriteProcessingMethod.Precise:
                    spriteMode = SpriteProcessor.AlphaSpriteMode.Precise;
                    break;
#endif // !SS_ADVANCED_METHODS_DISABLED
                case SpriteProcessingMethod.RectGrid:
                    spriteMode = SpriteProcessor.AlphaSpriteMode.RectGrid;
                    break;
                default:
                    throw new Exception("Unknown SpriteTightMeshSettings.ProcessingMethod value");
            }

            SpriteProcessor.ProcessingOptions processingOptions = SpriteProcessor.ProcessingOptions.None;
            if (database.SkipTextureSpriteExtrude) {
                processingOptions |= SpriteProcessor.ProcessingOptions.SkipTextureSpriteExtrude;
            }

            SpriteProcessor spriteProcessor =
                new SpriteProcessor(
                    texture,
                    sprite,
                    spriteTightMeshSettings,
                    spriteMode,
                    buildTargetGroup,
                    processingOptions
                    );

#if SS_TRACE
            Stopwatch sw = Stopwatch.StartNew();
#endif
            spriteProcessor.ProcessSprite();

            if (DatabaseProxy.Instance.WorkaroundSpriteAtlasRepacking) {
                textureImporter.userData = spriteTightMeshSettings.GetHashCode().ToString();
                EditorUtility.SetDirty(textureImporter);
            }

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("Processing '{0}' took {1} ms", sprite.name, sw.ElapsedMilliseconds);
#endif

            return true;
        }

#if !SS_ADVANCED_METHODS_DISABLED
        private static void UpdateAlphaSpriteLinks() {
            DatabaseProxy database = DatabaseProxy.Instance;
            if (database == null)
                return;

            List<DatabaseProxy.AlphaSpriteLink> alphaSpriteLinks = database.AlphaSpriteLinkData;
            List<Texture2D> changedTextures = new List<Texture2D>();

            foreach (DatabaseProxy.AlphaSpriteLink alphaSpriteLink in alphaSpriteLinks) {
                if (alphaSpriteLink.OpaqueSprite == null)
                    continue;

                Sprite opaqueSprite = database.GetMatchingSprite(alphaSpriteLink.OpaqueSprite, alphaSpriteLink.OpaqueSpriteTexture);
                if (opaqueSprite == null)
                    continue;

                SpriteTightMeshSettings spriteTightMeshSettings = database.GetTightMeshSettings(opaqueSprite, false);
                if (spriteTightMeshSettings == null)
                    continue;

                TextureImporter textureImporter = alphaSpriteLink.OpaqueSprite.texture.GetTextureImporter();
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(textureImporter.assetPath).OfType<Sprite>().ToArray();
                foreach (Sprite sprite in sprites) {
                    if (sprite.name != alphaSpriteLink.AlphaSpriteName)
                        continue;

                    SpriteLazyReference spriteLazyReference = sprite.ToSpriteLazyReference();
                    spriteTightMeshSettings.PlatformSharedTightMeshSettings.AlphaSprite = spriteLazyReference;

                    database.AlphaSpriteToOpaqueSprite[spriteLazyReference] = opaqueSprite.ToSpriteLazyReference();

                    changedTextures.Add(sprite.texture);
                    changedTextures.Add(opaqueSprite.texture);
                    break;
                }
            }

            alphaSpriteLinks.Clear();
            EditorTextureUtility.ReimportDistinctTextures(changedTextures);
        }
#endif // !SS_ADVANCED_METHODS_DISABLED
    }
}
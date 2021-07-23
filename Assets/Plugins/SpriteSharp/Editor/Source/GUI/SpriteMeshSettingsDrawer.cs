using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Gui.Internal;
using LostPolygon.SpriteSharp.Processing;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.Serialization.Internal;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;
using Debug = UnityEngine.Debug;

namespace LostPolygon.SpriteSharp.Gui {
    /// <summary>
    /// Renders and handles rendering the mesh settings GUI.
    /// </summary>
    public class SpriteMeshSettingsDrawer {
        private readonly Func<bool> _spriteTightMeshSettingsAllowChangeCheck;
        private readonly Action _spriteTightMeshSettingsChanging;
        private readonly Action _spriteTightMeshSettingsChanged;
        private readonly Action _spriteTightMeshSettingsSetDirty;
        private readonly AnimBool _rectGridOptimizationFoldout = new AnimBool(true);
        private GenericMenu _spriteMeshExportMenu;

        public SpriteMeshSettingsDrawer(
            Func<bool> spriteTightMeshSettingsAllowChangeCheck,
            Action spriteTightMeshSettingsChanging,
            Action spriteTightMeshSettingsChanged,
            Action spriteTightMeshSettingsSetDirty,
            UnityAction repaintAction
            ) {
            _spriteTightMeshSettingsAllowChangeCheck = spriteTightMeshSettingsAllowChangeCheck;
            _spriteTightMeshSettingsChanging = spriteTightMeshSettingsChanging;
            _spriteTightMeshSettingsChanged = spriteTightMeshSettingsChanged;
            _spriteTightMeshSettingsSetDirty = spriteTightMeshSettingsSetDirty;

            if (repaintAction != null) {
                _rectGridOptimizationFoldout.valueChanged.AddListener(repaintAction);
            }

            GUITextContent.FillCache();
        }

        public bool DrawSettings(
            Sprite[] sprites,
            SpriteTightMeshSettings[] spriteTightMeshSettings,
            BuildTargetGroup buildTargetGroup,
            bool drawOverridingToggle
            ) {
            bool isChanged = false;

            // Processing method
            SpriteProcessingMethod processingMethod;
            bool isMixedProcessingMethod = InspectorUtility<SpriteTightMeshSettings>.IsMixedValues(
                spriteTightMeshSettings,
                settings => settings[buildTargetGroup].ProcessingMethod,
                out processingMethod
            );

            // "Override" toggle
            if (drawOverridingToggle) {
                bool isChangedIsOverriding =
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].IsOverriding,
                        (index, settings, value) => settings[buildTargetGroup].IsOverriding = value,
                        value => {
                            bool isDefaultOverriding = DatabaseProxy.Instance.DefaultSpriteSettings.DefaultTightMeshSettings.IsOverriding;
                            bool isDefaultPlatform = buildTargetGroup == BuildPlatformsUtility.GetDefaultBuildTargetGroup();
                            string label;

                            if (isDefaultOverriding) {
                                if (isDefaultPlatform) {
                                    label = "Manual";
                                } else {
                                    label = "Manual for " + BuildPlatformsUtility.GetBuildPlatform(buildTargetGroup).Name;
                                }
                            } else {
                                if (isDefaultPlatform) {
                                    label = "Override";
                                } else {
                                    label = "Override for " + BuildPlatformsUtility.GetBuildPlatform(buildTargetGroup).Name;
                                }
                            }

                            return EditorGUILayout.ToggleLeft(label, value);
                        }
                    );

                isChanged |= isChangedIsOverriding;

                bool isOverridingValue;
                bool isMixedOverridingValue = InspectorUtility<SpriteTightMeshSettings>.IsMixedValues(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].IsOverriding,
                    out isOverridingValue
                );

                EditorGUI.BeginDisabledGroup(!isMixedOverridingValue && !isOverridingValue);
                EditorGUI.indentLevel++;
            }
#if !SS_ADVANCED_METHODS_DISABLED
            SpriteProcessingMethod newProcessingMethod;
            bool isChangedProcessingMethod =
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].ProcessingMethod,
                    (index, settings, value) => settings[buildTargetGroup].ProcessingMethod = value,
                    value => (SpriteProcessingMethod) EditorGUILayout.EnumPopup("Processing Mode", value),
                    out newProcessingMethod
                );

            if (isChangedProcessingMethod) {
                if (isMixedProcessingMethod ||
                    (processingMethod == SpriteProcessingMethod.AlphaSeparation &&
                    newProcessingMethod != SpriteProcessingMethod.AlphaSeparation)) {
                    UnlinkAlphaSprites(sprites, spriteTightMeshSettings);
                }
            }

            isChanged |= isChangedProcessingMethod;

            GUILayout.Space(5f);
#else
            bool isMixedProcessingMethod = false;
            SpriteProcessingMethod processingMethod = SpriteProcessingMethod.RectGrid;
#endif // !SS_ADVANCED_METHODS_DISABLED

            // Draw GUI for current processing method
            if (!isMixedProcessingMethod) {
                switch (processingMethod) {
#if !SS_ADVANCED_METHODS_DISABLED
                    case SpriteProcessingMethod.Normal:
                        isChanged |= DrawMainSettings(spriteTightMeshSettings, buildTargetGroup);
                        break;
                    case SpriteProcessingMethod.AlphaSeparation:
                        isChanged |= DrawAlphaSeparationSettings(sprites, spriteTightMeshSettings, buildTargetGroup);
                        break;
                    case SpriteProcessingMethod.Precise:
                        isChanged |= DrawPreciseSettings(spriteTightMeshSettings, buildTargetGroup);
                        break;
#endif // !SS_ADVANCED_METHODS_DISABLED
                    case SpriteProcessingMethod.RectGrid:
                        isChanged |= DrawRectGridSettings(spriteTightMeshSettings, buildTargetGroup);
                        break;
                    default:
                        throw new Exception("Unknown SpriteTightMeshSettings.ProcessingMethod value");
                }
            }

            if (drawOverridingToggle) {
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            return isChanged;
        }

        public bool DrawRectGridSettings(SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged = false;

            // X Subdivisions
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].RectGridTightMeshSettings.XSubdivisions,
                    (index, settings, value) => settings[buildTargetGroup].RectGridTightMeshSettings.XSubdivisions = Mathf.Max(1, value),
                    value => EditorGUILayout.IntSlider("X Subdivisions", value, 1, 50)
                    );

            // Y Subdivisions
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].RectGridTightMeshSettings.YSubdivisions,
                    (index, settings, value) => settings[buildTargetGroup].RectGridTightMeshSettings.YSubdivisions = Mathf.Max(1, value),
                    value => EditorGUILayout.IntSlider("Y Subdivisions", value, 1, 50)
                    );

            _rectGridOptimizationFoldout.target = EditorGUILayout.Foldout(_rectGridOptimizationFoldout.target, "Optimization");
            if (EditorGUILayout.BeginFadeGroup(_rectGridOptimizationFoldout.faded)) {
                EditorGUI.indentLevel++;

                const float triggerWidth = 455f;
                const float minWidth = 175f;
                if (Screen.width < triggerWidth)
                    EditorGUIUtility.labelWidth = minWidth;

                // Remove empty cells
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].RectGridTightMeshSettings.RemoveEmptyCells,
                        (index, settings, value) => settings[buildTargetGroup].RectGridTightMeshSettings.RemoveEmptyCells = value,
                        value => EditorGUILayout.Toggle("Remove Empty Cells", value)
                    );

                bool isRemoveEmptyCells;
                InspectorUtility<SpriteTightMeshSettings>.IsMixedValues(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].RectGridTightMeshSettings.RemoveEmptyCells,
                    out isRemoveEmptyCells
                );

                // Cull by bounding box
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].RectGridTightMeshSettings.CullByBoundingBox,
                        (index, settings, value) => settings[buildTargetGroup].RectGridTightMeshSettings.CullByBoundingBox = value,
                        value => EditorGUILayout.Toggle("Cull by Bounding Box", value)
                    );

                bool isCullByBoundingBox;
                InspectorUtility<SpriteTightMeshSettings>.IsMixedValues(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].RectGridTightMeshSettings.CullByBoundingBox,
                    out isCullByBoundingBox
                );

                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!isCullByBoundingBox);
                {
                    // Edge inflation
                    isChanged |=
                        InspectorUtility<SpriteTightMeshSettings>.DrawField(
                            spriteTightMeshSettings,
                            settings => settings[buildTargetGroup].RectGridTightMeshSettings.ScaleAroundCenter,
                            (index, settings, value) => settings[buildTargetGroup].RectGridTightMeshSettings.ScaleAroundCenter = value,
                            value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.RectGridSettingsScaleAroundCenter, value, 0, 10)
                        );
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;

                EditorGUI.BeginDisabledGroup(!isCullByBoundingBox && !isRemoveEmptyCells);
                {
                    isChanged |= DrawAlphaSourceChannelProperty(spriteTightMeshSettings, buildTargetGroup);
                    isChanged |= DrawAlphaToleranceProperty(spriteTightMeshSettings, buildTargetGroup);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth = 0f;
            }
            EditorGUILayout.EndFadeGroup();

            return isChanged;
        }

#if !SS_ADVANCED_METHODS_DISABLED
        public bool DrawMainSettings(SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged = false;

            // Detail
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].SharedTightMeshSettings.Detail,
                    (index, settings, value) => settings[buildTargetGroup].SharedTightMeshSettings.Detail = Mathf.Max(Vector3.kEpsilon, value),
                    value => EditorGUILayout.Slider(GUITextContent.Drawer.SharedSettingsDetail, value, Vector3.kEpsilon, 1f)
                    );

            DrawAlphaToleranceProperty(spriteTightMeshSettings, buildTargetGroup);

            // Merge distance
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].UnityMethodTightMeshSettings.VertexMergeDistance,
                    (index, settings, value) => settings[buildTargetGroup].UnityMethodTightMeshSettings.VertexMergeDistance = value,
                    value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.SharedSettingsMergeDistance, value, 0, 30)
                    );

            // Detect holes
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].UnityMethodTightMeshSettings.DetectHoles,
                    (index, settings, value) => settings[buildTargetGroup].UnityMethodTightMeshSettings.DetectHoles = value,
                    value => EditorGUILayout.Toggle(GUITextContent.Drawer.SharedSettingsDetectHoles, value)
                    );

            return isChanged;
        }

        public bool DrawPreciseSettings(SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged = false;

            isChanged |= DrawAlphaSourceChannelProperty(spriteTightMeshSettings, buildTargetGroup);
            isChanged |= DrawAlphaToleranceProperty(spriteTightMeshSettings, buildTargetGroup);

            // Edge inflation
            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].PreciseTightMeshSettings.EdgeInflation,
                    (index, settings, value) => settings[buildTargetGroup].PreciseTightMeshSettings.EdgeInflation = value,
                    value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.PreciseSettingsEdgeInflation, value, 0, 10)
                    );

            return isChanged;
        }

        public bool DrawAlphaSeparationSettings(Sprite[] sprites, SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged = false;
            bool isLinkAlpha;
            bool isMultipleSpriteMode;

            bool isMixedLinkAlpha =
                InspectorUtility<SpriteTightMeshSettings>.IsMixedValues(
                    spriteTightMeshSettings,
                    settings => !settings.PlatformSharedTightMeshSettings.AlphaSprite.IsNull(),
                    out isLinkAlpha
                );

            bool isMixedMultipleSpriteMode =
                InspectorUtility<Sprite>.IsMixedValues(
                    sprites,
                    sprite => sprite.texture.GetTextureImporter().spriteImportMode == SpriteImportMode.Multiple,
                    out isMultipleSpriteMode
                );

            EditorGUILayout.HelpBox("Alpha Separation", MessageType.None);

            isChanged |=
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings.PlatformSharedTightMeshSettings.AlphaSprite.GetSpriteInstance(),
                    (index, settings, value) => { },
                    value => {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.ObjectField(EditorGUILayout.GetControlRect(true, 16f), GUITextContent.Drawer.AlphaSeparationSettingsAlphaSprite, value, typeof(Sprite), false);
                        EditorGUI.EndDisabledGroup();

                        return value;
                    }
                );

            if (!isMixedLinkAlpha) {
                if (!isMultipleSpriteMode || isMixedMultipleSpriteMode) {
                    DrawIndented(
                        () => {
                            EditorGUILayout.HelpBox(
                                "To use alpha separation, Multiple sprite mode " +
                                "must be enabled in the texture import settings.",
                                MessageType.Info,
                                true
                                );
                            return false;
                        },
                        0f
                    );

                    if (DrawIndented(() => GUILayout.Button("Enable Multiple Sprite Mode"))) {
                        ExecuteNotifiedAction(() => EnableMultipleSpriteMode(sprites));
                    }
                } else {
                    if (isLinkAlpha) {
                        if (DrawIndented(() => GUILayout.Button(new GUIContent("Unlink Alpha Sprite", SpriteSharpEditorResources.LinkBreakIcon), GUILayout.Height(22f)))) {
                            ExecuteNotifiedAction(() => UnlinkAlphaSprites(sprites, spriteTightMeshSettings));
                        }

                        if (DrawIndented(() => {
                            EditorGUILayout.BeginVertical();
                            GUILayout.Space(5f);
                            bool result = GUILayout.Button(GUITextContent.Drawer.AlphaSeparationInstantiateSprites, GUILayout.Height(25f));
                            EditorGUILayout.EndVertical();
                            return result;
                        })) {
                            InstantiateAlphaSeparatedSprites(sprites);
                        }
                    } else {
                        EditorGUILayout.HelpBox(
                            "No alpha sprite linked. Sprite will be processed in Normal mode until alpha sprite is linked.",
                            MessageType.Warning,
                            true
                        );
                        if (DrawIndented(() => GUILayout.Button(new GUIContent("Create and Link Alpha Sprite", SpriteSharpEditorResources.LinkIcon), GUILayout.Height(22f)))) {
                            ExecuteNotifiedAction(() => LinkAlphaSprites(sprites));
                        }
                    }
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                DrawIndented(() => GUILayout.Button("—"));
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.HelpBox("Alpha Sprite", MessageType.None);

            isChanged |= DrawMainSettings(spriteTightMeshSettings, buildTargetGroup);

            if (!isMixedLinkAlpha && !(!isMultipleSpriteMode || isMixedMultipleSpriteMode) && isLinkAlpha) {
                EditorGUILayout.HelpBox("Opaque Sprite", MessageType.None);

                // Alpha tolerance
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueAlphaTolerance,
                        (index, settings, value) => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueAlphaTolerance = value,
                        value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.AlphaSeparationSettingsOpaqueAlphaTolerance, value, 0, 254)
                    );

                // Merge distance
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueVertexMergeDistance,
                        (index, settings, value) => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueVertexMergeDistance = value,
                        value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.SharedSettingsMergeDistance, value, 0, 30)
                        );

                // OpaqueNegativeExtrude
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueNegativeExtrude,
                        (index, settings, value) => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueNegativeExtrude = value,
                        value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.AlphaSeparationSettingsEdgeContraction, value, 0, 20, GUILayout.MaxHeight(16f))
                    );

                // ReduceAlphaBleed
                isChanged |=
                    InspectorUtility<SpriteTightMeshSettings>.DrawField(
                        spriteTightMeshSettings,
                        settings => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.ReduceAlphaBleed,
                        (index, settings, value) => settings[buildTargetGroup].AlphaSeparationTightMeshSettings.ReduceAlphaBleed = value,
                        value => EditorGUILayout.Toggle(GUITextContent.Drawer.AlphaSeparationSettingsReduceAlphaBleed, value, GUILayout.MaxHeight(16f))
                    );
            }

            return isChanged;
        }

        private void InstantiateAlphaSeparatedSprites(Sprite[] sprites) {
            Vector3 position;
            Quaternion rotation;
            SceneViewUtility.CalculateSimpleInstantiatePosition(out position, out rotation);

            Material opaqueSpriteMaterial = SpriteSharpUtility.GetOpaqueSpriteMaterial();
            if (opaqueSpriteMaterial == null) {
                Debug.LogError("Opaque sprite material 'Sprites-Opaque' not found.");
                return;
            }

            DatabaseProxy database = DatabaseProxy.Instance;
            List<GameObject> spawnedSpriteGameObjects = new List<GameObject>();
            foreach (Sprite opaqueSprite in sprites) {
                SpriteTightMeshSettings tightMeshSettings = database.GetTightMeshSettings(opaqueSprite, false);

                if (tightMeshSettings == null)
                    continue;

                Sprite alphaSprite = tightMeshSettings.PlatformSharedTightMeshSettings.AlphaSprite.GetSpriteInstance();
                if (alphaSprite == null)
                    continue;

                // Instantiate objects
                GameObject opaqueGameObject = new GameObject(opaqueSprite.name);
                SpriteRenderer opaqueSpriteRenderer = opaqueGameObject.AddComponent<SpriteRenderer>();
                opaqueSpriteRenderer.sprite = opaqueSprite;
                opaqueSpriteRenderer.material = opaqueSpriteMaterial;
                opaqueGameObject.transform.position = position;
                opaqueGameObject.transform.rotation = rotation;

                GameObject alphaGameObject = new GameObject(alphaSprite.name);
                SpriteRenderer alphaSpriteRenderer = alphaGameObject.AddComponent<SpriteRenderer>();
                alphaSpriteRenderer.sprite = alphaSprite;
                alphaGameObject.transform.parent = opaqueGameObject.transform;
                alphaGameObject.transform.localPosition = Vector3.zero;
                alphaGameObject.transform.localRotation = Quaternion.identity;

                spawnedSpriteGameObjects.Add(opaqueGameObject);
            }

            if (spawnedSpriteGameObjects.Count > 0) {
                EditorApplication.delayCall += () => {
                    foreach (GameObject gameObject in spawnedSpriteGameObjects) {
                        EditorGUIUtility.PingObject(gameObject);
                    }
                };

                foreach (GameObject gameObject in spawnedSpriteGameObjects) {
                    Undo.RegisterCreatedObjectUndo(gameObject, "Create Alpha Separated Sprites");
                }
            }
        }

#endif // !SS_ADVANCED_METHODS_DISABLED

        internal void DrawMeshExport(Sprite[] sprites) {
            if (_spriteMeshExportMenu == null) {
                _spriteMeshExportMenu = new GenericMenu();
                _spriteMeshExportMenu.AddItem(new GUIContent("Export Mesh Only"), false, () => SaveSpriteMeshes(sprites, false));
                _spriteMeshExportMenu.AddItem(new GUIContent("Export Mesh + Material"), false, () => SaveSpriteMeshes(sprites, true));
            }

            if (GUILayout.Button(GUITextContent.Drawer.ExportMesh, GUILayout.Height(25f))) {
                _spriteMeshExportMenu.ShowAsContext();
            }
        }

        private void ExecuteNotifiedAction(Action action, bool executeChangedHandler = true, bool setDirty = false) {
            if (setDirty) {
                if (_spriteTightMeshSettingsSetDirty != null)
                    _spriteTightMeshSettingsSetDirty();
            }

            bool canExecute = true;
            if (_spriteTightMeshSettingsAllowChangeCheck != null)
                canExecute = _spriteTightMeshSettingsAllowChangeCheck();

            if (canExecute) {
                if (_spriteTightMeshSettingsChanging != null)
                    _spriteTightMeshSettingsChanging();

                action();
                if (executeChangedHandler && _spriteTightMeshSettingsChanged != null)
                    _spriteTightMeshSettingsChanged();
            }
        }

        private static bool DrawAlphaSourceChannelProperty(SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged =
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].SharedTightMeshSettings.AlphaSourceChannel,
                    (index, settings, value) => settings[buildTargetGroup].SharedTightMeshSettings.AlphaSourceChannel = value,
                    value => (SpriteAlphaSourceChannel) EditorGUILayout.EnumPopup(GUITextContent.Drawer.SharedSettingsAlphaSourceChannel, value)
                );

            return isChanged;
        }

        private static bool DrawAlphaToleranceProperty(SpriteTightMeshSettings[] spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            bool isChanged =
                InspectorUtility<SpriteTightMeshSettings>.DrawField(
                    spriteTightMeshSettings,
                    settings => settings[buildTargetGroup].SharedTightMeshSettings.AlphaTolerance,
                    (index, settings, value) => settings[buildTargetGroup].SharedTightMeshSettings.AlphaTolerance = value,
                    value => (byte) EditorGUILayout.Slider(GUITextContent.Drawer.SharedSettingsAlphaTolerance, value, 0, 254)
                );

            return isChanged;
        }

        private static T DrawIndented<T>(Func<T> action, float indent = 16f) {
            T result;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(EditorGUI.indentLevel * indent);
                result = action();
            }
            EditorGUILayout.EndHorizontal();

            return result;
        }

        private static void EnableMultipleSpriteMode(Sprite[] sprites) {
            Texture2D[] spriteTextures = sprites.Select(sprite => sprite.texture).Distinct().ToArray();
            List<Texture2D> modifiedTextures = new List<Texture2D>();
            foreach (Texture2D texture in spriteTextures) {
                TextureImporter textureImporter = texture.GetTextureImporter();
                //if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                //    continue;

                textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                List<SpriteMetaData> spriteMetaData = new List<SpriteMetaData>(textureImporter.spritesheet);
                if (spriteMetaData.Count == 0) {
                    SpriteMetaData singleSpriteMetaData = new SpriteMetaData {
                        name = texture.name,
                        pivot = textureImporter.spritePivot
                    };

                    IntVector2 originalTextureSize = textureImporter.GetOriginalTextureSize();
                    singleSpriteMetaData.rect = new Rect(0f, 0f, originalTextureSize.x, originalTextureSize.y);

                    spriteMetaData.Add(singleSpriteMetaData);
                    textureImporter.spritesheet = spriteMetaData.ToArray();
                    textureImporter.SaveAndReimport();
                }

                EditorUtility.SetDirty(textureImporter);
                modifiedTextures.Add(texture);
            }

            EditorTextureUtility.ReimportDistinctTextures(modifiedTextures);
        }

#if !SS_ADVANCED_METHODS_DISABLED
        private static void UnlinkAlphaSprites(Sprite[] sprites, SpriteTightMeshSettings[] spriteTightMeshSettings) {
            DatabaseProxy database = DatabaseProxy.Instance;
            IDictionary<SpriteLazyReference, SpriteLazyReference> alphaSpriteToOpaqueSprite = database.AlphaSpriteToOpaqueSprite;
            List<Texture2D> modifiedTextures = new List<Texture2D>();
            for (int i = 0; i < spriteTightMeshSettings.Length; i++) {
                Sprite sprite = sprites[i];

                SpriteTightMeshSettings spriteTightMeshSetting = spriteTightMeshSettings[i];
                Sprite alphaSprite = spriteTightMeshSetting.PlatformSharedTightMeshSettings.AlphaSprite.GetSpriteInstance();

                if (alphaSprite == null)
                    continue;

                TextureImporter textureImporter = alphaSprite.texture.GetTextureImporter();
                List<SpriteMetaData> spritesheet = textureImporter.spritesheet.ToList();
                for (int j = spritesheet.Count - 1; j >= 0; j--) {
                    SpriteMetaData spriteMetaData = spritesheet[j];
                    if (spriteMetaData.name == alphaSprite.name) {
                        spritesheet.RemoveAt(j);
                    }
                }

                spriteTightMeshSetting.PlatformSharedTightMeshSettings.AlphaSprite = null;

                // Clear AlphaSprite from real stored settings
                SpriteTightMeshSettings tightMeshSettings = database.GetTightMeshSettings(sprite, false);
                if (tightMeshSettings != null) {
                    tightMeshSettings.PlatformSharedTightMeshSettings.AlphaSprite = null;
                }

                textureImporter.spritesheet = spritesheet.ToArray();
                EditorUtility.SetDirty(textureImporter);

                modifiedTextures.Add(alphaSprite.texture);

                SpriteLazyReference alphaSpriteLazyReference = alphaSprite.ToSpriteLazyReference();
                if (alphaSpriteToOpaqueSprite.ContainsKey(alphaSpriteLazyReference)) {
                    alphaSpriteToOpaqueSprite.Remove(alphaSpriteLazyReference);
                }
            }

            EditorTextureUtility.ReimportDistinctTextures(modifiedTextures);
        }

        private static void LinkAlphaSprites(Sprite[] sprites) {
            DatabaseProxy database = DatabaseProxy.Instance;
            List<DatabaseProxy.AlphaSpriteLink> alphaSpriteLinks = database.AlphaSpriteLinkData;
            foreach (Sprite sprite in sprites) {
                TextureImporter textureImporter = sprite.texture.GetTextureImporter();

                List<SpriteMetaData> spritesheet = textureImporter.spritesheet.ToList();

                // Find sprite with matching name
                SpriteMetaData matchingMetaData = spritesheet.FirstOrDefault(data => data.name == sprite.name);

                // Remove other sprites with alpha sprite name
                string alphaSpriteName = matchingMetaData.name + " (Alpha)";
                for (int j = spritesheet.Count - 1; j >= 0; j--) {
                    SpriteMetaData spriteMetaData = spritesheet[j];
                    if (spriteMetaData.name == alphaSpriteName) {
                        spritesheet.RemoveAt(j);
                    }
                }

                // Add new alpha sprite
                SpriteMetaData alphaMetaData = matchingMetaData;
                alphaMetaData.name = alphaSpriteName;
                spritesheet.Add(alphaMetaData);

                textureImporter.spritesheet = spritesheet.ToArray();
                EditorUtility.SetDirty(textureImporter);

                alphaSpriteLinks.Add(
                    new DatabaseProxy.AlphaSpriteLink {
                        AlphaSpriteName = alphaMetaData.name,
                        OpaqueSprite = sprite,
                        OpaqueSpriteTexture = sprite.texture
                    });
            }

            EditorTextureUtility.ReimportDistinctTextures(alphaSpriteLinks.Select(link => link.OpaqueSpriteTexture));
        }
#endif // !SS_ADVANCED_METHODS_DISABLED

        private static void SaveSpriteMeshes(Sprite[] sprites, bool createMaterials) {
            try {
                AssetDatabase.StartAssetEditing();

                if (sprites.Length > 1) {
                    string destinationDirectory = EditorUtility.SaveFolderPanel("Export Sprite Meshes", "Assets", "");
                    if (String.IsNullOrEmpty(destinationDirectory))
                        return;

                    for (int i = 0; i < sprites.Length; i++) {
                        Sprite sprite = sprites[i];

                        if (EditorUtility.DisplayCancelableProgressBar("Exporting Sprite Meshes", "Sprite: " + sprite.name, i / (float) sprites.Length))
                            break;

                        string meshAssetPath = Path.Combine(destinationDirectory, sprite.name + ".asset");
                        meshAssetPath = FilePathUtility.MakeRelativePath(meshAssetPath, Path.GetFullPath("Assets"));
                        meshAssetPath = AssetDatabase.GenerateUniqueAssetPath(meshAssetPath);

                        CreateSpriteMeshAsset(sprite, meshAssetPath);
                        if (createMaterials) {
                            CreateSpriteMeshMaterialAsset(sprite, meshAssetPath);
                        }
                    }
                } else {
                    string spriteMeshAssetPath = EditorUtility.SaveFilePanelInProject("Export Sprite Mesh", sprites[0].name, "asset", "");
                    if (String.IsNullOrEmpty(spriteMeshAssetPath))
                        return;

                    Mesh spriteMeshAsset = CreateSpriteMeshAsset(sprites[0], spriteMeshAssetPath);

                    if (createMaterials) {
                        Material spriteMeshMaterialAsset = CreateSpriteMeshMaterialAsset(sprites[0], spriteMeshAssetPath);
                        EditorApplication.delayCall += () => EditorGUIUtility.PingObject(spriteMeshMaterialAsset);
                    } else {
                        EditorApplication.delayCall += () => EditorGUIUtility.PingObject(spriteMeshAsset);
                    }
                }
            } finally {
                EditorUtility.ClearProgressBar();
                AssetDatabase.StopAssetEditing();
            }
        }

        private static Material CreateSpriteMeshMaterialAsset(Sprite sprite, string spriteMeshAssetPath) {
            if (spriteMeshAssetPath == null)
                throw new ArgumentNullException("spriteMeshAssetPath");

            string spriteMeshMaterialAssetPath = Path.Combine(
                Path.GetDirectoryName(spriteMeshAssetPath),
                Path.GetFileNameWithoutExtension(spriteMeshAssetPath)) +
                ".mat";
            spriteMeshMaterialAssetPath = AssetDatabase.GenerateUniqueAssetPath(spriteMeshMaterialAssetPath);

            Material spriteMeshMaterial = new Material(Shader.Find("Unlit/Transparent"));
            spriteMeshMaterial.mainTexture = sprite.texture;

            AssetDatabase.CreateAsset(spriteMeshMaterial, spriteMeshMaterialAssetPath);
            AssetDatabase.ImportAsset(spriteMeshMaterialAssetPath);

            return spriteMeshMaterial;
        }

        private static Mesh CreateSpriteMeshAsset(Sprite sprite, string meshAssetPath) {
            Mesh spriteMesh = SpriteSharpUtility.CreateMeshFromSprite(sprite);
            AssetDatabase.CreateAsset(spriteMesh, meshAssetPath);
            AssetDatabase.ImportAsset(meshAssetPath);

            return spriteMesh;
        }
    }
}
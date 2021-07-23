using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LostPolygon.SpriteSharp.Gui;
using LostPolygon.SpriteSharp.Gui.Internal;
using LostPolygon.SpriteSharp.Processing;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Experimental {
    public class SpriteDicingWindow : EditorWindow {
        private SpriteTightMeshSettings _spriteMeshSettings = new SpriteTightMeshSettings();
        private SpriteMeshSettingsDrawer _drawer;

        private GUIContent _diceContent;
        private GUIContent _instantiateContent;

        private UnityVersionParser _unityVersionParser;

        private void Awake() {
            _diceContent =
                new GUIContent(
                    "Dice",
                    SpriteSharpEditorResources.DiceIcon,
                    "Split texture into dice sprites"
                );

            _instantiateContent =
                new GUIContent(
                    "Instantiate",
                    SpriteSharpEditorResources.SpriteIcon,
                    "Assemble a single object from diced sprites and insert it into the scene"
                );

            _spriteMeshSettings.DefaultTightMeshSettings.RectGridTightMeshSettings.XSubdivisions = 12;
            _spriteMeshSettings.DefaultTightMeshSettings.RectGridTightMeshSettings.YSubdivisions = 12;
        }

        private void OnEnable() {
            _unityVersionParser = new UnityVersionParser(Application.unityVersion);
            titleContent = new GUIContent("SpriteSharp — Sprite Dicing");
            minSize = new Vector2(350f, 221f);
            maxSize = minSize;
        }

        private void OnGUI() {
            GUIStyle paddingStyle = new GUIStyle { padding = new RectOffset(5, 5, 7, 7) };
            EditorGUILayout.BeginVertical(paddingStyle);
            {
                DrawContents();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnSelectionChange() {
            Repaint();
        }

        private void DrawContents() {
            bool isUnity20171OrNewer = _unityVersionParser.VersionMajor >= 2017;

            Texture2D[] textures =
                Selection.objects
                    .OfType<Texture2D>()
                    .Concat(Selection.objects.OfType<Sprite>().Select(sprite => sprite.texture))
                    .Distinct()
                    .ToArray();

            if (textures.Length == 0) {
                EditorGUILayout.HelpBox(
                    "Select textures to process in the Project window.\n" +
                    "Note: all existing sprites in the texture will be removed and replaced by diced sprites.",
                    MessageType.Info);
                return;
            }

            Texture2D[] processedTextures;
            if (isUnity20171OrNewer) {
                processedTextures = textures;
            } else {
                processedTextures =
                    textures
                        .Where(d => !String.IsNullOrEmpty(d.GetTextureImporter().spritePackingTag))
                        .ToArray();

                if (processedTextures.Length == 0) {
                    EditorGUILayout.HelpBox(
                        "Selected textures must have packing tag set.",
                        MessageType.Error);
                    return;
                }
            }

            string message =
                String.Format("Selected {0} texture(s) for processing.", processedTextures.Length);

            if (!isUnity20171OrNewer && textures.Length != processedTextures.Length) {
                message += String.Format("\n{0} texture(s) don't have a packing tag", textures.Length - processedTextures.Length);
            }

            EditorGUILayout.HelpBox(message, MessageType.Info);

            if (_drawer == null) {
                _drawer = new SpriteMeshSettingsDrawer(
                    () => true,
                    () => { },
                    () => { },
                    () => { },
                    Repaint
                );
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                _drawer.DrawRectGridSettings(new[] { _spriteMeshSettings }, (BuildTargetGroup) (-1));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(GUILayout.Height(32f));
            {
                if (GUILayout.Button(_diceContent, GUILayout.Height(32f))) {
                    ProcessTextures(processedTextures, Dice);
                    if (processedTextures.Length == 1) {
                        EditorGUIUtility.PingObject(processedTextures[0]);
                    }
                }

                if (GUILayout.Button(_instantiateContent, GUILayout.Height(32f))) {
                    ProcessTextures(processedTextures, AssembleCombinedSprites, false);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ProcessTextures(Texture2D[] textures, Action<Texture2D> processAction, bool showProgressbar = true) {
            try {
                for (int i = 0; i < textures.Length; i++) {
                    if (showProgressbar) {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Processing diced texture",
                            String.Format("Processing {0}... {1} \u2044 {2}", textures[i].name, i + 1, textures.Length),
                            i / (float) textures.Length
                        ))
                            break;
                    }

                    processAction(textures[i]);
                }
            } finally {
                if (showProgressbar) {
                    EditorUtility.ClearProgressBar();
                }
                EditorReflectionWrapper.RepaintAllInspectors();
                SceneView.RepaintAll();
                GUIUtility.ExitGUI();
            }
        }

        private void AssembleCombinedSprites(Texture2D texture) {
            Sprite[] sprites =
                AssetDatabase
                    .LoadAllAssetsAtPath(texture.GetTextureImporter().assetPath)
                    .OfType<Sprite>()
                    .ToArray();
            Vector2 scale = Vector2.one / texture.GetTextureImporter().spritePixelsPerUnit;
            IntVector2 originalTextureSize = texture.GetTextureImporter().GetOriginalTextureSize();

            scale.x *= originalTextureSize.x / (float) texture.width;
            scale.y *= originalTextureSize.y / (float) texture.height;

            Material opaqueSpriteMaterial = SpriteSharpUtility.GetOpaqueSpriteMaterial();
            GameObject rootGameObject = new GameObject(texture.name);
            foreach (Sprite sprite in sprites) {
                GameObject tileGameObject = new GameObject(sprite.name);
                tileGameObject.transform.parent = rootGameObject.transform;
                Vector2 tilePosition = sprite.textureRect.position - sprite.textureRectOffset + sprite.pivot;
                tilePosition.x *= scale.x;
                tilePosition.y *= scale.y;
                tileGameObject.transform.localPosition = tilePosition;
                SpriteRenderer spriteRenderer = tileGameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;

                if (sprite.name.IndexOf("opaque", StringComparison.InvariantCultureIgnoreCase) != -1) {
                    spriteRenderer.material = opaqueSpriteMaterial;
                }
            }

            Vector3 rootPosition;
            Quaternion rootRotation;
            SceneViewUtility.CalculateSimpleInstantiatePosition(out rootPosition, out rootRotation);
            rootGameObject.transform.position = rootPosition;
            rootGameObject.transform.rotation = rootRotation;

            EditorApplication.delayCall += () => {
                EditorGUIUtility.PingObject(rootGameObject);
            };
        }

        private void Dice(Texture2D texture) {
            int xSubdivisions = _spriteMeshSettings.DefaultTightMeshSettings.RectGridTightMeshSettings.XSubdivisions;
            int ySubdivisions = _spriteMeshSettings.DefaultTightMeshSettings.RectGridTightMeshSettings.YSubdivisions;

            IntRect spriteRect;
            IntRect spriteRectLocal;
            IntRect originalSpriteRect = new IntRect(0, 0, texture.width, texture.height);
            SpriteRectMeshGenerator spriteRectMeshGenerator =
                new SpriteRectMeshGenerator(
                    texture,
                    originalSpriteRect,
                    _spriteMeshSettings.DefaultTightMeshSettings.SharedTightMeshSettings,
                    _spriteMeshSettings.DefaultTightMeshSettings.RectGridTightMeshSettings,
                    SpriteRectMeshGenerator.ProcessingOptions.None);

            List<SpriteRectMeshGenerator.RectGridCell> cells;
            spriteRectMeshGenerator.CalculateRectGrid(
                xSubdivisions,
                ySubdivisions,
                out spriteRect,
                out spriteRectLocal,
                out cells);

            TextureImporter textureImporter = texture.GetTextureImporter();
            IntVector2 originalTextureSize = textureImporter.GetOriginalTextureSize();
            Vector2 scale;
            scale.x = originalTextureSize.x / (float) texture.width;
            scale.y = originalTextureSize.y / (float) texture.height;

            List<SpriteMetaData> sprites = new List<SpriteMetaData>();
            foreach (SpriteRectMeshGenerator.RectGridCell cell in cells) {
                // Calculate whether sprite is opaque
                Color32[] texturePixels = spriteRectMeshGenerator.TexturePixels;

                byte minAlpha;
                byte maxAlpha;
                PreciseOutlineGenerator.GetMinMaxAlphaValue(texturePixels, texture.width, cell.Rect, SpriteAlphaSourceChannel.Alpha, out minAlpha, out maxAlpha);

                bool isOpaque = minAlpha == 255 && maxAlpha == 255;

                // Calculate sprite data
                SpriteMetaData sprite = new SpriteMetaData {
                    name =
                        String.Format(
                            "{0} [{2:00}x{3:00}{1}]",
                            texture.name,
                            isOpaque ? ", Opaque" : "",
                            cell.Index.x,
                            ySubdivisions - 1 - cell.Index.y),
                    alignment = (int) SpriteAlignment.TopLeft
                };
                Rect cellRect = (Rect) cell.Rect;
                cellRect.xMin = Mathf.Round(cellRect.xMin * scale.x);
                cellRect.yMin = Mathf.Round(cellRect.yMin * scale.x);

                cellRect.width = Mathf.Round(cell.Rect.width * scale.x);
                cellRect.height = Mathf.Round(cell.Rect.height * scale.y);

                cellRect.xMax = Mathf.Min(cellRect.xMax, originalTextureSize.x);
                cellRect.yMax = Mathf.Min(cellRect.yMax, originalTextureSize.y);
                sprite.rect = cellRect;
                sprites.Add(sprite);
            }

            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.spritesheet = sprites.ToArray();
            EditorUtility.SetDirty(textureImporter);
            EditorUtility.SetDirty(texture);
            textureImporter.SaveAndReimport();
        }
    }
}
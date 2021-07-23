#if !SS_ADVANCED_METHODS_DISABLED

using System;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Database;
using LostPolygon.SpriteSharp.Internal;
using LostPolygon.SpriteSharp.Processing;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;
using Random = UnityEngine.Random;

namespace LostPolygon.SpriteSharp.Gui.Internal {
    /// <summary>
    /// SpriteSharp tab in Unity Preferences window.
    /// </summary>
    internal static class EditorPreferencesWindow {
        private static SpriteMeshSettingsDrawer _drawer;
        private static SpriteProcessingMethodCut _spriteProcessingMethod;

        [PreferenceItem(SpriteSharp.Internal.Constants.kAssetName)]
        private static void PreferenceOnGUI() {
            if (!PrebuiltAssemblyDetector.CanWorkWithDatabase) {
                EditorGUILayout.HelpBox(
                    SpriteSharp.Internal.Constants.kPrebuiltAssemblyName + " prebuilt assembly is present alongside with SpriteSharp source code. " +
                    "Please delete " + SpriteSharp.Internal.Constants.kPrebuiltAssemblyName + ".dll file.",
                    MessageType.Error,
                    true
                    );

                DrawCopyrights();
                return;
            }

            EditorGUIUtility.labelWidth = 130f;

            if (_drawer == null) {
                _drawer = new SpriteMeshSettingsDrawer(
                    () => true,
                    () => { },
                    () => { },
                    () => { },
                    EditorWindow.focusedWindow.Repaint
                    );
            }

            DatabaseProxy database = DatabaseProxy.Instance;
            if (database == null)
                return;

            SpriteTightMeshSettings settings = database.DefaultSpriteSettings;
            SpriteTightMeshSettings[] settingsArray = { settings };

            BuildTargetGroup defaultBuildTargetGroup = BuildPlatformsUtility.GetDefaultBuildTargetGroup();

            bool isMustRepaint = false;
            bool isChanged = false;

            GUILayout.Space(10f);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(database.IsProcessingDisabled);
            {
                EditorGUI.BeginChangeCheck();
                settings[defaultBuildTargetGroup].IsOverriding = EditorGUILayout.ToggleLeft("Use Default Override Settings", settings[defaultBuildTargetGroup].IsOverriding);
                EditorGUI.indentLevel++;
                isMustRepaint |= EditorGUI.EndChangeCheck();
                EditorGUI.BeginDisabledGroup(!settings[defaultBuildTargetGroup].IsOverriding);
                {
                    _spriteProcessingMethod = (SpriteProcessingMethodCut) settings[defaultBuildTargetGroup].ProcessingMethod;
                    _spriteProcessingMethod = (SpriteProcessingMethodCut) EditorGUILayout.EnumPopup("Processing Mode", _spriteProcessingMethod);
                    settings[defaultBuildTargetGroup].ProcessingMethod = (SpriteProcessingMethod) _spriteProcessingMethod;

                    switch (_spriteProcessingMethod) {
                        case SpriteProcessingMethodCut.Normal:
                            isChanged |= _drawer.DrawMainSettings(settingsArray, defaultBuildTargetGroup);
                            break;
                        case SpriteProcessingMethodCut.Precise:
                            isChanged |= _drawer.DrawPreciseSettings(settingsArray, defaultBuildTargetGroup);
                            break;
                        case SpriteProcessingMethodCut.RectGrid:
                            isChanged |= _drawer.DrawRectGridSettings(settingsArray, defaultBuildTargetGroup);
                            break;
                        default:
                            throw new Exception("Unknown SpriteTightMeshSettings.ProcessingMethod value");
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;

                database.SkipTextureSpriteExtrude =
                    EditorGUILayout.ToggleLeft("Ignore Unity's 'Extrude Edges' Option On Overriden Sprites", database.SkipTextureSpriteExtrude);
                database.WorkaroundSpriteAtlasRepacking =
                    EditorGUILayout.ToggleLeft("Enable workaround with Sprite Atlas not repacking", database.WorkaroundSpriteAtlasRepacking);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(15f);
            EditorGUI.BeginChangeCheck();
            database.IsProcessingDisabled = EditorGUILayout.ToggleLeft("Disable " + SpriteSharp.Internal.Constants.kAssetName, database.IsProcessingDisabled);
            isMustRepaint |= EditorGUI.EndChangeCheck();
            if (database.IsProcessingDisabled) {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(300f));
                EditorGUILayout.HelpBox(
                    SpriteSharp.Internal.Constants.kAssetName + " is currently disabled. " +
                    "This means that all sprites imported now will have meshes generated by standard Unity method. " +
                    "All existing sprite meshes will remain unchanged until reimported again.",
                    MessageType.Warning,
                    true
                    );
                EditorGUILayout.EndHorizontal();
            }

            isChanged |= EditorGUI.EndChangeCheck();

            if (isChanged) {
                EditorUtility.SetDirty(DatabaseProxy.Instance);
            }

            if (isMustRepaint) {
                EditorReflectionWrapper.RepaintAllInspectors();
            }

            DrawCopyrights();
        }

        private static void DrawCopyrights() {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUIStyle.none, GUILayout.Height(20f), GUILayout.ExpandWidth(true));
            GUILayout.Space(0f);
            EditorGUILayout.EndVertical();

            Rect copyrightRect = GUILayoutUtility.GetLastRect();
            DrawCopyrights(copyrightRect);
        }

        private static void DrawCopyrights(Rect rect) {
            string copyrightText = String.Format("Lost Polygon © 2013-{0}, ", Mathf.Max(2015, DateTime.Now.Year));
            Rect copyrightRect = new Rect(rect.xMin, rect.yMin, GUI.skin.label.CalcSize(new GUIContent(copyrightText)).x, 20f);

            GUI.Label(copyrightRect, copyrightText);

            copyrightRect.x += copyrightRect.width;
            copyrightRect.width = GUI.skin.label.CalcSize(new GUIContent("LostPolygon.com")).x + 2f;
            Rect underlineRect = copyrightRect;
            underlineRect.width -= 2f;

            GUIStyle linkStyle = new GUIStyle(GUI.skin.label);
            Color linkColor = EditorGUIUtility.isProSkin ? (Color.blue * 0.25f + Color.cyan * 0.75f) : Color.blue;
            linkStyle.normal.textColor = linkColor;
            GUI.contentColor = linkColor;
            GUI.Label(
                underlineRect,
                "__________________",
                linkStyle);
            GUI.Label(
                copyrightRect,
                "LostPolygon.com",
                linkStyle);

            EditorGUIUtility.AddCursorRect(copyrightRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && copyrightRect.Contains(Event.current.mousePosition)) {
                Application.OpenURL("http://lostpolygon.com");
            }

            const string versionString = "v" + SpriteSharp.Internal.Constants.kAssetVersion;

            GUI.contentColor = Color.white;
            copyrightRect.x = rect.xMax - GUI.skin.label.CalcSize(new GUIContent(versionString)).x - 3f;
            GUI.Label(copyrightRect, versionString);
        }

        private enum SpriteProcessingMethodCut {
            Normal = SpriteProcessingMethod.Normal,
            Precise = SpriteProcessingMethod.Precise,
            RectGrid = SpriteProcessingMethod.RectGrid,
        }
    }
}

#endif // !SS_ADVANCED_METHODS_DISABLED
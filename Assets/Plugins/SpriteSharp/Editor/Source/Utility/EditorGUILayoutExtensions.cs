using UnityEditor;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Editor GUI drawing helpers.
    /// </summary>
    internal static class EditorGUILayoutExtensions {
        public static int BeginPlatformGrouping(int currentValue, BuildPlatform[] platforms, GUIContent defaultTab) {
            int platformsLength = platforms.Length;

            if (currentValue >= platformsLength) {
                currentValue = -1;
            }

            if (defaultTab == null && currentValue == -1) {
                currentValue = 0;
            }

            bool enabled = GUI.enabled;
            GUI.enabled = true;

            Rect rect = EditorGUILayout.BeginVertical(GUI.skin.box);
            rect.width -= 1f;

            const int tabHeight = 18;
            const float defaultPlatformIconWidth = 30f;

            float iconScaleFactor = 1f;

            if (defaultTab != null) {
                const float minDefaultButtonWidth = 50f;
                const float maxDefaultButtonWidth = 135f;
                float platformIconsTotalWidth = platformsLength * defaultPlatformIconWidth;
                float spaceLeftForDefaultButton = rect.width - platformIconsTotalWidth;
                if (spaceLeftForDefaultButton < minDefaultButtonWidth) {
                    float diff = minDefaultButtonWidth - spaceLeftForDefaultButton;
                    iconScaleFactor = (platformIconsTotalWidth - diff) / platformIconsTotalWidth;
                } else if (spaceLeftForDefaultButton > maxDefaultButtonWidth) {
                    float diff = maxDefaultButtonWidth - spaceLeftForDefaultButton;
                    iconScaleFactor = (platformIconsTotalWidth - diff) / platformIconsTotalWidth;
                }

                float defaultButtonWidth = rect.width - platformsLength * defaultPlatformIconWidth * iconScaleFactor;
                if (GUI.Toggle(new Rect(rect.x, rect.y, defaultButtonWidth, tabHeight), currentValue == -1, defaultTab, EditorStyles.toolbarButton)) {
                    currentValue = -1;
                }
            }

            for (int i = 0; i < platformsLength; i++) {
                Rect position;
                if (defaultTab != null) {
                    position = new Rect(rect.xMax - (platformsLength - i) * defaultPlatformIconWidth * iconScaleFactor, rect.y, defaultPlatformIconWidth * iconScaleFactor, tabHeight);
                } else {
                    int itemX = Mathf.RoundToInt(i * rect.width / platformsLength);
                    int itemWidth = Mathf.RoundToInt((i + 1) * rect.width / platformsLength);
                    position = new Rect(rect.x + itemX, rect.y, itemWidth - itemX, tabHeight);
                }

                if (GUI.Toggle(position, currentValue == i, new GUIContent(platforms[i].SmallIcon, platforms[i].Tooltip), EditorStyles.toolbarButton)) {
                    currentValue = i;
                }
            }

            GUILayoutUtility.GetRect(10f, tabHeight);
            GUI.enabled = enabled;

            return currentValue;
        }

        public static void EndPlatformGrouping() {
            EditorGUILayout.EndVertical();
        }
    }
}

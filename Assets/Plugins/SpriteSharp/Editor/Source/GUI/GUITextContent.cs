using UnityEngine;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Gui.Internal {
    /// <summary>
    /// Long GUI texts.
    /// </summary>
    internal static class GUITextContent {
        public static void FillCache() {
            Drawer.AlphaSeparationInstantiateSprites =
                new GUIContent(
                    "Instantiate Sprites",
                    SpriteSharpEditorResources.SpriteIcon,
                    "Insert separated sprites into scene"
                );

            Drawer.ExportMesh =
                new GUIContent(
                    "Export Sprite Meshes...",
                    SpriteSharpEditorResources.MeshIcon,
                    "Export meshes of selected sprites as assets"
                );
        }

        internal static class Drawer {
            public static GUIContent AlphaSeparationInstantiateSprites;
            public static GUIContent ExportMesh;

            public static readonly GUIContent SharedSettingsDetail = new GUIContent(
                "Detail",
                "Controls how detailed the sprite mesh must be. Lower values results in less triangles, " +
                "but more overdraw, high values result in low overdraw, but at the cost of more triangles. " +
                "A value of 0.3 is a good starting point."
            );

            public static readonly GUIContent SharedSettingsAlphaSourceChannel = new GUIContent(
                "Alpha Source Channel",
                "Selected channel will be treated an alpha channel and used to calculate the mesh outline."
            );

            public static readonly GUIContent SharedSettingsAlphaTolerance = new GUIContent(
                "Alpha Tolerance",
                "Pixels with alpha less than this value will be ignored and not included into the sprite mesh. " +
                "Value of 0 means all non-transparent pixels are included, and value of 254 means " +
                "only fully opaque pixels will end up in the sprite mesh."
            );

            public static readonly GUIContent SharedSettingsMergeDistance = new GUIContent(
                "Merge Distance",
                "Controls the maximum distance at which vertices are merged. " +
                "Merging vertices that are very close to each other is efficient " +
                "for decreasing the mesh complexity without sacrificing detail. "
            );

            public static readonly GUIContent SharedSettingsDetectHoles = new GUIContent(
                "Detect Holes",
                "Controls whether inner holes in the texture will be detected and " +
                "excluded from the sprite mesh, reducing the amount of overdraw."
            );

            public static readonly GUIContent PreciseSettingsEdgeInflation = new GUIContent(
                "Edge Inflation",
                "Controls how much the calculated opaque sprite mesh will be expanded.\n" +
                "Bigger values result in meshes with less polygons, but with some additional overdraw. " +
                "Value of 0 means that the generated mesh will be pixel-perfect."
            );

            public static readonly GUIContent RectGridSettingsScaleAroundCenter = new GUIContent(
                "Scale Around Center",
                "Controls how much the calculated grid mesh will be scaled around center."
            );

            public static readonly GUIContent AlphaSeparationSettingsAlphaSprite = new GUIContent(
                "Alpha Sprite",
                "Sprite that contains the separated non-opaque part of the sprite."
            );

            public static readonly GUIContent AlphaSeparationSettingsEdgeContraction = new GUIContent(
                "Edge Contraction",
                "Controls how much the calculated opaque sprite mesh will be contracted.\n" +
                "The bigger the value, the less the opaque sprite will be. " +
                "This is sometimes useful to avoid filtering artifacts in textures with smooth features."
            );

            public static readonly GUIContent AlphaSeparationSettingsReduceAlphaBleed = new GUIContent(
                "Reduce Alpha Bleed",
                "Controls whether the algorithm will attempt to avoid opaque sprite pixels bleeding into non-opaque pixels." +
                "The resulting opaque sprite will likely have more polygons when this option is enabled.\n" +
                "This option is useful to avoid artifacts when Edge Contraction or Vertex Merge values are high."
            );

            public static readonly GUIContent AlphaSeparationSettingsOpaqueAlphaTolerance = new GUIContent(
                "Alpha Tolerance",
                "Pixels with alpha less than this value will be considered opaque. " +
                "Value of 254 means only fully opaque pixels will be included in the opaque mesh."
            );
        }
    }
}

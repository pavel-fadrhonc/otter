using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.ClipperLib;
using LostPolygon.SpriteSharp.LibTessDotNet;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// The sprite processor.
    /// </summary>
    public class SpriteProcessor {
        private const float kOpaqueSpriteMinPathAreaRatio = 0.008f;

        private readonly Texture2D _texture;
        private readonly Sprite _sprite;
        private readonly SpriteTightMeshSettings _spriteTightMeshSettings;
        private readonly AlphaSpriteMode _alphaSpriteMode;
        private readonly BuildTargetGroup _buildTargetGroup;
        private readonly ProcessingOptions _processingOptions;
        private readonly SinglePlatformSpriteTightMeshSettings _activePlatformSpriteTightMeshSettings;
#pragma warning disable 0414
        private readonly TextureImporterSettings _textureImporterSettings;
#pragma warning restore 0414

        private bool SkipTextureSpriteExtrude {
            get { return (_processingOptions & ProcessingOptions.SkipTextureSpriteExtrude) != 0; }
        }

        public SpriteProcessor(
            Texture2D texture,
            Sprite sprite,
            SpriteTightMeshSettings spriteTightMeshSettings,
            AlphaSpriteMode alphaSpriteMode,
            BuildTargetGroup buildTargetGroup,
            ProcessingOptions processingOptions) {
            _texture = texture;
            _sprite = sprite;
            _spriteTightMeshSettings = spriteTightMeshSettings;
            _alphaSpriteMode = alphaSpriteMode;
            _buildTargetGroup = buildTargetGroup;
            _processingOptions = processingOptions;

            _activePlatformSpriteTightMeshSettings = _spriteTightMeshSettings[_buildTargetGroup];
            _textureImporterSettings = texture.GetTextureImporterSettings();
        }

        public void GenerateMesh(out Vector2[] newVertices, out ushort[] newIndices) {
            // RectGrid special case
            if (_alphaSpriteMode == AlphaSpriteMode.RectGrid) {
                SpriteRectMeshGenerator.ProcessingOptions processingOptions = SpriteRectMeshGenerator.ProcessingOptions.None;
                if (SkipTextureSpriteExtrude) {
                    processingOptions |= SpriteRectMeshGenerator.ProcessingOptions.SkipTextureSpriteExtrude;
                }

                SpriteRectMeshGenerator spriteRectMeshGenerator =
                    new SpriteRectMeshGenerator(
                        _sprite.texture,
                        _sprite.rect.ToIntRectWithPositiveBias(),
                        _activePlatformSpriteTightMeshSettings.SharedTightMeshSettings,
                        _activePlatformSpriteTightMeshSettings.RectGridTightMeshSettings,
                        processingOptions);
                spriteRectMeshGenerator.GenerateRectGridSpriteMesh(out newVertices, out newIndices);
                return;
            }

#if !SS_ADVANCED_METHODS_DISABLED
            // Generate paths from sprite image
            Vector2[][] paths;
            switch (_alphaSpriteMode) {
                case AlphaSpriteMode.Normal:
                    paths = ProcessNormalSprite();
                    break;
                case AlphaSpriteMode.Opaque:
                    paths = ProcessOpaqueSprite();
                    break;
                case AlphaSpriteMode.NonOpaque:
                    paths = ProcessNonOpaqueSprite();
                    break;
                case AlphaSpriteMode.Precise:
                    paths = ProcessPreciseSprite();
                    break;
                default:
                    throw new InvalidEnumArgumentException("Unknown AlphaSpriteMode " + _alphaSpriteMode);
            }

            // Triangulate paths
            PathUtility.Triangulate(paths, out newVertices, out newIndices, ContourOrientation.CounterClockwise);
            switch (_alphaSpriteMode) {
                case AlphaSpriteMode.Opaque:
                    if (newVertices.Length == 0 || newIndices.Length == 0) {
                        Debug.LogWarningFormat(
                            _texture,
                            "Empty mesh generated for sprite '{0}'. Probably the sprite has no opaque pixels?",
                            _sprite.name
                            );
                    }
                    break;
                case AlphaSpriteMode.NonOpaque:
                    if (newVertices.Length == 0 || newIndices.Length == 0) {
                        Debug.LogWarningFormat(
                            _texture,
                            "Empty mesh generated for sprite '{0}'. Probably the sprite has no transparent pixels?",
                            _sprite.name
                            );
                    }
                    break;
            }
#else
            throw new InvalidEnumArgumentException("Unknown AlphaSpriteMode " + _alphaSpriteMode);
#endif // !SS_ADVANCED_METHODS_DISABLED
        }

        public void ProcessSprite() {
            Vector2[] newVertices;
            ushort[] newIndices;
            GenerateMesh(out newVertices, out newIndices);

            if (newVertices.Length >= 65535) {
                Debug.LogErrorFormat(
                    _texture,
                    "Generated mesh for sprite '{0}' (texture '{1}') has more than 65535 vertices. " +
                    "Please adjust the sprite mesh settings.",
                    _sprite.name,
                    AssetDatabase.GetAssetPath(_texture)
                );
                return;
            }

            // Apply new geometry to the sprite
            _sprite.OverrideGeometry(newVertices, newIndices);
        }

#if !SS_ADVANCED_METHODS_DISABLED
        private Vector2[][] ProcessNormalSprite() {
            Vector2[][] paths = GenerateOutline(_sprite, _spriteTightMeshSettings, _buildTargetGroup);
            paths = ProcessSpritePaths(
                _sprite,
                paths,
                _spriteTightMeshSettings,
                _buildTargetGroup,
                _textureImporterSettings,
                false,
                SkipTextureSpriteExtrude);

            return paths;
        }

        private Vector2[][] ProcessPreciseSprite() {
            List<List<IntPoint>> pathsIntPoint =
                PreciseOutlineGenerator.GenerateOutline(
                    _sprite,
                    _activePlatformSpriteTightMeshSettings.SharedTightMeshSettings.AlphaSourceChannel,
                    _activePlatformSpriteTightMeshSettings.SharedTightMeshSettings.AlphaTolerance,
                    0f
                );

            Vector2[][] paths = pathsIntPoint.ToVector2ArrayArray();

            // Extrude the edges and reduce them with same tolerance
            paths = PathUtility.ExtrudePathsEdges(paths, _activePlatformSpriteTightMeshSettings.PreciseTightMeshSettings.EdgeInflation);

            // Paths start to intersect after extrusion
            pathsIntPoint = paths.ToIntPointListList();
            pathsIntPoint =
                PathUtility.CalculateMergedPathsAccountHoles(
                    pathsIntPoint,
                    PathUtility.CalculateIsPathHole(paths));

            // We can safely reduce the paths with the same tolerance as they were inflated
            for (int i = 0; i < pathsIntPoint.Count; i++) {
                pathsIntPoint[i] =
                    PathReductionUtility.DouglasPeuckerReduction(
                        pathsIntPoint[i],
                        _activePlatformSpriteTightMeshSettings.PreciseTightMeshSettings.EdgeInflation
                    );
            }

            paths = pathsIntPoint.ToVector2ArrayArray();
            PathUtility.ClampPathsToSpriteRect(_sprite, paths);
            return paths;
        }

        private Vector2[][] ProcessOpaqueSprite() {
            SpriteTightMeshSettings opaqueSettings;
            Vector2[][] paths = GenerateRawOpaqueSpritePaths(out opaqueSettings);
            paths = ProcessSpritePaths(_sprite, paths, opaqueSettings, _buildTargetGroup, _textureImporterSettings, true, true);

            return paths;
        }

        private Vector2[][] ProcessNonOpaqueSprite() {
            Vector2[][] opaquePaths = ProcessOpaqueSprite();
            Vector2[][] alphaPaths = GenerateOutline(_sprite, _spriteTightMeshSettings, _buildTargetGroup);
            alphaPaths = ProcessSpritePaths(
                _sprite,
                alphaPaths,
                _spriteTightMeshSettings,
                _buildTargetGroup,
                _textureImporterSettings,
                false,
                SkipTextureSpriteExtrude);

            Vector2[][] cutoutPaths = alphaPaths.Concat(opaquePaths).ToArray();
            return cutoutPaths;
        }

        private Vector2[][] GenerateRawOpaqueSpritePaths(out SpriteTightMeshSettings opaqueSpriteTightMeshSettings) {
            opaqueSpriteTightMeshSettings = new SpriteTightMeshSettings();
            _spriteTightMeshSettings.CopyTo(opaqueSpriteTightMeshSettings);
            opaqueSpriteTightMeshSettings[_buildTargetGroup].UnityMethodTightMeshSettings.VertexMergeDistance =
                opaqueSpriteTightMeshSettings[_buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueVertexMergeDistance;

            // Make sure opaque paths are not post-processed
            opaqueSpriteTightMeshSettings[_buildTargetGroup].SharedTightMeshSettings.AlphaTolerance =
                opaqueSpriteTightMeshSettings[_buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueAlphaTolerance;
            bool reduceAlphaBleed = opaqueSpriteTightMeshSettings[_buildTargetGroup].AlphaSeparationTightMeshSettings.ReduceAlphaBleed;

#if SS_TRACE
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
#endif

            // Generate the opaque outline
            List<List<IntPoint>> pathsInitial = PreciseOutlineGenerator.GenerateOutline(
                _sprite,
                SpriteAlphaSourceChannel.Alpha,
                opaqueSpriteTightMeshSettings[_buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueAlphaTolerance
                );

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("[1] Outline generation took {0} ms", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
#endif

            // Optimize the outline
            List<List<IntPoint>> pathsInitialMerged = pathsInitial.DeepCopy();
            PathReductionUtility.DouglasPeuckerReduction(pathsInitialMerged, 1f);
            Vector2[][] paths = pathsInitialMerged.ToVector2ArrayArray();

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("[2] Outline reduction took {0} ms", sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
#endif

            paths =
                PathUtility.ExtrudePathsEdges(
                    paths,
                    -opaqueSpriteTightMeshSettings[_buildTargetGroup].AlphaSeparationTightMeshSettings.OpaqueNegativeExtrude
                    );

#if SS_TRACE
            sw.Stop();
            Debug.LogFormat("[3] Outline extrusion took {0} ms", sw.ElapsedMilliseconds);
            sw.Reset();
#endif

            // Make sure alpha doesn't bleed into opaque
            if (reduceAlphaBleed) {
#if SS_TRACE
                sw.Start();
#endif
                List<List<IntPoint>> pathsMask = pathsInitial.DeepCopy();
                PathReductionUtility.DouglasPeuckerReduction(pathsMask, 1f);
                pathsMask = PathUtility.ExtrudePathsEdges(pathsMask.ToVector2ArrayArray(), -1f).ToIntPointListList();
                paths = PathUtility.MaskPaths(paths.ToIntPointListList(), pathsMask).ToVector2ArrayArray();

                List<List<IntPoint>> pathsReduced = paths.ToIntPointListList();
                paths = pathsReduced.ToVector2ArrayArray();
#if SS_TRACE
                sw.Stop();
                Debug.LogFormat("[4] Ensuring no bleeding {0} ms", sw.ElapsedMilliseconds);
                sw.Reset();
#endif
            }

            // Remove small garbage paths, as they only introduce unnecessary complexity
            paths = PathUtility.RemoveSmallPaths(paths, kOpaqueSpriteMinPathAreaRatio);

            return paths;
        }

        private static Vector2[][] GenerateOutline(Sprite sprite, SpriteTightMeshSettings spriteTightMeshSettings, BuildTargetGroup buildTargetGroup) {
            SinglePlatformSpriteTightMeshSettings singlePlatformSpriteTightMeshSettings = spriteTightMeshSettings[buildTargetGroup];
            return OutlineGenerator.GenerateOutline(
                sprite,
                CalculateNonLinearDetail(singlePlatformSpriteTightMeshSettings.SharedTightMeshSettings.Detail),
                singlePlatformSpriteTightMeshSettings.SharedTightMeshSettings.AlphaTolerance,
                singlePlatformSpriteTightMeshSettings.UnityMethodTightMeshSettings.DetectHoles
                );
        }

        private static Vector2[][] ProcessSpritePaths(
            Sprite sprite,
            Vector2[][] paths,
            SpriteTightMeshSettings tightMeshSettings,
            BuildTargetGroup buildTargetGroup,
            TextureImporterSettings textureImporterSettings,
            bool skipLocalSpaceTransform,
            bool skipSpriteEdgeExtrude
            ) {
            paths = paths.DeepCopy();

            // Transform vertices from world space to sprite rect local space
            if (!skipLocalSpaceTransform) {
                foreach (Vector2[] path in paths) {
                    for (int i = 0; i < path.Length; i++) {
                        path[i] *= sprite.pixelsPerUnit;
                        path[i] += sprite.pivot;
                    }
                }
            }

            // Apply post-processing to the paths
            paths = ProcessPaths(paths, tightMeshSettings, buildTargetGroup, textureImporterSettings, skipSpriteEdgeExtrude);
            PathUtility.ClampPathsToSpriteRect(sprite, paths);

            return paths;
        }

        private static Vector2[][] ProcessPaths(
            Vector2[][] paths,
            SpriteTightMeshSettings tightMeshSettings,
            BuildTargetGroup buildTargetGroup,
            TextureImporterSettings textureImporterSettings,
            bool skipSpriteEdgeExtrude
            ) {
            uint spriteExtrude = GetTextureImporterSpriteExtrude(textureImporterSettings);
            // Hole detection. Polygon is a hole if it is entirely inside other polygon
            bool[] isPathHole = null;
            if (!skipSpriteEdgeExtrude && spriteExtrude> 0) {
                isPathHole = PathUtility.CalculateIsPathHole(paths);
            }

            // Optimize paths
            for (int i = 0; i < paths.Length; i++) {
                paths[i] =
                    PathReductionUtility.DouglasPeuckerReduction(
                        paths[i],
                        tightMeshSettings[buildTargetGroup].UnityMethodTightMeshSettings.VertexMergeDistance
                        );
            }

            // Extrude edges. Hole paths must be extruded in opposite direction
            if (!skipSpriteEdgeExtrude && spriteExtrude > 0) {
                paths = PathUtility.ExtrudePathsEdges(paths, isPathHole, spriteExtrude);

                // Re-optimize paths
                for (int i = 0; i < paths.Length; i++) {
                    paths[i] =
                        PathReductionUtility.DouglasPeuckerReduction(
                            paths[i],
                            tightMeshSettings[buildTargetGroup].UnityMethodTightMeshSettings.VertexMergeDistance
                            );
                }
            }

            return paths;
        }

        private static uint GetTextureImporterSpriteExtrude(TextureImporterSettings textureImporterSettings) {
            return textureImporterSettings != null ? textureImporterSettings.spriteExtrude : 1;
        }

        private static float CalculateNonLinearDetail(float detail) {
            return Mathf.Pow(detail, 2.5f);
        }

#endif // !SS_ADVANCED_METHODS_DISABLED

        public enum AlphaSpriteMode {
#if !SS_ADVANCED_METHODS_DISABLED
            Normal = 0,
            Opaque = 1,
            NonOpaque = 2,
            Precise = 3,
#endif // !SS_ADVANCED_METHODS_DISABLED
            RectGrid = 4
        }

        [Flags]
        public enum ProcessingOptions {
            None = 0,
            SkipTextureSpriteExtrude = 1 << 0
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing {
    public class SpriteRectMeshGenerator {
        private readonly Texture2D _texture;
        private readonly TextureImporterSettings _textureImporterSettings;
        private readonly SharedTightMeshSettings _sharedTightMeshSettings;
        private readonly RectGridTightMeshSettings _rectGridTightMeshSettings;
        private readonly ProcessingOptions _processingOptions;
        private Color32[] _texturePixels;
        private IntRect _rect;

        private bool SkipTextureSpriteExtrude {
            get { return (_processingOptions & ProcessingOptions.SkipTextureSpriteExtrude) != 0; }
        }

        public Color32[] TexturePixels {
            get { return _texturePixels; }
        }

        public SpriteRectMeshGenerator(
            Texture2D texture,
            IntRect rect,
            SharedTightMeshSettings sharedTightMeshSettings,
            RectGridTightMeshSettings rectGridTightMeshSettings,
            ProcessingOptions processingOptions
            ) {
            _texture = texture;
            _rect = rect;
            _textureImporterSettings = texture.GetTextureImporterSettings();
            _processingOptions = processingOptions;

            _sharedTightMeshSettings = sharedTightMeshSettings;
            _rectGridTightMeshSettings = rectGridTightMeshSettings;
        }

        public void GenerateRectGridSpriteMesh(out Vector2[] vertices, out ushort[] indices) {
            int xSubdivisions = _rectGridTightMeshSettings.XSubdivisions;
            int ySubdivisions = _rectGridTightMeshSettings.YSubdivisions;

            IntRect spriteRect;
            IntRect spriteRectLocal;
            List<RectGridCell> cells;
            CalculateRectGrid(xSubdivisions, ySubdivisions, out spriteRect, out spriteRectLocal, out cells);

            GenerateRectGridSpriteMesh(
                _texture,
                ref _texturePixels,
                spriteRect,
                spriteRectLocal,
                cells,
                out vertices,
                out indices);

            int spriteExtrudePixels = 0;
            if (_textureImporterSettings != null && !SkipTextureSpriteExtrude) {
                spriteExtrudePixels += (int) _textureImporterSettings.spriteExtrude;
            }
            spriteExtrudePixels += _rectGridTightMeshSettings.ScaleAroundCenter;

            // Scale around center
            if (spriteExtrudePixels > 0) {
                Vector2 meshScale =
                    new Vector2(
                        1f + spriteExtrudePixels / (float) spriteRectLocal.width * 2f,
                        1f + spriteExtrudePixels / (float) spriteRectLocal.height * 2f
                        );

                Vector2 center = spriteRectLocal.center;
                for (int i = 0; i < vertices.Length; i++) {
                    Vector2 vertex = vertices[i];
                    vertex.x = (vertex.x - center.x) * meshScale.x + center.x;
                    vertex.y = (vertex.y - center.y) * meshScale.y + center.y;
                    vertices[i] = vertex;
                }
            }

            PathUtility.ClampVerticesToRect((Rect) _rect, vertices);
        }

        public void CalculateRectGrid(
            int xSubdivisions,
            int ySubdivisions,
            out IntRect spriteRect,
            out IntRect spriteRectLocal,
            out List<RectGridCell> cells) {
            if (_rectGridTightMeshSettings.CullByBoundingBox) {
                IntRect? spriteBoundingIntRect =
                    SpriteBoundingBoxCalculator.CalculateBoundingRect(
                        _texture,
                        _rect,
                        _sharedTightMeshSettings.AlphaSourceChannel,
                        _sharedTightMeshSettings.AlphaTolerance,
                        ref _texturePixels);

                spriteRect = spriteBoundingIntRect.GetValueOrDefault();
                spriteRect.width++;
                spriteRect.height++;

                spriteRectLocal = spriteRect;
                spriteRectLocal.x -= _rect.xMin;
                spriteRectLocal.y -= _rect.yMin;
            } else {
                spriteRect = _rect;
                spriteRectLocal = spriteRect;
                spriteRectLocal.x = 0;
                spriteRectLocal.y = 0;
            }

            if (_texturePixels == null) {
                _texturePixels = _texture.GetPixels32Reliable();
            }

            cells = CalculateTextureRectGridCells(
                _texturePixels,
                _texture.width,
                spriteRect,
                xSubdivisions,
                ySubdivisions,
                !_rectGridTightMeshSettings.RemoveEmptyCells,
                _sharedTightMeshSettings.AlphaSourceChannel,
                _sharedTightMeshSettings.AlphaTolerance
            );
        }

        private static void GenerateRectGridSpriteMesh(
            Texture2D texture,
            ref Color32[] texturePixels,
            IntRect spriteRect,
            IntRect spriteRectLocal,
            List<RectGridCell> cells,
            out Vector2[] vertices,
            out ushort[] indices) {
            if (texturePixels == null) {
                texturePixels = texture.GetPixels32Reliable();
            }

            // Two triangles per cell
            int numIndices = cells.Count * 2 * 3;

            // 4 vertices per cell (before vertex layout optimization)
            int numVertices = cells.Count * 4;

            vertices = new Vector2[numVertices];
            indices = new ushort[numIndices];

            // Generate vertices
            for (int i = 0; i < cells.Count; i++) {
                RectGridCell cell = cells[i];
                Rect cellRect = (Rect) cell.Rect;
                vertices[i * 4 + 0] = new Vector2(cellRect.x, cellRect.y);
                vertices[i * 4 + 1] = new Vector2(cellRect.x, cellRect.y + cellRect.height);
                vertices[i * 4 + 2] = new Vector2(cellRect.x + cellRect.width, cellRect.y);
                vertices[i * 4 + 3] = new Vector2(cellRect.x + cellRect.width, cellRect.y + cellRect.height);
            }

            for (int i = 0; i < vertices.Length; i++) {
                vertices[i].x -= spriteRect.xMin - spriteRectLocal.xMin;
                vertices[i].y -= spriteRect.yMin - spriteRectLocal.yMin;
            }

            // Generate indices
            int k = 0;
            for (int i = 0; i < cells.Count; i++) {
                indices[k + 0] = (ushort)(i * 4 + 0);
                indices[k + 1] = (ushort)(i * 4 + 1);
                indices[k + 2] = (ushort)(i * 4 + 2);

                indices[k + 3] = (ushort)(i * 4 + 1);
                indices[k + 4] = (ushort)(i * 4 + 3);
                indices[k + 5] = (ushort)(i * 4 + 2);

                k += 6;
            }

           SpriteMeshUtility.OptimizeMeshVertexLayout(ref vertices, ref indices);
        }

        private static List<RectGridCell> CalculateTextureRectGridCells(
            Color32[] texturePixels,
            int textureWidth,
            IntRect rect,
            int xSubdivisions,
            int ySubdivisions,
            bool includeEmptyCells,
            SpriteAlphaSourceChannel alphaSourceChannel,
            byte alphaTolerance)
            {
            if (xSubdivisions < 1)
                throw new ArgumentOutOfRangeException("xSubdivisions");

            if (ySubdivisions < 1)
                throw new ArgumentOutOfRangeException("ySubdivisions");

            Vector2 cellSize;
            cellSize.x = rect.width / (float) xSubdivisions;
            cellSize.y = rect.height / (float) ySubdivisions;

            List<RectGridCell> cells = new List<RectGridCell>();

            if (xSubdivisions == 1 && ySubdivisions == 1) {
                cells.Add(new RectGridCell(new IntVector2(0, 0), rect));
            } else {
                float yMin = rect.yMin;
                float yMax = rect.yMin;
                for (int j = 0; j < ySubdivisions; j++) {
                    float xMin = rect.xMin;
                    float xMax = rect.xMin;
                    yMax += cellSize.y;
                    for (int i = 0; i < xSubdivisions; i++) {
                        xMax += cellSize.x;

                        int xMinInt = Mathf.FloorToInt(xMin);
                        int xMaxInt = i != xSubdivisions - 1 ? Mathf.FloorToInt(xMax) : rect.xMax;
                        int widthInt = xMaxInt - xMinInt;

                        int yMinInt = Mathf.FloorToInt(yMin);
                        int yMaxInt = j != ySubdivisions - 1 ? Mathf.FloorToInt(yMax) : rect.yMax;
                        int heighInt = yMaxInt - yMinInt;

                        // Skip empty cells
                        if (widthInt == 0 || heighInt == 0) {
                            xMin = xMax;
                            continue;
                        }

                        IntRect cellRect = new IntRect(xMinInt, yMinInt, widthInt, heighInt);

                        if (!includeEmptyCells) {
                            IntRect? gridBoundingRect =
                                SpriteBoundingBoxCalculator.CalculateBoundingRect(
                                    texturePixels,
                                    textureWidth,
                                    cellRect,
                                    alphaSourceChannel,
                                    alphaTolerance
                                );

                            if (!gridBoundingRect.HasValue) {
                                xMin = xMax;
                                continue;
                            }
                        }

                        cells.Add(new RectGridCell(new IntVector2(i, j), cellRect));
                        xMin = xMax;
                    }

                    yMin = yMax;
                }
            }

            return cells;
        }

        [Flags]
        public enum ProcessingOptions {
            None = 0,
            SkipTextureSpriteExtrude = 1 << 0
        }

        public struct RectGridCell {
            public readonly IntVector2 Index;
            public readonly IntRect Rect;

            public RectGridCell(IntVector2 index, IntRect rect) {
                Index = index;
                Rect = rect;
            }

            public override string ToString() {
                return String.Format("Index: {0}, Rect: {1}", Index, Rect);
            }
        }
    }
}

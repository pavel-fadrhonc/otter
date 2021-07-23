using System;
using System.Collections.Generic;
using UnityEngine;
using LostPolygon.SpriteSharp.ClipperLib;
using LostPolygon.SpriteSharp.LibTessDotNet;
using LostPolygon.SpriteSharp.Utility;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// Path processing functions.
    /// </summary>
    public static class PathUtility {
        public static bool[] CalculateIsPathHole(Vector2[][] paths) {
            int pathsLength = paths.Length;
            Rect[] boundingRects = new Rect[pathsLength];
            for (int i = 0; i < pathsLength; i++) {
                boundingRects[i] = CalculateBoundingRect(paths[i]);
            }

            bool[] isPathHole = new bool[pathsLength];
            for (int i = 0; i < pathsLength; i++) {
                isPathHole[i] = false;
                for (int j = 0; j < pathsLength; j++) {
                    if (i == j)
                        continue;

                    if (!MathUtility.IsPolygonInsidePolygon(paths[i], boundingRects[i], paths[j], boundingRects[j]))
                        continue;

                    isPathHole[i] = true;
                    break;
                }
            }

            return isPathHole;
        }

        public static Rect CalculateBoundingRect(Vector2[] path) {
            Vector2 min, max;
            min.x = Single.MaxValue;
            min.y = Single.MaxValue;
            max.x = Single.MinValue;
            max.y = Single.MinValue;

            for (int i = 0, n = path.Length; i < n; i++) {
                Vector2 vertex = path[i];
                if (vertex.x < min.x)
                    min.x = vertex.x;
                else if (vertex.x > max.x)
                    max.x = vertex.x;

                if (vertex.y < min.y)
                    min.y = vertex.y;
                else if (vertex.y > max.y)
                    max.y = vertex.y;
            }

            if (min.x == max.x) {
                min.x = 0f;
                max.x = 0f;
            }

            if (min.y == max.y) {
                min.y = 0f;
                max.y = 0f;
            }

            Rect boundingRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            return boundingRect;
        }

        /// <summary>
        /// Correctly accounts for holes.
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="isPathHole"></param>
        /// <returns></returns>
        public static List<List<IntPoint>> CalculateMergedPathsAccountHoles(List<List<IntPoint>> paths, bool[] isPathHole) {
            List<List<IntPoint>> mergedPaths = new List<List<IntPoint>>(paths.Count);
            List<List<IntPoint>> holePaths = new List<List<IntPoint>>(paths.Count);
            List<List<IntPoint>> nonHolePaths = new List<List<IntPoint>>(paths.Count);

            for (int i = 0; i < paths.Count; i++) {
                List<IntPoint> path = paths[i];
                if (isPathHole[i]) {
                    holePaths.Add(path);
                } else {
                    nonHolePaths.Add(path);
                }
            }

            holePaths = CalculateMergedPaths(holePaths);
            nonHolePaths = CalculateMergedPaths(nonHolePaths);

            mergedPaths.AddRange(nonHolePaths);
            mergedPaths.AddRange(holePaths);

            mergedPaths.TrimExcess();

            return mergedPaths;
        }

        public static List<List<IntPoint>> CalculateMergedPaths(List<List<IntPoint>> paths) {
            List<List<IntPoint>> mergedPaths = new List<List<IntPoint>>(paths.Count);
            Clipper clipper = new Clipper();
            clipper.AddPaths(paths, PolyType.ptSubject, true);
            clipper.Execute(ClipType.ctUnion, mergedPaths, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            mergedPaths.TrimExcess();

            return mergedPaths;
        }

        public static Vector2[][] ExtrudePathEdges(Vector2[] path, float delta) {
            const float miterLimit = 2f;
            const float arcTolerance = 0.25f;
            ClipperOffset offsetter = new ClipperOffset(miterLimit, arcTolerance);

            List<IntPoint> pathIntPoints = path.ToIntPointList();

            offsetter.AddPath(pathIntPoints, JoinType.jtSquare, EndType.etClosedPolygon);

            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            offsetter.Execute(ref solution, delta);

            return solution.ToVector2ArrayArray();
        }

        public static Vector2[][] ExtrudePathsEdges(Vector2[][] paths, float delta) {
            if (Mathf.Abs(delta) < Vector2.kEpsilon)
                return paths;

            bool[] isPathHole = CalculateIsPathHole(paths);
            paths = ExtrudePathsEdges(paths, isPathHole, delta);

            return paths;
        }

        public static Vector2[][] ExtrudePathsEdges(Vector2[][] paths, bool[] isPathHole, float delta) {
            List<Vector2[]> tempPaths = new List<Vector2[]>();
            for (int i = 0; i < paths.Length; i++) {
                Vector2[][] extrusionPaths =
                    ExtrudePathEdges(
                        paths[i],
                        (isPathHole[i] ? -1 : 1) * delta
                    );

                for (int j = 0; j < extrusionPaths.Length; j++) {
                    tempPaths.Add(extrusionPaths[j]);
                }
            }

            paths = tempPaths.ToArray();

            return paths;
        }

        public static List<List<IntPoint>> MaskPaths(List<List<IntPoint>> paths, List<List<IntPoint>> maskPaths) {
            List<List<IntPoint>> mergedPaths = new List<List<IntPoint>>();
            Clipper clipper = new Clipper();
            clipper.AddPaths(paths, PolyType.ptSubject, true);
            clipper.AddPaths(maskPaths, PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, mergedPaths, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            mergedPaths.TrimExcess();

            return mergedPaths;
        }

        public static void Triangulate(
            Tess tess,
            out Vector2[] outVertices,
            out ushort[] outIndices
            ) {
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            int actualElementsCount = tess.ElementCount * 3;
            outIndices = new ushort[actualElementsCount];
            for (int i = 0; i < actualElementsCount; i++) {
                outIndices[i] = (ushort) tess.Elements[i];
            }

            ContourVertex[] contourVertices = tess.Vertices;
            outVertices = contourVertices.ToVector2Array(tess.VertexCount);
        }

        public static void Triangulate(
            Vector2[][] paths,
            out Vector2[] outVertices,
            out ushort[] outIndices,
            ContourOrientation contourOrientation = ContourOrientation.Original
            ) {
            Tess tess = new Tess();
            for (int i = 0; i < paths.Length; i++) {
                Vector2[] path = paths[i];
                tess.AddContour(path.ToContourVertexArray(), contourOrientation);
            }

            Triangulate(tess, out outVertices, out outIndices);
        }


        public static void ClampVerticesToRect(Rect rect, Vector2[] vertices) {
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i].x = Mathf.Clamp(vertices[i].x, 0f, rect.width);
                vertices[i].y = Mathf.Clamp(vertices[i].y, 0f, rect.height);
            }
        }

        public static void ClampPathsToSpriteRect(Sprite sprite, Vector2[][] paths) {
            // Clamp vertices to sprite rect
            Rect spriteRect = sprite.rect;
            foreach (Vector2[] path in paths) {
                ClampVerticesToRect(spriteRect, path);
            }
        }

        public static Vector2[][] RemoveSmallPaths(Vector2[][] paths, float minRatio) {
            bool[] isPathHole = CalculateIsPathHole(paths);
            float maxArea = Single.MinValue;
            float[] areas = new float[paths.Length];
            for (int i = 0; i < paths.Length; i++) {
                //Rect boundingRect = PathUtilities.CalculateBoundingRect(paths[i]);
                areas[i] = MathUtility.CalculatePolygonArea(paths[i]);
                if (areas[i] > maxArea) {
                    maxArea = areas[i];
                }
            }

            List<Vector2[]> cleanedPaths = new List<Vector2[]>();
            for (int i = 0; i < paths.Length; i++) {
                float ratio = areas[i] / maxArea;
                if (isPathHole[i] || ratio > minRatio) {
                    cleanedPaths.Add(paths[i]);
                }
            }

            return cleanedPaths.ToArray();
        }

        public static List<List<IntPoint>> DeepCopy(this List<List<IntPoint>> paths) {
            List<List<IntPoint>> pathsCopy = new List<List<IntPoint>>(paths.Count);
            for (int i = 0, n1 = paths.Count; i < n1; i++) {
                List<IntPoint> src = paths[i];
                List<IntPoint> dst = src.GetRange(0, src.Count);

                pathsCopy.Add(dst);
            }

            return pathsCopy;
        }

        public static Vector2[][] DeepCopy(this Vector2[][] paths) {
            Vector2[][] pathsCopy = new Vector2[paths.Length][];
            for (int i = 0; i < pathsCopy.Length; i++) {
                pathsCopy[i] = (Vector2[]) paths[i].Clone();
            }

            paths = pathsCopy;
            return paths;
        }

        public static Vector2[][] ToVector2ArrayArray(this List<List<IntPoint>> paths) {
            int pathCount = paths.Count;

            Vector2[][] outPaths = new Vector2[pathCount][];
            for (int i = 0; i < pathCount; i++) {
                outPaths[i] = paths[i].ToVector2Array();
            }

            return outPaths;
        }

        public static Vector2[] ToVector2Array(this List<IntPoint> vertices, int vertexCount = -1) {
            if (vertexCount == -1) {
                vertexCount = vertices.Count;
            }

            Vector2[] points = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++) {
                IntPoint intPoint = vertices[i];
                Vector2 point;
                point.x = intPoint.X;
                point.y = intPoint.Y;
                points[i] = point;
            }

            return points;
        }

        public static Vector2[] ToVector2Array(this ContourVertex[] vertices, int vertexCount = -1) {
            if (vertexCount == -1) {
                vertexCount = vertices.Length;
            }

            Vector2[] points = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++) {
                Vec3 contourVertexPoint = vertices[i].Position;
                Vector2 point;
                point.x = contourVertexPoint.X;
                point.y = contourVertexPoint.Y;
                points[i] = point;
            }

            return points;
        }

        public static List<List<IntPoint>> ToIntPointListList(this Vector2[][] paths) {
            int pathCount = paths.Length;

            List<List<IntPoint>> outPaths = new List<List<IntPoint>>(pathCount);
            for (int i = 0; i < pathCount; i++) {
                outPaths.Add(paths[i].ToIntPointList());
            }

            return outPaths;
        }

        public static List<IntPoint> ToIntPointList(this Vector2[] vertices, int vertexCount = -1) {
            if (vertexCount == -1) {
                vertexCount = vertices.Length;
            }

            List<IntPoint> points = new List<IntPoint>(vertexCount);
            for (int i = 0; i < vertexCount; i++) {
                Vector2 point = vertices[i];
                IntPoint intPoint;
                intPoint.X = Mathf.RoundToInt(point.x);
                intPoint.Y = Mathf.RoundToInt(point.y);
                points.Add(intPoint);
            }

            return points;
        }

        public static ContourVertex[] ToContourVertexArray(this Vector2[] vertices, int vertexCount = -1) {
            if (vertexCount == -1) {
                vertexCount = vertices.Length;
            }

            ContourVertex[] countourVertices = new ContourVertex[vertexCount];
            Vec3 contourVertexPoint;
            contourVertexPoint.Z = 0f;
            for (int i = 0; i < vertexCount; i++) {
                Vector2 point = vertices[i];
                contourVertexPoint.X = point.x;
                contourVertexPoint.Y = point.y;
                countourVertices[i].Position = contourVertexPoint;
            }

            return countourVertices;
        }
    }
}
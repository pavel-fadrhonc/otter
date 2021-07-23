using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Various math and geometry functions.
    /// </summary>
    public static class MathUtility {
        /// <summary>
        /// Calculate the line segment normal.
        /// </summary>
        public static Vector2 CalculateLineSegmentNormal(Vector2 start, Vector2 end) {
            float dx = end.x - start.x;
            float dy = end.y - start.y;
            Vector2 normal = new Vector2(-dy, dx).normalized;

            return normal;
        }

        /// <summary>
        /// Checks whether two line segments intersect each other.
        /// </summary>
        public static bool AreLineSegmentsIntersecting(
            Vector2 line1Start,
            Vector2 line1End,
            Vector2 line2Start,
            Vector2 line2End
            ) {
            Vector2 diffLine1;
            diffLine1.x = line1End.x - line1Start.x;
            diffLine1.y = line1End.y - line1Start.y;
            Vector2 diffLine2;
            diffLine2.x = line2End.x - line2Start.x;
            diffLine2.y = line2End.y - line2Start.y;
            Vector2 diffLineStarts;
            diffLineStarts.x = line1Start.x - line2Start.x;
            diffLineStarts.y = line1Start.y - line2Start.y;

            float normCoeff = 1f / (-diffLine2.x * diffLine1.y + diffLine1.x * diffLine2.y);
            float s = (-diffLine1.y * diffLineStarts.x + diffLine1.x * diffLineStarts.y) * normCoeff;
            float t = (diffLine2.x * diffLineStarts.y - diffLine2.y * diffLineStarts.x) * normCoeff;

            return s > 0f && s < 1f && t > 0f && t < 1f;
        }

        /// <summary>
        /// Сalculates the area of triangle.
        /// </summary>
        public static float CalculateTriangleArea(Vector2 point1, Vector2 point2, Vector2 point3) {
            float doubleSignedArea =
                (point2.x - point1.x) * (point3.y - point1.y) -
                (point3.x - point1.x) * (point2.y - point1.y);

            float area = Mathf.Abs(doubleSignedArea) * 0.5f;

            return area;
        }

        /// <summary>
        /// Сalculates the area of polygon.
        /// </summary>
        public static float CalculatePolygonArea(Vector2[] polygon) {
            int polygonLength = polygon.Length;
            if (polygonLength < 3)
                return 0f;

            float doubleSignedArea = 0f;
            for (int i = 0, j = polygonLength - 1; i < polygonLength; ++i) {
                doubleSignedArea += (polygon[j].x + polygon[i].x) * (polygon[j].y - polygon[i].y);
                j = i;
            }

            float area = Mathf.Abs(doubleSignedArea) * 0.5f;
            return area;
        }

        /*
        Copyright 2000 softSurfer, 2012 Dan Sunday
        This code may be freely used and modified for any purpose
        providing that this copyright notice is included with it.
        SoftSurfer makes no warranty for this code, and cannot be held
        liable for any real or imagined damage resulting from its use.
        Users of this code must verify correctness for their application.

        cn_PnPoly(): crossing number test for a point in a polygon
             Input:   P = a point,
                      V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
             Return:  0 = outside, 1 = inside
        This code is patterned after [Franklin, 2000]
        */
        public static bool IsPointInsidePolygon(Vector2 point, Vector2[] polygon) {
            int crossingNumber = 0; // the  crossing number counter

            // Loop through all edges of the polygon
            for (int i = 0, n = polygon.Length; i < n; i++) {
                Vector2 next = i == n - 1 ? polygon[0] : polygon[i + 1];

                // Edge from V[i]  to V[i+1]
                if ((!(polygon[i].y <= point.y) || !(next.y > point.y)) &&
                    (!(polygon[i].y > point.y) || !(next.y <= point.y)))
                    continue;

                // Compute  the actual edge-ray intersect x-coordinate
                float vt = (point.y - polygon[i].y) / (next.y - polygon[i].y);
                if (point.x < polygon[i].x + vt * (next.x - polygon[i].x)) // P.x < intersect
                    ++crossingNumber; // a valid crossing of y=P.y right of P.x
            }

            return (crossingNumber & 1) == 1; // 0 if even (out), and 1 if  odd (in)
        }

        public static bool IsPolygonInsidePolygon(
            Vector2[] innerPolygon,
            Rect innerPolygonRect,
            Vector2[] outerPolygon,
            Rect outerPolygonRect
            ) {
            if (!AreRectsIntersecting(innerPolygonRect, outerPolygonRect))
                return IsPointInsidePolygon(innerPolygon[0], outerPolygon);

            return IsPolygonInsidePolygon(innerPolygon, outerPolygon);
        }

        public static bool IsPolygonInsidePolygon(Vector2[] innerPolygon, Vector2[] outerPolygon) {
            for (int i = 0, n1 = innerPolygon.Length; i < n1; i++) {
                Vector2 nextInner = i == n1 - 1 ? innerPolygon[0] : innerPolygon[i + 1];
                for (int j = 0, n2 = outerPolygon.Length; j < n2; j++) {
                    Vector2 nextOuter = j == n2 - 1 ? outerPolygon[0] : outerPolygon[j + 1];

                    if (AreLineSegmentsIntersecting(innerPolygon[i], nextInner, outerPolygon[j], nextOuter))
                        return false;
                }
            }

            return IsPointInsidePolygon(innerPolygon[0], outerPolygon);
        }

        public static bool AreRectsIntersecting(Rect a, Rect b) {
            return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
        }

        public static float CalculateMeshArea(ushort[] triangles, Vector2[] vertices) {
            float square = 0f;
            for (int i = 0; i < triangles.Length; i += 3) {
                square +=
                    CalculateTriangleArea(
                        vertices[triangles[i]],
                        vertices[triangles[i + 1]],
                        vertices[triangles[i + 2]]
                    );
            }

            return square;
        }

        public static float CalculateMeshAreaAndBoundingBox(
            ushort[] triangles,
            Vector2[] vertices,
            out Vector2 min,
            out Vector2 max
            ) {
            min = new Vector2(float.MaxValue, float.MaxValue);
            max = new Vector2(float.MinValue, float.MinValue);
            float area = 0f;
            for (int i = 0; i < triangles.Length; i += 3) {
                Vector2 vertex1 = vertices[triangles[i]];
                Vector2 vertex2 = vertices[triangles[i + 1]];
                Vector2 vertex3 = vertices[triangles[i + 2]];
                area +=
                    CalculateTriangleArea(
                        vertex1,
                        vertex2,
                        vertex3
                    );

                if (vertex1.x < min.x)
                    min.x = vertex1.x;
                if (vertex2.x < min.x)
                    min.x = vertex2.x;
                if (vertex3.x < min.x)
                    min.x = vertex3.x;
                if (vertex1.x > max.x)
                    max.x = vertex1.x;
                if (vertex2.x > max.x)
                    max.x = vertex2.x;
                if (vertex3.x > max.x)
                    max.x = vertex3.x;

                if (vertex1.y < min.y)
                    min.y = vertex1.y;
                if (vertex2.y < min.y)
                    min.y = vertex2.y;
                if (vertex3.y < min.y)
                    min.y = vertex3.y;
                if (vertex1.y > max.y)
                    max.y = vertex1.y;
                if (vertex2.y > max.y)
                    max.y = vertex2.y;
                if (vertex3.y > max.y)
                    max.y = vertex3.y;
            }

            return area;
        }
    }
}
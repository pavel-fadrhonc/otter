using System.Collections.Generic;
using UnityEngine;
using LostPolygon.SpriteSharp.ClipperLib;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// Function for reducing the number of points in curves approximated by a series of points.
    /// </summary>
    public static class PathReductionUtility {
        public static void DouglasPeuckerReduction(List<List<IntPoint>> paths, float tolerance) {
            for (int i = 0; i < paths.Count; i++) {
                paths[i] = DouglasPeuckerReduction(paths[i], tolerance);
            }
        }

        public static List<IntPoint> DouglasPeuckerReduction(List<IntPoint> points, float tolerance, int minPoints = 4) {
            if (tolerance <= Vector2.kEpsilon)
                return points;

            if (points.Count < minPoints)
                return points;

            int firstPoint = 0;
            int lastPoint = points.Count - 1;

            // Add the first and last index to the keepers
            List<int> pointIndexesToKeep = new List<int> { firstPoint, lastPoint };

            // The first and the last point can not be the same
            while (points[firstPoint] == points[lastPoint]) {
                lastPoint--;
            }

            DouglasPeuckerReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexesToKeep);

            List<IntPoint> returnPoints = new List<IntPoint>();
            pointIndexesToKeep.Sort();
            for (int i = 0, n = pointIndexesToKeep.Count; i < n; i++) {
                returnPoints.Add(points[pointIndexesToKeep[i]]);
            }

            return returnPoints;
        }

        private static void DouglasPeuckerReduction(
            List<IntPoint> points,
            int firstPoint,
            int lastPoint,
            float tolerance,
            ref List<int> pointIndexesToKeep) {
            float maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++) {
                float distance =
                    PerpendicularDistance(
                        points[firstPoint].X,
                        points[firstPoint].Y,
                        points[lastPoint].X,
                        points[lastPoint].Y,
                        points[index].X,
                        points[index].Y
                        );
                if (distance > maxDistance) {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0) {
                // Add the largest point that exceeds the tolerance
                pointIndexesToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexesToKeep);
                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexesToKeep);
            }
        }

        public static Vector2[] DouglasPeuckerReduction(Vector2[] points, float tolerance) {
            if (points.Length < 3 || tolerance < Vector3.kEpsilon)
                return points;

            int firstPoint = 0;
            int lastPoint = points.Length - 1;
            List<int> pointIndexesToKeep = new List<int>();

            // Add the first and last index to the keepers
            pointIndexesToKeep.Add(firstPoint);
            pointIndexesToKeep.Add(lastPoint);

            // The first and the last point can not be the same
            while (points[firstPoint] == points[lastPoint]) {
                lastPoint--;
            }

            DouglasPeuckerReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexesToKeep);

            Vector2[] returnPoints = new Vector2[pointIndexesToKeep.Count];
            pointIndexesToKeep.Sort();
            for (int i = 0, n = pointIndexesToKeep.Count; i < n; i++) {
                returnPoints[i] = points[pointIndexesToKeep[i]];
            }

            return returnPoints;
        }

        private static void DouglasPeuckerReduction(
            Vector2[] points,
            int firstPoint,
            int lastPoint,
            float tolerance,
            ref List<int> pointIndexesToKeep) {
            float maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++) {
                float distance =
                    PerpendicularDistance(
                        points[firstPoint].x,
                        points[firstPoint].y,
                        points[lastPoint].x,
                        points[lastPoint].y,
                        points[index].x,
                        points[index].y
                        );
                if (distance > maxDistance) {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0) {
                // Add the largest point that exceeds the tolerance
                pointIndexesToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexesToKeep);
                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexesToKeep);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        private static float PerpendicularDistance(
            float point1X,
            float point1Y,
            float point2X,
            float point2Y,
            float point3X,
            float point3Y
            ) {
            // Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            // Base = √((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            // Area = .5*Base*H                                          *Solve for height
            // Height = Area/.5/Base

            float area =
                Mathf.Abs(0.5f * (
                point1X * point2Y +
                point2X * point3Y +
                point3X * point1Y -
                point2X * point1Y -
                point3X * point2Y -
                point1X * point3Y));

            float tempDiffX = point1X - point2X;
            float tempDiffY = point1Y - point2Y;
            float bottom = Mathf.Sqrt(tempDiffX * tempDiffX + tempDiffY * tempDiffY);
            float height = area / bottom * 2f;

            return height;
        }
    }
}

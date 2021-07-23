using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Various mesh functions.
    /// </summary>
    public static class SpriteMeshUtility {
        public static void OptimizeMeshVertexLayout(ref Vector2[] vertices, ref ushort[] indices) {
            // Create array of only unique vertices
            Vector2[] uniqueVertices = new HashSet<Vector2>(vertices, Vector2EqualityComparer.Comparer).ToArray();

            for (int i = 0; i < indices.Length; i++) {
                Vector2 searchedVertex = vertices[indices[i]];
                for (int j = 0; j < uniqueVertices.Length; j++) {
                    Vector2 currentVertex = uniqueVertices[j];
                    if (searchedVertex.x == currentVertex.x && searchedVertex.y == currentVertex.y) {
                        indices[i] = (ushort) j;
                        break;
                    }
                }
            }

            vertices = uniqueVertices;
        }

        public static void OptimizeMeshIndexLayout(ref Vector2[] vertices, ref ushort[] indices) {
            // Vertex layout optimization
            // Initialize old-to-new index map with -1
            int[] indexMap = new int[vertices.Length];
            for (int i = 0; i < indexMap.Length; i++)
                indexMap[i] = -1;

            // Fill old-to-new index map
            int index = 0;
            for (int i = 0; i < indices.Length; i++) {
                if (indexMap[indices[i]] == -1)
                    indexMap[indices[i]] = index++;
            }

            // Allocate memory for optimized layout
            Vector2[] newVertices = new Vector2[index];

            // Fill attributes
            for (int i = 0; i < indexMap.Length; i++) {
                index = indexMap[i];
                if (index != -1) {
                    newVertices[index] = vertices[i];
                }
            }

            // Remap indices
            for (int i = 0; i < indices.Length; i++)
                indices[i] = (ushort) indexMap[indices[i]];

            vertices = newVertices;
        }

        private class Vector2EqualityComparer : IEqualityComparer<Vector2> {
            public static readonly Vector2EqualityComparer Comparer = new Vector2EqualityComparer();

            public bool Equals(Vector2 a, Vector2 b) {
                return a.x == b.x && a.y == b.y;
            }

            public int GetHashCode(Vector2 obj) {
                return obj.GetHashCode();
            }
        }
    }
}
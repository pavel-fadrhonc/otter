using LostPolygon.SpriteSharp.Processing;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// <see cref="Rect"/> helper extensions.
    /// </summary>
    public static class RectExtensions {
        public static IntRect ToIntRectWithPositiveBias(this Rect rect) {
            return new IntRect(
                Mathf.FloorToInt(rect.xMin),
                Mathf.FloorToInt(rect.yMin),
                Mathf.CeilToInt(rect.width),
                Mathf.CeilToInt(rect.height)
            );
        }
    }
}
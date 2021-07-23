using UnityEngine;
using LostPolygon.SpriteSharp.ClipperLib;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    ///     <see cref="IntPointExtensions" /> helper extensions.
    /// </summary>
    internal static class IntPointExtensions {
        public static Vector2 ToVector2(this IntPoint point) {
            return new Vector2(point.X, point.Y);
        }

        public static IntPoint ToIntPoint(this Vector2 point) {
            return new IntPoint(point.x, point.y);
        }
    }
}

using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Processing {
    public struct IntVector2 {
        public int x, y;

        public IntVector2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool Equals(IntVector2 other) {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is IntVector2 && Equals((IntVector2) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (x * 397) ^ y;
            }
        }

        public static bool operator ==(IntVector2 left, IntVector2 right) {
            return left.Equals(right);
        }

        public static bool operator !=(IntVector2 left, IntVector2 right) {
            return !left.Equals(right);
        }

        public static explicit operator IntVector2(Vector2 val) {
            return new IntVector2((int) val.x, (int) val.y);
        }

        public static explicit operator Vector2(IntVector2 val) {
            return new Vector2(val.x, val.y);
        }

        public override string ToString() {
            return String.Format("({0}, {1})", x, y);
        }
    }
}
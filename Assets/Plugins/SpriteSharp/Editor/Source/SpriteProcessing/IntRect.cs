using System;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    ///     <para>A 2D Rectangle defined by x, y position and width, height.</para>
    ///     <para>
    ///         The <c>Rect</c> structure is mainly used for 2D operations. The UnityGUI system uses it extensively and it is
    ///         also to set the onscreen position of a Camera's view.The rectangle can be specified in two different ways. The
    ///         first way involves supplying the top-left corner coordinate along with the width and height. This is done using
    ///         the <see cref="UnityEngine.Rect.x" />, <see cref="UnityEngine.Rect.y" />, <see cref="UnityEngine.Rect.width" />
    ///         and <see cref="UnityEngine.Rect.height" /> properties. The second way to specify the rectangle is to supply the
    ///         X coordinates of its left and right sides and the Y coordinates of its top and bottom sides. These are denoted
    ///         by the <see cref="UnityEngine.Rect.xMin" />, <see cref="UnityEngine.Rect.xMax" />,
    ///         <see cref="UnityEngine.Rect.yMin" /> and <see cref="UnityEngine.Rect.yMax" /> properties.Although the
    ///         <see cref="UnityEngine.Rect.x" /> and <see cref="UnityEngine.Rect.y" /> properties may seem to be the same as
    ///         <see cref="UnityEngine.Rect.xMin" /> and <see cref="UnityEngine.Rect.xMax" />, their behaviour is actually
    ///         slightly different. The <c>x</c> and <c>y</c> values are assumed to be used along with <c>width</c> and
    ///         <c>height</c>. This means that if you change <c>x</c> or <c>y</c> without changing <c>width</c> or
    ///         <c>height</c> then the rectangle will change position but stay the same size. On the other hand, if you change
    ///         the values of <c>xMin</c> or <c>yMin</c> without changing <c>xMax</c> or <c>yMax</c> then the rectangle will
    ///         change size and the top-left corner will also change.See Also: GUI Scripting Guide,
    ///         <see cref="UnityEngine.Camera.rect" />, <see cref="UnityEngine.Camera.pixelRect" />.
    ///     </para>
    /// </summary>
    [Serializable]
    public struct IntRect {
        [SerializeField]
        private int _xMin;

        [SerializeField]
        private int _yMin;

        [SerializeField]
        private int _width;

        [SerializeField]
        private int _height;

        /// <summary>Left coordinate of the rectangle.</summary>
        public int x {
            get { return _xMin; }
            set { _xMin = value; }
        }

        /// <summary>Top coordinate of the rectangle.</summary>
        public int y {
            get { return _yMin; }
            set { _yMin = value; }
        }

        /// <summary>
        ///     <para>The top left coordinates of the rectangle.</para>
        ///     <para>
        ///         This is the <see cref="UnityEngine.Rect.x" /> and <see cref="UnityEngine.Rect.y" /> coordinates in a combined
        ///         IntVector2.
        ///     </para>
        /// </summary>
        public IntVector2 position {
            get { return new IntVector2(_xMin, _yMin); }
            set {
                _xMin = value.x;
                _yMin = value.y;
            }
        }

        /// <summary>
        ///     <para>Lower left corner of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve the right and top side of rectangle (so
        ///         <see cref="UnityEngine.Rect.width" /> and <see cref="UnityEngine.Rect.height" /> will change as well).
        ///     </para>
        /// </summary>
        public IntVector2 min {
            get { return new IntVector2(xMin, yMin); }
            set {
                xMin = value.x;
                yMin = value.y;
            }
        }

        /// <summary>
        ///     <para>Upper right corner of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve the left and bottom side of rectangle (so
        ///         <see cref="UnityEngine.Rect.width" /> and <see cref="UnityEngine.Rect.height" /> will change as well).
        ///     </para>
        /// </summary>
        public IntVector2 max {
            get { return new IntVector2(xMax, yMax); }
            set {
                xMax = value.x;
                yMax = value.y;
            }
        }

        /// <summary>Width of the rectangle.</summary>
        public int width {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>Height of the rectangle.</summary>
        public int height {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        ///     <para>The size of the rectangle.</para>
        ///     <para>
        ///         This is the <see cref="UnityEngine.Rect.width" /> and <see cref="UnityEngine.Rect.height" /> in a combined
        ///         IntVector2.
        ///     </para>
        /// </summary>
        public IntVector2 size {
            get { return new IntVector2(_width, _height); }
            set {
                _width = value.x;
                _height = value.y;
            }
        }

        /// <summary>Center coordinate of the rectangle.</summary>
        public Vector2 center {
            get { return new Vector2(x + _width / 2f, y + _height / 2f); }
        }

        /// <summary>
        ///     <para>Left coordinate of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve right side of rectangle (so <see cref="UnityEngine.Rect.width" /> will
        ///         change as well).
        ///     </para>
        /// </summary>
        public int xMin {
            get { return _xMin; }
            set {
                int xMax = this.xMax;
                _xMin = value;
                _width = xMax - _xMin;
            }
        }

        /// <summary>
        ///     <para>Top coordinate of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve bottom side of rectangle (so <see cref="UnityEngine.Rect.height" /> will
        ///         change as well).
        ///     </para>
        /// </summary>
        public int yMin {
            get { return _yMin; }
            set {
                int yMax = this.yMax;
                _yMin = value;
                _height = yMax - _yMin;
            }
        }

        /// <summary>
        ///     <para>Right coordinate of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve left side of rectangle (so <see cref="UnityEngine.Rect.width" /> will
        ///         change as well).
        ///     </para>
        /// </summary>
        public int xMax {
            get { return _width + _xMin; }
            set { _width = value - _xMin; }
        }

        /// <summary>
        ///     <para>Bottom coordinate of the rectangle.</para>
        ///     <para>
        ///         Changing this value will preserve top side of rectangle (so <see cref="UnityEngine.Rect.height" /> will
        ///         change as well).
        ///     </para>
        /// </summary>
        public int yMax {
            get { return _height + _yMin; }
            set { _height = value - _yMin; }
        }

        /// <summary>Creates a new rectangle.</summary>
        public IntRect(int left, int top, int width, int height) {
            _xMin = left;
            _yMin = top;
            _width = width;
            _height = height;
        }

        /// <summary>
        ///     <para>Creates a rectangle given a size and position.</para>
        ///     <para>This form of the constructor is convenient when you are already working with IntVector2 values.</para>
        /// </summary>
        /// <param name="position">The position of the top-left corner.</param>
        /// <param name="size">The width and height.</param>
        public IntRect(IntVector2 position, IntVector2 size) {
            _xMin = position.x;
            _yMin = position.y;
            _width = size.x;
            _height = size.y;
        }

        public IntRect(IntRect source) {
            _xMin = source._xMin;
            _yMin = source._yMin;
            _width = source._width;
            _height = source._height;
        }

        public static bool operator !=(IntRect lhs, IntRect rhs) {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.width != rhs.width || lhs.height != rhs.height;
        }

        public static bool operator ==(IntRect lhs, IntRect rhs) {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
        }

        /// <summary>Creates a rectangle from min/max coordinate values.</summary>
        public static IntRect MinMaxRect(int left, int top, int right, int bottom) {
            return new IntRect(left, top, right - left, bottom - top);
        }

        /// <summary>Set components of an existing Rect.</summary>
        public void Set(int left, int top, int width, int height) {
            _xMin = left;
            _yMin = top;
            _width = width;
            _height = height;
        }

        /// <summary>Returns a nicely formatted string for this Rect.</summary>
        public override string ToString() {
            return string.Format("(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", x, y, width, height);
        }

        /// <summary>Returns a nicely formatted string for this Rect.</summary>
        public string ToString(string format) {
            return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format), y.ToString(format), width.ToString(format), height.ToString(format));
        }

        /// <summary>
        ///     Returns true if the <c>x</c> and <c>y</c> components of <c>point</c> is a point inside this rectangle. If
        ///     <c>allowInverse</c> is present and true, the width and height of the Rect are allowed to take negative values (ie,
        ///     the min value is greater than the max), and the test will still work.
        /// </summary>
        /// <param name="point">Point to test.</param>
        public bool Contains(IntVector2 point) {
            return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
        }

        private static IntRect OrderMinMax(IntRect rect) {
            if (rect.xMin > (double) rect.xMax) {
                int xMin = rect.xMin;
                rect.xMin = rect.xMax;
                rect.xMax = xMin;
            }
            if (rect.yMin > (double) rect.yMax) {
                int yMin = rect.yMin;
                rect.yMin = rect.yMax;
                rect.yMax = yMin;
            }
            return rect;
        }

        /// <summary>
        ///     Returns true if the other rectangle overlaps this one. If <c>allowInverse</c> is present and true, the widths
        ///     and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the
        ///     test will still work.
        /// </summary>
        /// <param name="other">Other rectangle to test overlapping with.</param>
        public bool Overlaps(IntRect other) {
            return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
        }

        /// <summary>
        ///     Returns true if the other rectangle overlaps this one. If <c>allowInverse</c> is present and true, the widths
        ///     and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the
        ///     test will still work.
        /// </summary>
        /// <param name="other">Other rectangle to test overlapping with.</param>
        /// <param name="allowInverse">Does the test allow the Rects' widths and heights to be negative?</param>
        public bool Overlaps(IntRect other, bool allowInverse) {
            IntRect rect = this;
            if (allowInverse) {
                rect = OrderMinMax(rect);
                other = OrderMinMax(other);
            }
            return rect.Overlaps(other);
        }

        public static explicit operator IntRect(Rect rect) {
            return new IntRect((int) rect.xMin, (int) rect.yMin, (int) rect.width, (int) rect.height);
        }

        public static explicit operator Rect(IntRect intRect) {
            return new Rect(intRect._xMin, intRect._yMin, intRect._width, intRect._height);
        }

        public override int GetHashCode() {
            return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
        }

        public override bool Equals(object other) {
            if (!(other is IntRect)) {
                return false;
            }
            IntRect rect = (IntRect) other;
            if (x.Equals(rect.x) && y.Equals(rect.y) && width.Equals(rect.width)) {
                return height.Equals(rect.height);
            }
            return false;
        }
    }
}

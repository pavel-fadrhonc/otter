using System.Runtime.InteropServices;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    [StructLayout(LayoutKind.Explicit)]
    internal struct Color32UintUnion {
        [FieldOffset(0)]
        public Color32 Color32;

        [FieldOffset(0)]
        public uint ColorUint;
    }
}
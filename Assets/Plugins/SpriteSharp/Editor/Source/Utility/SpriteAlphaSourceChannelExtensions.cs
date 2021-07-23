using System;
using LostPolygon.SpriteSharp.TightMeshSettings;

namespace LostPolygon.SpriteSharp.Utility {
    internal static class SpriteAlphaSourceChannelExtensions {
        public static void GetMask(this SpriteAlphaSourceChannel alphaSourceChannel, out uint mask, out int rightShift) {
            switch (alphaSourceChannel) {
                case SpriteAlphaSourceChannel.Red:
                    mask = 0x000000FF;
                    rightShift = 0;
                    break;
                case SpriteAlphaSourceChannel.Green:
                    mask = 0x0000FF00;
                    rightShift = 8;
                    break;
                case SpriteAlphaSourceChannel.Blue:
                    mask = 0x00FF0000;
                    rightShift = 16;
                    break;
                case SpriteAlphaSourceChannel.Alpha:
                    mask = 0xFF000000;
                    rightShift = 24;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("alphaSourceChannel", alphaSourceChannel, null);
            }
        }
    }
}

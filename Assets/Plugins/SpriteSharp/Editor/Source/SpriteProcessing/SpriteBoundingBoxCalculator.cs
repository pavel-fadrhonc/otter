using LostPolygon.SpriteSharp.TightMeshSettings;
using UnityEngine;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing {
    public static class SpriteBoundingBoxCalculator {
        public static IntRect? CalculateBoundingRect(Sprite sprite, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance, ref Color32[] pixels) {
            return CalculateBoundingRect(sprite.texture, sprite.rect.ToIntRectWithPositiveBias(), alphaSourceChannel, alphaTolerance, ref pixels);
        }

        public static IntRect? CalculateBoundingRect(Texture2D texture, IntRect rect, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance, ref Color32[] pixels) {
            if (pixels == null) {
                pixels = texture.GetPixels32Reliable();
            }
            int mipWidth = Mathf.Max(1, texture.width);
            //int mipHeight = Mathf.Max(1, texture.height >> mipLevel);

            IntRect? boundingRect = CalculateBoundingRect(pixels, mipWidth, rect, alphaSourceChannel, alphaTolerance);
            return boundingRect;
        }

        public static IntRect? CalculateBoundingRect(Color32[] pixels, int rowLength, IntRect rect, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance) {
            uint channelMask;
            int channelRightShift;
            alphaSourceChannel.GetMask(out channelMask, out channelRightShift);

            Color32UintUnion color32UintUnion;
            color32UintUnion.ColorUint = 0;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            int iMax = rect.xMin + rect.width;
            int jMax = rect.yMin + rect.height;
            for (int j = rect.yMin; j < jMax; j++) {
                for (int i = rect.xMin; i < iMax; i++) {
                    int index = j * rowLength + i;
                    color32UintUnion.Color32 = pixels[index];

                    byte alphaSourceChannelValue = (byte) ((color32UintUnion.ColorUint & channelMask) >> channelRightShift);
                    if (alphaSourceChannelValue <= alphaTolerance)
                        continue;

                    if (i < minX)
                        minX = i;
                    if (i > maxX)
                        maxX = i;

                    if (j < minY)
                        minY = j;
                    if (j > maxY)
                        maxY = j;
                }
            }

            bool isEmpty = false;
            if (minX == int.MaxValue)
                isEmpty = true;
            else if (minY == int.MaxValue)
                isEmpty = true;
            else if (maxX == int.MinValue)
                isEmpty = true;
            else if (maxY == int.MinValue)
                isEmpty = true;

            if (isEmpty)
                return null;

            return IntRect.MinMaxRect(minX, minY, maxX, maxY);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using LostPolygon.SpriteSharp.ClipperLib;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// Precise method outline generator.
    /// Generates outline paths from texture.
    /// </summary>
    public static class PreciseOutlineGenerator {
        public static List<List<IntPoint>> GenerateOutline(Sprite sprite, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance, float reductionTolerance = 0f) {
            return GenerateOutline(sprite.texture, sprite.rect.ToIntRectWithPositiveBias(), alphaSourceChannel, alphaTolerance, reductionTolerance);
        }

        public static List<List<IntPoint>> GenerateOutline(Texture2D texture, IntRect rect, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance, float reductionTolerance) {
            List<List<IntPoint>> paths = GenerateOutline(texture, rect, alphaSourceChannel, alphaTolerance);
            PathReductionUtility.DouglasPeuckerReduction(paths, reductionTolerance);

            return paths;
        }

        public static List<List<IntPoint>> GenerateOutline(Texture2D texture, IntRect rect, SpriteAlphaSourceChannel alphaSourceChannel, byte alphaTolerance) {
            const int mipLevel = 0;
            Color32[] pixels = texture.GetPixels32Reliable(mipLevel);
            int mipWidth = Mathf.Max(1, texture.width >> mipLevel);
            //int mipHeight = Mathf.Max(1, texture.height >> mipLevel);

            List<List<IntPoint>> paths =
                CollectPixelPaths(
                    pixels,
                    mipWidth,
                    rect,
                    alphaSourceChannel,
                    alphaTolerance
                );

            List<List<IntPoint>> mergedPaths = PathUtility.CalculateMergedPaths(paths);

            return mergedPaths;
        }

        public static void GetMinMaxAlphaValue(
            Color32[] pixels,
            int rowLength,
            IntRect rect,
            SpriteAlphaSourceChannel alphaSourceChannel,
            out byte minAlpha,
            out byte maxAlpha
        ) {
            uint channelMask;
            int channelRightShift;
            alphaSourceChannel.GetMask(out channelMask, out channelRightShift);

            Color32UintUnion color32UintUnion;
            color32UintUnion.ColorUint = 0;

            minAlpha = byte.MaxValue;
            maxAlpha = byte.MinValue;

            int iMax = rect.xMin + rect.width;
            int jMax = rect.yMin + rect.height;
            for (int j = rect.yMin; j < jMax; j++) {
                for (int i = rect.xMin; i < iMax; i++) {
                    int index = j * rowLength + i;
                    color32UintUnion.Color32 = pixels[index];
                    byte alphaSourceChannelValue = (byte) ((color32UintUnion.ColorUint & channelMask) >> channelRightShift);

                    if (alphaSourceChannelValue < minAlpha) {
                        minAlpha = alphaSourceChannelValue;
                    }

                    if (alphaSourceChannelValue > maxAlpha) {
                        maxAlpha = alphaSourceChannelValue;
                    }
                }
            }
        }

        private static List<List<IntPoint>> CollectPixelPaths(
            Color32[] pixels,
            int rowLength,
            IntRect rect,
            SpriteAlphaSourceChannel alphaSourceChannel,
            byte alphaTolerance
            ) {
            uint channelMask;
            int channelRightShift;
            alphaSourceChannel.GetMask(out channelMask, out channelRightShift);

            Color32UintUnion color32UintUnion;
            color32UintUnion.ColorUint = 0;

            List<List<IntPoint>> tempPaths = new List<List<IntPoint>>(rect.height);
            int sequenceLength = 0;
            bool sequenceStarted = false;

            int iMax = rect.xMin + rect.width;
            int jMax = rect.yMin + rect.height;
            for (int j = rect.yMin; j < jMax; j++) {
                for (int i = rect.xMin; i < iMax; i++) {
                    int index = j * rowLength + i;
                    color32UintUnion.Color32 = pixels[index];
                    byte alphaSourceChannelValue = (byte) ((color32UintUnion.ColorUint & channelMask) >> channelRightShift);

                    if (alphaSourceChannelValue <= alphaTolerance) {
                        if (sequenceStarted) {
                            tempPaths.Add(CreateSquarePath(i - rect.xMin - sequenceLength, j - rect.yMin, sequenceLength));
                            sequenceLength = 0;
                            sequenceStarted = false;
                        }

                        continue;
                    }

                    sequenceLength++;
                    sequenceStarted = true;
                }

                if (sequenceStarted) {
                    tempPaths.Add(CreateSquarePath(iMax - rect.xMin - sequenceLength, j - rect.yMin, sequenceLength));
                    sequenceLength = 0;
                    sequenceStarted = false;
                }
            }

            return tempPaths;
        }

        private static List<IntPoint> CreateSquarePath(int x, int y, int length) {
            List<IntPoint> path = new List<IntPoint>(4);
            path.Add(new IntPoint(x, y));
            path.Add(new IntPoint(x + length, y));
            path.Add(new IntPoint(x + length, y + 1));
            path.Add(new IntPoint(x, y + 1));

            return path;
        }
    }
}

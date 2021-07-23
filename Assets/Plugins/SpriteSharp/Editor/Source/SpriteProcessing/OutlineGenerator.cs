using System;
using System.Reflection;
using UnityEditor.Sprites;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Processing {
    /// <summary>
    /// Unity built-in outline generator.
    /// Generates outline paths from texture.
    /// </summary>
    public static class OutlineGenerator {
        private delegate void GenerateOutlineFromSpriteDelegate(
            Sprite sprite,
            float detail,
            byte alphaTolerance,
            bool holeDetection,
            out Vector2[][] paths
            );

        private static readonly GenerateOutlineFromSpriteDelegate _generateOutlineFromSpriteMethod;

        static OutlineGenerator() {
            MethodInfo methodInfo =
                typeof(SpriteUtility)
                .GetMethod("GenerateOutlineFromSprite", BindingFlags.Static | BindingFlags.NonPublic);

            _generateOutlineFromSpriteMethod =
                (GenerateOutlineFromSpriteDelegate)
                    Delegate.CreateDelegate(typeof(GenerateOutlineFromSpriteDelegate), methodInfo);
        }

        public static Vector2[][] GenerateOutline(
            Sprite sprite,
            float detail,
            byte alphaTolerance,
            bool holeDetection
            ) {
            Vector2[][] paths;
            _generateOutlineFromSpriteMethod(sprite, detail, alphaTolerance, holeDetection, out paths);
            return paths;
        }
    }
}
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// <see cref="Texture"/> helper extensions.
    /// </summary>
    internal static class EditorTextureExtensions {
        public static TextureImporter GetTextureImporter(this Texture2D texture) {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = EditorTextureUtility.GetTextureImporter(assetPath);

            return textureImporter;
        }

        public static TextureImporterSettings GetTextureImporterSettings(this Texture2D texture) {
            TextureImporter textureImporter = GetTextureImporter(texture);
            if (textureImporter == null)
                return null;

            TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(textureImporterSettings);
            return textureImporterSettings;
        }

        public static bool IsTightSpriteMesh(this Texture2D texture) {
            TextureImporterSettings settings = texture.GetTextureImporterSettings();
            if (settings == null)
                return false;

            return settings.spriteMeshType == SpriteMeshType.Tight;
        }

        public static Color32[] GetPixels32Reliable(this Texture2D texture, int mipLevel = 0) {
            TextureImporter importer = texture.GetTextureImporter();
            bool isReadable = importer == null || importer.isReadable;

            return
                isReadable ?
                    texture.GetPixels32(mipLevel) :
                    GetPixels32WithRenderTexture(texture, mipLevel);
        }

        public static Color32[] GetPixels32WithRenderTexture(this Texture2D texture, int mipLevel = 0) {
            int mipWidth = Mathf.Max(1, texture.width >> mipLevel);
            int mipHeight = Mathf.Max(1, texture.height >> mipLevel);

            Texture2D tempTexture = new Texture2D(mipWidth, mipHeight, TextureFormat.ARGB32, false);

            GL.PushMatrix();
            Rect textureRect = new Rect(0f, 0f, mipWidth, mipHeight);
            RenderTexture rt = RenderTexture.GetTemporary(mipWidth, mipHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(texture, rt);
            RenderTexture.active = rt;
            tempTexture.ReadPixels(textureRect, 0, 0, false);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            GL.PopMatrix();

            Color32[] pixelsBuffer = tempTexture.GetPixels32(0);
            Object.DestroyImmediate(tempTexture);

            return pixelsBuffer;
        }
    }
}
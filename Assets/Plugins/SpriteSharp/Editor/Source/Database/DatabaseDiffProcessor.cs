using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LostPolygon.SpriteSharp.Serialization;
using LostPolygon.SpriteSharp.TightMeshSettings;
using LostPolygon.SpriteSharp.Utility;
using LostPolygon.SpriteSharp.Utility.Internal;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace LostPolygon.SpriteSharp.Database.Internal {
    internal static class DatabaseDiffProcessor {
        public static bool ProcessDiff(string databasePath, string databaseDiffCopyPath) {
            Stopwatch sw = Stopwatch.StartNew();

            string[] texturesToReimport;
            if (!ProcessDiffInternal(databasePath, databaseDiffCopyPath, out texturesToReimport))
                return false;

            sw.Stop();
#if SS_TRACE
            Debug.LogFormat("[Diff] done in {0} ms", sw.ElapsedMilliseconds);
#endif
            if (texturesToReimport.Length > 0) {
                AssetDatabaseUtility.ReimportAssets(texturesToReimport);
            }

            return true;
        }

        private static bool ProcessDiffInternal(string databasePath, string databaseDiffCopyPath, out string[] texturesToReimport) {
            texturesToReimport = null;
            if (!File.Exists(databasePath) || !File.Exists(databaseDiffCopyPath))
                return false;

            string databaseJson = File.ReadAllText(databasePath);
            string databaseDiffCopyJson = File.ReadAllText(databaseDiffCopyPath);
            // Short circuit if databases are identical
            if (databaseJson == databaseDiffCopyJson)
                return false;

            DatabaseContainer diffCopyDatabase = DatabaseProxy.LoadFromFileRaw(databaseDiffCopyJson);

            IDictionary<SpriteLazyReference, SpriteTightMeshSettings> currentSpritesSettings = DatabaseProxy.Instance.SpriteSettings;
            IDictionary<SpriteLazyReference, SpriteTightMeshSettings> diffCopySpritesSettings = diffCopyDatabase.SpriteMeshSettings;

            // Calculate diff
            KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings>[] addedSprites, removedSprites, remainedSprites;
            IDictionaryUtility.CalculateDiff(
                currentSpritesSettings,
                diffCopySpritesSettings,
                out addedSprites,
                out removedSprites,
                out remainedSprites,
                new SpriteLazyReferenceKeyComparer() // We only need to diff the sprites themselves, not their settings
                );

#if SS_TRACE
            Debug.LogFormat(
                "[Diff] Added sprites:\n{0}\nRemoved sprites:\n{1}\nRemained sprites count:\n{2}\n",
                String.Join("\n", addedSprites.Select(pair => GetSpriteName(pair.Key)).ToArray()),
                String.Join("\n", removedSprites.Select(pair => GetSpriteName(pair.Key)).ToArray()),
                remainedSprites.Length
            );
#endif

            HashSet<SpriteLazyReference> spritesToReimport = new HashSet<SpriteLazyReference>();
            // Always reimport new sprites
            spritesToReimport.UnionWith(addedSprites.Select(pair => pair.Key));

            // Only add sprites that were already in the database if settings have changed
            foreach (var remainedSprite in remainedSprites) {
                SpriteTightMeshSettings currentSpriteTightMeshSettings = currentSpritesSettings[remainedSprite.Key];
                SpriteTightMeshSettings diffCopySpriteTightMeshSettings = diffCopySpritesSettings[remainedSprite.Key];
                if (currentSpriteTightMeshSettings.Equals(diffCopySpriteTightMeshSettings))
                    continue;

                spritesToReimport.Add(remainedSprite.Key);
            }

            if (spritesToReimport.Count == 0)
                return false;

            // Paths of textures used by sprites
            texturesToReimport =
                spritesToReimport
                    .Select(reference => AssetDatabase.GUIDToAssetPath(reference.Guid))
                    .Where(path => !String.IsNullOrEmpty(path))
                    .Distinct()
                    .ToArray();

#if SS_TRACE
            Debug.LogFormat(
                "[Diff] Modified sprites:\n{0}\nReimported textures:\n{1}",
                String.Join("\n", spritesToReimport.Select(s => GetSpriteName(s)).ToArray()),
                String.Join("\n", texturesToReimport)
            );
#endif

            return true;
        }

        private static string GetSpriteName(SpriteLazyReference spriteLazyReference) {
            return spriteLazyReference.Guid + "/" + spriteLazyReference.LocalIdentifier;
        }

        private class SpriteLazyReferenceKeyComparer : IEqualityComparer<KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings>> {
            public bool Equals(KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> x, KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> y) {
                return System.Object.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<SpriteLazyReference, SpriteTightMeshSettings> obj) {
                return obj.Key != null ? obj.Key.GetHashCode() : 0;
            }
        }
    }
}

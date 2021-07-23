using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using LostPolygon.SpriteSharp.Utility.Internal;

namespace LostPolygon.SpriteSharp.Internal {
    internal class VersionDependentFile {
        private static readonly UnityVersionParser _unityVersionParser;
        private readonly string _versionsRootFolder;
        private readonly string _versionDependentFile;

        static VersionDependentFile() {
            _unityVersionParser = new UnityVersionParser(Application.unityVersion);
        }

        public VersionDependentFile(string versionsRootFolder, string versionDependentFile) {
            _versionsRootFolder = versionsRootFolder;
            _versionDependentFile = versionDependentFile;
        }

        public void Update() {
            const string versionExtension = ".version";
            const string versionSeparator = "-";
            string fileName = Path.GetFileName(_versionDependentFile);
            string[] versionsFilePaths = Directory.GetFiles(_versionsRootFolder, fileName + versionSeparator + "*" + versionExtension);

            Utility.Internal.Tuple<string, Version> bestVersion = null;
            foreach (string versionFilePath in versionsFilePaths) {
                string versionFileName = Path.GetFileName(versionFilePath);
                if (versionFileName == null)
                    continue;

                string versionText = versionFileName.Replace(fileName + versionSeparator, "");
                int lastIndexOfVersionExtension = versionText.LastIndexOf(versionExtension, StringComparison.OrdinalIgnoreCase);
                if (lastIndexOfVersionExtension == -1)
                    continue;
                
                versionText = versionText.Remove(lastIndexOfVersionExtension, versionExtension.Length);
                if (String.IsNullOrEmpty(versionText))
                    continue;

                Version versionValue = new Version(versionText);
                if ((bestVersion == null || bestVersion.Second < versionValue) &&
                    versionValue <= _unityVersionParser.Version) {
                    bestVersion = new Utility.Internal.Tuple<string, Version>(versionFilePath, versionValue);
                }
            }

            if (bestVersion == null)
                throw new FileNotFoundException(String.Format("No best version found for '{0}' in '{1}'", _versionDependentFile, _versionsRootFolder));

            // Check file contents
            FileInfo targetFileInfo = new FileInfo(_versionDependentFile);
            FileInfo bestVersionFileInfo = new FileInfo(bestVersion.First);
            if (targetFileInfo.CreationTimeUtc == bestVersionFileInfo.CreationTimeUtc &&
                targetFileInfo.Length == bestVersionFileInfo.Length)
                return;

            UpdateFile(bestVersion.First);
            AssetDatabase.ImportAsset(_versionDependentFile, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
        }

        private void UpdateFile(string bestVersionPath) {
            File.Copy(bestVersionPath, _versionDependentFile, true);
            File.SetCreationTimeUtc(_versionDependentFile, File.GetCreationTimeUtc(bestVersionPath));
        }
    }
}
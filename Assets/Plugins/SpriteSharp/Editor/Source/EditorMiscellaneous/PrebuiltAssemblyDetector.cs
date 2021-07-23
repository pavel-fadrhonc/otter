using System;
using System.Reflection;
using UnityEngine;
using LostPolygon.SpriteSharp.Internal;

namespace LostPolygon.SpriteSharp.Internal {
    internal static class PrebuiltAssemblyDetector {
        private static bool _isChecked;
        private static bool _isPrebuiltAssemblyPresent;
        private static bool _isRunningFromPrebuiltAssembly;
        private static bool _isErrorShown;

        public static bool IsRunningFromPrebuiltAssembly {
            get {
                Check();
                return _isPrebuiltAssemblyPresent;
            }
        }

        public static bool CanWorkWithDatabase {
            get {
                Check();
                if (_isRunningFromPrebuiltAssembly)
                    return true;

                return !_isPrebuiltAssemblyPresent;
            }
        }

        private static void Check() {
            if (_isChecked)
                return;

            _isChecked = true;
            _isRunningFromPrebuiltAssembly = typeof(PrebuiltAssemblyDetector).Assembly.GetName().Name == PrebuiltAssemblyName;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                if (assembly.GetName().Name == PrebuiltAssemblyName) {
                    _isPrebuiltAssemblyPresent = true;
                    break;
                }
            }

            if (!CanWorkWithDatabase && !_isErrorShown) {
                Debug.LogError(PrebuiltAssemblyName + " prebuilt assembly is present alongside with SpriteSharp source code. " +
                               "Please delete " + PrebuiltAssemblyName + ".dll file.");
                _isErrorShown = true;
            }
        }

        private static string PrebuiltAssemblyName {
            get { return Constants.kPrebuiltAssemblyName; }
        }
    }
}

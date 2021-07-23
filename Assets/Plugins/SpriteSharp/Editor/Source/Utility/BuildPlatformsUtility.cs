using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Utilities for working with build platforms.
    /// </summary>
    [InitializeOnLoad]
    public static class BuildPlatformsUtility {
        private static readonly Func<BuildTarget, BuildTargetGroup> _getBuildTargetGroupFunc;
        private static readonly BuildPlatform[] _validBuildPlatforms;
        private static readonly Dictionary<BuildTargetGroup, BuildPlatform> _buildTargetGroupToBuildPlatformMap;

        public static BuildTargetGroup ActiveBuildTargetGroup {
            get {
                return _getBuildTargetGroupFunc(EditorUserBuildSettings.activeBuildTarget);
            }
        }

        public static BuildPlatform[] ValidBuildPlatforms {
            get {
                return _validBuildPlatforms;
            }
        }

        static BuildPlatformsUtility() {
            Assembly editorAssembly = typeof(Editor).Assembly;

            // BuildPipeline.GetBuildTargetGroup method is internal in Unity 5.0,
            // so we use reflection to cover all versions
            MethodInfo getBuildTargetGroupMethodInfo = typeof(BuildPipeline)
                .GetMethod("GetBuildTargetGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            _getBuildTargetGroupFunc =
                (Func<BuildTarget, BuildTargetGroup>) Delegate.CreateDelegate(typeof(Func<BuildTarget, BuildTargetGroup>), getBuildTargetGroupMethodInfo);

            // Get List<UnityEditor.Build.BuildPlatform>
            bool isUnity2017 = true;
            MethodInfo getValidPlatformsMethod =
                    editorAssembly.GetType("UnityEditor.Build.BuildPlatforms") != null ?
                    editorAssembly.GetType("UnityEditor.Build.BuildPlatforms")
                    .GetMethod("GetValidPlatforms", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) :
                    null;

            if (getValidPlatformsMethod == null) {
                // Get List<UnityEditor.BuildPlayerWindow.BuildPlatform>
                getValidPlatformsMethod =
                    editorAssembly
                        .GetType("UnityEditor.BuildPlayerWindow")
                        .GetMethod("GetValidPlatforms", BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (getValidPlatformsMethod != null) {
                    isUnity2017 = false;
                }
            }

            object platforms;
            if (isUnity2017) {
                object buildPlatformsInstance =
                    editorAssembly
                    .GetType("UnityEditor.Build.BuildPlatforms")
                    .GetProperty("instance", BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);

                platforms = getValidPlatformsMethod.Invoke(buildPlatformsInstance, null);
            } else {
                platforms = getValidPlatformsMethod.Invoke(null, null);
            }

            // Convert to UnityEditor.BuildPlayerWindow.BuildPlatform[]
            Array originalValidBuildPlatforms =
                (Array) platforms
                    .GetType()
                    .GetMethod("ToArray")
                    .Invoke(platforms, null);

            // Retrieve fields of UnityEditor.BuildPlayerWindow.BuildPlatform
            Type buildPlatformType =
                editorAssembly
                .GetType(isUnity2017 ? "UnityEditor.Build.BuildPlatform" : "UnityEditor.BuildPlayerWindow+BuildPlatform");

            FieldInfo targetGroupFieldInfo =
                buildPlatformType
                .GetField("targetGroup", BindingFlags.Public | BindingFlags.Instance);

            FieldInfo nameFieldInfo =
                buildPlatformType
                .GetField("name", BindingFlags.Public | BindingFlags.Instance);

            FieldInfo tooltipFieldInfo =
                buildPlatformType
                .GetField("tooltip", BindingFlags.Public | BindingFlags.Instance);

            PropertyInfo smallIconFieldInfo =
                buildPlatformType
                .GetProperty("smallIcon", BindingFlags.Public | BindingFlags.Instance);

            // Copy data from UnityEditor.BuildPlayerWindow.BuildPlatform to our internal version
            _validBuildPlatforms = new BuildPlatform[originalValidBuildPlatforms.Length];
            for (int i = 0; i < originalValidBuildPlatforms.Length; i++) {
                BuildTargetGroup buildTargetGroup = (BuildTargetGroup) targetGroupFieldInfo.GetValue(originalValidBuildPlatforms.GetValue(i));
                string name = (string) nameFieldInfo.GetValue(originalValidBuildPlatforms.GetValue(i));
                string tooltip = (string) tooltipFieldInfo.GetValue(originalValidBuildPlatforms.GetValue(i));
                Texture smallIcon = (Texture) smallIconFieldInfo.GetValue(originalValidBuildPlatforms.GetValue(i));
                _validBuildPlatforms[i] = new BuildPlatform(buildTargetGroup, name, tooltip, smallIcon);
            }

            // Match BuildTargetGroup to BuildPlatform
            _buildTargetGroupToBuildPlatformMap = new Dictionary<BuildTargetGroup, BuildPlatform>(_validBuildPlatforms.Length);
            foreach (BuildPlatform buildPlatform in _validBuildPlatforms) {
                _buildTargetGroupToBuildPlatformMap.Add(buildPlatform.BuildTargetGroup, buildPlatform);
            }
        }

        public static BuildPlatform GetBuildPlatform(BuildTargetGroup buildTargetGroup) {
            return _buildTargetGroupToBuildPlatformMap[buildTargetGroup];
        }

        public static BuildTargetGroup GetDefaultBuildTargetGroup() {
            return (BuildTargetGroup) (-1);
        }
    }
}

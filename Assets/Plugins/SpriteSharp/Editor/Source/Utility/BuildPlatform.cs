using UnityEditor;
using UnityEngine;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Contains information about <see cref="BuildTargetGroup"/> for use in GUI.
    /// </summary>
    public class BuildPlatform {
        public readonly BuildTargetGroup BuildTargetGroup;
        public readonly string Name;
        public readonly string Tooltip;
        public readonly Texture SmallIcon;

        public BuildPlatform(BuildTargetGroup buildTargetGroup, string name, string tooltip, Texture smallIcon) {
            BuildTargetGroup = buildTargetGroup;
            Name = name;
            Tooltip = tooltip;
            SmallIcon = smallIcon;
        }
    }
}
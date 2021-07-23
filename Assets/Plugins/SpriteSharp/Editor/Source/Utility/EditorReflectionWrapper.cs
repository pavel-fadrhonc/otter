using System.Reflection;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    /// <summary>
    /// Exposes non-public members of the <see cref="UnityEditor.Editor"/> via reflection.
    /// </summary>
    internal static class EditorReflectionWrapper {
        private static readonly PropertyInfo _referenceTargetIndexPropertyInfo;
        private static readonly FieldInfo _hideInspectorFieldInfo;

        static EditorReflectionWrapper() {
            _referenceTargetIndexPropertyInfo =
                typeof(Editor)
                .GetProperty("referenceTargetIndex", BindingFlags.NonPublic | BindingFlags.Instance);

            _hideInspectorFieldInfo =
                typeof(Editor)
                .GetField("hideInspector", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static int GetReferenceTargetIndex(this Editor editor) {
            return (int) _referenceTargetIndexPropertyInfo.GetValue(editor, null);
        }

        public static void SetHideInspector(this Editor editor, bool hide) {
            _hideInspectorFieldInfo.SetValue(editor, hide);
        }

        public static void SetReferenceTargetIndex(this Editor editor, int referenceTargetIndex) {
            _referenceTargetIndexPropertyInfo.SetValue(editor, referenceTargetIndex, null);
        }

        public static void RepaintAllInspectors() {
            typeof(Editor)
                .Assembly
                .GetType("UnityEditor.InspectorWindow")
                .GetMethod("RepaintAllInspectors", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, null);
        }
    }
}
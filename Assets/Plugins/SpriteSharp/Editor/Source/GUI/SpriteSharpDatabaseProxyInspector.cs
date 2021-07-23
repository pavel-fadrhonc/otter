using System;
using UnityEditor;
using LostPolygon.SpriteSharp.Database;

namespace LostPolygon.SpriteSharp.Gui.Internal {
    /// <summary>
    /// A simple help inspector for <see cref="DatabaseProxy"/>.
    /// </summary>
    [CustomEditor(typeof(DatabaseProxy))]
    [CanEditMultipleObjects]
    internal class SpriteSharpDatabaseProxyInspector : Editor {
        public override void OnInspectorGUI() {
            DatabaseProxy database = (DatabaseProxy) target;

            EditorGUILayout.HelpBox(
                String.Format(
                    "Total of {0} sprites are currently in the database.",
                    database.SpriteSettings.Count
                ),
                MessageType.Info,
                true
            );
        }
    }
}
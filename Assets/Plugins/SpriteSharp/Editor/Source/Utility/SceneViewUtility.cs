using UnityEngine;
using UnityEditor;

namespace LostPolygon.SpriteSharp.Utility {
    /// <summary>
    /// Helper utilities for working with the scene view.
    /// </summary>
    public static class SceneViewUtility {
        public static void CalculateSimpleInstantiatePosition(out Vector3 position, out Quaternion rotation) {
            // Calculate sprite position and rotation
            position = Vector3.zero;
            rotation = Quaternion.identity;
            if (SceneView.lastActiveSceneView != null) {
                position = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                rotation = Quaternion.Euler(0f, SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles.y, 0f);

                if (SceneView.lastActiveSceneView.in2DMode) {
                    position.z = 0f;
                }
            }
        }
    }
}
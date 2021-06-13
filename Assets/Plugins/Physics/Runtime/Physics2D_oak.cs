using UnityEngine;

namespace o2f.Physics
{
    public static class Physics2D_Oak
    {
        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, Color drawColor, float distance = Mathf.Infinity,
            int layerMask = Physics2D.DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity)
        {
            Debug.DrawLine(origin, origin + direction * distance, drawColor);
            return Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
        }        
    }
}
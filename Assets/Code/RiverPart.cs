using System;
using of2.DebugTools;
using UnityEngine;

namespace DefaultNamespace
{
    [ExecuteInEditMode]
    public class RiverPart : MonoBehaviour
    {
        public Transform riverPartStart;
        public Transform riverPartEnd;

        private void Update()
        {
#if UNITY_EDITOR
            var rends = GetComponentsInChildren<Renderer>();
            var totalBound = new Bounds(transform.position, Vector3.zero);
            foreach (var rend in rends)
            {
                totalBound.Encapsulate(rend.bounds);
            }
            
            if (riverPartStart != null)
            {
                Debug.DrawLine(riverPartStart.position - 20 * Vector3.right, riverPartStart.position + 20 * Vector3.right, Color.red);

                riverPartStart.position = riverPartStart.position.WithX(totalBound.center.x);
            }

            if (riverPartEnd != null)
            {
                Debug.DrawLine(riverPartEnd.position - 20 * Vector3.right, riverPartEnd.position + 20 * Vector3.right, Color.red);
                riverPartEnd.position = riverPartEnd.position.WithX(totalBound.center.x);
            }
#endif            
        }
    }
}
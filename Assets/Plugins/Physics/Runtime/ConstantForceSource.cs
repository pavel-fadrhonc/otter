using UnityEngine;
using System.Collections;

namespace o2f.Physics
{
    /// <summary>
    /// Put this on trigger collider to apply constant force on objects that are staying inside of that collider.
    /// </summary>
    public class ConstantForceSource : MonoBehaviour
    {
        public Vector3 ForceToApplyLocal; //force to apply on local space - will be normalized
        public float ForceToApplyLocalMagnitude; //world vector of force to apply - magnitude
        public float ForceToApplyRelative; // magnitude of force to apply relative to position of GO
        public LayerMask AffectedLayers;

        private void OnTriggerStay(Collider col)
        {
            if (col.attachedRigidbody != null && (AffectedLayers.value & (1 << col.gameObject.layer)) != 0)
            {
                col.attachedRigidbody.AddForce(transform.TransformDirection(ForceToApplyLocal) * ForceToApplyLocalMagnitude);
                col.attachedRigidbody.AddForce((col.gameObject.transform.position - transform.position).normalized * ForceToApplyRelative);
            }
        }
    }
}



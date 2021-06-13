using UnityEngine;
using System.Collections.Generic;

namespace o2f.Physics
{
    /// <summary>
    /// Currently works only on spherical colliders. 
    /// Also assumes that this has box collider and it's up vector is in general direction of world up (dot(this.up, worldUp) > 0)
    /// Also doesn't account for partial contact (like on corner of box collider) cause it unnecessary cause it doesn't make sense for water.
    /// </summary>
    public class WaterBuoyancy : MonoBehaviour
    {
        private enum eTriggerState
        {
            None = -1,
            Enter,
            Exit,
            Stay
        }

        public LayerMask FilterLayers;
        public string[] filterTags;

        public float damper = 0.1f;
        public GameObject testObject;
        public GameObject testObject2;

        [Tooltip("Density of the water (liquid). Basically scales the buoyancy force.")]
        public float density;

        private BoxCollider m_BoxCollider;

        private List<SphereCollider> m_TriggeringColliders = new List<SphereCollider>();

        protected void Awake()
        {
            m_BoxCollider = GetComponent<BoxCollider>();
        }

        protected void FixedUpdate()
        {
            for (int i = 0; i < m_TriggeringColliders.Count; i++)
            {
                ApplyBuoyancy(m_TriggeringColliders[i]);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var sphereCollider = FilterObject(other.gameObject);
            if (!sphereCollider || other.isTrigger)
                return;

            m_TriggeringColliders.Add(sphereCollider);
        }

        //private void OnTriggerStay(Collider other)
        //{
        //    var sphereCollider = FilterObject(other.gameObject);
        //    if (!sphereCollider)
        //        return;

        //    ApplyBuoyancy(sphereCollider);
        //}

        private void OnTriggerExit(Collider other)
        {
            var sphereCollider = FilterObject(other.gameObject);
            if (!sphereCollider)
                return;

            m_TriggeringColliders.Remove(sphereCollider);
        }

        private void ApplyBuoyancy(SphereCollider col)
        {
            var sphereRigidbody = col.gameObject.GetComponent<Rigidbody>();
            if (!sphereRigidbody)
                return;

            Vector3 closestPoint = Vector3.zero; // closest point on surface of water (box collider) to the center of sphere, used to be BoxCollider.ClosestPoint but that yield weird results
            RaycastHit hitInfo;
            var centerOfSphere = col.gameObject.transform.TransformPoint(col.center);
            var sphereRadius = col.radius * col.gameObject.transform.localScale.x;

            var hit = UnityEngine.Physics.Raycast(centerOfSphere, -1 * transform.up, out hitInfo, sphereRadius, 1 << gameObject.layer);
            if (hit)
                closestPoint = hitInfo.point;
            else
            {
                hit = UnityEngine.Physics.Raycast(centerOfSphere + transform.up.normalized * sphereRadius, -1 * transform.up, out hitInfo, sphereRadius, 1 << gameObject.layer);
                if (hit)
                {
                    closestPoint = hitInfo.point;
                }
            }

            //Debug.DrawLine(centerOfSphere, centerOfSphere + -1 * transform.up * sphereRadius, Color.cyan);
            //Debug.DrawLine(centerOfSphere + transform.up.normalized * sphereRadius, centerOfSphere + -1 * transform.up * sphereRadius, Color.cyan);

            //testObject.transform.position = closestPoint;
            //testObject2.transform.position = centerOfSphere;

            var sphereVolume = (sphereRadius * sphereRadius * sphereRadius) * Mathf.PI * (4f / 3f);
            float sphereVolumeInLiquid = 0;

            if (closestPoint == Vector3.zero)
            { // we're fully in water
                sphereVolumeInLiquid = sphereVolume;

                // actually maybe we're just frame behind, better check if we're in the collider
                if (!m_BoxCollider.OverlapsPoint(centerOfSphere))
                    return;
            }
            else
            { // we're partially in the water
                var toTheContact = closestPoint - centerOfSphere;
                float h = sphereRadius - toTheContact.magnitude;
                sphereVolumeInLiquid = (1f / 3f) * Mathf.PI * h * h * (3 * sphereRadius - h);

                if (Vector3.Dot(toTheContact, transform.up) > 0)
                {
                    sphereVolumeInLiquid = sphereVolume - sphereVolumeInLiquid;
                }
            }

            // apply the Archimedes law minus some damping force to stabilize faster
            var buoyancyForce = density * sphereVolumeInLiquid * UnityEngine.Physics.gravity.normalized * -1;
            
            var dampingForce = -sphereRigidbody.velocity * damper * sphereRigidbody.mass;
            sphereRigidbody.AddForce(buoyancyForce + dampingForce, ForceMode.Force);
            //Debug.Log("Sphere volume total: " + sphereVolume + "  Sphere volume in liquid: " + sphereVolumeInLiquid);
            //Debug.Log("applying buoyancyForce " + buoyancyForce.y);
        }

        private SphereCollider FilterObject(GameObject go)
        {
            bool hasTagOrLayer = false;

            if (((1 << go.layer) & FilterLayers.value) != 0)
                hasTagOrLayer = true;

            for (int i = 0; i < filterTags.Length; i++)
            {
                if (go.CompareTag(filterTags[i]))
                    hasTagOrLayer = true;
            }

            if (!hasTagOrLayer)
                return null;
            else
                return go.GetComponent<SphereCollider>();
        }
    }

}


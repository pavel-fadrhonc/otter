#pragma warning disable 649

using UnityEngine;
using System.Collections;
using OakFramework2.BaseMono;

namespace o2f.Physics
{
    /// <summary>
    /// Whenever this collides with something that has nontrigger collider and nonkinematic rigidbody applies extra force to it
    /// </summary>
    [RequireComponent(typeof(FilteredCollisionEventSender))]
    public class AddForceOnCollision : of2GameObject
    {
        private enum eCollisionType
        {
            Collision,
            Trigger,
            Both
        }

        private FilteredCollisionEventSender m_eventSender;
        
        [Tooltip("Type of collision to react to.")]
        [SerializeField]
        private eCollisionType m_CollisionType;
        [Tooltip("force applied to colliding object")]
        [SerializeField]
        private Vector3 m_Force;
        [SerializeField]
        [Tooltip("force is in local space")]
        private bool m_InLocalSpace;
        [SerializeField]
        [Tooltip("Normalize the force before multiplier by multiplier and scaling by velocity.")]
        private bool m_Normalize;
        [SerializeField]
        [Tooltip("Force magnitude multiplier")]
        private float m_Multiplier = 1.0f;
        [SerializeField]
        [Tooltip("If this has rigidbody, scale the force by it's velocity magnitude.")]
        private bool m_ScaleWithVelocityMagnitude;
        [SerializeField]
        private ForceMode m_ForceMode;
        [SerializeField]
        [Tooltip("The delay between we're able to add any force using the script again.")]
        private float m_HandsOffDelay;

        private bool m_HandsOffActive = false;

        
        protected override void Awake()
        {
            m_eventSender = GetComponent<FilteredCollisionEventSender>();
            m_eventSender.CollisionEnterEvent += OnCollisionEnterEvent;
            m_eventSender.TriggerEnterEvent += OnTriggerEnterEvent;
        }

        private void OnTriggerEnterEvent(Collider collider, GameObject sender)
        {
            if (m_CollisionType == eCollisionType.Collision)
                return;

            ProcessCollision(collider.gameObject);
        }

        private void OnCollisionEnterEvent(Collision collision, GameObject sender)
        {
            if (m_CollisionType == eCollisionType.Trigger)
                return;

            ProcessCollision(collision.gameObject);
        }

        private void ProcessCollision(GameObject go)
        {
            if (m_HandsOffActive)
                return;

            var rb = go.GetComponent<Rigidbody>();
            if (!rb)
                return;

            Vector3 force = m_InLocalSpace ? transform.TransformDirection(m_Force) : m_Force;
            force = m_Normalize ? force.normalized : force;
            force *= m_Multiplier;
            force = m_ScaleWithVelocityMagnitude && cachedRigidbody != null ? force * cachedRigidbody.velocity.magnitude : force;

            rb.AddForce(force, m_ForceMode);

            StartCoroutine(HandsOffProtection());
        }

        private IEnumerator HandsOffProtection()
        {
            m_HandsOffActive = true;

            yield return new WaitForSeconds(m_HandsOffDelay);

            m_HandsOffActive = false;
        }
    }
}
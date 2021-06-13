using System;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class River : MonoBehaviour
    {
        [Tooltip("strength applied to otter to reach target speed.")]
        public float strength;
        [Tooltip("Target speed when reached, not strength is applied anymore.")]
        public float targetSpeed;
        
        private Rigidbody2D _otter1rb;
        private Rigidbody2D _otter2rb;

        private Vector2 _riverVector;
        
        private void Start()
        {
            _otter1rb = Locator.Instance.Otter1.Rigidbody2D;
            _otter2rb = Locator.Instance.Otter2.Rigidbody2D;

            _riverVector = transform.up;
        }

        private void FixedUpdate()
        {
            EvaluateRiverStrength(_otter1rb);
            EvaluateRiverStrength(_otter2rb);
        }

        private void EvaluateRiverStrength(Rigidbody2D rb)
        {
            var projVect1 = (Vector2.Dot(rb.velocity, _riverVector) / _riverVector.magnitude) * _riverVector;
            if (projVect1.magnitude < targetSpeed)
                rb.AddForce(_riverVector * strength * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }
}
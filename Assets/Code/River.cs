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

        [Tooltip("what fraction of strength and what is the target speed for when otter is perpendicular to river stream direction.")]
        public float strengthFractionWhenPerpendicular = 0.5f;

        public float speedMultiplierWhenBehind = 4.0f;
        
        private Rigidbody2D _otter1rb;
        private Rigidbody2D _otter2rb;

        private Vector2 _riverVector;

        private Camera _mainCamera;
        
        private void Start()
        {
            _otter1rb = Locator.Instance.Otter1.Rigidbody2D;
            _otter2rb = Locator.Instance.Otter2.Rigidbody2D;

            _riverVector = transform.up;
            
            _mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            EvaluateRiverStrength(_otter1rb);
            EvaluateRiverStrength(_otter2rb);
        }

        private void EvaluateRiverStrength(Rigidbody2D rb)
        {
            var projVect1 = (Vector2.Dot(rb.velocity, _riverVector) / _riverVector.magnitude) * _riverVector;

            var otterRiverDot = Mathf.Abs(Vector2.Dot(rb.transform.up, _riverVector));

            var otterTargetSpeed =
                Mathf.Lerp(targetSpeed * strengthFractionWhenPerpendicular, targetSpeed, otterRiverDot);
            var otterRiverStrength = Mathf.Lerp(strength * strengthFractionWhenPerpendicular, strength, otterRiverDot);

            if (rb.transform.position.y < _mainCamera.transform.position.y - _mainCamera.orthographicSize)
                otterRiverStrength *= speedMultiplierWhenBehind;

            if (rb.transform.position.y > _mainCamera.transform.position.y + _mainCamera.orthographicSize)
                otterRiverStrength = 0;
            
            if (projVect1.magnitude < otterTargetSpeed)
                rb.AddForce(_riverVector * otterRiverStrength * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }
}
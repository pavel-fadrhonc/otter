using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraController : MonoBehaviour
    {
        public float smoothTime;
        
        private Vector3 _targetPos;
        private Vector3 _currentVelocity;

        private float _xPos;

        private void Start()
        {
            _xPos = transform.position.x;
        }

        private void LateUpdate()
        {
            var otter1Pos = Locator.Instance.Otter1.transform.position;
            var otter2Pos = Locator.Instance.Otter2.transform.position;
            
            _targetPos = (otter2Pos - otter1Pos) * 0.5f + otter1Pos;
            _targetPos = _targetPos.WithX(_xPos);
            
            transform.position = Vector3.SmoothDamp(transform.position.WithZ(0), _targetPos.WithZ(0), ref _currentVelocity, smoothTime) + Vector3.back * 10;
        }
    }
}
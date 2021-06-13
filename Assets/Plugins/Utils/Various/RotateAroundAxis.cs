using UnityEngine;

namespace Animation
{
    [ExecuteInEditMode]
    public class RotateAroundAxis : MonoBehaviour
    {
        [Tooltip("in degrees per second")]
        public float speed;
        public Vector3 axis;

        private void Update()
        {
            transform.Rotate(axis, speed * Time.deltaTime);
        }
    }
}
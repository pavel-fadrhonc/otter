using UnityEngine;

namespace DefaultNamespace
{
    public class GameControls : MonoBehaviour
    {
        [Header("Left otter control")]
        public KeyCode otter1LeftLegControl;
        public KeyCode otter1RightLegControl;
        public KeyCode otter1HandReachControl;
        
        [Header("Right otter control")]
        public KeyCode otter2LeftLegControl;
        public KeyCode otter2RightLegControl;
        public KeyCode otter2HandReachControl;
    }
}
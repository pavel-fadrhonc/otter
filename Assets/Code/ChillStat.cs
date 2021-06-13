using System;
using System.Net;
using UnityEngine;

namespace DefaultNamespace
{
    public class ChillStat : MonoBehaviour
    {
        public float Value
        {
            get;
            private set;
        }
        
        public float naturalGrowth = 0.05f;

        public float joinedGrowthMultiplier = 4f;
        [Tooltip("How much do we stop growing when someone moves.")]
        public float onMoveGrowthStopTime = 3f;

        private float _growthStartTime;

        private void Start()
        {
            Locator.Instance.Otter1.MovedEvent += OnOtterMoved;
            Locator.Instance.Otter2.MovedEvent += OnOtterMoved;
        }

        private void Update()
        {
            if (Time.time < _growthStartTime)
                return;

            Value += naturalGrowth * Time.deltaTime * (Locator.Instance.Otter1.OtterState == EOtterState.Joined ? joinedGrowthMultiplier : 1f);
        }

        private void OnOtterMoved()
        {
            _growthStartTime = Time.time + onMoveGrowthStopTime;
        }
        
    }
}
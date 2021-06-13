using System;
using System.Net;
using UnityEngine;

namespace DefaultNamespace
{
    public class ChillStat : MonoBehaviour
    {
        public float Value
        {
            get => _value;
            private set
            {
                _value = value;
                _value = Mathf.Clamp01(_value);
            }
        }

        public float naturalGrowth = 0.05f;

        public float joinedGrowthMultiplier = 4f;
        [Tooltip("How much do we stop growing when someone moves.")]
        public float onMoveGrowthStopTime = 3f;

        [Tooltip("How much chill do we lose when we hit something.")]
        public float hitGrowthPenalty = 0.05f;

        private float _growthStartTime;
        private float _value;

        private void Start()
        {
            Locator.Instance.Otter1.MovedEvent += OnOtterMoved;
            Locator.Instance.Otter2.MovedEvent += OnOtterMoved;
            
            Locator.Instance.Otter1.HitEvent += OnOtterHit;
            Locator.Instance.Otter2.HitEvent += OnOtterHit;
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

        private void OnOtterHit()
        {
            Value -= hitGrowthPenalty;
        }
    }
}
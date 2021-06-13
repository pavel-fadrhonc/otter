using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace DefaultNamespace
{
    public class ChillMeterEffects : MonoBehaviour
    {
        [Tooltip("At which point during the chill meter do we start lerping into the effects")][Range(0f,1f)]
        private float chillAmountForStart = 0.5f;

        public PostProcessVolume normalPostProcessVolume;
        public PostProcessVolume megaChillPostProcessVolume;

        public AudioClip megaChillAudioClip;

        private AudioSource _megaChillAudioSource;
        
        private void Start()
        {
            _megaChillAudioSource = Locator.Instance.AudioManager.PlayAudio(megaChillAudioClip, 0f, 1f, true);
        }

        private void Update()
        {
            var effectsLerp =
                Mathf.Clamp01((Locator.Instance.ChillStat.Value - chillAmountForStart) / chillAmountForStart);

            _megaChillAudioSource.volume = Mathf.Lerp(0f, 1f, effectsLerp);

            normalPostProcessVolume.weight = 1f - effectsLerp;
            megaChillPostProcessVolume.weight = effectsLerp;
        }
    }
}
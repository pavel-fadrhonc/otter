using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class AudioManager : MonoBehaviour
    {
        [Header("General")]
        public int startPoolSize = 10;

        [Header("River")]
        public AudioClip riverClip;
        public float riverVolume = 0.8f;
        
        [Header("Splash")]
        public AudioClip splashClip;
        public float minSplashDelay;
        public float splashVolume;
        public Vector2 splashPitchSpan = new Vector2(0.8f, 1.2f); 
        
        private List<AudioSource> usedAudioSources = new List<AudioSource>();
        private List<AudioSource> freeAudioSources = new List<AudioSource>();

        private int _lastIdx = 0;

        private float _lastSplashPlayTime;

        private void Start()
        {
            for (int i = 0; i < startPoolSize; i++)
            {
                SpawnAudioClip();
            }   
            
            PlayAudio(riverClip, riverVolume, 1f, true);
        }

        private void Update()
        {
            var notPlaying = usedAudioSources.Where(audio => !audio.isPlaying);
            usedAudioSources.RemoveAll(audio => notPlaying.Contains(audio));
            freeAudioSources.AddRange(notPlaying);
        }

        public void PlaySplashSound()
        {
            if (Time.time < _lastSplashPlayTime + minSplashDelay)
                return;

            PlayAudio(splashClip, splashVolume, UnityEngine.Random.Range(splashPitchSpan.x, splashPitchSpan.y));
            _lastSplashPlayTime = Time.time;
        }

        public void PlayAudio(AudioClip clip, float volume, float pitch = 1.0f, bool loop = false)
        {
            if (freeAudioSources.Count == 0)
                SpawnAudioClip();

            var audioSource = freeAudioSources[0];

            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.Play();

            freeAudioSources.Remove(audioSource);
            usedAudioSources.Add(audioSource);
        }

        private void SpawnAudioClip()
        {
            var clipGo = new GameObject($"AudioClip {_lastIdx++}");
            var clip = clipGo.AddComponent<AudioSource>();
            freeAudioSources.Add(clip);
        }
    }
}
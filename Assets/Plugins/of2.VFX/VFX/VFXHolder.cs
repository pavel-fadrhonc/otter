using System.Collections.Generic;
using OakFramework2.BaseMono;
using UnityEngine;

namespace of2.VFX
{
    public class VFXHolder : of2GameObject
    {
        [SerializeField]
        ParticleSystem[] _coloredParticleSystems = new ParticleSystem[0];
        [SerializeField]
        TrailRenderer[] _coloredTrailRenderers = new TrailRenderer[0];        
        
        [SerializeField]
        private bool _colorStartColor = true;
        [SerializeField]
        private bool _colorColorOverLifeTime = true;
        [SerializeField]
        private bool _colorRenderer = true;

        private Transform _followTransform;
        private Quaternion _followRotation;
        private Vector3 _followOffset;
        private bool _followPositionOnly;
        private ParticleSystem _cachedParticleSystem;
        public ParticleSystem cachedParticleSystem
        {
            get
            {
                // VFXs are structured with an empty game object holder as the parent
                // and one child object, which contains the actual particle system.
                if (_cachedParticleSystem == null)
                    _cachedParticleSystem = cachedTransform.GetChild(0).GetComponent<ParticleSystem>();
                return _cachedParticleSystem;
            }
        }

        bool _hasParticleSystem;

        private Dictionary<ParticleSystem, float> _particleSystemDefaultStartSizes = new Dictionary<ParticleSystem, float>();

        protected override void Awake()
        {
            base.Awake();
            
            _hasParticleSystem = cachedParticleSystem != null;

            foreach (var particleSys in GetComponentsInChildren<ParticleSystem>())
            {
                _particleSystemDefaultStartSizes.Add(particleSys, particleSys.GetStartSize());
            }
        }

        public void Play()
        {
            if (_hasParticleSystem)
            {
                cachedParticleSystem.Play(true);
            }
        }

        public void Play(Vector3 position, Vector3 normal, Transform followTransform, bool followPositionOnly)
        {
            if (followTransform != null)
                _followOffset = position - followTransform.position;

            _followPositionOnly = followPositionOnly;

            cachedTransform.position = position;

            _followRotation = Quaternion.FromToRotation(Vector3.forward, normal);
            cachedTransform.rotation = _followRotation;

            _followTransform = followTransform;

            Play();
        }

        protected void Update()
        {
            if (_followTransform != null)
            {
                cachedTransform.position = _followTransform.position + _followOffset;
                if (!_followPositionOnly)
                {
                    cachedTransform.rotation = _followTransform.rotation * _followRotation;
                }
            }
        }

        public void Stop()
        {
            D.CoreWarningFormat("DEBUG : Particle [{0}] stopped.", name);
            if (_hasParticleSystem)
            {
                cachedParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void SetScale(float scale)
        {
            foreach (var particleSys in GetComponentsInChildren<ParticleSystem>())
            {
                float defaultSize = _particleSystemDefaultStartSizes[particleSys];
                particleSys.SetStartSize(scale * defaultSize);
            }

            foreach (var meshRend in GetComponentsInChildren<MeshRenderer>())
            {
                meshRend.transform.localScale = Vector3.one * scale;
            }

            foreach (var lineRend in GetComponentsInChildren<LineRenderer>())
            {
                Debug.LogWarning("Scaling PRT-LineRenderer is not possible!");
                // lineRend.transform.localScale = scale;
            }
        }

        public float GetDuration()
        {
            if (cachedParticleSystem)
            {
                return cachedParticleSystem.main.duration + cachedParticleSystem.main.startLifetime.constant + cachedParticleSystem.main.startDelay.constant;
            }

            return -1;
        }

        public bool IsLooping()
        {
            if (cachedParticleSystem)
            {
                return cachedParticleSystem.IsLooping();
            }

            return true;
        }

        public void SetColors(Color color)
        {
            for (int i = 0; i < _coloredParticleSystems.Length; i++)
            {
                ParticleSystem s = _coloredParticleSystems[i];
                if (s != null)
                {
                    // start color
                    if (_colorStartColor)
                    {
                        var mainModule = _coloredParticleSystems[i].main;
                        mainModule.startColor = new ParticleSystem.MinMaxGradient(color);
                    }

                    if (_colorColorOverLifeTime)
                    {
                        // color over lifetime
                        var colorOverLifeTimeModule = _coloredParticleSystems[i].colorOverLifetime;
                        if (colorOverLifeTimeModule.color.gradient != null)
                        {
                            var gradient = new Gradient();
                            GradientColorKey[] colorKeys;
                            if (colorOverLifeTimeModule.color.gradient.colorKeys != null)
                            {
                                colorKeys = new GradientColorKey[colorOverLifeTimeModule.color.gradient.colorKeys.Length];

                                for (int j = 0; j < colorOverLifeTimeModule.color.gradient.colorKeys.Length; j++)
                                {
                                    var colorKey = colorOverLifeTimeModule.color.gradient.colorKeys[j];
                                    colorKeys[j] = new GradientColorKey(color, colorKey.time);
                                }

                                gradient.colorKeys = colorKeys;
                                gradient.alphaKeys = colorOverLifeTimeModule.color.gradient.alphaKeys;
                                gradient.mode = colorOverLifeTimeModule.color.gradient.mode;
                                colorOverLifeTimeModule.color = gradient;
                            }
                        }
                        else
                        {
                            colorOverLifeTimeModule.color = new ParticleSystem.MinMaxGradient(color);
                        }
                    }

                    if (_colorRenderer)
                    {
                        var particleRenderer = _coloredParticleSystems[i].gameObject.GetComponent<Renderer>();
                        particleRenderer.material.SetColor("_Color", color);
                    }
                }
            }

            for (int i = 0; i < _coloredTrailRenderers.Length; i++)
            {
                TrailRenderer ts = _coloredTrailRenderers[i];
                if (ts != null)
                {
                    if (_colorStartColor) ts.startColor = color;
                    if (_colorColorOverLifeTime) ts.endColor = color;
                    if (_colorRenderer) ts.material.color = color;
                }
            }
        }
    }
}
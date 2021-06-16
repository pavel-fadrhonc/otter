using System;
using System.Collections;
using of2.Pool;
using UnityEngine;

namespace of2.VFX
{
    public interface IVFXManager
    {
        VFXHolder PlayVFX(VFXList.EVFX ps, VFXManager.PlayConfig playConfig);
        void StopVFX(VFXHolder ps, bool instantKill = false);
        void StopAllVFXs();
    }
    
    [Serializable]
    public class VFXManager : PoolManager, IVFXManager
    {
        public struct PlayConfig
        {
            public Vector3 Position;
            public Vector3 NormalVector;
            public float Duration;
            public bool KeepRunning;
            public Transform FollowTransform;
            public bool FollowPositionOnly;
            
            public PlayConfig GetNewCustom(Vector3 pos, Vector3 normal, float duration = -1f, bool keepRunning = false, Transform followTransform = null,
                bool followPositionOnly = false)
            {
                return new PlayConfig()
                {
                    Position = pos,
                    NormalVector = normal,
                    Duration = duration,
                    KeepRunning = keepRunning,
                    FollowTransform = followTransform,
                    FollowPositionOnly = followPositionOnly
                };
            }
            
            public static PlayConfig GetNewFacingUp(Vector3 pos, float duration = -1f, bool keepRunning = false, Transform followTransform = null,
                bool followPositionOnly = false)
            {
                return new PlayConfig()
                {
                    Position = pos,
                    NormalVector = Vector3.up,
                    Duration = duration,
                    KeepRunning = keepRunning,
                    FollowTransform = followTransform,
                    FollowPositionOnly = followPositionOnly
                };
            }
            
            public static PlayConfig GetNew2DFollowTransform(Vector3 pos,  Transform followTransform = null)
            {
                return new PlayConfig()
                {
                    Position = pos,
                    NormalVector = Vector3.back,
                    Duration = -1,
                    KeepRunning = false,
                    FollowTransform = followTransform,
                    FollowPositionOnly = false
                };
            }
            
            public static PlayConfig GetNew2DFollowPosition(Vector3 pos,  Transform followTransform = null)
            {
                return new PlayConfig()
                {
                    Position = pos,
                    NormalVector = Vector3.back,
                    Duration = -1,
                    KeepRunning = false,
                    FollowTransform = followTransform,
                    FollowPositionOnly = true
                };
            }                                    

            public static PlayConfig Default => new PlayConfig()
            {
                Position = Vector3.zero,
                NormalVector = Vector3.up,
                Duration = -1,
                FollowTransform = null,
                FollowPositionOnly = false,
                KeepRunning = false
            };
        }

        [SerializeField]
        private VFXManagerData _VFXManagerData;
        public VFXManagerData VFXManagerData
        {
            get => _VFXManagerData;
            set => _VFXManagerData = value;
        }

        [SerializeField]
        private int _initialCountOfEach = 4;

        protected override void Awake()
        {
            base.Awake();

            //D.CoreLog("VFX manager start time: " + Time.realtimeSinceStartup);
            for (int i = 0; i < _VFXManagerData.VFXs.Count; i++)
            {
                // Don't put any objects in the pool yet. We only create them once they are needed.
                if (_VFXManagerData.VFXs[i] != null && _VFXManagerData.VFXs[i].VFXPrefab != null)
                    CreatePoolFromPrefab(_VFXManagerData.VFXs[i].VFXPrefab.gameObject, _initialCountOfEach);
            }
            D.CoreLog("Particle manager end time: " + Time.realtimeSinceStartup);
        }

        public VFXHolder PlayVFX(VFXList.EVFX ps, VFXManager.PlayConfig playConfig)
        {
            if (ps == VFXList.EVFX.NONE)
                return null;
            
            string name = VFXList.Get(ps);
            VFXHolder prefab = GetVFXPrefabFromName(name);
            if (prefab == null)
            {
                Debug.LogError("Did not find VFX with name '" + name + "'!");
                return null;
            }

            // If a custom duration is set, use that, otherwise get the duration to whatever the particle system is.
            // Duration of -1 means the VFX will not be auto-returned. Looping particle systems will never be auto-returned.
            float duration = playConfig.Duration;
            if (playConfig.KeepRunning)
            {
                // If 'Keep Running' is enabled, duration is set to -1, to keep the VFX playing.
                duration = -1;
            }
            else
            {
                // If 'Keep Running' is not enabled, and duration is set to negative, we get the duration from the particle system.
                if (duration < 0)
                    duration = prefab.GetDuration();
            }
            GameObject vfxGO = RentObject(prefab.gameObject, duration);
            if (vfxGO != null)
            {
                VFXHolder holder = vfxGO.GetComponent<VFXHolder>();
                if (holder != null)
                {
                    holder.Play(playConfig.Position, playConfig.NormalVector, playConfig.FollowTransform, playConfig.FollowPositionOnly);
                    return holder;
                }
            }
            else
            {
                Debug.LogError("Failed to start VFX! Unable to rent '" + (vfxGO != null ? vfxGO.name : "NULL") + " from vfx pool.", cachedGameObject);
                return null;
            }

            return null;
        }

        public void StopVFX(VFXHolder vfx, bool instantKill = false)
        {
            if (vfx == null)
            {
                Debug.LogError("Trying to stop non-existing VFX.");
                return;
            }
            if (instantKill)
            {
                vfx.Stop();
                ReturnObject(vfx.cachedGameObject);
                return;
            }

            StartCoroutine(ReturnObjectDelayed(vfx.cachedGameObject, vfx));
        }

        public void StopAllVFXs()
        {
            PullBackAllRentedObjects();
        }

        IEnumerator ReturnObjectDelayed(GameObject go, VFXHolder vfx)
        {
            ParticleSystem[] children = vfx.gameObject.GetComponentsInChildren<ParticleSystem>();
            bool[] loops = new bool[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                var main = children[i].main;
                loops[i] = main.loop;
                main.loop = false;
            }
            while (vfx.cachedParticleSystem.IsAlive(true))
                yield return null;

            for (int i = 0; i < children.Length; i++)
            {
                var main = children[i].main;
                main.loop = loops[i];
            }
            vfx.Stop();
            ReturnObject(go);
        }

        private VFXHolder GetVFXPrefabFromName(string name)
        {
            VFXData data = _VFXManagerData.VFXs.Find(p => p.Name == name);
            return data?.VFXPrefab;
        }
    }
}
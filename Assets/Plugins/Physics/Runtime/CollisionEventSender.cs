using System;
using System.Collections.Generic;
using of2.Base;
using UnityEngine;

namespace o2f.Physics
{
    public delegate void CollisionEventHandler(Collision collision, GameObject sender);
    public delegate void CollisionEvent2DHandler(Collision2D collision, GameObject sender);
    public delegate void TriggerEventHandler(Collider collider, GameObject sender);
    public delegate void TriggerEvent2DHandler(Collider2D collider, GameObject sender);

    public class CollisionEventSender : MonoBehaviour, IEnablable
    {
        public bool DisabledCollidersWithThis { get; set; } = true;
        
        public event CollisionEventHandler CollisionEnterEvent;
        public event CollisionEventHandler CollisionStayEvent;
        public event CollisionEventHandler CollisionExitEvent;
        
        public event CollisionEvent2DHandler CollisionEnter2DEvent;
        public event CollisionEvent2DHandler CollisionStay2DEvent;
        public event CollisionEvent2DHandler CollisionExit2DEvent;

        public event TriggerEventHandler TriggerEnterEvent;
        public event TriggerEventHandler TriggerStayEvent;
        public event TriggerEventHandler TriggerExitEvent;
        
        public event TriggerEvent2DHandler TriggerEnter2DEvent;
        public event TriggerEvent2DHandler TriggerStay2DEvent;
        public event TriggerEvent2DHandler TriggerExit2DEvent;

        private List<Collider> _colliders;
        private List<Collider2D> _colliders2D;

        /// <summary>
        /// Since OnDisabled does not always get called when setting MonoBehaviour.enabled = false, this is the only sure way to get the disabling / enabling functionality
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value == enabled) return;
                
                enabled = value;
                if (value)
                    EnableCallback();
                else
                    DisableCallback();
            }
        }

        private void InitColliders()
        {
            if (_colliders == null)
            {
                _colliders = new List<Collider>(GetComponents<Collider>());
                _colliders2D = new List<Collider2D>(GetComponents<Collider2D>());
            }
        }
        
        private void Awake()
        {
            InitColliders();
        }

        private void EnableCallback()
        {
            if (DisabledCollidersWithThis)
            {
                InitColliders();
                
                _colliders.ForEach(c => c.enabled = true);
                _colliders2D.ForEach(c => c.enabled = true);
            }
        }

        private void DisableCallback()
        {
            if (DisabledCollidersWithThis)
            {
                InitColliders();
                
                _colliders.ForEach(c => c.enabled = false);
                _colliders2D.ForEach(c => c.enabled = false);
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (CollisionEnterEvent != null)
                CollisionEnterEvent(collision, gameObject);
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (CollisionStayEvent != null)
                CollisionStayEvent(collision, gameObject);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (CollisionExitEvent != null)
                CollisionExitEvent(collision, gameObject);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (TriggerEnterEvent != null)
                TriggerEnterEvent(collider, gameObject);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            if (TriggerStayEvent != null)
                TriggerStayEvent(collider, gameObject);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            if (TriggerExitEvent != null)
                TriggerExitEvent(collider, gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (CollisionEnter2DEvent != null)
                CollisionEnter2DEvent(collision, gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (CollisionStay2DEvent != null)
                CollisionStay2DEvent(collision, gameObject);
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            if (CollisionExit2DEvent != null)
                CollisionExit2DEvent(collision, gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (TriggerEnter2DEvent != null)
                TriggerEnter2DEvent(collider, gameObject);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            if (TriggerStay2DEvent != null)
                TriggerStay2DEvent(collider, gameObject);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (TriggerExit2DEvent != null)
                TriggerExit2DEvent(collider, gameObject);
        }
    }
}

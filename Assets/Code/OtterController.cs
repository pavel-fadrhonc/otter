﻿using System;
using o2f.Physics;
using UnityEditor.Animations;
using UnityEngine;

namespace DefaultNamespace
{
    public enum EOtterControlType
    {
        Otter1,
        Otter2
    }

    public enum EOtterState
    {
        Separate,
        Connecting,
        Connected
    }
    
    public class OtterController : UnityEngine.MonoBehaviour
    {
        [Header("Settings")]
        public float rotateStrength;

        [Tooltip("when hand colliders touch, with which force are otter attracted to hands")]
        public float connectingForce;

        public float reachHandAnimSpeed = 1.0f;

        [HideInInspector][Tooltip("How far is ok for hands to be apart from each other in order for otters to hold hands and join.")]
        public float handToleranceForHolding;

        [HideInInspector][Tooltip("When connecting and further than this distance from each other we return to Separate state.")]
        public float connectingBreakDistance;

        public EOtterControlType otterControlType;
        
        [Header("References")]
        public Transform rightLegForcePoint;
        public Transform leftLegForcePoint;

        public CollisionEventSender rightHandSender;
        public CollisionEventSender leftHandSender;

        private EOtterState _otterState;
        public EOtterState OtterState
        {
            get => _otterState;
            private set => _otterState = value;
        }

        private Transform _otherConnectingTransform;
        public Transform OtherConnectingTransform
        {
            get => _otherConnectingTransform;
            set => _otherConnectingTransform = value;
        }

        public Rigidbody2D Rigidbody2D => _rigidbody;

        private Transform _thisConnectingTransform;

        private Rigidbody2D _rigidbody;
        private HingeJoint2D _connectJoint;
        private GameControls _gameControls;
        private OtterController _otherOtter;
        
        private Animator _animator;
        private float _reachHandParamValue;

        private const string ReachHandsAnimParamName = "ReachHands";
        private const string LeftLegSwimAnimParamName = "LeftLegSwim";
        private const string RightLegSwimAnimParamName = "RightLegSwim";

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _connectJoint = GetComponent<HingeJoint2D>();
            _animator = GetComponentInChildren<Animator>();

            //_connectJoint.enabled = false;
        }

        private void Start()
        {
            _gameControls = Locator.Instance.GameControls;
            
            rightHandSender.Enabled = false;
            leftHandSender.Enabled = false;
            
            rightHandSender.CollisionEnter2DEvent += OnRightHandTriggerEnterEvent;
            leftHandSender.CollisionEnter2DEvent += OnLeftHandTriggerEnterEvent;
        }

        public void SetConnecting(OtterController withOtter, Transform otherConnectingTransform, Transform yourConnectionTransform)
        {
            _otherOtter = withOtter;
            _otherConnectingTransform = otherConnectingTransform;
            _thisConnectingTransform = yourConnectionTransform;
            ChangeState(EOtterState.Connecting);
        }

        public void SetConnected(bool configureJoint, Transform thisConnectingTransform, Transform otherConnectingTransform, OtterController otherOtter)
        {
            ChangeState(EOtterState.Connected);

            if (configureJoint)
            {
                _connectJoint.enabled = true;
                _connectJoint.connectedBody = otherOtter.GetComponent<Rigidbody2D>();
                _connectJoint.anchor = transform.InverseTransformPoint(thisConnectingTransform.position);
                _connectJoint.connectedAnchor = otherOtter.transform.InverseTransformPoint(otherConnectingTransform.position);
                
                rightHandSender.Enabled = leftHandSender.Enabled = false;
            }
        }
        
        private void OnRightHandTriggerEnterEvent(Collision2D col, GameObject sender)
        {
            ProcessHandCollision(leftHandSender.transform, col.collider);
        }
        
        private void OnLeftHandTriggerEnterEvent(Collision2D col, GameObject sender)
        {
            ProcessHandCollision(leftHandSender.transform, col.collider);
        }

        private void ProcessHandCollision(Transform handTransform, Collider2D theirCollider)
        {
            var otter = theirCollider.gameObject.GetComponentInParent<OtterController>();
            if (otter == null || otter.OtterState != EOtterState.Separate)
                return;
            
            ConnectHands(handTransform, theirCollider.transform,otter);            
        }

        private void FixedUpdate()
        {
            switch (OtterState)
            {
                case EOtterState.Separate:
                    var leftLegControl = otterControlType == EOtterControlType.Otter1
                        ? _gameControls.otter1LeftLegControl
                        : _gameControls.otter2LeftLegControl;
                
                    var rightLegControl = otterControlType == EOtterControlType.Otter1
                        ? _gameControls.otter1RightLegControl
                        : _gameControls.otter2RightLegControl;

                    var handReachControl = otterControlType == EOtterControlType.Otter1
                        ? _gameControls.otter1HandReachControl
                        : _gameControls.otter2HandReachControl;

                    if (Input.GetKeyDown(leftLegControl))
                    {
                        _rigidbody.AddForceAtPosition(leftLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, leftLegForcePoint.position, ForceMode2D.Impulse);
                        _animator.SetTrigger(LeftLegSwimAnimParamName);
                    }

                    if (Input.GetKeyDown(rightLegControl))
                    {
                        _rigidbody.AddForceAtPosition(rightLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, rightLegForcePoint.position , ForceMode2D.Impulse);
                        _animator.SetTrigger(RightLegSwimAnimParamName);
                    }

                    // evaluate hands
                    if (Input.GetKey(handReachControl))
                    {
                        rightHandSender.Enabled = leftHandSender.Enabled = true;
                        _reachHandParamValue = Mathf.Min(_reachHandParamValue + reachHandAnimSpeed * Time.deltaTime, 1.0f);
                    }
                    else
                    {
                        rightHandSender.Enabled = leftHandSender.Enabled = false;
                        _reachHandParamValue = Mathf.Max(_reachHandParamValue - reachHandAnimSpeed * Time.deltaTime, 0.0f);
                    }
                    
                    _animator.SetFloat(ReachHandsAnimParamName, _reachHandParamValue);

                    break;
                
                case EOtterState.Connected:
                    break;
                
                case EOtterState.Connecting:
                    _rigidbody.AddForceAtPosition(((Vector2) (OtherConnectingTransform.position - _thisConnectingTransform.position)).normalized * connectingForce * Time.fixedDeltaTime, _thisConnectingTransform.position, ForceMode2D.Force);
                    var connectingTransformDistance =
                        (OtherConnectingTransform.position - _thisConnectingTransform.position).magnitude;
                    if (connectingTransformDistance < handToleranceForHolding)
                    {
                        SetConnected(true, _thisConnectingTransform, _otherConnectingTransform, _otherOtter);
                        _otherOtter.SetConnected(false, _otherConnectingTransform, _thisConnectingTransform, this);
                    }
                    else if (connectingTransformDistance > connectingBreakDistance)
                    {
                        ChangeState(EOtterState.Separate);
                        _otherOtter.ChangeState(EOtterState.Separate);
                    }

                    break;
            }
        }

        private void ChangeState(EOtterState otterState)
        {
            OtterState = otterState;
            switch (otterState)
            {
                case EOtterState.Separate:
                    break;
                case EOtterState.Connecting:
                    rightHandSender.Enabled = leftHandSender.Enabled = false;
                    break;
                case EOtterState.Connected:

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(otterState), otterState, null);
            }
        }

        private void ConnectHands(Transform ourConnectTransform, Transform theirConnectTransform, OtterController otherOtter)
        {
            _thisConnectingTransform = ourConnectTransform;
            
            SetConnected(true, ourConnectTransform, theirConnectTransform, otherOtter);
            otherOtter.SetConnected(false, theirConnectTransform, ourConnectTransform, this);
            
            // SetConnecting(otherOtter, theirConnectTransform, ourConnectTransform);
            // otherOtter.SetConnecting(this, ourConnectTransform, theirConnectTransform);
        }
    }
}
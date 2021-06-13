using System;
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
        Joining,
        Joined
    }

    public class OtterInput
    {
        public bool LeftLeg;
        public bool RightLeg;
        public bool ReachHands;
    }

    public enum EJoinedType
    {
        Left,
        Right
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

        [Header("Joint setup")] 
        public float jointBreakingForce;
        
        [Header("References")]
        public Transform rightLegForcePoint;
        public Transform leftLegForcePoint;

        public CollisionEventSender rightHandSender;
        public CollisionEventSender leftHandSender;

        public ParticleSystem rightLegRippleParticleSystem;
        public ParticleSystem leftLegRippleParticleSystem;

        private OtterInput _input = new OtterInput();

        private EOtterState _otterState;
        public EOtterState OtterState
        {
            get => _otterState;
            private set => _otterState = value;
        }

        public EJoinedType JoinedType
        {
            get;
            set;
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

            SetupJoint(_connectJoint);

            //_connectJoint.enabled = false;
        }

        private void Start()
        {
            _gameControls = Locator.Instance.GameControls;
            
            rightHandSender.Enabled = false;
            leftHandSender.Enabled = false;
            
            rightHandSender.CollisionEnter2DEvent += OnRightHandTriggerEnterEvent;
            leftHandSender.CollisionEnter2DEvent += OnLeftHandTriggerEnterEvent;
            
            ReadInput();
        }

        public void SetJoining(OtterController withOtter, Transform otherConnectingTransform, Transform yourConnectionTransform)
        {
            _otherOtter = withOtter;
            _otherConnectingTransform = otherConnectingTransform;
            _thisConnectingTransform = yourConnectionTransform;
            ChangeState(EOtterState.Joining);
        }

        public void SetJoined(bool configureJoint, Transform thisConnectingTransform, Transform otherConnectingTransform, OtterController otherOtter, EJoinedType joinedType)
        {
            ChangeState(EOtterState.Joined);
            JoinedType = joinedType;

            if (configureJoint)
            {
                _connectJoint.enabled = true;
                _connectJoint.connectedBody = otherOtter.GetComponent<Rigidbody2D>();
                _connectJoint.anchor = transform.InverseTransformPoint(thisConnectingTransform.position);
                _connectJoint.connectedAnchor = otherOtter.transform.InverseTransformPoint(otherConnectingTransform.position);
                
                rightHandSender.Enabled = leftHandSender.Enabled = false;
            }
        }

        private void OnJointBreak2D(Joint2D joint2D)
        {
            // regenerate joint
            _connectJoint = gameObject.AddComponent<HingeJoint2D>();
            SetupJoint(_connectJoint);
            ChangeState(EOtterState.Separate);
            _otherOtter.ChangeState(EOtterState.Separate);
        }
        
        private void OnRightHandTriggerEnterEvent(Collision2D col, GameObject sender)
        {
            ProcessHandCollision(rightHandSender.transform, col.collider);
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

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            switch (OtterState)
            {
                case EOtterState.Separate:

                    if (_input.LeftLeg)
                    {
                        _rigidbody.AddForceAtPosition(leftLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, leftLegForcePoint.position, ForceMode2D.Impulse);
                        _animator.SetTrigger(LeftLegSwimAnimParamName);
                        leftLegRippleParticleSystem.Play();
                        Locator.Instance.AudioManager.PlaySplashSound();
                    }

                    if (_input.RightLeg)
                    {
                        _rigidbody.AddForceAtPosition(rightLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, rightLegForcePoint.position , ForceMode2D.Impulse);
                        _animator.SetTrigger(RightLegSwimAnimParamName);
                        rightLegRippleParticleSystem.Play();
                        Locator.Instance.AudioManager.PlaySplashSound();
                    }

                    // evaluate hands
                    if (_input.ReachHands)
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
                
                case EOtterState.Joined:
                    if (JoinedType == EJoinedType.Left && _input.LeftLeg)
                    {
                        _rigidbody.AddForceAtPosition(leftLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, leftLegForcePoint.position, ForceMode2D.Impulse);
                        _rigidbody.AddForceAtPosition(leftLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, rightLegForcePoint.position , ForceMode2D.Impulse);
                        
                        _animator.SetTrigger(LeftLegSwimAnimParamName);
                        _animator.SetTrigger(RightLegSwimAnimParamName);
                        
                        leftLegRippleParticleSystem.Play();
                        rightLegRippleParticleSystem.Play();
                        
                        Locator.Instance.AudioManager.PlaySplashSound();
                    }
                    else if (JoinedType == EJoinedType.Right && _input.RightLeg)
                    {
                        _rigidbody.AddForceAtPosition(rightLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, leftLegForcePoint.position, ForceMode2D.Impulse);
                        _rigidbody.AddForceAtPosition(rightLegForcePoint.right * rotateStrength * Time.fixedDeltaTime, rightLegForcePoint.position , ForceMode2D.Impulse);
                        
                        _animator.SetTrigger(LeftLegSwimAnimParamName);
                        _animator.SetTrigger(RightLegSwimAnimParamName);
                        
                        leftLegRippleParticleSystem.Play();
                        rightLegRippleParticleSystem.Play();
                        
                        Locator.Instance.AudioManager.PlaySplashSound();
                    }

                    break;
                
                case EOtterState.Joining:
                    _rigidbody.AddForceAtPosition(((Vector2) (OtherConnectingTransform.position - _thisConnectingTransform.position)).normalized * connectingForce * Time.fixedDeltaTime, _thisConnectingTransform.position, ForceMode2D.Force);
                    var connectingTransformDistance =
                        (OtherConnectingTransform.position - _thisConnectingTransform.position).magnitude;
                    if (connectingTransformDistance < handToleranceForHolding)
                    {
                        // SetJoined(true, _thisConnectingTransform, _otherConnectingTransform, _otherOtter);
                        // _otherOtter.SetJoined(false, _otherConnectingTransform, _thisConnectingTransform, this);
                    }
                    else if (connectingTransformDistance > connectingBreakDistance)
                    {
                        ChangeState(EOtterState.Separate);
                        _otherOtter.ChangeState(EOtterState.Separate);
                    }

                    break;
            }

            _input.LeftLeg = false;
            _input.RightLeg = false;
            //_input.ReachHands = false;
        }

        private void ChangeState(EOtterState otterState)
        {
            OtterState = otterState;
            switch (otterState)
            {
                case EOtterState.Separate:
                    break;
                case EOtterState.Joining:
                    rightHandSender.Enabled = leftHandSender.Enabled = false;
                    break;
                case EOtterState.Joined:

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(otterState), otterState, null);
            }
        }

        private void ReadInput()
        {
            var leftLegControl = otterControlType == EOtterControlType.Otter1
                ? _gameControls.otter1LeftLegControl
                : _gameControls.otter2LeftLegControl;
                
            var rightLegControl = otterControlType == EOtterControlType.Otter1
                ? _gameControls.otter1RightLegControl
                : _gameControls.otter2RightLegControl;

            var handReachControl = otterControlType == EOtterControlType.Otter1
                ? _gameControls.otter1HandReachControl
                : _gameControls.otter2HandReachControl;

            var leftLeg = Input.GetKeyDown(leftLegControl);
            var rightLeg = Input.GetKeyDown(rightLegControl);
            var reachHands = Input.GetKey(handReachControl);

            if (leftLeg && !_input.LeftLeg)
                _input.LeftLeg = true;

            if (rightLeg && !_input.RightLeg)
                _input.RightLeg = true;

            //if (reachHands && !_input.ReachHands)
                _input.ReachHands = reachHands;
        }
    
        private void ConnectHands(Transform ourConnectTransform, Transform theirConnectTransform, OtterController otherOtter)
        {
            _thisConnectingTransform = ourConnectTransform;

            _otherOtter = otherOtter;

            var thisJoinedType = transform.position.x < otherOtter.transform.position.x
                ? EJoinedType.Left
                : EJoinedType.Right;
            var otherJoinedType = thisJoinedType == EJoinedType.Right ? EJoinedType.Left : EJoinedType.Right; 
            
            SetJoined(true, ourConnectTransform, theirConnectTransform, otherOtter, thisJoinedType);
            otherOtter.SetJoined(false, theirConnectTransform, ourConnectTransform, this, otherJoinedType);
            
            // SetConnecting(otherOtter, theirConnectTransform, ourConnectTransform);
            // otherOtter.SetConnecting(this, ourConnectTransform, theirConnectTransform);
        }

        private void SetupJoint(HingeJoint2D joint2D)
        {
            joint2D.enabled = false;
            joint2D.autoConfigureConnectedAnchor = false;
            joint2D.breakForce = jointBreakingForce;
            joint2D.enableCollision = true;
        }
    }
}
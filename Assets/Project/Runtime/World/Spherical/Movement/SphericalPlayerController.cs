using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pokemon.Project.World.Spherical.Movement
{
    /// <summary>PC input adapter and Unity lifecycle for the spherical player motor.</summary>
    [DisallowMultipleComponent]
    public sealed class SphericalPlayerController : MonoBehaviour
    {
        [Header("Movement (metres / seconds)")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 5f;
        [SerializeField, Min(0.1f)] private float groundAcceleration = 24f;
        [SerializeField, Min(0.1f)] private float groundDeceleration = 30f;
        [SerializeField, Range(0f, 1f)] private float airControl = 0.25f;
        [SerializeField, Min(0.1f)] private float gravity = 18f;
        [SerializeField, Min(0.1f)] private float jumpImpulse = 5.5f;
        [SerializeField, Min(0.2f)] private float capsuleHeight = 1.8f;
        [SerializeField, Min(0.1f)] private float maximumWalkableSlope = 45f;
        [SerializeField, Min(0.05f)] private float groundProbeRange = 0.5f;

        private SphericalGeographicFieldSet _fields;
        private SphericalThirdPersonCamera _cameraRig;
        private SphericalPlayerMotor _motor;
        private Vector3 _planetCentre;
        private float _planetRadius;
        private Vector2 _moveInput;
        private bool _jumpRequested;
        private bool _initialized;

        public bool IsGrounded => _motor != null && _motor.IsGrounded;
        public Vector3 Velocity => _motor != null ? _motor.Velocity : Vector3.zero;

        public void Initialize(SphericalGeographicFieldSet fields, Vector3 planetCentre, float planetRadius,
            SphericalThirdPersonCamera cameraRig, float heightMeters)
        {
            _fields = fields;
            _planetCentre = planetCentre;
            _planetRadius = Mathf.Max(1f, planetRadius);
            _cameraRig = cameraRig;
            capsuleHeight = Mathf.Max(0.2f, heightMeters);
            _motor = new SphericalPlayerMotor();
            _initialized = fields != null;
            if (_initialized) ResolveInitialGroundContact();
        }

        private void Update()
        {
            if (!_initialized || Keyboard.current == null) return;
            Keyboard keyboard = Keyboard.current;
            _moveInput = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) _moveInput.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) _moveInput.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) _moveInput.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) _moveInput.x -= 1f;
            _moveInput = Vector2.ClampMagnitude(_moveInput, 1f);
            if (keyboard.spaceKey.wasPressedThisFrame) _jumpRequested = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized || _motor == null) return;
            float dt = Time.fixedDeltaTime;
            Vector3 radialUp = (transform.position - _planetCentre).normalized;
            Vector3 forward, right;
            if (_cameraRig != null) _cameraRig.GetMovementBasis(radialUp, out forward, out right);
            else SphericalSurfaceQuery.GetTangentFrame(radialUp, out forward, out right);

            Vector3 desired = (forward * _moveInput.y + right * _moveInput.x) * walkSpeed;
            desired = Vector3.ProjectOnPlane(desired, radialUp);
            float acceleration = _moveInput.sqrMagnitude > 0.001f ? groundAcceleration : groundDeceleration;
            if (!_motor.IsGrounded) acceleration *= airControl;
            _motor.Accelerate(desired, radialUp, acceleration, dt);

            if (_jumpRequested)
            {
                if (SphericalGroundProbe.TrySample(_fields, _planetCentre, _planetRadius, radialUp, out var jumpGround) &&
                    jumpGround.IsLand && jumpGround.SlopeDegrees <= maximumWalkableSlope)
                    _motor.AddJumpImpulse(jumpGround.Normal, jumpImpulse);
                _jumpRequested = false;
            }

            _motor.IntegrateGravity(_planetCentre, transform.position, gravity, dt);
            Vector3 nextPosition = transform.position;
            _motor.IntegratePosition(ref nextPosition, dt);

            Vector3 nextRadial = (nextPosition - _planetCentre).normalized;
            if (SphericalGroundProbe.TrySample(_fields, _planetCentre, _planetRadius, nextRadial, out var ground))
            {
                float supportRadius = capsuleHeight * 0.5f;
                float signedGap = Vector3.Distance(nextPosition, _planetCentre) -
                                  (Vector3.Distance(ground.SurfacePosition, _planetCentre) + supportRadius);
                bool descending = Vector3.Dot(_motor.Velocity, ground.Normal) <= 0f;
                bool withinProbe = signedGap <= groundProbeRange && signedGap >= -groundProbeRange;

                if (!ground.IsLand)
                {
                    // Water is not a floor. Prevent grounded walking into it; airborne motion remains physical.
                    if (_motor.IsGrounded && _moveInput.sqrMagnitude > 0.001f)
                    {
                        nextPosition = transform.position;
                        _motor.SetVelocity(Vector3.ProjectOnPlane(_motor.Velocity, radialUp));
                    }
                    else
                    {
                        _motor.SetAirborne();
                    }
                }
                else if (ground.SlopeDegrees > maximumWalkableSlope && _motor.IsGrounded && _moveInput.sqrMagnitude > 0.001f)
                {
                    nextPosition = transform.position;
                    _motor.SetVelocity(Vector3.ProjectOnPlane(_motor.Velocity, radialUp));
                }
                else if (withinProbe && descending)
                {
                    nextPosition = ground.SurfacePosition + radialUp * supportRadius;
                    _motor.SetGrounded(ground.Normal);
                }
                else
                {
                    _motor.SetAirborne();
                }

                Vector3 up = (nextPosition - _planetCentre).normalized;
                Vector3 tangentForward = Vector3.ProjectOnPlane(forward, up);
                if (tangentForward.sqrMagnitude < 1e-6f)
                    SphericalSurfaceQuery.GetTangentFrame(up, out tangentForward, out _);
                transform.SetPositionAndRotation(nextPosition, Quaternion.LookRotation(tangentForward.normalized, up));
            }
            else
            {
                _motor.SetAirborne();
                transform.position = nextPosition;
            }
        }

        private void ResolveInitialGroundContact()
        {
            Vector3 radialUp = (transform.position - _planetCentre).normalized;
            if (!SphericalGroundProbe.TrySample(_fields, _planetCentre, _planetRadius, radialUp, out var ground) || !ground.IsLand)
            {
                _motor.SetAirborne();
                return;
            }
            transform.position = ground.SurfacePosition + radialUp * (capsuleHeight * 0.5f + 0.03f);
            _motor.SetGrounded(ground.Normal);
        }

        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0.1f, walkSpeed);
            groundAcceleration = Mathf.Max(0.1f, groundAcceleration);
            groundDeceleration = Mathf.Max(0.1f, groundDeceleration);
            gravity = Mathf.Max(0.1f, gravity);
            jumpImpulse = Mathf.Max(0.1f, jumpImpulse);
            capsuleHeight = Mathf.Max(0.2f, capsuleHeight);
            maximumWalkableSlope = Mathf.Clamp(maximumWalkableSlope, 1f, 89f);
            groundProbeRange = Mathf.Max(0.05f, groundProbeRange);
        }
    }
}

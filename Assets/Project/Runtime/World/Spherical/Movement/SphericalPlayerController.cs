using UnityEngine;
using UnityEngine.InputSystem;

namespace Pokemon.Project.World.Spherical.Movement
{
    /// <summary>
    /// Temporary kinematic capsule controller for WORLD-001. It constrains motion to
    /// the analytic spherical surface and uses a local tangent frame instead of world Y.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float movementSpeedMetersPerSecond = 8f;
        [SerializeField, Min(0.1f)] private float capsuleHeightMeters = 1.8f;

        private SphericalWorldSettings _settings;
        private SphericalThirdPersonCamera _cameraRig;

        public void Initialize(SphericalWorldSettings settings, SphericalThirdPersonCamera cameraRig, float movementSpeed)
        {
            _settings = settings;
            _cameraRig = cameraRig;
            movementSpeedMetersPerSecond = Mathf.Max(0.1f, movementSpeed);
            SnapToSurface();
        }

        private void Update()
        {
            if (_settings == null || Keyboard.current == null)
                return;

            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;

            if (input.sqrMagnitude < 1e-6f)
                return;

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 up = transform.position.normalized;
            Vector3 forward;
            Vector3 right;

            if (_cameraRig != null)
                _cameraRig.GetMovementBasis(up, out forward, out right);
            else
                SphericalSurfaceQuery.GetTangentFrame(up, out forward, out right);

            Vector3 tangentMovement = (forward * input.y + right * input.x).normalized;
            float currentRadius = SphericalSurfaceQuery.GetSurfaceRadius(up, _settings);
            float angularStep = movementSpeedMetersPerSecond * Time.deltaTime / Mathf.Max(1f, currentRadius);
            Vector3 nextDirection = (up + tangentMovement * angularStep).normalized;
            ApplySurfacePosition(nextDirection);
        }

        private void SnapToSurface()
        {
            if (_settings == null)
                return;

            Vector3 direction = transform.position.sqrMagnitude > 1e-6f
                ? transform.position.normalized
                : Vector3.up;
            ApplySurfacePosition(direction);
        }

        private void ApplySurfacePosition(Vector3 direction)
        {
            Vector3 surfacePosition = SphericalSurfaceQuery.GetSurfacePosition(direction, _settings);
            transform.position = surfacePosition + direction * (capsuleHeightMeters * 0.5f);
            Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, direction);
            if (tangentForward.sqrMagnitude < 1e-5f)
            {
                SphericalSurfaceQuery.GetTangentFrame(direction, out tangentForward, out _);
            }
            transform.rotation = Quaternion.LookRotation(tangentForward.normalized, direction);
        }

        public float CapsuleHeightMeters => capsuleHeightMeters;
    }

}

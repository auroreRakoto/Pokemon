using UnityEngine;
using UnityEngine.InputSystem;

namespace Pokemon.Project.World.Spherical.Movement
{
    /// <summary>
    /// Pokémon-style orbit camera aligned to the player's local tangent frame.
    /// PageUp/PageDown adjust debug altitude; the mouse wheel adjusts follow distance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalThirdPersonCamera : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float followDistanceMeters = 9f;
        [SerializeField, Range(10f, 89f)] private float pitchDegrees = 28f;
        [SerializeField, Range(10f, 80f)] private float minimumPitchDegrees = 12f;
        [SerializeField, Range(10f, 89f)] private float maximumPitchDegrees = 78f;
        [SerializeField, Min(0f)] private float debugAltitudeMeters;
        [SerializeField, Min(0.1f)] private float altitudeStepMeters = 500f;
        [SerializeField, Min(0.1f)] private float distanceScrollSpeed = 2f;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;

        private Transform _target;
        private Camera _camera;
        private float _yawDegrees;
        private Vector3 _lastBasisUp;
        private Vector3 _transportedNorth;

        public void Initialize(Transform target, Camera cameraComponent)
        {
            _target = target;
            _camera = cameraComponent;
            if (_target != null)
            {
                _lastBasisUp = _target.position.normalized;
                SphericalSurfaceQuery.GetTangentFrame(_lastBasisUp, out _transportedNorth, out _);
            }
            if (_camera != null)
                _camera.fieldOfView = 60f;
            LateUpdate();
        }

        private void LateUpdate()
        {
            if (_target == null || _camera == null)
                return;

            ReadInput();

            Vector3 up = _target.position.normalized;
            GetMovementBasis(up, out Vector3 forward, out _);
            Vector3 horizontalOffset = -forward * (followDistanceMeters * Mathf.Cos(pitchDegrees * Mathf.Deg2Rad));
            Vector3 verticalOffset = up * (debugAltitudeMeters + followDistanceMeters * Mathf.Sin(pitchDegrees * Mathf.Deg2Rad));
            Vector3 cameraPosition = _target.position + horizontalOffset + verticalOffset;
            Vector3 lookTarget = _target.position + forward * (debugAltitudeMeters * 0.5f);

            transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(lookTarget - cameraPosition, up));
        }

        public float DebugAltitudeMeters => debugAltitudeMeters;

        public void GetMovementBasis(Vector3 up, out Vector3 forward, out Vector3 right)
        {
            up.Normalize();
            if (_lastBasisUp.sqrMagnitude < 1e-6f)
            {
                _lastBasisUp = up;
                SphericalSurfaceQuery.GetTangentFrame(up, out _transportedNorth, out _);
            }
            else if (Vector3.Dot(_lastBasisUp, up) < 0.999999f)
            {
                Quaternion transport = Quaternion.FromToRotation(_lastBasisUp, up);
                _transportedNorth = Vector3.ProjectOnPlane(transport * _transportedNorth, up).normalized;
                if (_transportedNorth.sqrMagnitude < 1e-6f)
                    SphericalSurfaceQuery.GetTangentFrame(up, out _transportedNorth, out _);
                _lastBasisUp = up;
            }

            forward = (Quaternion.AngleAxis(_yawDegrees, up) * _transportedNorth).normalized;
            right = Vector3.Cross(up, forward).normalized;
        }

        private void ReadInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    _yawDegrees += delta.x * mouseSensitivity;
                    pitchDegrees = Mathf.Clamp(pitchDegrees - delta.y * mouseSensitivity, minimumPitchDegrees, maximumPitchDegrees);
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    followDistanceMeters = Mathf.Clamp(followDistanceMeters - scroll * 0.01f * distanceScrollSpeed, 3f, 80f);
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.pageUpKey.wasPressedThisFrame)
                debugAltitudeMeters = Mathf.Min(debugAltitudeMeters + altitudeStepMeters, 20000f);
            if (keyboard.pageDownKey.wasPressedThisFrame)
                debugAltitudeMeters = Mathf.Max(0f, debugAltitudeMeters - altitudeStepMeters);
            if (keyboard.homeKey.wasPressedThisFrame)
                debugAltitudeMeters = 0f;
        }
    }
}

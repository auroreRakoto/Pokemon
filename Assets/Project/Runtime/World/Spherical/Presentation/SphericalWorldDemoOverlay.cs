using Pokemon.Project.World.Spherical.Movement;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Presentation
{
    /// <summary>Small, temporary on-screen control guide for the WORLD-001 prototype.</summary>
    [DisallowMultipleComponent]
    public sealed class SphericalWorldDemoOverlay : MonoBehaviour
    {
        private SphericalThirdPersonCamera _cameraRig;
        private int _seed;
        private float _radiusMeters;

        public void Initialize(SphericalThirdPersonCamera cameraRig, int seed, float radiusMeters)
        {
            _cameraRig = cameraRig;
            _seed = seed;
            _radiusMeters = radiusMeters;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 340f, 150f), GUI.skin.box);
            GUILayout.Label("WORLD-001 | SPHERICAL WORLD PROTOTYPE");
            GUILayout.Label("Move: WASD / Arrow keys");
            GUILayout.Label("Orbit camera: hold Right Mouse + move mouse");
            GUILayout.Label("Zoom: Mouse Wheel | Altitude: Page Up / Page Down");
            GUILayout.Label("Reset altitude: Home");
            GUILayout.Label($"Seed: {_seed} | Radius: {_radiusMeters:N0} m");
            GUILayout.Label($"Debug camera altitude: {(_cameraRig != null ? _cameraRig.DebugAltitudeMeters : 0f):N0} m");
            GUILayout.EndArea();
        }
    }
}

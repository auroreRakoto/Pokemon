using Pokemon.Project.World.Spherical.Movement;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Inspection
{
    /// <summary>
    /// Small runtime HUD for the temporary spherical-world inspection scene.
    /// Keeps the scene's documented controls and generation parameters visible
    /// without introducing a dependency on a UI package or input action asset.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SphericalWorldDemoOverlay : MonoBehaviour
    {
        private SphericalThirdPersonCamera _cameraRig;
        private int _seed;
        private float _planetRadiusMeters;
        private GUIStyle _panelStyle;
        private GUIStyle _headingStyle;
        private GUIStyle _bodyStyle;

        public void Initialize(SphericalThirdPersonCamera cameraRig, int seed, float planetRadiusMeters)
        {
            _cameraRig = cameraRig;
            _seed = seed;
            _planetRadiusMeters = Mathf.Max(0f, planetRadiusMeters);
        }

        private void OnGUI()
        {
            EnsureStyles();

            const float width = 330f;
            const float height = 196f;
            GUILayout.BeginArea(new Rect(16f, 16f, width, height), _panelStyle);
            GUILayout.Label("SPHERICAL WORLD — INSPECTION", _headingStyle);
            GUILayout.Space(5f);
            GUILayout.Label($"Seed: {_seed}", _bodyStyle);
            GUILayout.Label($"Planet radius: {_planetRadiusMeters / 1000f:F1} km", _bodyStyle);
            GUILayout.Space(7f);
            GUILayout.Label("WASD / Arrows   Move", _bodyStyle);
            GUILayout.Label("Space                 Jump", _bodyStyle);
            GUILayout.Label("Right mouse       Orbit camera", _bodyStyle);
            GUILayout.Label("Mouse wheel       Zoom", _bodyStyle);
            GUILayout.Label("Page Up / Down  Debug altitude", _bodyStyle);
            GUILayout.Label("Home                  Reset altitude", _bodyStyle);
            if (_cameraRig == null)
                GUILayout.Label("Camera rig not initialized", _bodyStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_panelStyle != null)
                return;

            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 10, 10)
            };
            _headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                normal = { textColor = Color.white }
            };
            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
        }
    }
}

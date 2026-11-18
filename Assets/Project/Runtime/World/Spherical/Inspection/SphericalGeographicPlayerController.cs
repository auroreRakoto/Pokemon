using System;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Inspection
{
    /// <summary>Compatibility marker for the temporary WORLD-003 controller, replaced by WORLD-004.</summary>
    [Obsolete("Use Pokemon.Project.World.Spherical.Movement.SphericalPlayerController.")]
    [DisallowMultipleComponent]
    public sealed class SphericalGeographicPlayerController : MonoBehaviour
    {
    }
}

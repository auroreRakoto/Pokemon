using System.Collections.Generic;
using UnityEngine;

namespace Pokemon.Project.World.Spherical.Environment
{
    /// <summary>Small Unity asset linking the editable JSON document to the prefab catalog.</summary>
    [CreateAssetMenu(menuName = "Pokemon/World/Environment Placement Config", fileName = "CFG_EnvironmentPlacement")]
    public sealed class SphericalEnvironmentPlacementConfig : ScriptableObject
    {
        [SerializeField] private TextAsset placementJson;
        [SerializeField] private SphericalEnvironmentPrefabCatalog prefabCatalog;

        public bool TryLoad(out SphericalEnvironmentPlacementJson.ResolvedDocument resolved, out List<string> issues)
            => SphericalEnvironmentPlacementJson.TryLoad(placementJson, prefabCatalog, out resolved, out issues);
    }
}

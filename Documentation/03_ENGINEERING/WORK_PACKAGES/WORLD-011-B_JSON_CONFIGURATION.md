# WORLD-011-B — JSON Placement Configuration

- **Status:** IMPLEMENTED IN SOURCE PACKAGE — Unity compilation and EditMode execution pending user-side verification.
- **Scope:** replace serialized per-biome placement rules with a JSON document plus a Unity prefab catalog.
- **Biome scope:** Plains and Forest only.
- **Schema:** version 1.

## 1. Architecture

1. `CFG_SphericalEnvironmentPlacement.json` is the source of truth for placement parameters: biome enablement, rule enablement, weights, target density, spacing, transform ranges, surface offsets, exclusion radii and geographic filters.
2. `SphericalEnvironmentPrefabCatalog` is a Unity `ScriptableObject` containing stable ID → prefab references. JSON never stores Unity asset paths or GUIDs.
3. `SphericalEnvironmentPlacementConfig` is a small Unity asset that references the JSON `TextAsset` and the prefab catalog.
4. `SphericalEnvironmentPlacementJson.TryLoad` parses the document, validates schema and values, resolves enabled rules to prefab references and returns a runtime-only resolved representation.

## 2. Paths

- Runtime: `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPrefabCatalog.cs`
- Runtime: `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementJson.cs`
- Runtime: `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementConfig.cs`
- JSON: `Assets/Project/Content/Settings/World/EnvironmentPlacement/CFG_SphericalEnvironmentPlacement.json`
- Tests: `Assets/Project/Tests/EditMode/World/Spherical/Environment/SphericalEnvironmentPlacementSettingsTests.cs`

## 3. Stable prefab IDs

The JSON currently lists IDs for the known production environment models: `grass_short`, `grass_wild`, `grass_tall`, `reed_clump`, `leaf_litter`, `mushroom_red`, `fallen_log`, `tree_stump`, `stone_cluster_small`, and `stone_cluster_large`.

These IDs are logical keys, not inferred prefab paths. Assign the actual generated prefab assets to matching entries in `CAT_EnvironmentPrefabs`. Do not assume prefab output paths from the older source archive; the current generated prefab hierarchy was not included in the inspected snapshot.

## 4. Safe initial state

All sample rules in the JSON are deliberately `enabled: false` and have `targetDensityPerSquareKilometre: 0`. This prevents accidental mass placement before the real prefab bounds, pivots and physical scales are checked. Numeric ranges are initial editable placeholders, not final art-direction decisions.

The runtime loader only requires a prefab reference for an enabled rule. Disabled rules can be configured before their prefab is assigned.

## 5. Unity setup — automatic

1. Import the files preserving their paths and let Unity compile.
2. Ensure the generated environment prefabs exist under `Assets/Project/Content/Prefabs/Environment/`, using the folder/name convention created by `EnvironmentPrefabGeneratorWindow`.
3. Run **Tools → World → Generate Environment Placement Assets**.
4. The tool creates or refreshes `CAT_EnvironmentPrefabs.asset` and `CFG_EnvironmentPlacement.asset` beside the JSON, then assigns every matching prefab reference automatically.
5. Review the completion dialog. Missing prefabs are listed with their expected paths and their catalog entries remain empty.
6. Keep all rules disabled until prefab audit and placement tuning are complete.

The generator is idempotent: running it again refreshes the catalog references and config links without requiring manual Inspector work. It does not overwrite the JSON rule values.

## 6. Migration from the previous B implementation

The previous package introduced `SphericalEnvironmentPlacementSettings.cs`, which stored all rules and prefab references directly in one ScriptableObject. Remove that file from the project when applying this replacement; it is superseded by the JSON/catalog architecture. Replace its old test file with the new JSON loader tests at the path listed above. If an old `.asset` was created from the superseded type, remove it after confirming it is not referenced by a scene or another asset.

## 7. Validation

The included EditMode tests cover valid disabled rules, missing references for enabled rules, unsupported schema/biomes, duplicate biome definitions and invalid numeric ranges. These tests have been authored but have **not** been run in Unity in this environment. User-side Unity compilation and Test Runner results are required before marking WORLD-011-B validated.

## 8. Commit

After Unity compilation and all included tests pass:

`feat(world): load environment placement rules from JSON`

Until then, use a checkpoint commit message that states the implementation is added but Unity validation is pending.

## 9. Automatic asset setup tool

- Editor tool: `Assets/Project/Editor/Tools/World/EnvironmentPlacementAssetGenerator.cs`
- Menu: `Tools/World/Generate Environment Placement Assets`
- It resolves the ten known IDs against the exact output paths from the environment prefab generator convention.
- It creates/updates both ScriptableObject assets and assigns the JSON/catalog references automatically.
- Unity execution is still required to validate the Editor API calls and actual asset paths.

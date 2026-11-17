# Existing Planar World Generation — Test Plan

**Important:** the inspected project contains a planar `World/Generation` implementation. It is separate from the spherical WORLD-002 pipeline and must not be used as a substitute for spherical seam-safe hydrology.

## A. Settings and deterministic generation

Source:
- `WorldGenerationSettings`
- `WorldGenerationConfig`
- `WorldGenerationConfigLoader`
- `WorldGenerator`

Required EditMode tests:
1. `SettingsValidation_AcceptsValidConfiguration`
2. `SettingsValidation_RejectsResolutionBelowMinimum`
3. `SettingsValidation_RejectsNonPositiveWorldDimensions`
4. `SettingsValidation_RejectsNegativeHeightScale`
5. `SettingsValidation_RejectsUnorderedSettlementThresholds`
6. `Generate_SameSeedAndSettingsProducesIdenticalFields`
7. `Generate_DifferentSeedsChangeAtLeastOneField`
8. `Generate_AllArraysMatchResolutionSquared`
9. `Generate_AllNumericFieldsAreFinite`
10. `Generate_NormalizedFieldsStayWithinZeroAndOne`
11. `Generate_HeightIsScaledToConfiguredMeters`
12. `Generate_ProducesOnlyDefinedBiomeAndWaterTypes`

## B. Hydrology, climate and ecological fields

`WorldGenerator.Generate` currently constructs height, temperature, water mask/type, distance-to-water, moisture, vegetation suitability, biome, settlement suitability, traversal cost, road mask, settlement sites and road routes.

Required EditMode tests:
- Ocean classification follows `oceanLevel`.
- Lakes and rivers use valid enum values and consistent water masks.
- River cells are connected along the generated route where the algorithm produces a route; no route index leaves grid bounds.
- Distance-to-water is zero on water cells and non-negative elsewhere.
- Moisture decreases consistently with increasing distance-to-water for controlled fixtures, allowing local climate terms where relevant.
- Vegetation suitability and settlement suitability remain in `[0,1]`.
- Water cells and slopes beyond walkability/cliff thresholds receive the documented traversal exclusion/cost.
- Biome classification obeys the configured/current classification precedence.
- Settlement count does not exceed `maxSettlements`; sites satisfy separation and non-water constraints.
- Road routes reference valid settlement IDs and valid grid cells; roads do not cross prohibited traversal cells.
- Border cells and minimum supported resolution do not produce out-of-range indices.

Where the algorithm uses private static methods, test through `WorldGenerator.Generate` or extract pure internal helpers only if justified; do not make implementation details public solely for testing.

## C. Result data and coordinates

Source: `WorldGenerationResult`.

Required EditMode tests:
- `Index(x,z)` maps row-major coordinates correctly.
- `CellToWorld` maps first, last and centre cells to expected world coordinates.
- All output arrays have the same `Width × Height` length.
- Invalid coordinates have explicit failure behaviour.
- Result arrays are not accidentally aliased or modified by consumers if immutability is part of the contract.

## D. Terrain mesh and runtime scene integration

Source: `WorldTerrainBuilder`, `GeneratedWorldSurface`.

Required EditMode tests:
- `BuildMesh` rejects null result, dimensions below 2 and negative vertical exaggeration.
- Mesh vertex/index counts match result dimensions.
- Triangle indices are valid; winding and normals face upward.
- UVs span the expected normalized grid range.
- Vertex colors correspond to water, biome and road-mask categories.
- Bounds contain every vertex.
- Large meshes select 32-bit index format when vertex count exceeds 65,535.

Required PlayMode tests:
- `CreateSceneObject` creates MeshFilter, MeshRenderer, optional MeshCollider and ownership tracker.
- `ApplyToScene` creates when target is null and reuses an existing target otherwise.
- Repeated application replaces prior generated mesh without duplicating components.
- Collider is refreshed when enabled and removed when disabled.
- Externally supplied materials are not destroyed by generated-object cleanup.
- Runtime-created material and mesh are released when the generated root is destroyed.
- Replacing an owned mesh/material releases the previous owned resource.
- Parent and object name are applied as requested.
- Missing shader fails with an actionable exception and does not leave orphaned objects/resources.

## E. Editor and configuration tooling

Source:
- `WorldGenerationWindow`
- `WorldGenerationConfigBuildProcessor`
- `WorldGenerationConfigLoader`

Required Editor tests/manual checks:
- Editor window loads valid config and reports missing config clearly.
- Generation is repeatable for same seed/settings.
- Repeated generate/clear does not duplicate scene objects or assets.
- Build processor includes or validates required configuration.
- Build fails with actionable diagnostics when mandatory config is missing/invalid.
- Config loader handles missing, malformed and valid config according to an explicit fallback policy.

These are gaps in the inspected automated test inventory; do not report them as implemented tests until test files and results exist.

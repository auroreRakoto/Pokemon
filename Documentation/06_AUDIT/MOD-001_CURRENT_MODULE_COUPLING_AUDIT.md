# MOD-001 — Current module coupling audit

- **Status:** STATIC AUDIT
- **Source:** owner-supplied `Pokemon-clean.zip`
- **SHA-256:** `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`

## Confirmed current structure

The spherical world is currently grouped in one Runtime assembly:
`Assets/Project/Runtime/World/Spherical/Pokemon.Project.World.Spherical.asmdef`

This assembly references `Unity.InputSystem`. Its code is organized into:
- topology;
- geography;
- Studio/profile;
- presentation;
- environment placement;
- movement;
- inspection/composition.

Runtime optionality is not currently represented by separate module assemblies. That is not automatically a defect: runtime module activation should be designed at the composition/lifecycle layer first. Assembly splitting is not authorized by this audit.

## Coupling points found

1. `SphericalPlanetStudioSettings` has independently enabled relief layers, but no general module configuration.
2. `SphericalWorldProfile` schema version 1 serializes world generation values, but no module selection.
3. `SphericalGeographicFieldGenerator.Generate` always creates biome arrays and classifies biomes.
4. `SphericalGeographicFieldSet` stores biome arrays and exposes `GetBiome` / `SampleBiome` as part of the field contract.
5. `SphericalBiomeInspectionBootstrap.GenerateInspectionWorld` composes field generation, requested biome spawn selection, environment placement and biome surface presentation in one path.
6. `SphericalEnvironmentPlacementGenerator.Generate` uses water and biome data when accepting/rejecting candidates.
7. Player movement consumes the geographic field set through `SphericalGroundProbe` and uses the existing third-person camera basis. The player must be removable at composition time without making World Core depend on it.
8. The current World Runtime has one assembly definition. Do not create one assembly per feature unless the reviewed dependency graph demonstrates a need.

## Owner-provided test evidence

The Test Runner screenshot shows:
- Movement EditMode assembly: 10 tests, green.
- Overall World/Spherical EditMode group: 4 failures, all under `SphericalBiomeTextureGeneratorTests`.
- Failure details were not included, so root causes are unknown.

These failures are independent of the requested optional-module architecture and must not be hidden or attributed to MOD-001 without evidence.

## No changes made

No Runtime code, scenes, prefabs, profile assets, assembly definitions, `.meta` files or GUIDs are changed by this audit.

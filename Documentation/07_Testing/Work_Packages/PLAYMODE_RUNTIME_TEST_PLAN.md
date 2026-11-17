# PlayMode Test Plan — Implemented Runtime Components

This plan complements the initial four `SphericalPlanetStudioPlayModeTests`. It focuses on runtime components already present in the inspected source snapshot.

## Existing PlayMode smoke tests

- `Runtime_GeneratesSixValidGlobeFaceMeshes`
- `Runtime_GeneratesValidPlanisphereMesh`
- `Runtime_GeneratedGlobeMeshCanBeAttachedToSceneAndCleanedUp`
- `Runtime_GenerationCanBeRepeatedWithoutReusingDestroyedMeshes`

Owner reports that the current PlayMode tests pass.

## Next required PlayMode tests

| Component | Test | Acceptance criterion |
|---|---|---|
| `SphericalPlanetPresenter` | Build/clear/rebuild | No duplicate roots; only owned generated content is removed |
| `SphericalWorldDemoBootstrap` | Generate then clear | Demo creates expected runtime objects and removes them safely |
| `SphericalPlayerController` | Move on sphere | Player remains constrained to the spherical surface and local up follows radial direction |
| `SphericalThirdPersonCamera` | Follow target | Camera tracks target and maintains configured orbit behaviour |
| Camera/controller | Near-pole movement | No NaN transforms, sudden 180-degree flips or invalid movement basis |
| `WorldTerrainBuilder.CreateSceneObject` | Runtime scene creation | MeshFilter/Renderer/optional Collider are correctly configured |
| `WorldTerrainBuilder.ApplyToScene` | Runtime regeneration | Existing target is reused; mesh/collider updated; no duplicate components |
| `GeneratedWorldSurface` | Resource cleanup | Owned mesh/material are destroyed on replacement and root destruction |
| Runtime generation | Repeated generate/clear | No orphaned objects, meshes, colliders or owned materials |
| Runtime scene | Scene unload/reload | Generated content and runtime references do not leak across scene lifecycle |

## Test design requirements

- Use a minimal deterministic scene fixture.
- Keep resolutions small for ordinary CI tests; separate high-resolution performance tests.
- Yield frames only when validating Unity lifecycle, deferred destruction or physics.
- Assert observable behaviour through public components and scene state.
- Always clean up objects created by the test, even after assertion failure.
- Never use deprecated Unity APIs in tests.

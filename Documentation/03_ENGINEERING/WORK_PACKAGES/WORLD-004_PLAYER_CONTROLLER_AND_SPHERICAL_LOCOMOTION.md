# WORLD-004 — Player Controller & Spherical Locomotion

- **Feature ID:** WORLD-004
- **Owning domain:** World / Movement
- **Status:** IMPLEMENTATION IN PROGRESS — first runtime slice authored; Unity validation pending
- **Related work:** WORLD-001 spherical foundation and camera; WORLD-002 geographic fields; WORLD-003 temporary playable biome inspection
- **Target runtime:** Unity 6.4, URP, PC, Input System package already used by the project

## Purpose and player-visible outcome

Replace the temporary WORLD-003 inspection-only capsule controller with the first reusable player locomotion implementation for the spherical game world. The player must be able to start on generated land, walk over the actual WORLD-002 surface, leave the ground and fall under spherical gravity, jump, and remain correctly oriented as the local up direction changes around the planet.

The result is the first gameplay movement foundation, not a complete trainer controller. It must be reusable by later player presentation, Pokémon interaction, traversal abilities and session composition without coupling movement rules to the inspection scene.

## Current implementation findings

The current `SphericalGeographicPlayerController` is in `Assets/Project/Runtime/World/Spherical/Inspection/` and is explicitly temporary. It:

- reads keyboard input directly in `Update`;
- changes the radial direction by a small tangent step;
- stores jump height as a scalar offset;
- recomputes the player position directly from the sampled altitude every frame;
- has no independent world-space velocity, ground contact solver, falling state, collision response or slope blocking.

Consequently, the player is pinned to the sampled surface and cannot genuinely fall. WORLD-003 intentionally creates no planet-scale `MeshCollider`, because those six face meshes are too coarse for stable physics queries and produce Unity's large-triangle warnings.

Existing reusable pieces:
- `SphericalGeographicFieldSet` exposes bilinear continuous sampling for altitude and slope, plus nearest-cell categorical sampling for water and biome.
- `SphericalSurfaceQuery.GetTangentFrame` supplies a stable local tangent basis.
- `SphericalThirdPersonCamera` already implements a spherical third-person follow camera and camera-relative movement basis. Reuse and extend it only where required; do not create a second competing camera system.
- `SphericalBiomeInspectionBootstrap` is a temporary composition root and must not become the owner of production movement logic.

## Scope

### Included

1. A production-oriented spherical player motor independent of WORLD-003 scene setup.
2. Configurable walking speed, acceleration/deceleration, jump impulse, radial gravity, capsule dimensions, ground probe distance and maximum walkable slope.
3. Tangent-plane movement relative to camera heading, transported smoothly as the player moves over the globe.
4. Real velocity integration in world space, including radial gravitational acceleration toward the planet centre.
5. Field-aware ground probing against the exact WORLD-002 `SphericalGeographicFieldSet` used to render the planet.
6. Ground contact correction, falling, landing and jumping; no artificial surface pinning.
7. Slope evaluation and a configurable walkability limit. Non-walkable slopes block uphill movement; falling/ground contact remain stable.
8. Water-aware movement policy: ocean/lake cells are not treated as solid ground. In this first milestone, entering water is blocked at the ground-probe/movement boundary; swimming and water locomotion are explicitly deferred.
9. Spherical character orientation using the local ground normal (with radial direction as the stable fallback).
10. Integration into the WORLD-003 playable inspection scene so the developer can test the same generated globe immediately.
11. Automated EditMode tests for pure movement math and PlayMode tests for runtime movement/grounding integration.
12. Documentation and a reproducible validation checklist.

### Excluded

- Final character model, rig, animation controller, foot IK or animation blending.
- Swimming, diving, flying, climbing, riding or species-specific traversal.
- Combat, capture, trainer interaction, Pokémon-following or inventory.
- Multiplayer replication, prediction, server authority and persistence.
- Dynamic destruction/deformation of terrain.
- Full physical collision against trees, rocks, buildings or other world props.
- A final chunk-streamed terrain collider system. The ground probe in this work package samples the shared geographic field data; collider-backed obstacle handling is a later integration.
- Replacing the existing spherical third-person camera wholesale.

## Behaviour rules

### Coordinate and surface model

- Planet centre is the origin of the generated inspection planet's local coordinate frame.
- Player radial up is `normalize(playerPosition - planetCentre)`.
- Terrain radius in a direction is `planetRadiusMeters + sampledAltitudeMeters` from the same `SphericalGeographicFieldSet` used by the renderer.
- Continuous altitude and slope are sampled with `SphericalGeographicFieldSet.Sample`.
- Water and biome are categorical and must use `SampleWaterClass` / `SampleBiome`; never interpolate enum values.
- The motor must not read the older WORLD-001 height sampler as a fallback.
- Tangential movement is projected onto the local ground tangent plane. The camera's transported basis supplies heading; movement must not introduce abrupt north/east flips at cube-face seams or near poles.

### Gravity, movement and ground contact

- Store player velocity in world-space metres per second; do not represent jumping only as an independent height scalar.
- Apply gravity as an acceleration toward the planet centre while airborne.
- Integrate input acceleration and velocity with `Time.fixedDeltaTime` in a deterministic physics/update phase; do not mix movement integration between `Update` and `LateUpdate`.
- Use a custom field-aware ground probe because the current world meshes intentionally have no physics colliders. Probe along the local radial direction and evaluate the terrain surface at the probe direction.
- Ground contact must resolve the capsule's support distance against the sampled terrain surface and remove only velocity directed into the ground. It must not snap a player who is clearly airborne back onto the surface.
- Landing occurs when the player is descending and the probe detects the surface within the configured contact range.
- Jump is accepted only while grounded; jump adds an impulse along the current ground normal.
- Ground normal should be estimated from neighbouring sampled surface positions (or an equivalent documented finite-difference method), with safe fallback to radial up if samples are degenerate. Do not assume radial up equals the terrain normal on steep slopes.
- A walkable slope is determined from the angle between ground normal and radial up (or the equivalent sampled slope field, after validating its convention). The implementation must document which value is authoritative and test it against the WORLD-002 `SlopeDegrees` field.
- The maximum walkable slope is configurable. The initial default is 45 degrees as a tuning starting point, not an approved final gameplay value; keep it exposed for playtesting.

### Water and edge behaviour

- Ocean and lake classifications are not ground. The motor must not land on or walk over them as if they were solid terrain.
- First implementation blocks movement into water and preserves the last valid land position. If the player is already over water due to spawn/teleport/test setup, report a clear invalid-ground state rather than silently snapping onto water.
- River cells are treated as water for this first movement pass; ford/bridge traversal is deferred.
- No invisible planet boundary is introduced. The spherical surface remains continuous; water is a gameplay traversal constraint, not a world edge.
- If no valid ground sample can be obtained, retain velocity and continue falling under gravity rather than teleporting to a default position.

### Camera and input

- Reuse `SphericalThirdPersonCamera` and its transported tangent basis.
- Preserve the confirmed Pokémon-style third-person camera at a distance with adjustable viewing angle.
- Movement input remains on the already installed Unity Input System. For this initial PC slice, WASD/arrow keys, Space, right mouse drag and wheel zoom remain supported.
- Camera input and player movement must have distinct ownership; the motor consumes a movement intent/basis and must not read mouse input itself.
- Input rebinding, gamepad mappings and accessibility options are deferred, but the input source should not be hard-wired into the motor.

## Proposed runtime responsibilities

Keep each responsibility narrow. Proposed files are targets, subject to the implementation review:

- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerMotor.cs` — world-space velocity integration, gravity, jump and movement state.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalGroundProbe.cs` — field-aware surface position, normal, water and slope query.
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerController.cs` — Unity input adapter and motor orchestration; no terrain algorithm duplication.
- Existing `SphericalThirdPersonCamera.cs` — camera follow and transported tangent frame; extend only if the approved controller requires a narrow interface change.
- WORLD-003 bootstrap — composition only: creates the player, supplies fields/settings, configures the camera and input adapter. It must not contain movement physics.

Keep the motor's calculation core testable without a scene where practical. Avoid adding a new package. Do not add a global event bus. Do not make the player motor depend on `UnityEditor`.

## Terrain collision strategy and limitation

The six current full-face preview meshes must remain without `MeshCollider`; do not reintroduce them as a shortcut. WORLD-004 uses the generated field set as the authoritative terrain surface for ground contact. This gives reliable terrain following and falling on the currently generated static planet without giant colliders.

This does not yet provide physical collision against arbitrary geometry (rocks, trees, structures) or dynamic terrain. Those require spatially bounded collider chunks or another validated collision backend, with triangle sizes and update costs measured against the actual planet scale. Record this limitation in runtime diagnostics and documentation rather than implying full physics collision exists.

## Data and configuration

All tuning values must be serialized/configurable and validated. Initial proposed defaults:

| Setting | Initial default | Unit / note |
|---|---:|---|
| Walk speed | 5.0 | m/s; tuning starting point |
| Ground acceleration | 24 | m/s²; tuning starting point |
| Ground deceleration | 30 | m/s²; tuning starting point |
| Air control factor | 0.25 | normalized fraction of ground control |
| Gravity | 18 | m/s² toward planet centre; tuning starting point |
| Jump impulse | 5.5 | m/s along ground normal; tuning starting point |
| Capsule height | 1.8 | m |
| Capsule radius | 0.35 | m |
| Maximum walkable slope | 45 | degrees; tuning starting point |
| Ground probe range | 0.5 | m beyond expected capsule support |

These are prototype defaults for first playtesting, not final game balance or planet-scale travel tuning. Use metres and seconds in names/documentation. Clamp invalid values and produce actionable diagnostics.

## Lifecycle

1. WORLD-003 bootstrap generates the exact WORLD-002 fields.
2. It chooses a valid land spawn (currently low-slope plains) and samples the ground surface.
3. It creates the player root and presentation capsule, attaches the controller/input adapter, and supplies the field set, planet centre, radius and movement configuration.
4. The controller initializes grounded only after a valid ground probe; otherwise it enters falling state.
5. Each physics tick reads the latest movement intent, integrates velocity/gravity, probes the surface and resolves contact/water/slope constraints.
6. Camera follows the player in LateUpdate using the transported local tangent basis.
7. On teardown, the bootstrap destroys only its owned runtime objects and releases references to generated fields/meshes.

## Dependencies and interfaces

- `SphericalGeographicFieldSet` and `SphericalGeographicFieldGenerator` (WORLD-002) are authoritative for altitude, slope and water classification.
- `SphericalSurfaceQuery` supplies tangent-frame math; its WORLD-001 height-field methods must not be used for WORLD-002 ground heights.
- `SphericalThirdPersonCamera` supplies the camera-relative tangent movement basis.
- The World domain remains the owner of Transform mutation. Future Pokémon action behaviours consume `IWorldMovement` intent; they must not directly modify a Transform.
- No changes to cross-domain public contracts are required in this first player-only slice unless implementation discovers an actual consumer need. Any such change requires contract review and an amended plan.

## Testing

### EditMode

- tangent basis remains normalized, orthogonal and finite at cube-face seams, poles and representative arbitrary directions;
- gravity points toward the planet centre for all six cardinal directions and arbitrary positions;
- velocity integration is stable for fixed test steps and does not create radial/tangential component confusion;
- ground probe returns the sampled WORLD-002 altitude and a finite normal;
- water classifications never produce grounded contact;
- slope threshold accepts below-limit slopes and rejects above-limit slopes;
- invalid/zero direction, missing field set and invalid configuration fail safely with actionable errors;
- deterministic movement math produces identical results for identical initial state/input/time steps.

### PlayMode

- player spawns on the sampled plains surface without penetration or visible floating;
- with no movement input, grounded player remains in stable contact;
- when deliberately spawned above the surface, player falls and lands under radial gravity;
- jump only triggers while grounded and returns to grounded state;
- WASD moves across the local tangent plane and follows the globe through all six cube-sphere faces;
- orientation remains continuous across seams and near poles;
- slope limit prevents walking uphill beyond the configured threshold;
- water does not act as a ground surface and blocks entry according to the first-pass policy;
- camera-relative movement, orbit and zoom remain functional;
- exiting Play Mode cleans up generated player/camera/world objects and does not modify source scenes or settings.

### Manual validation

Use the WORLD-003 inspection scene with the existing seed first, then at least one additional seed. Test walking across biome boundaries, across a cube-sphere face seam, on a slope, near coast/water and at different latitudes. Record Unity version, seed, face resolution and tuning values. Profile only after the functional checks pass; do not claim performance acceptance from static inspection.

## Acceptance criteria

1. The production controller no longer pins the player to the surface each frame.
2. The player has a world-space velocity and experiences radial gravity when airborne.
3. Ground contact is derived from the exact WORLD-002 altitude field rendered by the planet.
4. The player can walk continuously around the globe, including across cube-sphere face seams and near poles, without orientation flips.
5. Jumping is grounded-only and uses the local ground normal.
6. Steep slopes and water are not treated as ordinary walkable ground.
7. The existing spherical third-person camera remains compatible and movement is camera-relative.
8. The full-face planet meshes remain free of `MeshCollider` components and the previous oversized-triangle warnings do not return.
9. Automated tests cover the motor/probe invariants; PlayMode checks cover actual Unity integration.
10. WORLD-003 remains a scene/bootstrap fixture only; production movement code lives in the World movement module.
11. Documentation accurately states that terrain-field contact is supported but arbitrary prop collision, swimming and traversal abilities are not yet implemented.

## Open questions / implementation gates

- **Ground contact implementation:** choose the simplest robust field-based capsule support/probe method after inspecting the exact field sampling and generated mesh coordinate spaces. Do not introduce a full collider chunk system in this package.
- **Ground normal:** validate finite-difference normal against WORLD-002 slope data; document and test the selected source.
- **Input adapter:** retain current direct Input System usage only in the controller adapter; do not put input polling inside the motor.
- **Physics timestep:** use fixed-step integration; confirm Unity project fixed timestep and avoid silently changing project-wide settings.
- **World-local origin:** current generated inspection planet is centred at local/world origin. Keep planet centre explicit in the API so a later session can place the planet elsewhere.

No owner-blocking design question is currently required to implement the first slice. If source inspection reveals that the generated mesh transform or field coordinate frame is not the origin-based model described above, stop and amend the plan before implementation.

## Documentation and code locations

- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md`
- `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-004_PLAYER_CONTROLLER_AND_SPHERICAL_LOCOMOTION.md`
- `Documentation/03_ENGINEERING/TESTING_AND_VALIDATION.md` (only if test policy needs a reusable addition)
- `Assets/Project/Runtime/World/Spherical/Movement/`
- `Assets/Project/Tests/EditMode/World/Spherical/Movement/`
- `Assets/Project/Tests/PlayMode/World/Spherical/Movement/`
- `Assets/Project/Runtime/World/Spherical/Inspection/` (bootstrap integration only)


## Implementation progress — first runtime slice

### Authored in this increment

- `Movement/SphericalGroundProbe.cs`: samples WORLD-002 altitude, slope, water and biome data by spherical direction. The terrain normal is estimated from four nearby sampled surface positions, with radial-up fallback for degenerate samples.
- `Movement/SphericalPlayerMotor.cs`: world-space velocity, radial gravity, grounded-only jump impulse, tangential acceleration and ground-contact velocity resolution.
- `Movement/SphericalPlayerController.cs`: PC Input System adapter (WASD/arrows + Space), fixed-step movement, camera-relative tangent movement, field-based contact, water blocking while grounded and configurable slope limit.
- WORLD-003 bootstrap now composes the new movement controller and keeps the six planet face meshes free of `MeshCollider` components.
- EditMode motor tests cover six radial gravity directions, grounded-only jump, preservation of radial velocity during tangential acceleration and surface-contact velocity resolution.

### Important implementation boundaries

- The motor is not yet a complete reusable gameplay input/action framework; this is the first PC movement adapter.
- Ground support is a radial capsule-support approximation using half capsule height. It does not yet provide swept capsule collision, step climbing, arbitrary prop collision or dynamic terrain collision.
- Water blocks grounded entry; swimming and water-surface traversal are not implemented.
- The sampled WORLD-002 `SlopeDegrees` field is currently authoritative for the walkability threshold. The estimated normal is used for orientation and jump impulse.
- The camera remains the existing WORLD-001 `SphericalThirdPersonCamera`; no competing camera was introduced.

### Validation status

- Static source review: performed for the authored files and their referenced field/camera APIs.
- Unity compilation: NOT VERIFIED in this environment.
- EditMode / PlayMode tests: authored, NOT RUN in Unity.
- Manual movement around face seams, slopes and water: PENDING user validation.

### First Unity validation sequence

1. Apply the implementation ZIP to the project and wait for Unity compilation.
2. Run the new `Pokemon.Project.Tests.World.Spherical.Movement.EditMode` tests.
3. Open `WORLD-002_BiomeInspection` and enter Play Mode.
4. Confirm the player settles on the plains surface, then walk in all four directions.
5. Hold Space while grounded, release, and confirm falling/landing; try Space while airborne and confirm no second jump.
6. Walk toward a coast and confirm the player does not treat ocean/lake/river as solid ground.
7. Walk across a visible biome transition and, where possible, a cube-sphere face seam.
8. Confirm no planet-scale MeshCollider warnings return.

Record any compile error or visible behaviour issue before tuning speeds or adding animation.

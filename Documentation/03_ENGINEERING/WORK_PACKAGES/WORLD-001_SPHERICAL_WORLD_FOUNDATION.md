# WORLD-001 — Spherical World Foundation Prototype

- **Feature ID:** WORLD-001
- **Owning domain:** WORLD
- **Status:** IMPLEMENTATION IN PROGRESS — project owner confirmed approval and plan milestone push
- **Related decisions:** DEC-034 (subtle ground/high-altitude curvature); DEC-035–DEC-039 (camera, world scale, dual player role, first prototype and PC target)
- **Unity version inspected:** 6000.4.11f1
- **Input package:** `com.unity.inputsystem` 1.19.0; project input handling is set to the new Input System

## Purpose and player-visible outcome

Create the first reproducible playable prototype of the confirmed spherical world direction. The player can walk on a continuous spherical surface while a Pokémon-style third-person camera follows at a distance with an adjustable angle. The terrain has deterministic relief. The world is mathematically spherical at all times, but curvature must remain visually subtle during ordinary ground play and become progressively apparent from elevated viewpoints.

This is a foundation prototype, not the production planet renderer or the final world-generation system.

## Rules and behaviour

1. Represent the planet as six cube faces projected onto a sphere (Cube-Sphere candidate).
2. Use a fixed, deliberately small face resolution for this milestone. Do not implement quadtree/adaptive LOD, chunk streaming or hydrology in WORLD-001.
3. Face-edge and face-corner positions must be generated from one canonical cube-space coordinate mapping so adjacent faces meet exactly.
4. Use a deterministic, seam-safe height function sampled from the normalized spherical direction (3D-domain noise or an equivalent direction-based field). Do not sample independent 2D noise maps per face.
5. Keep planet radius configurable and explicitly marked as a prototype parameter. Do not write a production radius into the authoritative world-generation configuration.
6. Use a radial surface-up direction and a local tangent frame for grounded movement and camera orientation. Do not use world Y as the universal up direction.
7. The camera follows the player from a Pokémon-like third-person distance, with adjustable yaw/pitch and a bounded pitch range. Initial distance/FOV/altitude values are prototype tuning values, not final design decisions.
8. Grounded movement follows the local tangent plane. The prototype controller must remain attached to the surface and transition correctly across cube-face boundaries. Flight controls are out of scope; elevated camera viewpoints can be tested by a debug altitude control.
9. The existing planar `WorldGenerator` and `WorldTerrainBuilder` remain untouched and continue to serve the existing planar test workflow. WORLD-001 introduces a separate spherical path.
10. Generation must be reproducible from an explicit seed and settings. No reliance on Unity's global random state.

## Data model

Prototype settings should be a small serializable settings object with:
- integer seed;
- configurable planet radius in metres;
- integer face resolution (minimum 2; test default selected in code/config after profiling);
- maximum terrain elevation in metres;
- deterministic height-field frequency/amplitude settings;
- camera follow distance, field of view and pitch bounds;
- player movement speed and gravity/ground-attachment parameters.

All distances are metres, speeds are metres per second, angles are degrees unless an API explicitly uses radians. Prototype settings must be separated from the existing planar `WorldGenerationSettings` and must not silently change `world_generation.json`.

## Lifecycle

- A dedicated demo scene or reproducible scene-setup tool creates the planet, player capsule and camera rig.
- Generation creates the spherical mesh and collider(s) from the same settings snapshot.
- Regeneration replaces only objects owned by the prototype root; it must not accumulate duplicate meshes, materials or scene roots.
- Runtime-created meshes/materials must have explicit ownership and cleanup.
- The player controller obtains the surface frame from the spherical world service and updates movement/orientation using that frame.

## Interfaces

Proposed narrow runtime responsibilities (final names may follow existing namespace conventions):
- Cube-face coordinate mapping: face + integer grid coordinate → cube-space coordinate → normalized direction.
- Spherical topology utility: direction/face coordinate conversion and edge/corner mapping helpers.
- Spherical terrain generator: settings + seed → immutable mesh data/height samples.
- Spherical terrain presenter: mesh data → Unity Mesh and collider.
- Surface query: world position/direction → surface position, elevation and local up/normal.
- Prototype movement adapter: input intent → tangent-plane movement constrained to the spherical surface.
- Camera rig: target transform + local tangent frame → third-person camera pose.

Pure coordinate/noise calculations should remain testable without scene objects where practical. Unity Mesh, Collider, Camera and input polling belong in presentation/integration adapters.

## Authority

Local single-player prototype only. No networking, dedicated-server generation, replication or authoritative multiplayer movement changes in this work package. Existing networking architecture decisions remain untouched.

## Presentation

- Simple neutral terrain material/vertex colours are sufficient; no final biome art, vegetation dressing or settlement content.
- Include a long, relatively flat vista as well as hills/mountains so terrain relief does not hide unwanted curvature.
- Include reproducible ground-level and elevated viewpoints/screenshots or a scene control to compare them.
- Player representation may be a capsule placeholder.

## Dependencies

- Unity 6000.4.11f1.
- Existing Universal Render Pipeline project.
- Existing Input System package 1.19.0.
- Unity Test Framework 1.6.0.
- No new package/dependency is proposed.

## Performance

The only target is the owner's current PC. Do not define a broad minimum-hardware specification. Record generation time, mesh vertex/triangle counts, collider construction time and observed frame behaviour on the owner's machine when available. Do not claim a performance pass without measurements on that machine.

## Testing

### EditMode / pure logic
- All six face mappings produce finite, normalized directions.
- Every face edge maps to the same spherical positions as its adjacent face, within a documented floating-point tolerance.
- Cube corners are shared consistently by all three incident faces.
- Face-coordinate round trips remain within tolerance for face interiors and boundaries.
- Same seed/settings produce identical height samples and mesh vertex positions.
- Different seeds change the generated height field.
- Radius changes scale the base sphere as expected without changing normalized direction samples.
- No degenerate or non-finite vertex positions are produced for valid settings.

### PlayMode / scene integration
- The demo scene generates and displays a complete six-face sphere with relief.
- No visible cracks or missing triangles appear at face boundaries.
- Surface normals/orientation remain stable when walking across all six face boundaries, including near cube corners.
- The player remains attached to the surface and moves in the local tangent plane.
- The camera follows at a distance, allows adjustable angle, and remains oriented relative to local surface up.
- Regeneration does not accumulate prototype roots, meshes or materials.
- Curvature is not conspicuous from normal ground-level camera views on the flat-ish vista; curvature becomes progressively visible from the elevated debug view. The radius remains a tuning parameter until this visual test passes.

## Acceptance criteria

WORLD-001 is ready for QA when:
1. A dedicated reproducible scene displays a complete relief-bearing spherical planet generated from a fixed seed.
2. All six face edges and corners pass automated continuity tests; no cracks are visible in the scene.
3. Deterministic generation tests pass.
4. The player can walk across all face boundaries without orientation flips, falling through the surface or losing surface attachment.
5. The Pokémon-style camera follows the player and supports adjustable viewing angle.
6. Ground-level curvature is visually subtle while elevated curvature is perceptible, with no abrupt visual transition. Evidence includes the same seed/settings and documented camera/radius parameters for both views.
7. Existing planar world-generation behaviour is unchanged.
8. Runtime resource ownership/cleanup is verified.
9. Development checks and QA report are recorded separately. QA must independently report PASS / FAIL / BLOCKED; the work package remains open until project-owner approval.

## Implementation progress — visible runtime demo

The first runtime presentation batch is recorded in `WORLD-001_IMPLEMENTATION_PROGRESS_01.md`. It adds the six-face presenter, surface query, temporary radial/tangent movement controller, Pokémon-style debug camera, runtime demo bootstrap, and an Editor menu command that creates a dedicated demo scene. The scene is generated at runtime from a single bootstrap object; it is not committed as a pre-authored scene asset.

The current demo defaults (50,000 m radius, resolution 256, seed 481516, 350 m maximum elevation) are prototype-only tuning values. They do not establish the production radius. The mesh remains fixed-resolution and does not provide close-range production terrain detail. Unity compilation, EditMode tests, PlayMode behaviour, collider cooking, visual curvature checks and PC profiling remain unverified until run in the owner's Unity Editor.

## Exclusions

- Adaptive quadtree LOD, streaming and asynchronous chunk scheduling.
- Final production planet radius/world scale.
- Spherical hydrology, rivers, lakes, moisture and biome generation.
- Player flight, flying Pokémon and species-specific traversal.
- Pokémon/ecosystem integration, spawning, habitats and simulation.
- Seasons, day/night, settlements, paths and persistent terrain modifications.
- Multiplayer, server authority, save schema and network replication.
- Final art direction, vegetation, props, materials and optimization beyond profiling this prototype on the owner's PC.

## Open questions

- Exact cube-to-sphere mapping variant and face resolution: benchmark continuity, vertex distribution and mesh quality during implementation; record the selected variant in the implementation report.
- Exact prototype radius/camera/FOV/altitude values: tune using the scripted visual test; do not promote them to production decisions without owner review.
- Whether the prototype scene should be added to an existing world scene or created as a dedicated new scene: inspect existing scene contents and choose the least disruptive reproducible option; document the choice.

## Documentation and code locations

Expected implementation locations (subject to repository conventions after inspection):
- `Assets/Project/Runtime/World/Spherical/` — spherical coordinate/topology and deterministic data logic.
- `Assets/Project/Runtime/World/Spherical/Presentation/` — Unity mesh, collider and scene presentation adapters.
- `Assets/Project/Runtime/World/Spherical/Prototype/` — prototype movement and camera integration, if these cannot be cleanly placed in existing presentation/input ownership.
- `Assets/Project/Content/Scenes/Worlds/` — dedicated reproducible prototype scene.
- `Assets/Tests/EditMode/World/` and `Assets/Tests/PlayMode/World/` (or the existing project test convention if one is discovered) — tests.
- `Documentation/01_ARCHITECTURE/SPHERICAL_WORLD_ARCHITECTURE_PROPOSAL.md` — record validated decisions/deviations after prototype evidence.
- `Documentation/02_DOMAINS/TERRAIN_AND_RENDERING.md` and `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md` — update only what implementation evidence confirms.

Existing planar implementation inspected and explicitly excluded from modification in this work package:
- `Assets/Project/Runtime/World/Generation/WorldGenerator.cs`
- `Assets/Project/Runtime/World/Generation/WorldTerrainBuilder.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationSettings.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationResult.cs`

## Plan milestone checklist

- [x] Read project Git workflow, coding standards, testing strategy, world architecture and spherical proposal.
- [x] Inspect current planar world-generation implementation, Unity version, package dependencies and existing test/scene layout.
- [x] Define scope, exclusions, ownership, interfaces, lifecycle, authority, dependencies, performance implications, tests and acceptance criteria.
- [x] Project owner approved this plan (confirmed in conversation on 2026-11-17).
- [x] Project owner confirmed the plan was committed and pushed to the working branch before implementation began.


## Implementation progress — initial code slice

The first implementation slice introduces a separate spherical path and does not modify the existing planar generator.

Implemented in the working project snapshot:
- `Assets/Project/Runtime/World/Spherical/SphericalWorldSettings.cs` — prototype-only serializable settings and validation.
- `Assets/Project/Runtime/World/Spherical/CubeSphereTopology.cs` — six canonical face bases, cube-to-sphere projection and direction-to-face conversion.
- `Assets/Project/Runtime/World/Spherical/SphericalHeightField.cs` — seeded 3D value-noise fBm sampled from normalized directions.
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetMeshGenerator.cs` — six fixed-resolution face meshes with matching canonical edge positions.
- `Assets/Project/Tests/EditMode/World/Spherical/` — EditMode tests for direction mapping and deterministic height sampling.

The current projection is the normalized-cube mapping (`normalize(faceNormal + u * axisU + v * axisV)`). It is an initial prototype choice, not yet a final production topology decision. Mesh normals are currently radial placeholders; slope-aware, seam-consistent normals remain part of mesh validation/refinement.

Validation status: a separate Python geometry check confirmed all six face bases have outward orientation and sampled edge directions map consistently across at least two incident faces. Unity compilation and EditMode tests have **not** been run in this environment and remain pending in the Unity Editor. No PlayMode scene, player movement or camera rig is included in this initial code slice.

## Approved-scope history and proposed extension

The original WORLD-001 foundation plan and its approval/push history remain preserved above. The project owner has since requested a broader first-prototype workflow: a smooth base sphere, three independently inspectable non-destructive terrain layers, independent thematic maps, and spherical plus planisphere views.

This is a material scope expansion, so it is specified separately in `WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md`. The amendment must be approved and pushed as an updated plan milestone on the same working branch before implementation of that expanded scope begins. Do not silently treat the original plan approval as approval of this extension.

## Approved scope amendment

The approved layered-generation and dual-view studio scope is defined in `WORLD-001_LAYERED_PLANET_STUDIO_AMENDMENT.md`. That amendment extends the original foundation milestone without rewriting its history.

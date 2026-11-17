# WORLD-001 — Implementation Progress 01: Visible Runtime Demo

- **Work package:** WORLD-001
- **Milestone:** B — implementation in progress
- **Scope:** Runtime presentation, reproducible demo-scene creation, temporary surface controller and camera.
- **Plan:** `WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` (approved and pushed by the project owner before this implementation batch)
- **Status:** Code prepared; Unity compilation and PlayMode verification are pending in the owner's editor.

## Delivered

- `SphericalPlanetPresenter` creates six child face objects with `MeshFilter`, `MeshRenderer` and optional `MeshCollider` components. It owns and releases generated meshes and its fallback material.
- Terrain vertex normals are estimated from the same direction-based height field using a local tangent finite difference, so relief affects lighting without per-face normal seams being intentionally introduced.
- `SphericalSurfaceQuery` exposes direction-based surface position/radius queries and a stable local tangent frame.
- `SphericalPlayerController` provides temporary WASD/arrow-key movement constrained to the analytic spherical surface. It uses radial up and a local tangent movement basis; it is not the production character controller.
- `SphericalThirdPersonCamera` provides a third-person orbit camera, right-mouse yaw/pitch, wheel zoom and debug altitude controls.
- `SphericalWorldDemoBootstrap` composes the planet, capsule placeholder, camera, lighting and on-screen control guide at runtime. Rebuild/clear operations are scoped to its generated root.
- `SphericalWorldDemoSceneTool` adds `Tools > World > Create Spherical World Demo Scene`, which creates and saves a dedicated scene at `Assets/Project/Content/Scenes/Worlds/SphericalWorldDemo.unity`.
- EditMode tests cover surface radius queries, tangent-frame orthogonality and six-face mesh generation.
- The spherical runtime assembly now references the already-installed `Unity.InputSystem` assembly. No package was added.

## Prototype defaults

The demo bootstrap uses a 50,000 m radius, face resolution 256, seed 481516, maximum elevation 350 m and five noise octaves. These are demo tuning values only; they do not change the authoritative planar world-generation settings or establish the production planet radius.

## Controls

- Move: WASD or arrow keys.
- Orbit camera: hold right mouse button and move the mouse.
- Zoom: mouse wheel.
- Raise/lower debug camera altitude: Page Up / Page Down (500 m per press).
- Reset debug altitude: Home.

## Verification status

- Static source inspection: completed in the implementation environment.
- Unity compilation: **not run**; no Unity Editor/runtime is available in this environment.
- EditMode tests: **not run** for the same reason.
- PlayMode scene generation, collider cooking, seam appearance, controller movement and camera curvature presentation: **not run**.
- Performance on the owner's PC: **not measured**.

## Known limitations

- The controller is a temporary analytic-surface kinematic controller, not a production physics controller. It does not use a Rigidbody or perform slope/obstacle collision response.
- Face meshes use a fixed resolution; there is no adaptive LOD, streaming or local terrain refinement. The mesh is intended to validate globe topology and presentation, not final close-up terrain detail.
- The debug altitude camera is a presentation aid, not a flight system.
- The generated terrain uses a simple fallback material and has no biome, water, vegetation or settlement presentation.
- Mesh collider cooking and actual ground/high-altitude visual criteria still require owner-side PlayMode verification.

## Next verification actions

1. Open the project in Unity 6000.4.11f1 and allow the new Input System assembly reference to resolve.
2. Run `Tools > World > Create Spherical World Demo Scene`.
3. Open `SphericalWorldDemo.unity` and press Play.
4. Confirm all six faces render without visible cracks, then walk using WASD and cross face boundaries.
5. Compare ground view with debug altitude steps; record radius, altitude, camera distance and screenshots.
6. Run the spherical EditMode test assembly and report exact results.
7. Profile generation time, collider cooking and frame behaviour on the owner's PC.

This progress report does not mark WORLD-001 complete. QA review and project-owner approval remain outstanding.

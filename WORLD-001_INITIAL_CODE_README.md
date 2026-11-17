# WORLD-001 — Initial code slice

This patch contains the first spherical-world foundation scripts and EditMode tests. It is designed to be copied into the approved project branch without replacing unrelated project files.

## Contents

- `Assets/Project/Runtime/World/Spherical/SphericalWorldSettings.cs`
- `Assets/Project/Runtime/World/Spherical/CubeSphereTopology.cs`
- `Assets/Project/Runtime/World/Spherical/SphericalHeightField.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetMeshGenerator.cs`
- Runtime assembly definition and Unity `.meta` files
- `Assets/Project/Tests/EditMode/World/Spherical/` — EditMode tests and test assembly definition
- Updated WORLD-001 work-package and implementation-report documentation

## Integration

1. Copy the included `Assets/` and `Documentation/` paths into the project root, preserving folder structure and `.meta` files.
2. Open the project in Unity 6000.4.11f1 and wait for script compilation.
3. Run the `Pokemon.Project.Tests.World.Spherical.EditMode` tests from the Unity Test Runner.
4. Do not integrate this into the existing planar generator; the new code is intentionally isolated under `World/Spherical/`.

## Basic API

- `CubeSphereTopology.GetDirection(face, u, v)` maps face coordinates in `[-1, 1]` to a unit direction.
- `CubeSphereTopology.DirectionToFaceCoordinates(direction, out face, out coordinates)` performs the inverse dominant-face lookup.
- `SphericalHeightField.SampleHeight(direction, settings)` returns a deterministic elevation in `[0, maximumElevationMeters]`.
- `SphericalPlanetMeshGenerator.GenerateAllFaces(settings)` returns six Unity meshes. The caller owns those meshes and must destroy them when they are no longer used.

## Important limitations

- The 50,000 m radius default is a temporary prototype starting value, not a production decision.
- The generator uses a fixed face resolution; no adaptive LOD, chunk streaming or hydrology is included.
- Mesh normals are radial placeholders. Slope-aware normals and Unity seam verification are still required.
- No scene, collider presenter, player controller or camera rig is included yet.
- Unity compilation and NUnit tests have not been run in this environment.

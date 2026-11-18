# WORLD-009-A — Opaque Biome Inspection Terrain

- **Status:** IMPLEMENTATION CHECKPOINT — code change prepared; Unity compilation and visual validation pending
- **Depends on:** WORLD-002 playable biome inspection; URP project configuration

## Goal

Ensure the playable spherical terrain is rendered as opaque 3D geometry. This step isolates the rendering issue before changing player movement or camera behaviour.

## Finding

`SphericalBiomeInspectionBootstrap.ResolveBiomeMaterial()` previously selected `Sprites/Default` when no material was assigned. That is a sprite-oriented shader and is not an appropriate default for opaque spherical terrain.

The project already contains `Assets/Project/Runtime/World/Generation/WorldVertexColor.shader`, registered as `Pokemon/WorldVertexColor`. It is a URP shader that reads mesh vertex colours, declares the opaque render type, writes depth (`ZWrite On`) and back-face culls (`Cull Back`).

The generated biome inspection meshes store their biome palette in vertex colours, so this existing shader matches the mesh data contract. No new shader asset is required.

## Implementation

- Resolve `Pokemon/WorldVertexColor` as the required material shader.
- Always create an owned material using that shader for the playable inspection terrain.
- Set the material render queue to Geometry and its base colour to white so generated vertex colours remain unchanged.
- Do not use a legacy serialized `biomeMaterial` override when it references another shader; emit a warning instead.
- Leave player movement, gravity, ground probing and camera code unchanged.

## Acceptance criteria

1. Unity compiles without errors.
2. The generated playable planet is visually opaque from all camera angles.
3. Near-side terrain correctly occludes far-side terrain.
4. All four biome vertex colours remain visible and unchanged.
5. Ocean/land colours remain opaque.
6. No regression in the WORLD-002 generated geometry or player movement.
7. If a legacy sprite material is assigned to `biomeMaterial`, the bootstrap warns and still uses the opaque shader.

## Manual validation

- Generate Tiny World and a larger profile.
- Inspect the planet from above, near the horizon, and from the surface.
- Confirm the far side is not visible through the near-side terrain.
- Enter Play Mode and check that the capsule remains visible and the same movement behaviour is retained.
- Record Unity version, profile, radius, seed, and screenshots.

## Scope boundary

This step does not claim to fix the player's floating sensation. It removes an invalid transparent rendering fallback so the remaining ground-contact and movement behaviour can be assessed accurately. If floating persists after this validation, proceed to WORLD-009-B ground-contact investigation.


## Background / horizon correction

The inspection camera follows the player's local spherical tangent frame. Unity's default skybox horizon is aligned to global world-up, so as the camera moves around the sphere its horizon can appear tilted relative to the local ground.

The inspection camera therefore uses `CameraClearFlags.SolidColor` with a uniform soft blue background. This removes the misleading global horizon reference without rotating the camera, changing player orientation, or changing the planet geometry.

Additional acceptance criteria:
- The Game view background has no visible skybox horizon line.
- The background remains visually uniform as the player walks around the sphere.
- The camera still follows the local tangent frame and the terrain remains opaque.

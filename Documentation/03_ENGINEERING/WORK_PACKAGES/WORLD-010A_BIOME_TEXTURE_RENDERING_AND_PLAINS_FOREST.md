# WORLD-010-A/B — Biome Texture Rendering and Plains/Forest Detail

- **Status:** PRODUCTION TEXTURES RESTORED — Unity visual validation pending
- **Depends on:** WORLD-002 geographic fields; WORLD-003 playable biome inspection; existing URP biome materials and texture assets
- **Scope:** WORLD-010-A texture rendering pipeline; WORLD-010-B Plains and Forest surface detail

## Repository findings

The repository already contains the key building blocks:
- `SphericalBiomeSurfaceMeshGenerator` stores normalized Plains/Forest/Desert/Mountains weights in vertex RGBA and ocean weight in UV2.x.
- `SphericalBiomeBlend.shader` is an opaque URP shader. It blends biome albedos and samples textures with world-space triplanar projection.
- `SphericalBiomeTextureGenerator` creates deterministic, tileable neutral detail textures.
- `SphericalBiomeMaterialGenerator` creates four ground textures, four ground materials, a water material and `CFG_BiomeMaterialMapping`.
- `SphericalBiomeMaterialMapping` configures the blend shader from the generated material assets.

The gap was integration: the playable inspection bootstrap still generated flat biome colour meshes and used a sprite material fallback instead of the biome-weight mesh and blend shader.

## Implementation

### WORLD-010-A — Rendering integration
- Generate the six playable faces with `SphericalBiomeSurfaceMeshGenerator.GenerateAllFaces`.
- Use the shared `SphericalBiomeBlend.shader` and `CFG_BiomeMaterialMapping`.
- Preserve smooth biome transitions through normalized vertex weights.
- Preserve ocean masking through UV2.x.
- Use world-space triplanar texture projection to avoid cube-face UV seams and keep texture dimensions in metres, independent of planet radius.
- Keep the surface opaque and depth-writing.
- Retain the uniform solid-colour camera background; do not reintroduce the global skybox horizon.

### WORLD-010-B — Plains and Forest
- Give Plains a subtle grass-striation/tuft pattern.
- Give Forest an organic moss/leaf-litter mottling pattern.
- Keep generated textures deterministic, tileable, opaque and low contrast because the material tint supplies the biome palette.
- Keep Desert and Mountains on their current generated detail patterns; their dedicated art pass is WORLD-010-C.

## Asset and scene setup

1. Run `Tools/World/Generate or Refresh Biome Materials (URP)` once to refresh the generated textures/materials and `CFG_BiomeMaterialMapping`.
2. Run `Tools/World/Assign Biome Mapping to Current Inspection Scene` in the open playable inspection scene. This assigns and saves the mapping reference without recreating the scene.
3. Enter Play Mode to inspect the textured sphere.

New scenes created through `Tools/World/Create Playable Biome Inspection Scene` also receive the mapping automatically when the mapping asset exists.

## Physical texture scale

The blend shader uses world-space coordinates divided by `_TextureWorldSize` (currently 18 metres). Therefore the same texture motif retains the same physical size on a 2 km Tiny World and on larger planets. Planet radius does not rescale the textures.

## Acceptance criteria

- The playable scene uses the opaque `Pokemon/World/SphericalBiomeBlend` shader, not `Sprites/Default`.
- All six cube-sphere faces use the same mapping and blend material.
- Plains and Forest are visibly distinguishable at close range and remain readable at medium distance.
- Biome boundaries blend smoothly without hard colour seams.
- No visible texture discontinuity appears at cube-face boundaries.
- Texture scale remains physically consistent on Tiny World and a larger profile.
- Ocean areas remain opaque and use the existing ocean tint.
- EditMode texture tests pass.
- Unity compilation and visual Play Mode validation pass.

## Out of scope

- Dedicated Desert and Mountains texture art pass (WORLD-010-C).
- Normal maps, displacement, procedural grass meshes, trees, rocks or other vegetation instances.
- Per-profile texture overrides.
- Player movement and camera changes.

## Commit policy

Do not mark WORLD-010-A/B complete until Unity compilation, EditMode tests and manual visual checks have passed. If work stops before then, use a checkpoint commit message that states validation is pending.


## Visibility correction — first texture pass was too subtle

The initial Plains texture used a pale yellow-green material tint and low-amplitude neutral grayscale variation. The generated texture assets and mapping were valid, but the surface detail was too weak to read in the playable camera.

The visibility pass therefore:
- Changes the Plains base tint to a grass green.
- Generates colored grass-blade streaks, clustered tufts and sparse darker ground flecks.
- Increases Forest moss and leaf-litter contrast.
- Changes world-space repeat size from 18 m to 8 m so detail reads at the 2 km Tiny World scale while remaining independent of planet radius.
- Adds an EditMode contrast regression test.

The legacy `Biome Material` field on `SphericalBiomeInspectionBootstrap` is intentionally unused by the blended renderer and may remain `None`. The required `Biome Material Mapping` field is the reference that must be assigned.


## Follow-up: diagnostic result and production restoration

The temporary high-contrast checkerboard test was successful once `WORLD-001_PlanetStudioPreview` was disabled. This confirms that the playable WORLD-002 mesh, biome weights, texture references and triplanar shader sampling are connected. The earlier apparently flat result was caused by the separate legacy Planet Studio preview sphere covering the playable textured planet.

The diagnostic checkerboard generator and diagnostic shader path are not part of the production version. Production procedural Plains/Forest textures and the green Plains material tint are restored. The playable inspection bootstrap now disables the exact `WORLD-001_PlanetStudioPreview` root at runtime before generating the playable world. This change is Play Mode-only; Unity restores the scene object's authored active state when Play Mode stops.

### Additional acceptance criteria
- No checkerboard/diagnostic mode or diagnostic texture generation remains in the production shader or texture generator.
- Entering Play Mode in a scene containing both roots automatically disables `WORLD-001_PlanetStudioPreview`.
- The generated WORLD-002 playable planet is the visible surface and shows the production biome textures.
- Stopping Play Mode restores the scene's authored preview-object state.


## WORLD-010-B — Plains grass readability pass

The first production texture was technically connected but read as broad green/brown noise in the playable camera. This pass replaces the subtle sinusoidal Plains marks with deterministic, tapered grass-blade strokes rasterized into the tile.

- Grass blades have varied lengths, widths, lean and curvature.
- Light and shadow blades create visible green variation without geometry.
- Periodic cell hashing and wrapped pixel writes preserve tileability at texture borders.
- Plains base tint is brighter green to avoid the muddy olive appearance in the first in-game capture.
- The 8 m world-space repeat is retained; a 512 px texture therefore represents an 8 m ground patch.

### Additional acceptance criteria
- Individual grass strokes are visible at close/medium gameplay distance.
- Plains read as grass rather than generic mottled noise.
- The texture remains tileable at all four edges.
- No change to Forest, Desert or Mountains appearance in this pass.
- Check the same profile at 2 km and a larger world radius; physical texture scale must remain unchanged.

Status: implementation checkpoint; Unity compilation, texture regeneration and visual validation are pending.


## WORLD-010-C — Forest floor texture

- Add deterministic, tileable moss/ground-cover patches.
- Add small warm leaf-litter details to distinguish Forest from Plains.
- Preserve the shared opaque triplanar material pipeline and physical world-space scale.
- No tree or large vegetation meshes are introduced in this pass.

## Playable inspection — selectable spawn biome

The `SphericalBiomeInspectionBootstrap` now exposes `Spawn Biome` in the Inspector, with Plains as the default. The four supported choices are Plains, Forest, Desert and Mountains.

Spawn selection searches dry-land cells belonging to the selected biome. It prefers a low-slope location away from biome boundaries. If no cell meets the preferred slope limit, it falls back to the flattest available dry-land cell in that biome and logs a warning. If the selected biome has no dry-land cells, generation fails with an actionable error instead of silently spawning in Plains.

### Validation
- Change Spawn Biome in the bootstrap Inspector before Play Mode.
- Confirm the log's RequestedSpawnBiome and ActualSpawnBiome match.
- Test all four biomes on at least one profile where each biome exists.
- Confirm a selected biome absent from the generated world produces the clear error.
- Confirm Forest texture has visibly different moss/leaf detail from Plains.
- Re-run the existing texture EditMode tests.

Status: implementation checkpoint; Unity compilation, EditMode tests and visual Play Mode validation pending.

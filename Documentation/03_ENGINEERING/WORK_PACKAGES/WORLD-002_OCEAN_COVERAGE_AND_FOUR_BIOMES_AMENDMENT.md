# WORLD-002 — Ocean Coverage and Four-Biome Classification Amendment

- **Work package:** WORLD-002
- **Agent:** WORLD
- **Status:** PLAN MILESTONE — owner-approved scope; requires push before implementation
- **Related decisions:** DEC-043, DEC-044 (DEC-022 superseded)
- **Depends on:** WORLD-001 layered Planet Studio; WORLD-002 spherical height/temperature/slope fields; validated spherical drainage prototype

## 1. Goal

Extend the current spherical geographic-field pipeline so the first useful world preview has a substantial, configurable ocean system and four terrestrial biomes. This is a data-generation and inspection milestone, not final ocean rendering, final materials, or complete climate simulation.

## 2. Confirmed owner decisions

- Default ocean target: **65% of the planet's total spherical surface area**.
- Ocean coverage is editable in generation settings; 65% is the default, not a permanent hard-coded value.
- Initial terrestrial biomes: **Plains, Forest, Desert, Mountains**.
- Mountains are classified primarily from elevation and slope.
- Ocean, inland lakes/ponds and rivers are water classes, not terrestrial biomes.
- Villages remain settlements, not biomes.
- The same generated fields must be visible in the spherical globe and planisphere; switching presentation must not regenerate data.

## 3. Technical design

### 3.1 Ocean coverage and sea level

1. Reuse the WORLD-001 spherical terrain height output as the elevation input; do not create an unrelated second continent map in the biome stage.
2. Add a configurable `oceanCoverageTargetPercent`, default `65`, with a validated safe range (proposed initial UI range: 50–80%).
3. Derive the sea-level threshold from the terrain elevation distribution so the area below the threshold matches the requested ocean coverage.
4. Compute coverage using spherical cell-area weights (or an equivalently validated area-correct spherical sampling method). Do not treat all raw Cube-Sphere face samples as equal-area.
5. Use deterministic tie-breaking when multiple cells share the threshold elevation, so same seed/settings/version produce the same mask.
6. Store the derived sea-level threshold in the generated result/metadata. Keep the target percentage and derived threshold distinct: changing resolution may slightly change the threshold due to sampling, but must not silently change the requested target.
7. Classify inland lakes/ponds and rivers after the ocean mask; exclude them from the ocean-coverage statistic. Rivers should drain toward ocean or inland terminal basins according to the validated drainage rules.
8. Do not promise a specific count of disconnected ocean basins. The 65% target controls area; visual review must confirm broad ocean expanses, multiple continents and islands are produced by the existing deterministic continental relief. If the landmass arrangement is unsatisfactory, adjust the continentalness/relief model in a separately measured generation setting rather than faking ocean fragments.

### 3.2 Four terrestrial biomes

- **Mountains:** primary classification from configurable elevation and/or slope thresholds; must not be classified underwater.
- **Desert:** low derived moisture, with temperature/terrain constraints; must not be produced by an independent random desert noise mask alone.
- **Forest:** suitable moisture and temperature, with terrain/elevation constraints.
- **Plains:** remaining suitable non-water land not classified as mountains, forest or desert.

The exact thresholds and blend bands remain tunable generation settings and must be exposed/recorded in metadata. The first implementation should use explicit priority/order rules to prevent ambiguous overlapping classes. Suggested priority: water exclusion → mountains → desert → forest → plains fallback. A coastal transition/shoreline mask may be added separately but is not a fifth biome.

### 3.3 Data and presentation

- Add explicit categorical water and biome outputs to the shared spherical field set; avoid Editor-only duplicate classification logic.
- Keep continuous fields (altitude, temperature, slope, moisture) distinct from categorical fields (ocean/lake/river/land and biome IDs).
- Globe and planisphere sample the same canonical spherical data.
- Add a readable legend and distinct palette for all four biomes plus ocean/lake/river.
- The initial visual pass may use field colours; final terrain materials, underwater rendering, waves and shore effects are out of scope unless separately approved.
- Keep existing drainage preview and its diagnostic input modes intact; do not modify the drainage algorithm as a shortcut for ocean coverage.

## 4. Required tests

### Ocean coverage
- Default target is 65% and is validated/clamped.
- Measured ocean area ratio is within the documented cell-area quantization tolerance of the target at resolutions 96, 256 and 512.
- Coverage measurement uses spherical area weights, not raw sample counts.
- Same seed/settings/version yields identical sea-level threshold and ocean mask.
- Different target percentages change the derived threshold and measured ocean coverage in the expected direction.
- Ocean classification is seam-continuous across all six Cube-Sphere face boundaries and at poles.
- Inland lakes/ponds and rivers do not affect the ocean coverage metric.
- No ocean cell is simultaneously assigned a terrestrial biome.

### Biome classification
- Only the four approved terrestrial biome IDs are emitted for land cells.
- Every valid non-water land cell receives exactly one biome ID.
- No water cell receives a terrestrial biome ID.
- Mountain classification responds to elevation/slope settings and is not driven by temperature/moisture alone.
- Desert classification responds to derived moisture; changing moisture inputs can change desert suitability without a separate desert noise map.
- Forest/plains classification remains deterministic and respects configured climate/terrain constraints.
- Classification is seam-continuous and identical when sampled through globe and planisphere projections.
- Same seed/settings/version yields identical biome maps; changing presentation settings does not mutate source fields.

## 5. Manual Unity acceptance

1. Generate the same seed at globe field resolution 96 and inspect both globe and planisphere.
2. Select the Ocean map and verify large, coherent ocean areas, multiple continents and visible islands; inspect coastlines and all face seams.
3. Verify the displayed ocean-area percentage is approximately 65% and the derived sea-level threshold is visible in diagnostics.
4. Change the ocean target (e.g. 60%, then 70%), regenerate, and verify ocean area changes in the expected direction while inland lakes/rivers remain separately classified.
5. Select the Biomes map and verify four clearly distinguishable land classes: plains, forest, desert and mountains.
6. Inspect the same seed and fields in planisphere; no data changes when switching views.
7. Regenerate the original seed/settings and verify deterministic equality.
8. Confirm drainage preview still works and its diagnostic controls/results are not silently repurposed.

## 6. Explicit non-goals

- Final ocean shader, animated waves, underwater fog/lighting, beaches or foam.
- Full weather, rainfall, ocean currents, erosion or tectonic simulation.
- Additional terrestrial biomes (tundra, swamp, jungle, alpine variants, etc.).
- Final biome-specific vegetation/material production and asset authoring.
- Final lake/river realism beyond the already-scoped validated drainage pipeline.

## 7. Implementation sequence after plan push

1. Update shared field contracts/types and settings with ocean target and four biome IDs.
2. Implement area-weighted sea-level derivation and ocean mask in the runtime-safe spherical geography layer.
3. Integrate the validated drainage outputs without altering their algorithm; distinguish ocean/lake/river.
4. Implement deterministic four-biome classification from shared fields.
5. Add globe/planisphere thematic visualisation, legend, ocean coverage/derived sea-level diagnostics.
6. Add EditMode tests for thresholds, area coverage, determinism, seams, poles and classification invariants.
7. Update WORLD-002 progress, test plan and changelog; run static checks, then request Unity Test Runner and manual visual validation from the owner.

## 8. Acceptance gate

This plan milestone is complete only when it is committed and pushed on the feature branch before implementation begins, per DEC-016 / `GIT_WORKFLOW.md`. Implementation is a separate milestone and must include tests, documentation, QA review and owner acceptance. No Git push is claimed by this deliverable.

# WORLD-002 — Implementation Progress 02: Oceans and Four Biomes

**Status:** Implementation tranche prepared; Unity compilation, automated Test Runner execution and visual acceptance pending.

## Implemented in this tranche

- Added stable water classes (`Land`, `Ocean`, reserved `LakeOrPond`, reserved `River`) and biome IDs (`None`, `Plains`, `Forest`, `Desert`, `Mountains`).
- Added a configurable ocean coverage target (default 65%, allowed 50–80%). Existing serialized Planet Studio settings receive a versioned migration to the approved defaults.
- Derived the sea-level threshold from the generated WORLD-001 terrain heightfield. Ocean coverage is measured with spherical cell-area weights computed from spherical triangle areas; it does not use raw Cube-Sphere sample counts.
- Aggregated face-cell area contributions onto canonical graph nodes so shared face edges and cube corners are counted once as spherical surface locations.
- Selected the representable elevation threshold closest to the requested area target and exposed actual coverage plus a conservative elevation-class quantization bound.
- Added a multi-source Dijkstra distance-to-ocean prototype over the canonical spherical neighbour graph. Its indexed heap has at most one live entry per node.
- Added `DistanceToWaterMeters` and `MoistureNormalized` continuous fields. Moisture is derived from graph distance to the ocean, temperature-dependent evaporation and slope loss; it is not generated from an independent moisture noise mask.
- Added deterministic four-biome classification. Priority: ocean exclusion → mountains (elevation or slope) → desert (low moisture plus minimum temperature) → forest (moisture and temperature band) → plains fallback.
- Added globe and planisphere inspection maps for Water, Biomes, Distance to Water and Moisture. Categorical maps use nearest-cell sampling rather than interpolating enum IDs.
- Added diagnostics reporting target ocean coverage, measured spherical area coverage, quantization bound and derived sea level.
- Added EditMode tests for target coverage, deterministic outputs, water/biome mutual exclusion, complete land classification, distance-to-ocean basics, seam agreement and a small-graph great-circle comparison.

## Important scope limitations

- Ocean is the only active water class in this tranche. Lake/pond and river IDs are reserved but are not yet populated by the drainage result.
- Moisture is a prototype derived from nearest-ocean graph distance. It does not yet include lakes/rivers, rainfall, wind, prevailing currents, seasonal climate, soil or full water-retention simulation.
- The graph path is a discrete geodesic approximation to continuous great-circle distance. The new small-resolution comparison test is a first error check, not a production accuracy certification.
- The current terrain generator controls continental layout. A 65% ocean-area target does not by itself guarantee a preferred number of continents or islands; inspect the resulting landmass arrangement and adjust the existing continental relief parameters separately if needed.
- No final ocean shader, wave system, underwater rendering, shoreline material, biome-specific production materials or vegetation instances are included.
- A perfectly flat heightfield cannot define a meaningful sea-level percentile. The generator now fails clearly and asks for macro-relief to be enabled instead of classifying the entire sphere as ocean.

## Validation status

- Source-level consistency review: performed.
- Static brace and API-reference checks: performed.
- Unity compilation: NOT RUN in this environment.
- EditMode tests: added, NOT RUN in this environment.
- 96/256 generation timing and memory: NOT MEASURED.
- Globe/planisphere visual inspection: PENDING owner validation.
- Independent QA: PENDING.

## Required owner validation

1. Open Tools → World → Open Planet Generation Studio.
2. Keep at least Layer 1 enabled; use the existing terrain layers for a non-flat heightfield.
3. Select `Water`, keep the target at 65%, and click Generate / Refresh Preview.
4. Confirm the blue ocean surface is on the outside of the globe and the console reports target, actual coverage, quantization tolerance and sea level.
5. Select `Biomes` and confirm four distinguishable land classes plus blue ocean.
6. Select `DistanceToWater` and `Moisture` to inspect their gradients.
7. Switch to Planisphere and regenerate; verify the same seed/field values and coherent wrap/poles.
8. Change ocean target to 60% then 70%; confirm actual area and derived sea level change monotonically.
9. Run the complete EditMode suite. Report exact pass/fail/ignored counts and any exception text.

Do not mark this tranche accepted until Unity compilation, the new tests, owner visual inspection and independent QA pass.

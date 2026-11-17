# WORLD-002 — Spherical Geographic Fields: Detailed Test Plan

**Scope inspected:** current implemented spherical Planet Studio and geographic-field prototype. This plan intentionally separates implemented fields from future hydrology/biome work.

## A. Planet Studio settings and terrain layers

### Existing EditMode coverage
- `SphericalPlanetStudioTests.DisabledLayersProducePerfectBaseSphere`
- `SphericalPlanetStudioTests.LayerContributionsAreDeterministicAndIndependent`
- `SphericalPlanetStudioTests.PreviewStageCanReturnToEarlierLayerWithoutLosingLaterLayerSettings`
- `SphericalPlanetStudioTests.ChangingThematicMapDoesNotChangeTerrainGeometry`
- `SphericalPlanetStudioTests.SettingsValidationClampsSamplingResolutionsAndRadius`

### Additional tests required
- Each relief layer can be enabled/disabled independently without mutating the other layer settings.
- Same seed + same settings produce identical sampled values and mesh vertices.
- Changing the seed changes a representative sample set.
- Invalid enum values and invalid/non-finite sample directions have explicit behaviour.
- Every generated face has valid triangles, normals, bounds and expected counts at multiple resolutions.
- Thematic map selection changes presentation data only, never the underlying terrain geometry.

## B. Globe and planisphere

### Existing EditMode coverage
- `PlanisphereAndGlobeUseSameDirectionBasedHeightField`
- `PlanisphereWrapBoundaryMapsToSameSphericalDirection`
- `TemperatureMapIsBounded`

### Additional tests required
- Round-trip mapping across multiple longitudes, latitudes, poles and wrap boundaries.
- Globe and planisphere sample the same altitude and temperature for the same canonical direction.
- No NaN/Infinity at the poles or map boundaries.
- Mesh UVs and indices remain valid for minimum and maximum supported map resolutions.
- Projection seam visual check for the wrap boundary.

## C. Geographic field storage and generation

### Existing EditMode coverage
- `SphericalGeographicFieldGeneratorTests.Generate_ProducesExpectedResolutionAndMetadata`
- `Generate_IsDeterministicForSameSettings`
- `Generate_FieldsStayWithinDeclaredRanges`
- `DirectionSampling_ReturnsFiniteValuesForAllGeneratedFields`
- `SharedFaceEdgeSamples_HaveMatchingAltitudeAndTemperature`

Implemented fields in the inspected source:
- altitude (consumed from the existing WORLD-001 composed height sampler);
- normalized temperature (latitude trend, seeded spherical noise and altitude cooling);
- slope in degrees (finite-difference sampling in a local tangent basis).

### Additional tests required
| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W002-FLD-01 | `SlopeOnConstantHeightFieldIsApproximatelyZero` | Flat/constant analytic input produces near-zero slope | EditMode |
| W002-FLD-02 | `SlopeOnKnownAnalyticSurfaceMatchesReference` | Slope agrees with an analytic/reference gradient within a documented angular tolerance | EditMode |
| W002-FLD-03 | `SlopeRemainsContinuousAcrossEveryFaceSeam` | Paired samples across all face seams remain within the approved degree tolerance | EditMode |
| W002-FLD-04 | `FieldSetRejectsInvalidIndicesAndFieldTypes` | Invalid face coordinates/field enum produce explicit failures | EditMode |
| W002-FLD-05 | `SamplingAtPolesAndSeamsIsFinite` | Every generated field samples finite values at poles, seams and corners | EditMode |
| W002-FLD-06 | `GenerationDoesNotMutateInputSettingsAsset` | Settings supplied by caller remain unchanged after generation | EditMode |
| W002-FLD-07 | `FieldsAreStableAcrossSupportedResolutions` | Resolution changes refine sampling without producing invalid ranges or seam discontinuities | EditMode |

## D. Spherical neighbour graph and distance prototype

### Existing EditMode coverage
- `Build_MergesSharedFaceEdgesAndCorners`
- `Build_ProducesSymmetricConnectedPositiveEdges`
- `FaceSeams_MapToTheSameCanonicalNode`
- `MultiSourceDijkstra_IsDeterministicAndZeroAtSources`
- `Benchmark_ReportsGraphAndGreatCircleError`

### Additional tests required
- Exact node-count formula `6N² + 2` at several supported resolutions.
- Each of the twelve face seams and eight cube corners maps to one canonical node.
- Edge lengths match the great-circle formula for known neighbouring directions.
- A small hand-built/reference graph returns exact expected multi-source distances.
- Empty sources, invalid source IDs, invalid graph dimensions/radius and unreachable nodes follow explicit failure semantics.
- Distances are non-negative; source distances are zero; relaxation never increases a shortest-path estimate.
- Benchmark error and runtime are recorded at several resolutions with fixed source placement and seed.
- Do not accept production hydrology until a maximum/mean error target and performance budget have been approved.

## E. Not implemented in this spherical slice — do not claim tested

The following remain future WORLD-002 stages in the inspected source:
- spherical water/ocean/lake/river classification;
- spherical nearest-water distance for production;
- moisture derived from spherical hydrology;
- vegetation suitability;
- biome classification;
- settlement suitability/candidate generation;
- biome-aware ground materials and 3D vegetation preview.

These are marked **NOT IMPLEMENTED / NOT TESTABLE YET**, not failed and not passed.

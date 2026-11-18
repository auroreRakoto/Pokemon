# WORLD-002 — Ocean Coverage and Four-Biome Test Plan

## Automated EditMode tests

Run `SphericalGeographicFieldGeneratorTests`.

| Test | Expected invariant |
|---|---|
| `Generate_ProducesExpectedResolutionAndMetadata` | Field resolution, seed and radius metadata are preserved |
| `Generate_IsDeterministicForSameSettings` | Continuous source fields are deterministic |
| `Generate_FieldsStayWithinDeclaredRanges` | Temperature and slope stay within declared ranges |
| `SharedFaceEdgeSamples_HaveMatchingAltitudeAndTemperature` | Continuous, categorical and moisture values agree across a shared face edge |
| `Generate_OceanCoverageMatchesAreaWeightedTargetWithinCellQuantization` | Measured ocean surface area matches the target within the elevation-class quantization bound |
| `Generate_WaterAndBiomeClassesAreMutuallyExclusiveAndComplete` | Ocean has no terrestrial biome; every land sample has exactly one approved biome; moisture is normalized |
| `Generate_OceanAndBiomeFieldsAreDeterministic` | Sea level, coverage, water classes, biome IDs and moisture repeat exactly |
| `Generate_HigherOceanTargetProducesHigherOrEqualSeaLevel` | Raising target coverage cannot lower the derived sea-level threshold or measured coverage |
| `Generate_DistanceToOceanIsZeroOnOceanAndPositiveOnLand` | Ocean distance is zero and land distance is non-negative/positive |
| `Generate_GeodesicDistanceApproximatesNearestOceanGreatCircle` | Small-resolution graph distance is no shorter than direct great-circle distance and remains within the test's provisional 1.5× plus two-cell allowance |
| `SettingsMigration_RestoresApprovedOceanTargetForPreWorld002Assets` | Existing serialized settings assets migrate to the approved 65% ocean target |

## Area-weighting requirements

- The area measure is computed from spherical triangle areas on each Cube-Sphere face cell.
- Each cell's area is distributed to its four corner nodes; contributions from adjacent face cells accumulate at canonical shared seam/corner node IDs.
- Ocean coverage is computed from the summed spherical weights of ocean nodes divided by total spherical weights.
- Raw vertex counts are not an acceptable coverage metric.
- The closest representable elevation class is selected. The generator reports a conservative quantization bound based on the largest equal-elevation class.

## Manual Unity visual checks

- `Water`: ocean is blue and visibly follows the derived sea-level mask; land remains distinct.
- `Biomes`: Plains, Forest, Desert and Mountains have distinct readable colours; ocean is not assigned a land biome.
- `DistanceToWater`: zero/dark-blue values occur at ocean; values increase inland.
- `Moisture`: moisture follows the distance field and the configured temperature/slope modifiers.
- Globe and planisphere consume the same generated fields; projection changes must not alter the generated data.
- Inspect all six face seams, eight cube corners, both poles and the planisphere left/right wrap.
- Check 60%, 65% and 70% ocean targets. Report actual coverage and the quantization bound from the console.

## Known limitations / interpretation

- This tranche's water mask contains oceans only. Lakes and rivers remain separate reserved water classes until drainage integration is implemented.
- The nearest-water calculation is a graph-geodesic approximation. Passing the provisional small-resolution reference test is not a substitute for a measured 96/256 error/performance report.
- Do not treat this test plan as evidence that hydrological networks or final climate/biome realism are approved.

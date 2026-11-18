# WORLD-007 — Planet Size and Ocean Range Test Plan

- **Status:** Tests authored; execution pending in Unity Editor.
- **Work package:** WORLD-007

## Automated EditMode coverage

Run the Unity Test Runner for the EditMode assembly and confirm:

1. `SphericalPlanetStudioTests.DisabledLayersProducePerfectBaseSphereAtConfiguredRadius` passes for 1 km, 5 km and 50 km and confirms the generated mesh uses the selected radius.
2. `SphericalPlanetStudioTests.SettingsValidationPreservesFiniteConfigurablePlanetRadii` passes for 100 m, 5 km, 50 km and 1,000 km.
3. `SphericalPlanetStudioTests.SettingsValidationRecoversInvalidPlanetRadius` restores NaN and positive/negative infinity to the existing 50 km default.
4. `SphericalPlanetStudioTests.SettingsValidationPreservesOceanCoverageTargetsInApprovedRange` preserves 1%, 25%, 49%, 50%, 65%, 80% and 99%.
5. `SphericalPlanetStudioTests.SettingsValidationClampsOceanCoverageToOneThroughNinetyNinePercent` clamps out-of-range values to 1% and 99%.
6. `SphericalPlanetStudioTests.SettingsValidationRestoresDefaultOceanTargetForNonFiniteValues` restores non-finite targets to 65%.
7. `SphericalGeographicFieldGeneratorTests.Generate_UsesConfiguredPlanetRadiusAcrossWorldSizes` generates geographic fields at 5 km, 50 km and 1,000 km and reports the selected radius.
8. `SphericalGeographicFieldGeneratorTests.Generate_OceanCoverageSupportsLowDefaultAndHighTargets` checks 1%, 35%, 65% and 99% against the generator's spherical-area quantization tolerance.
9. `SphericalGeographicFieldGeneratorTests.Generate_ChangingPlanetRadiusDoesNotRescaleTerrainReliefSettings` confirms relief amplitudes and sampled directional terrain height are independent of radius.
9. Existing WORLD-001/WORLD-002 EditMode regression tests remain green.

## Manual Unity validation

1. Open **Tools → World → Open Planet Generation Studio**.
2. Confirm Planet definition displays **Planet radius (km)** and that 50 km is the default.
3. Enter 5 km, generate the spherical preview, and confirm the generated globe radius is 5,000 m (not 5 m and not 50,000 m).
4. Enter 50 km and repeat. Confirm the same generator path is used.
5. Open `WORLD-002_BiomeInspection.unity`, regenerate/refresh the geographic preview through the existing workflow, enter Play Mode, and confirm the playable bootstrap uses the selected radius.
6. Confirm player dimensions, movement speed, gravity, and terrain relief amplitudes do not silently change when radius changes.
7. Generate ocean targets of 35%, 65%, and 90%. Confirm the UI accepts each value and the generation log reports target/actual coverage within the quantization tolerance.
8. Generate the 1% and 99% endpoints. Inspect actual coverage, sea level, and land/ocean existence. Record any resolution-dependent quantization limitations rather than assuming exact equality.
9. Save the settings asset, close/reopen Planet Studio, and confirm radius and ocean target persist.

## Evidence to record

For each run, record Unity version, seed, radius (km), face resolution, requested ocean target, actual ocean coverage, quantization tolerance, derived sea level, and pass/fail observations. Capture the Console and Game view for the small- and large-radius playable checks.

## Completion gate

Source review is not Unity validation. Do not mark WORLD-007 complete until compilation, automated EditMode tests, and manual checks have been executed and their evidence recorded. If work stops earlier, create a checkpoint commit message that accurately says validation is pending.

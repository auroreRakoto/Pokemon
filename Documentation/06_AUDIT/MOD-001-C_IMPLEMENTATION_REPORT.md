# MOD-001-C — Implementation report: World Core without biome classification

- **Status:** IMPLEMENTED — owner Unity validation required.
- **Work package:** MOD-001-C.
- **Owner module:** WORLD / World Core.
- **Plan milestone:** owner-confirmed approved and pushed; exact commit hash not supplied in this conversation.
- **Implementation delivery:** generated source ZIP; no local repository commit or push performed.
- **Source baseline:** `Pokemon-clean.zip` SHA-256 `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`.
- **Generated overlays:** MOD-001-A SHA-256 `48f2b8f8d25f958b9983347aba57d0552f8d1e46c067adf60cab36d88e912814`; MOD-001-B SHA-256 `fd91c6efb694af7ed17aa1da8a645fd89f4585229c8243272773e82bf510ba68`.

## 1. Implemented

- Added `Generate(settings, includeBiomeClassification)` while preserving `Generate(settings)` as the historical biome-enabled overload.
- World Core always generates altitude, temperature, slope, water class, distance-to-water and moisture.
- When biome classification is disabled, the generator does not allocate biome node arrays or six-face biome arrays and does not execute `ClassifyNodes`.
- Added `HasBiomeClassification`, `TryGetBiome` and `TrySampleBiome`.
- Kept legacy `GetBiome` and `SampleBiome`, but they now throw an explicit `InvalidOperationException` when biome data is absent. No fallback to Plains is introduced.
- Updated `SphericalGroundProbe` so altitude, slope, water, surface point and normal are available without biome classification. The sample now exposes `HasBiomeClassification`; `Biome` is `SphericalBiome.None` only when that flag is false.
- Split geographic generation helpers into cohesive partial files and extracted the indexed min-heap and weighted elevation comparer into dedicated files to comply with the 10-function file limit.
- Added a pure-data Editor preview at `Tools > World > Spherical Geographic Fields Preview`. It clones the selected profile, uses a fixed seed/resolution fixture, creates no scene objects and exposes Generate, Clear, Reset to Fixture, Validate and diagnostics.
- Added six dedicated EditMode tests for missing biome data, explicit legacy failure, World Core parity, legacy overload compatibility and ground-probe independence.

## 2. Consumer audit

All direct `SampleBiome`/`GetBiome` consumers in the supplied source snapshot were located:
- `SphericalGroundProbe`: updated to use `TrySampleBiome`; no longer depends on classification.
- `SphericalBiomeSurfaceMeshGenerator`: remains biome-dependent; legacy access now fails explicitly if called with a field set lacking classification. Activation/wiring is MOD-001-D.
- `SphericalPlanetStudioGenerator` Biomes thematic map: remains biome-dependent; legacy access now fails explicitly if called without classification.
- `SphericalEnvironmentPlacementGenerator`: remains biome-dependent under its current resolved placement rules; policy-aware optional placement is MOD-001-E.
- `SphericalBiomeInspectionBootstrap`: remains a biome inspection composition and continues using the legacy biome-enabled generation path until MOD-001-D/E/F composition work.
- `SphericalPlanetGenerationStudioWindow`: retains the legacy generation overload so existing Studio workflows continue to include biomes.

The current inspection bootstrap and environment placement are not yet wired to generate without biomes. This is intentional scope control, not a claim that the full module composition is complete.

## 3. Code-standard audit

Static scan of modified/new C# files:
- No file exceeds 10 functions/methods (the generator facade contains 10 including its four nested data-holder constructors).
- No function exceeds 50 logical code lines.
- Every function/constructor has a short descriptive XML comment.
- No new package, asmdef, global mutable state, service locator or UnityEditor reference was added to Runtime.
- Existing `.meta` files for modified scripts were preserved. New scripts received new `.meta` GUIDs.

## 4. Validation

| Check | Status | Evidence |
|---|---|---|
| Supplied source snapshot inspected | PASS | `Pokemon-clean.zip` plus MOD-001-A/B overlays |
| All biome field consumers searched | PASS | `SampleBiome`/`GetBiome` references audited |
| Source brace-balance static check | PASS | All 14 changed/new C# files balanced |
| Function count/line limit/comment audit | PASS — static | No file >10 functions; no function >50 logical lines |
| New EditMode tests authored | IMPLEMENTED | `SphericalGeographicBiomeOptionalTests.cs` — 6 tests |
| Independent pure-data preview authored | IMPLEMENTED | `SphericalGeographicFieldsPreviewWindow.cs` |
| Unity compilation | NOT RUN | No Unity Editor/runtime available in this environment |
| EditMode tests | NOT RUN | Must be executed by owner in Unity |
| Existing World/Spherical regression suite | NOT RUN | Must be executed by owner in Unity |
| Preview execution | NOT RUN | Must be opened in Unity |
| Full integration / target scene | BLOCKED | MOD-001-D/E/F have not yet wired module composition |

## 5. Known limitations

- No-biome generation is available at the World Core data API, but the existing inspection bootstrap still requests the legacy biome-enabled path.
- Biome renderer, biome thematic map, environment placement and inspection bootstrap remain explicit biome consumers until their approved follow-up work packages connect activation.
- This increment does not make terrain presentation optional; that is MOD-001-D.
- Four historical `SphericalBiomeTextureGeneratorTests` failures were previously reported in the project history. They were not changed or reclassified by this increment.

## 6. Owner Unity validation steps

1. Extract the ZIP at the project root.
2. Wait for Unity compilation and capture any compiler errors.
3. Run `SphericalGeographicBiomeOptionalTests`.
4. Run the full relevant World/Spherical EditMode suite, including topology, geographic field, biome rendering and movement tests.
5. Open `Tools > World > Spherical Geographic Fields Preview`.
6. Assign a `SphericalPlanetStudioSettings` asset and use Reset to Fixture.
7. Generate with Biome Classification enabled; Validate; record diagnostics.
8. Generate with Biome Classification disabled using the same seed/profile; Validate; compare ocean coverage, sea level, altitude range, water counts and other World Core fields.
9. Confirm no scene objects are created and the source profile asset remains unchanged.
10. Reopen the normal Planet Studio/inspection workflow and confirm its legacy biome-enabled path remains unchanged.

## 7. Next work package

`MOD-001-D` remains blocked until the owner validates MOD-001-C in Unity. D will connect the validated module activation report to optional terrain presentation and the neutral opaque terrain path.

## 8. Checkpoint commit

```text
refactor(world): separate biome classification from world core

Work-Package: MOD-001-C
Agent: WORLD
```

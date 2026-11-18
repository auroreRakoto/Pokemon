# WORLD-007 — Configurable Planet Size and Ocean Coverage Range

- **Work package:** WORLD-007
- **Agent:** WORLD
- **Status:** PLAN MILESTONE — owner-approved scope; implementation is gated on this plan being committed and pushed
- **Depends on:** WORLD-001 Planet Studio settings and spherical mesh generation; WORLD-002 geographic field generation; WORLD-003 playable biome inspection
- **Related decisions:** DEC-036, DEC-043, DEC-045

## 1. Goal

Make planet radius a normal world-generation choice rather than a test-only or production-only mode. The same generation pipeline and playable-world workflow must support small and large spherical worlds.

Also remove the current 50% lower bound on ocean coverage so the owner can create worlds with less than half their surface covered by ocean.

This work package changes configuration and validation. It does not introduce separate Test/Production profiles, automatic planet-size presets, or automatic scaling of gameplay/terrain parameters.

## 2. Confirmed owner decisions

1. A world can be created and played at a small or large radius using the same generation system.
2. There is no distinct test-scale mode and no production-scale mode.
3. Planet radius is a freely configurable world-generation parameter, stored internally in metres and presented to the user in kilometres.
4. The existing 50,000 m (50 km) radius remains the default unless the owner changes it.
5. Terrain relief, mountain thresholds, player dimensions, movement speed, gravity, and other gameplay values remain independently configurable. They must not be silently multiplied by a planet-radius scale factor.
6. Ocean coverage is configurable from 1% to 99% of spherical surface area. The default remains 65%.
7. Ocean coverage continues to be measured with spherical surface-area weighting; lakes and rivers remain separate from the ocean statistic.
8. The same generated settings and fields are used by the existing playable inspection workflow. A separate runtime new-game menu is not part of this tranche.

## 3. Current implementation findings

The supplied repository snapshot already contains a serialized `radiusMeters` setting and the playable inspection bootstrap passes it to the geographic generator, player controller, camera clipping setup, and overlay. The Planet Studio currently exposes the radius as a raw metres field.

Ocean coverage is currently restricted in three places:
- `SphericalPlanetStudioSettings.oceanCoverageTargetPercent` uses `[Range(50f, 80f)]`.
- `SphericalPlanetStudioSettings.Validate()` clamps the value to 50–80%.
- `SphericalPlanetGenerationStudioWindow.DrawOceanAndBiomeSettings()` displays a 50–80% slider.

The implementation must update all three restrictions. Updating only the UI would leave the runtime validation clamp in place.

## 4. Scope

### 4.1 Planet radius configuration

- Keep `radiusMeters` as the serialized/runtime source of truth to avoid changing downstream contracts and existing assets.
- Keep the existing minimum physical radius of 100 metres unless code review identifies a concrete reason to change it.
- Present the value in Planet Studio as `Planet radius (km)` for readability.
- Convert kilometres to metres at the UI boundary; do not change the units used by runtime geometry, geographic fields, movement, or metadata.
- Accept finite positive input at or above the documented minimum. Reject or safely normalize NaN and infinity.
- Do not add a fixed upper limit solely for UI convenience. Any practical upper bound must be justified by numeric precision, rendering, or performance evidence.
- Preserve the current 50 km default and serialized settings asset.
- Do not automatically rescale terrain amplitudes, mountain elevation thresholds, capsule size, movement speed, gravity, camera distance, or other gameplay settings when radius changes.
- Ensure the existing WORLD-003 playable inspection scene uses the selected radius from the same settings asset.

### 4.2 Ocean coverage configuration

- Change the serialized field's inspector range to 1–99%.
- Change `Validate()` to clamp to 1–99%.
- Change the Planet Studio slider to 1–99%.
- Preserve 65% as the default and preserve the existing geographic settings migration behaviour.
- Keep the area-weighted spherical ocean selection algorithm unchanged unless endpoint tests reveal a real defect.
- Keep inland lakes/rivers excluded from the ocean coverage percentage.

### 4.3 User-facing guidance

Planet Studio should explain that:
- radius changes the planet's physical size and curvature;
- terrain relief and gameplay parameters remain independent;
- smaller radii may require the owner to tune terrain amplitudes and mountain thresholds to achieve the desired geography;
- the ocean percentage is a target measured by spherical surface area, with small sampling/quantization differences reported by the generator.

## 5. Explicit non-goals

- Separate Test Scale / Production Scale modes or presets.
- Automatic proportional scaling of terrain relief, player size, speed, gravity, or jump.
- A runtime new-game menu or save-slot browser.
- Rework of the spherical terrain renderer, noise frequencies, or cube-sphere topology.
- New biome types, water classes, hydrology algorithms, or ocean rendering.
- Final biome textures/materials or Pokémon placement.
- Performance claims for arbitrarily large radii without profiling on the owner's PC.

## 6. Files expected to change

Expected implementation targets:
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioSettings.cs`
- `Assets/Project/Editor/World/Spherical/SphericalPlanetGenerationStudioWindow.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalPlanetStudioTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalGeographicFieldGeneratorTests.cs`
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md`
- `Documentation/03_ENGINEERING/ROADMAP_AND_VERTICAL_SLICE.md` or the relevant WORLD-007 progress record
- `Documentation/07_Testing/Work_Packages/WORLD-007_PLANET_SIZE_AND_OCEAN_RANGE_TEST_PLAN.md`
- `Documentation/00_GOVERNANCE/DOCUMENTATION_CHANGELOG.md`
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md`

Do not modify player movement code or generation algorithms unless tests demonstrate a specific dependency defect.

## 7. Required automated tests

### Settings validation
- Radius values at the minimum, 1 km, 5 km, 50 km, and a substantially larger value remain finite and are preserved.
- NaN and positive/negative infinity are normalized safely and do not propagate into generated geometry.
- Ocean targets 1%, 25%, 49%, 50%, 65%, 80%, and 99% survive validation unchanged.
- Out-of-range ocean values are clamped to 1% and 99%, not restored to the historical 50–80% range.
- Pre-WORLD-002 settings migration still restores the approved 65% default.

### Geographic generation
- Generate fields at at least 5 km and 50 km radii with the same seed/resolution; each result reports the exact configured radius.
- For identical seed and non-radius settings, changing radius does not silently alter configured terrain amplitudes or other independent settings.
- Ocean coverage at 1%, a below-50% value (e.g. 35%), 65%, and 99% is within the generator's documented area-weighted quantization tolerance where representable at the test resolution.
- Derived sea level remains finite at low and high ocean targets.
- Raising ocean target produces a non-decreasing measured ocean area and derived sea level.
- Existing determinism and cube-sphere seam tests continue to pass.

## 8. Manual Unity acceptance

1. Open Planet Studio and confirm the radius is displayed in kilometres.
2. Set a small playable radius (for example 5 km), generate the WORLD-002 biome inspection world, enter Play Mode, and verify the planet is rendered and the player can move on it.
3. Set the radius to 50 km using the same workflow and verify the same systems still work.
4. Confirm the player capsule size, walk speed, gravity, and terrain layer amplitudes have not been silently rescaled by changing radius.
5. Adjust terrain relief settings independently if the chosen small world needs gentler geography; confirm this is an explicit owner-controlled choice.
6. Generate worlds with ocean targets below 50% (for example 35%), at the 65% default, and above 80% (for example 90%).
7. Confirm the displayed/diagnostic actual ocean coverage tracks the requested target within quantization tolerance.
8. Confirm changing radius or ocean coverage does not mutate the approved default values in other settings assets or scenes.

Record Unity version, seed, radius, face resolution, target ocean percentage, actual ocean percentage, and any visual issues.

## 9. Acceptance criteria

- Planet radius is a clear, editable world-generation setting expressed in kilometres in the Planet Studio UI.
- Runtime generation and the playable inspection bootstrap use the selected radius without introducing a separate test mode.
- Small and large radii are supported by the same code path.
- Terrain/gameplay parameters remain independent of radius unless the owner explicitly changes them.
- Ocean target range is 1–99% in the serialized attribute, validation, and UI.
- The 65% default and legacy migration are preserved.
- Automated tests cover settings validation, multiple radii, and low/high ocean coverage.
- Unity compilation, EditMode tests, and manual Play Mode checks are reported separately; no feature is called complete until all required validation and QA gates pass.

## 10. Implementation sequence after plan push

1. Update radius validation for finite values and make the Planet Studio radius input readable in kilometres while retaining metres internally.
2. Update the ocean coverage attribute, validation clamp, and slider to 1–99%.
3. Add focused settings and geographic-generation tests for multiple radii and ocean endpoints.
4. Update the WORLD-007 test plan, decision register, roadmap/handoff references, and changelog.
5. Run available static checks; then request Unity compilation, EditMode tests, and manual Play Mode validation.
6. Prepare an implementation commit message that accurately reflects the actual validation state.

## 11. Workflow gate

Per `GIT_WORKFLOW.md` and DEC-016, this plan must be committed and pushed to the working branch before implementation starts. The supplied project archive has no Git metadata, so this deliverable prepares the plan but does not claim a commit or push. Implementation must begin only after the owner has committed and pushed this plan milestone.

# MOD-001-A — World profile module configuration

- **Status:** IMPLEMENTED — owner Unity validation required
- **Work package:** MOD-001
- **Increment:** MOD-001-A
- **Owner:** ARCH
- **Source archive:** `Pokemon-clean.zip`
- **Source SHA-256:** `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`
- **Plan milestone:** owner-reported pushed; exact commit hash not supplied.

## 1. Delivered

- Added `SphericalWorldModuleOptions`, a serializable profile-owned selection for:
  - terrain 3D presentation;
  - biome classification;
  - biome ground presentation;
  - environment assets;
  - player spawn/control;
  - Pokémon capability.
- Added the module options to `SphericalPlanetStudioSettings` so the existing Planet Studio profile load/save path preserves the selected values.
- Added a dedicated Editor window at `Tools > World > Configure World Modules`.
- The window edits an existing JSON world profile and saves the options to that same file.
- Added `moduleConfigurationVersion` as a versioned sub-schema. Existing world profile `schemaVersion` remains 1 to preserve the current profile loader contract.
- Existing profile JSON files were updated with explicit defaults matching the current inspected composition:
  - terrain presentation: enabled;
  - biome classification: enabled;
  - biome ground presentation: enabled;
  - environment assets: disabled;
  - player: enabled;
  - Pokémon: disabled.
- Legacy profiles without `moduleConfigurationVersion` migrate to the same defaults when `NormalizeModuleConfiguration` is called.
- Unsupported module configuration versions and missing current options are rejected with explicit exceptions.

## 2. Deliberate scope boundary

This increment persists and edits the selected module set. It does **not yet** change runtime composition. The current inspection bootstrap still creates its existing terrain/biome/player output according to its old flow; environment placement remains controlled by its existing config reference.

The following increments are still required before these options change generated runtime objects:
- dependency validation and activation report;
- optional biome classification and neutral terrain fallback;
- optional terrain presentation;
- optional environment placement;
- optional player/camera composition;
- future Pokémon implementation and activation.

Do not describe these checkboxes as already disabling runtime systems until those increments are implemented.

## 3. Compatibility

- World profile `schemaVersion` remains 1 because the existing Editor profile browser accepts only version 1.
- The new `moduleConfigurationVersion` sub-schema is 1.
- Profiles authored before this increment omit the new fields. Their missing integer version reads as 0 and is upgraded to defaults by `NormalizeModuleConfiguration`.
- Existing profile planet/climate values are unchanged.
- Existing ScriptableObject, scene, prefab and script GUIDs are unchanged. The settings asset receives one additive serialized `modules` field.
- The current Editor window and existing profile browser are not modified.

## 4. Code standard audit

| File | Functions | Max logical lines/function | Description comments |
|---|---:|---:|---|
| `SphericalWorldProfile.cs` | 7 | 15 | Present above every function |
| `SphericalWorldModuleOptions.cs` | 1 | 9 | Present above every function |
| `SphericalPlanetStudioSettings.cs` | 1 | 43 | Present above every function |
| `SphericalWorldModuleConfigurationWindow.cs` | 7 | 18 | Present above every function |
| `SphericalWorldModuleOptionsTests.cs` | 7 | 21 | Present above every test |

Logical-line counts are static estimates using the project convention (blank/comment-only lines excluded; structural code lines included).

## 5. Validation

| Check | Status | Evidence |
|---|---|---|
| Source snapshot and callers inspected | PASS | Profile model, JSON loader/browser, tests and Studio settings inspected |
| Profile JSON updated | PASS | All 6 existing JSON profiles include module config |
| Legacy profile migration path authored | IMPLEMENTED | `NormalizeModuleConfiguration` |
| Unsupported version rejection authored | IMPLEMENTED | EditMode tests added |
| JSON round-trip and migration tests authored | IMPLEMENTED | 7 new EditMode tests |
| Function count/line limit/comment audit | PASS — static | See code standard table |
| Unity compilation | NOT RUN | Unity Editor is owner-operated |
| EditMode test execution | NOT RUN | Requires owner Unity Test Runner |
| Editor window UI validation | NOT RUN | Requires owner Unity Editor |
| Runtime module activation | NOT IMPLEMENTED | Explicitly deferred to subsequent MOD-001 increments |

## 6. Owner runbook

1. Extract the ZIP at the Unity project root, preserving paths.
2. Review the diff; the ZIP contains only the five intended code files, three new `.meta` files, six profile JSON updates and this report/queue documentation.
3. Let Unity compile.
4. Open `Window > General > Test Runner`, choose EditMode and run `SphericalWorldModuleOptionsTests`.
5. Open `Tools > World > Configure World Modules`.
6. Load `Assets/Project/Data/WorldProfiles/temperate_balanced.json`.
7. Confirm the defaults shown; toggle one or more options and save.
8. Reopen the same profile and confirm values persist.
9. Load a copy of a legacy profile with the module fields removed; confirm it loads with compatibility defaults. Do not overwrite the original legacy profile during this check.
10. Load the profile in Planet Studio, save it as a new profile, and confirm the module options are preserved.
11. Send Console output, Test Runner results and a screenshot of the module window.

## 7. Limitations / next increment

- Options are currently profile metadata only; runtime consumers do not yet obey them.
- The module dependency/failure policy remains open. This increment does not infer or silently resolve dependencies.
- Pokémon remains a future capability flag only; no Pokémon source or placeholder is introduced.
- The module selection is made at world creation and immutable for the active session. Live hot-toggle is excluded.

Next increment: MOD-001-B — validate requested capabilities and produce a deterministic activation/dependency report before world composition.

## 8. Implementation checkpoint commit

`feat(world): persist optional module selection in world profiles`

`Work-Package: MOD-001`
`Agent: ARCH`

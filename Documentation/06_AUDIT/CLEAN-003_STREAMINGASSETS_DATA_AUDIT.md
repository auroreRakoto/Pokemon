# CLEAN-003 — StreamingAssets Starter-Data Audit

- **Status:** APPROVED — plan milestone ready; implementation not started
- **Audit owner:** TOOLS
- **Snapshot:** supplied Assets.zip, Tools.zip, Data.zip and Documentation.zip (archive entries dated 2026-11-17; uploaded 2026-11-19)
- **Repository access:** archive snapshot only; no live Git metadata or Unity Editor session was available to the assistant.

## 1. Candidate

`Assets/StreamingAssets/PokemonData/` is a legacy planar starter-data fixture for `world_meadow_01`. The supplied snapshot contains 47 files:

| Category | Count |
|---|---:|
| JSON definitions/manifests/schemas | 14 |
| Binary float32 map layers | 4 |
| Unity `.meta` files (files and folders) | 29 |
| **Total** | **47** |

There are 18 non-`.meta` files in total. The subtree includes two biome definitions, one legacy generation config, four map manifests, four binary map layers, two Pokémon species definitions, four schemas, and one world definition (these category counts are grouped descriptively; JSON count includes the definitions, manifests and schemas).

## 2. Static consumer audit

Searches across the supplied Assets snapshot found:

- No C# references to `PokemonData`, `Assets/StreamingAssets/PokemonData`, `world_meadow_01`, `chenipotte`, `mystherbe`, or the legacy schema filenames outside the fixture itself.
- No direct text references to the 29 `.meta` GUIDs in other text-based Assets files.
- No matching references in the supplied scenes, prefabs, ScriptableObjects or `.asmdef` files.

The audit is static and snapshot-bound. It cannot prove the absence of dynamic path construction in unprovided files, nor replace Unity Editor validation on the owner's current checkout.

## 3. Important distinction: active spherical build pipeline

The supplied archive includes:

- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs`
- `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs`
- `Assets/Project/Data/WorldGeneration/world_generation.json`
- `Assets/Project/Data/WorldProfiles/*.json`

The loader's runtime path is `Application.streamingAssetsPath/world_generation.json`, directly under StreamingAssets. The build processor copies the canonical `Assets/Project/Data/WorldGeneration/world_generation.json` there before build and deletes the generated copy afterward. It does not read `StreamingAssets/PokemonData/generation/world_generation.json`.

Therefore CLEAN-003 must preserve these files and the `Assets/StreamingAssets/` parent. The prior CLEAN-002 plan's claim that the loader and build processor had already been removed by CLEAN-001 is contradicted by this supplied snapshot and must not be repeated as fact.

## 4. Validator audit

`Tools/validate_data.py` identifies itself as a REBOOT-001 starter-data validator. It requires world definitions and map-layer manifests. CLEAN-002's reported removal leaves only `Data/biomes`, `Data/pokemon`, and `Data/schemas` at the root, so the root `Data/` is no longer a valid complete target. The only reported passing target was the `PokemonData` fixture now proposed for retirement. This supports removing the validator with the fixture rather than leaving an executable tool with no valid in-repository dataset.

Retain the root biome, species and schema JSON examples; CLEAN-003 does not remove them.

## 5. Documentation drift

The supplied `PROJECT_FILE_AND_FEATURE_INDEX.md` describes `PokemonData` as runtime-readable data and enumerates its folder tree. The supplied `CODE_FUNCTION_OWNERSHIP_INVENTORY.md` lists `PokemonDataSyncMenu.cs` and `JsonToScriptableObjectProjection.cs`, but neither file is present in the supplied `Assets.zip`. This mismatch requires a live `git ls-files` check before documentation edits. Do not assume the missing files are deleted from the current repo solely because they are absent from this archive.

## 6. Governance/source-status discrepancy

The standalone uploaded copies of `DEVELOPMENT_CONSTITUTION.md`, `SOURCE_REFERENCE_INDEX.md`, `ARCHITECTURE_BASELINE_V2.md`, `MASTER_ROADMAP_V2.md`, and `MODULE_PREVIEW_AND_TEST_STANDARD.md` all label themselves `PROPOSED — owner approval required`. The project instructions describe some of these as approved. This audit does not silently promote those documents or rewrite their status. CLEAN-003 is bounded by the explicit owner instruction to proceed and by the confirmed repository Git workflow; any broader governance status reconciliation remains a separate owner decision.

## 7. Required owner checks before deletion

```bash
git status --short --branch
git ls-files -- Assets/StreamingAssets/PokemonData Tools/validate_data.py
git grep -n -I -E 'PokemonData|world_meadow_01|chenipotte|mystherbe|validate_data.py' -- Assets Tools Documentation || true
```

After deletion and documentation updates:

```bash
git diff --check
git status --short
git diff --stat
git diff --cached --check
```

Unity owner checks:

1. Open the project and wait for asset import to finish.
2. Confirm no missing-script/reference/import errors in Console.
3. Compile scripts and run the existing relevant EditMode and PlayMode suites.
4. Build a player using the existing build configuration.
5. Confirm `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor` still stages the canonical JSON at the root StreamingAssets path and removes its temporary source copy after build.

No Unity or build check has been run by the assistant.

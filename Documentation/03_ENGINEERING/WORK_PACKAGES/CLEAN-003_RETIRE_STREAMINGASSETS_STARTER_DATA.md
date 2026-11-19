# CLEAN-003 — Retire the Legacy StreamingAssets Starter-Data Fixture

- **Status:** APPROVED — plan milestone ready for owner commit/push; implementation blocked until that push
- **Owner module:** TOOLS
- **Supporting modules:** ARCH, DOC, QA
- **Priority:** P0 — continuation of the bounded cleanup sequence
- **Owner authorization:** The project owner instructed: “go clean003” after reporting the CLEAN-002 deletion and validator result.
- **Snapshot basis:** supplied `Assets.zip`, `Tools.zip`, `Data.zip`, `Documentation.zip`, dated 2026-11-19 in the archive metadata; this is a snapshot, not a live repository inspection.
- **Dependencies:** CLEAN-002 removal reported by owner; CLEAN-002 plan commit hash not yet supplied; owner must verify current tracked paths before applying this plan.

## 1. Goal

Retire the remaining legacy planar starter-data fixture under `Assets/StreamingAssets/PokemonData/` and the Python validator dedicated to that data schema. Preserve the current spherical world-generation configuration, world profiles, runtime loader/build-copy mechanism, and the retained root JSON examples/schemas.

This package does **not** remove the general `Assets/StreamingAssets/` directory or the root-level `world_generation.json` that the existing build processor creates temporarily from the canonical source file.

## 2. Audit findings

### 2.1 Target fixture

The supplied `Assets.zip` contains `Assets/StreamingAssets/PokemonData/` with:

- 18 non-`.meta` data files: two biome JSON definitions, one legacy generation JSON, four map manifests, four binary `values.f32` layers, two Pokémon species JSON definitions, four JSON schemas, and one world JSON definition.
- 29 Unity `.meta` files, including folder metadata and file metadata.
- 47 total files in the subtree.

The fixture describes `world_meadow_01`, a 64×64 planar demo dataset. Its `generation/world_generation.json` identifies generator version `0.1.0` and calls itself a demonstration configuration. It is separate from the canonical spherical generation source and versioned spherical profiles.

### 2.2 Code consumers and similarly named active pipeline

A static search of the supplied `Assets.zip` found no C# reference to `PokemonData`, `Assets/StreamingAssets/PokemonData`, its JSON filenames, or its `world_meadow_01` data paths.

The archive **does contain** these files, contrary to the CLEAN-002 plan's statement that they had been removed by CLEAN-001:

- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs`
- `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs`

They are not consumers of the nested `PokemonData` fixture. The loader reads `Application.streamingAssetsPath/world_generation.json` (the root of StreamingAssets). The build processor copies `Assets/Project/Data/WorldGeneration/world_generation.json` to that root-level destination before a build and removes the generated copy afterward. This is a distinct path and must remain untouched.

The canonical source `Assets/Project/Data/WorldGeneration/world_generation.json` and `Assets/Project/Data/WorldProfiles/*.json` are also out of scope.

### 2.3 Unity serialized references and GUIDs

A text scan of the supplied Assets snapshot found no references to any of the 29 GUIDs belonging to the `PokemonData` subtree in the other text-based Assets files. No scene, prefab, ScriptableObject, or `.asmdef` in the snapshot contains a direct textual reference to this subtree.

This is useful static evidence, but it does not replace opening the current project in Unity. The owner must still check the current repository and run the Editor import/compile/build validation after applying the deletion. The `.meta` files must be removed together with the exact subtree they describe; do not regenerate or preserve orphaned metadata.

### 2.4 Dedicated validator

`Tools/validate_data.py` is explicitly described in its source as validating the `REBOOT-001 starter data`. It requires at least one world JSON and one biome JSON, and validates planar map manifests and float32 grid files. After CLEAN-002, the remaining root `Data/` contains only biome, Pokémon and schema examples; it has no world or map fixture and therefore cannot pass this validator. The only passing target used in CLEAN-002 was `Assets/StreamingAssets/PokemonData`, which this package retires.

Therefore the validator has no remaining complete dataset in the supplied snapshot and is included for removal in this package. The retained root `Data/biomes/`, `Data/pokemon/`, and `Data/schemas/` are preserved as reference/source examples; no claim is made that they are currently consumed by runtime code.

### 2.5 Documentation inconsistencies

The supplied documentation snapshot still lists `Assets/StreamingAssets/PokemonData/` as runtime-readable data and contains a C# inventory entry for `PokemonDataSyncMenu` and `JsonToScriptableObjectProjection`. Those two C# files are absent from the supplied `Assets.zip`. Treat the inventory entries as stale/historical until the owner confirms the live tracked paths; do not infer that those source files exist in the current repository.

The CLEAN-002 plan also incorrectly states that `WorldGenerationConfigLoader` and `WorldGenerationConfigBuildProcessor` were removed by CLEAN-001. The supplied Assets snapshot proves that both files are present. CLEAN-003 corrects this distinction in its audit; it does not modify those files.

## 3. Exact scope

### Included deletions

- Entire `Assets/StreamingAssets/PokemonData/` subtree, including its 18 data files and 29 `.meta` files (47 files total in the supplied snapshot).
- `Tools/validate_data.py`.

### Included documentation updates

- `Documentation/01_ARCHITECTURE/PROJECT_FILE_AND_FEATURE_INDEX.md` — remove the obsolete `PokemonData` runtime-data entries and describe the canonical spherical config/profile paths accurately; retain the root `Data/biomes`, `Data/pokemon`, and `Data/schemas` examples if still present after CLEAN-002.
- `Documentation/06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md` — reconcile the stale `PokemonDataSyncMenu` and `JsonToScriptableObjectProjection` entries against the live tracked code before editing; if absent, mark the entries as historical or remove them from the current inventory with a note about the snapshot discrepancy.
- `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md` — replace any claim that `Tools/validate_data.py` is an active passing validator with historical context and record the retirement.
- `Documentation/06_AUDIT/CLEAN-003_STREAMINGASSETS_DATA_AUDIT.md` — record the actual path list, static-reference findings, owner-run validation evidence and commit hash.
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` and `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md` only if their current contents require a cleanup progression/index entry; do not reorder unrelated work.

### Explicitly excluded / preserve

- `Assets/StreamingAssets.meta` and the `Assets/StreamingAssets/` parent directory.
- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs` and its `.meta`.
- `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs` and its `.meta`.
- `Assets/Project/Data/WorldGeneration/world_generation.json` and its `.meta`.
- `Assets/Project/Data/WorldProfiles/` and all spherical world-generation code/data.
- Root `Data/biomes/`, `Data/pokemon/`, and `Data/schemas/` examples.
- All scenes, prefabs, ScriptableObjects, shaders, materials, Blender assets, packages, ProjectSettings, network/server architecture, and gameplay code.
- Any files not named above.

## 4. Ownership and architecture impact

- **Owner:** TOOLS — legacy data/tool retirement.
- **Dependencies:** none at runtime after the static consumer scan, subject to current-repository confirmation.
- **Public contracts:** no runtime API or serialized spherical-world schema changes.
- **Build behaviour:** preserve the separate root-level StreamingAssets build-copy mechanism for the canonical spherical generation JSON.
- **Architecture impact:** none intended. This removes a legacy planar data fixture and its validator; it does not change DEC-020 or DEC-046.
- **No new packages, frameworks, global state, or cross-domain dependencies.**

## 5. Implementation sequence

1. Owner verifies the CLEAN-002 implementation commit and current branch status; do not stage unrelated deletions or uploaded archives.
2. Verify exact tracked targets with `git ls-files -- Assets/StreamingAssets/PokemonData Tools/validate_data.py`.
3. Search the current checkout for code/path references and all 29 GUIDs; stop if any live consumer is found.
4. Review current `CODE_FUNCTION_OWNERSHIP_INVENTORY.md` entries for the two absent pipeline files; reconcile only after checking `git ls-files` and current source.
5. Delete the `Assets/StreamingAssets/PokemonData/` subtree with its `.meta` files and delete `Tools/validate_data.py`.
6. Update only the documentation files listed in Section 3, based on the actual post-CLEAN-002 repository state.
7. Run static path/GUID searches and `git diff --check`.
8. Open Unity, allow import to finish, inspect Console, compile, and run relevant existing EditMode/PlayMode suites. Build validation must verify the root-level canonical world-generation JSON is still copied and cleaned up correctly.
9. Commit and push the implementation milestone on the same branch as the plan milestone.

## 6. Validation and acceptance

| Check | Expected | Status before implementation |
|---|---|---|
| Current tracked path audit | Only the listed subtree and validator are removed | OWNER TO RUN |
| Current C# and string-path scan | No live consumer of `PokemonData` | PASS on supplied snapshot; repeat on current checkout |
| GUID scan | No references to 29 fixture GUIDs | PASS on supplied snapshot; repeat on current checkout |
| Static path scan after deletion | No active documentation/code reference to the retired fixture | OWNER TO RUN after docs update |
| `git diff --check` | No whitespace errors | OWNER TO RUN |
| Unity import and compile | No missing references or compile errors | NOT RUN — owner must run in Unity |
| Existing EditMode tests | Relevant suites pass | NOT RUN — owner must run in Unity |
| Existing PlayMode tests | Relevant suites pass | NOT RUN — owner must run in Unity |
| Player build | Root `StreamingAssets/world_generation.json` is staged from canonical source and post-build cleanup still works | NOT RUN — owner must run in Unity |
| Blender | No Blender files or pipeline changed | N/A |
| QA / owner acceptance | QA PASS followed by explicit owner approval | PENDING |

Acceptance requires: exact deletion scope; preserved canonical spherical config/profile/build pipeline; no unresolved references; documentation reflects actual repository state; Unity import/compile and relevant tests pass; build processor behaviour is verified; QA PASS; owner acceptance.

## 7. Rollback

Restore only the exact deleted paths from the pre-CLEAN-003 commit using Git. Do not force-push. If Unity reports missing references or the player build no longer stages the canonical root JSON, stop and restore the relevant files before investigating. Do not regenerate `.meta` GUIDs manually.

## 8. Milestone commit messages

### Milestone A — plan

```text
docs(cleanup): plan legacy StreamingAssets data retirement

Audit the obsolete planar PokemonData fixture, its GUIDs and the
legacy validator. Preserve the canonical spherical generation config,
profiles and root StreamingAssets build-copy pipeline.

Work-Package: CLEAN-003
Agent: TOOLS
```

### Milestone B — implementation

```text
refactor(data): retire legacy StreamingAssets starter fixture

Remove the unused planar PokemonData fixture and its dedicated
validator while preserving the spherical world-generation pipeline.

Work-Package: CLEAN-003
Plan-Commit: <clean-003-plan-commit-hash>
Agent: TOOLS
```

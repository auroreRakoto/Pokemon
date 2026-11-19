# CLEAN-002 — Retire the Planar Demo Data Pipeline

- **Status:** APPROVED — plan milestone pending owner commit/push
- **Owner module:** TOOLS (data-generation tooling)
- **Supporting modules:** ARCH, DOC, QA
- **Priority:** P0 — cleanup sequence after CLEAN-001
- **Dependencies:** CLEAN-001 implementation commit `c316fb6` reported by owner; current supplied snapshot; no Unity execution required for this root-level data/tool removal
- **Target milestone:** remove the abandoned planar map dataset and its generator while preserving reusable species/biome/schema data and the validated StreamingAssets fixture.

## 1. Goal

Remove the obsolete external planar map-generation demo (`Data/generation`, `Data/maps/world_meadow_01`, `Data/worlds/world_meadow_01.json`) and its Python generator (`Tools/generate_demo_maps.py`). Preserve the reusable data definitions and schema samples, and retain the validator as a tool for the remaining validated runtime dataset.

## 2. Scope

### Included
- Delete `Data/generation/world_generation.json`.
- Delete all files under `Data/maps/world_meadow_01/`.
- Delete `Data/worlds/world_meadow_01.json`.
- Delete `Tools/generate_demo_maps.py`.
- Update documentation that describes the deleted root-level planar dataset or generator.
- Keep `Tools/validate_data.py`, but use it against `Assets/StreamingAssets/PokemonData` (the remaining complete dataset), not the reduced root `Data/` reference folder.

### Explicitly excluded
- `Data/biomes/*.json`.
- `Data/pokemon/*.json`.
- `Data/schemas/*.json`.
- `Tools/validate_data.py`.
- All of `Assets/StreamingAssets/PokemonData/` (deferred to CLEAN-003; it currently validates successfully).
- `Assets/Project/Data/WorldProfiles/` and spherical world-generation data.
- Unity scenes, prefabs, code, packages and Blender assets.
- Any change to DEC-007 dedicated-server direction or future Pokémon/Ecology product goals.

## 3. Current-state audit

Evidence is based on the supplied `Data.zip`, `Tools.zip`, `Assets.zip` and `Documentation.zip` snapshot dated 2026-11-19.

### Root Data content

Retain:
- `Data/biomes/forest.json`
- `Data/biomes/meadow.json`
- `Data/pokemon/chenipotte.json`
- `Data/pokemon/mystherbe.json`
- `Data/schemas/biome.schema.json`
- `Data/schemas/map_layer_manifest.schema.json`
- `Data/schemas/pokemon_species.schema.json`
- `Data/schemas/world.schema.json`

Delete:
- `Data/generation/world_generation.json` (580 bytes)
- `Data/maps/world_meadow_01/` (8 files, 68,190 bytes total)
- `Data/worlds/world_meadow_01.json` (971 bytes)

### Tools

- `Tools/generate_demo_maps.py` (4,405 bytes) creates deterministic flat X/Z float32 map grids and rewrites the planar world definition. It requires the old `Data/worlds/world_meadow_01.json` and writes into `Data/maps/`.
- `Tools/validate_data.py` (7,680 bytes) validates JSON definitions, references and map grids. Keep it. Its default root is `Data`, so after this cleanup the explicit validation target must be `Assets/StreamingAssets/PokemonData`.

### Validation evidence

- `python Tools/validate_data.py --data Data` on the supplied snapshot: FAIL, 12 errors. Each of the four map manifests disagrees with the root world definition in resolution, origin and world size.
- `python Tools/validate_data.py --data Assets/StreamingAssets/PokemonData` on the supplied snapshot: PASS. It validates definitions, references and map grids.
- `Assets/StreamingAssets/PokemonData` is a separate Unity asset dataset with `.meta` files; it is deliberately not removed in this work package.
- Static source scan of the supplied `Assets` C# files found no reference to the root `Data/` folder or `Tools/generate_demo_maps.py`.
- The old `WorldGenerationConfigBuildProcessor` and `WorldGenerationConfigLoader` referenced `StreamingAssets/world_generation.json`, but those were removed by CLEAN-001. The current spherical code does not reference `PokemonData`.
- Documentation still describes the root `Data/maps`, `Data/worlds`, `Data/generation` and old validator result in `PROJECT_FILE_AND_FEATURE_INDEX.md` and the historical repository audit. These references must be corrected/marked historical.

## 4. Ownership and architecture

- **Owning module:** TOOLS.
- **Data ownership:** the remaining root `Data/biomes`, `Data/pokemon` and `Data/schemas` are retained as reusable reference/source JSON examples; the Unity `Assets/StreamingAssets/PokemonData` dataset remains a separate runtime-readable fixture for now.
- **Dependencies:** Python standard library only for `validate_data.py`.
- **No new dependencies or frameworks.**
- **No runtime contract changes.**

## 5. Implementation plan

| Step | Change | Files | Validation |
|---|---|---|---|
| 1 | Push plan milestone | This work package and audit report | Owner reviews staged diff and pushes |
| 2 | Remove old world-generation config | `Data/generation/world_generation.json` | `git diff --cached --name-status` |
| 3 | Remove old planar map layers | `Data/maps/world_meadow_01/**` | Confirm exactly 8 files removed |
| 4 | Remove old world definition | `Data/worlds/world_meadow_01.json` | Confirm exact path |
| 5 | Remove old generator | `Tools/generate_demo_maps.py` | Confirm validator remains |
| 6 | Update file/feature index and historical audit wording | `PROJECT_FILE_AND_FEATURE_INDEX.md`, `REPOSITORY_AUDIT_REPORT.md` | Search for stale root Data/generator claims |
| 7 | Run retained validator against StreamingAssets fixture | `Tools/validate_data.py` | PASS expected; owner runs locally |
| 8 | Commit/push implementation milestone | Exact paths only | Owner supplies hash/output |

## 6. Tests and acceptance

| Check | Expected result | Status |
|---|---|---|
| Static C# scan for root `Data/` / generator references | No consumers | PASS on supplied snapshot |
| Root Data validator before cleanup | 12 metadata mismatch errors | PASS (failure reproduced) |
| StreamingAssets validator | Definitions, references and map grids valid | PASS on supplied snapshot |
| Git path check | Exact five targets/groups tracked and staged | OWNER TO RUN |
| Python validator after cleanup | `python Tools/validate_data.py --data Assets/StreamingAssets/PokemonData` passes | OWNER TO RUN |
| Unity import/compile | No Unity assets changed; still not run as part of this package | N/A for root-level data-only removal; broader Unity validation remains NOT RUN |
| Blender | No impact | N/A |

Acceptance:
- [ ] Only the approved root planar config/map/world and Python generator are deleted.
- [ ] Reusable root biome/species/schema JSON remains.
- [ ] `Tools/validate_data.py` remains.
- [ ] `Assets/StreamingAssets/PokemonData` remains unchanged.
- [ ] The retained validator passes against `Assets/StreamingAssets/PokemonData` on the owner's machine.
- [ ] Documentation no longer describes deleted root map data as active.
- [ ] Exact implementation commit is pushed.

## 7. Risks and rollback

- The root `Data` folder is outside Unity's `Assets`; no Unity `.meta` files are expected for these files.
- Do not delete the entire `Data/` or `Tools/` folder.
- Do not delete `Assets/StreamingAssets/PokemonData` in this package.
- If another owner-side tool references the old `Data/maps` paths, stop and add that consumer to the plan before deleting.
- Rollback exact paths from the pre-cleanup commit using Git; do not force-push.

## 8. Documentation impact

Expected implementation updates:
- `Documentation/01_ARCHITECTURE/PROJECT_FILE_AND_FEATURE_INDEX.md` — remove root planar map/config/world entries; keep retained definitions and StreamingAssets fixture entries.
- `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md` — mark old validator result historical and note current validation target.
- `Documentation/06_AUDIT/CLEAN-002_DATA_PIPELINE_AUDIT.md` — record actual removed paths and post-removal validator result.
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` — update cleanup progression if owner wants queue maintained after this lot.
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md` — index this work package/audit if required by current index format.

## 9. Commit messages

### Milestone A — plan

```text
docs(cleanup): add CLEAN-002 data pipeline retirement plan

Record the bounded removal scope for the obsolete planar demo dataset
and its generator. Preserve reusable JSON definitions, schemas and the
validated StreamingAssets fixture.

Work-Package: CLEAN-002
Agent: TOOLS
```

### Milestone B — implementation

```text
refactor(data): retire legacy planar demo dataset

Remove the obsolete planar map fixture and generator; retain reusable
biome/species/schema data and validate the StreamingAssets fixture.

Work-Package: CLEAN-002
Plan-Commit: <plan-commit-hash>
Agent: TOOLS
```

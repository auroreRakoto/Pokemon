# CLEAN-001 — Proposed Legacy Removal Plan

- **Status:** PROPOSED — owner approval required before deletion
- **Owner module:** ARCH (project architecture cleanup)
- **Supporting modules:** TOOLS, WORLD, DOC, QA
- **Target:** remove abandoned planar prototype residue while preserving the spherical-world foundation.

## 1. Goal

After the owner approves the scope, remove confirmed obsolete prototype residue and, if explicitly approved, retire the old planar world-generation feature as one coherent unit. Leave a clean spherical-world project that can be imported, compiled, previewed and tested independently.

## 2. Dependencies and owner decisions

1. Confirm whether the planar generation feature is to be retired despite `WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` §9.
2. Confirm that recovery scenes may be archived outside `Assets` and removed from the Unity project.
3. Keep the Blender pipeline outside this purge until its source/config/manifests are supplied or explicitly excluded by the owner.
4. Keep the dedicated-server direction in DEC-007 unchanged.

## 3. Batch plan

### Batch A — serialized legacy scene residue

Proposed removal:
- `Assets/_Recovery/0.unity` and `.meta`
- `Assets/_Recovery/0 (1).unity` and `.meta`
- `Assets/_Recovery/0 (2).unity` and `.meta`
- `Assets/_Recovery/0 (3).unity` and `.meta`
- `Assets/_Recovery/0 (4).unity` and `.meta`
- `Assets/_Recovery/0 (5).unity` and `.meta`
- `Assets/_Recovery.meta`
- `Assets/Project/Content/Scenes/Worlds/World01.unity` and `.meta`

Before deletion:
1. Owner creates or retains a backup outside the Unity project.
2. In Unity, confirm none of these scenes is open or required by a local workflow.
3. Recheck Build Settings and scene/prefab/asset references.
4. Delete through Unity Project window or a reviewed file operation so `.meta` sidecars are removed with their assets.
5. Reopen the remaining preview scenes and inspect Console.

Expected result: no recovery scenes or old planar scene remain in the active Unity project.

### Batch B — orphaned generated mesh outputs

Proposed removal:
- The eight `*.asset` meshes listed in CLEAN-001_AUDIT_REPORT.md §4 CLEAN-F03.
- Each matching `.meta` file.

Keep `Assets/Generated/Worlds/GeneratedWorldVertexColor.mat` only if the planar generator is retained.

Before deletion:
1. Re-run GUID/reference search against the current owner working tree.
2. Confirm no Addressables/catalog/manual path reference exists.
3. Confirm these are generated outputs, not authored source meshes.
4. Remove only the exact eight assets and sidecars.

Expected result: remove 116.1 MiB of unreferenced generated mesh assets from the repository snapshot.

### Batch C — retire the planar generation feature (ONLY IF OWNER APPROVES)

Remove as one feature, not as isolated files:

Runtime:
- `Assets/Project/Runtime/World/Generation/WorldGenerationConfig.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationResult.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerationSettings.cs`
- `Assets/Project/Runtime/World/Generation/WorldGenerator.cs`
- `Assets/Project/Runtime/World/Generation/WorldTerrainBuilder.cs`
- `Assets/Project/Runtime/World/Generation/WorldVertexColor.shader`
- `Assets/Project/Runtime/DataServices/Loading/WorldGenerationConfigLoader.cs`

Editor:
- `Assets/Project/Editor/WorldGeneration/WorldGenerationWindow.cs`
- `Assets/Project/Editor/WorldGeneration/WorldGenerationConfigBuildProcessor.cs`

Data/output:
- `Assets/Project/Data/WorldGeneration/world_generation.json`
- `Assets/Resources/WorldGeneration/world_generation.json`
- `Assets/Resources/WorldGeneration/` folder
- `Assets/Generated/Worlds/` folder and generated material, if no longer required
- all corresponding `.meta` sidecars/folder sidecars

Consumers found in the supplied source:
- `WorldGenerationWindow` calls `WorldGenerator` and `WorldTerrainBuilder`.
- `WorldGenerationWindow` calls `WorldGenerationConfigLoader.LoadForEditor()`.
- `WorldGenerationConfigBuildProcessor` uses the source path from `WorldGenerationConfigLoader`.
- No current spherical source consumer was found.
- `WorldGenerationConfig.Load()` has no caller in the supplied Assets source.

Required documentation migration:
- `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-001_SPHERICAL_WORLD_FOUNDATION.md` §9 — amend/supersede the planar preservation clause.
- `Documentation/07_Testing/Work_Packages/PLANAR_WORLD_GENERATION_TEST_PLAN.md` — mark historical/retired.
- `Documentation/07_Testing/COVERAGE_MATRIX.md` §4 — remove active planar coverage claims/gaps or mark historical.
- `Documentation/01_ARCHITECTURE/PROJECT_FILE_AND_FEATURE_INDEX.md` — remove planar runtime/editor entries.
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` — update actual next work and owner-approved ordering.
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` — record the owner decision that retires the planar pipeline and supersedes the old clause.
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md` and changelog — update links/statuses.

### Batch D — external planar demo data

Do not delete `Data/` or `Tools/` automatically.

Owner chooses:
- archive the planar map dataset and Python scripts outside the project; or
- regenerate it with `Tools/generate_demo_maps.py`, rerun `Tools/validate_data.py`, and keep it as an isolated reference tool.

Do not delete the two Pokémon species JSON examples or schemas until the POK/ECO data architecture identifies their replacement/canonical location.

### Batch E — package cleanup

Review after Batch A:
- `com.unity.ai.navigation`
- `com.unity.timeline`
- `com.unity.visualscripting`
- `com.unity.ugui` (old recovery scenes serialize UGUI components)

Do not remove until:
- all scene/prefab/asset serialized references are checked;
- Package Manager dependency graph is reviewed in Unity;
- package removal is followed by import/compile/test validation.

Retain Input System, URP and Test Framework. Do not interpret `com.unity.multiplayer.center` as the project's network stack; preserve DEC-007.

### Batch F — spherical base revalidation

Preserve and validate:
- `SphericalWorldDemo.unity`
- `WORLD-002_BiomeInspection.unity`
- `World02.unity` until Planet Studio preview regeneration is verified
- all current spherical runtime/editor scripts and tests
- world profiles, biome mapping, environment placement config/catalog and ten environment prefabs

Owner-run validation:
1. Open the project in Unity `6000.4.11f1`.
2. Wait for import; record Console errors/warnings.
3. Run all EditMode tests and export results.
4. Open `SphericalWorldDemo.unity`, enter Play Mode, generate/clear/regenerate.
5. Open `WORLD-002_BiomeInspection.unity`, enter Play Mode, verify terrain, spawn and environment placement.
6. Open `World02.unity`, regenerate the Planet Studio preview, compare output and decide whether to keep the saved scene.
7. Check Build Settings. The supplied snapshot currently contains only `Assets/Scenes/SampleScene.unity`; owner must decide which scene should be the launch/build scene before a build claim is made.
8. Run a target build only after compile and preview checks pass.
9. Return Console output, Test Runner export, screenshots and Build report.

## 4. Acceptance criteria

- [ ] All approved legacy removal candidates are removed with their `.meta` files.
- [ ] No retained scene contains missing custom script components.
- [ ] No remaining source references a removed type/path.
- [ ] Unity import and compile complete with zero errors.
- [ ] EditMode tests pass; exact Test Runner evidence is attached.
- [ ] Spherical demo preview generates, clears and regenerates without duplicates.
- [ ] Playable biome inspection generates and displays the current spherical world.
- [ ] WORLD-011 placement configuration/catalog/prefab references remain valid.
- [ ] Build scene selection is explicitly confirmed by the owner.
- [ ] Documentation, queue, indexes and decision record match the actual repository.
- [ ] QA reports PASS and owner accepts the cleanup.

## 5. Rollback

- Work on the same owner-selected branch for plan and implementation milestones.
- Before deleting, keep an external backup or rely on a confirmed Git commit.
- Delete only explicit paths; never use `git add .` or broad wildcard deletion without reviewing the staged diff.
- If Unity reports missing GUIDs or serialized references, stop and restore the relevant file and `.meta` pair.
- Restore via `git restore --source=<known-good-commit> -- <exact-paths>` only after the owner confirms the correct commit.
- Do not force-push or rewrite shared history.

## 6. Current validation status

| Check | Status |
|---|---|
| Static archive and reference audit | IMPLEMENTED for supplied snapshot |
| Unity import/compile | NOT RUN |
| EditMode | NOT RUN |
| PlayMode | NOT RUN |
| Build | NOT RUN |
| Blender round-trip | BLOCKED — Blender source omitted |
| Owner decision on planar retirement | PENDING |
| QA / owner acceptance | PENDING |

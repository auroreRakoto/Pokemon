# CLEAN-001 — Legacy Purge and Project Reconstruction

- **Status:** PROPOSED — audit complete; destructive scope requires owner approval
- **Owner module:** ARCH
- **Supporting modules:** TOOLS, WORLD, DOC, QA
- **Priority:** P0 — owner-requested first milestone
- **Roadmap phase:** Phase 0 / engineering baseline (proposed roadmap)
- **Dependencies:** owner decision on WORLD-001 §9; current project snapshot; Blender audit deferred
- **Target milestone:** clean spherical-world foundation ready for repair and revalidation

## 1. Goal

Remove obsolete prototype residue and, if explicitly approved, retire the isolated planar world-generation feature as one coherent unit. Preserve the current spherical-world direction and leave the Unity project importable, compilable and independently previewable.

## 2. User/developer value

The project no longer carries abandoned scene snapshots, unreferenced generated planar mesh outputs or duplicate planar configuration paths as active project content. The remaining source and documentation describe the actual spherical-world baseline.

## 3. Scope

### Included
- Audit and removal of approved recovery/legacy scenes.
- Removal of unreferenced generated planar mesh outputs.
- Conditional retirement of the planar generator/editor pipeline.
- Cleanup of the duplicate Resources world-generation loader/config.
- Reconciliation of stale documentation, indexes, queue and test references.
- Unity import/compile/test/preview validation after owner applies the delivered changes.

### Explicitly excluded
- Deleting or rewriting the spherical world runtime.
- Removing Planet Studio, spherical profiles, geography, movement, presentation or inspection tools.
- Removing WORLD-011 environment placement, configuration, prefab catalog, ten prefabs or production FBX assets.
- Changing DEC-007 dedicated-server direction or any approved networking architecture.
- Removing Pokémon species/schema samples before a POK/ECO data decision.
- Deleting Blender source/export content or changing the Blender sync pipeline without its source/config/manifests.
- Selecting the final Build Settings launch scene without owner decision.
- Broad refactors or speculative optimization.

## 4. Ownership and architecture

- **Owning module:** ARCH — project architecture cleanup.
- **Authoritative state owner:** project owner; Unity Git repository remains canonical.
- **Composition root:** no new runtime composition root is introduced.
- **Public contracts consumed:** current spherical-world public contracts are preserved; planar generator contracts are removed only if the owner approves the retirement.
- **Public contracts changed:** planar `WorldGeneration*` types and loader APIs, if Batch C is approved.
- **Allowed dependencies:** existing Unity Editor, Unity Test Framework and current spherical-world assemblies.
- **Forbidden dependencies:** new packages/frameworks, new global state, cross-domain coupling, new runtime dependency on Editor code.
- **Data categories:** legacy planar generated data and historical scene data; spherical world profiles/configuration remain authoritative for the current direction.

## 5. Current-state audit

- Current snapshot: six ZIP archives supplied on 2026-11-19.
- Current Assets inventory: 63 C# files, 11 scenes, 10 environment prefabs, 23 FBX files.
- Current spherical runtime is under `Assets/Project/Runtime/World/Spherical/`.
- Legacy planar generator is isolated to `Assets/Project/Editor/WorldGeneration/WorldGenerationWindow.cs` and its helper/build processor in the supplied source.
- `World01.unity` and recovery scenes contain old missing custom script components.
- Eight generated mesh outputs under `Assets/Generated/Worlds/` have no external GUID references in supplied Assets YAML.
- Ten environment prefabs are present and referenced by the environment prefab catalog.
- No Unity compile/test/preview/build was run.
- Blender source/config/manifests were not supplied.

## 6. Design and removal rules

- Remove legacy functionality in coherent batches; do not leave a half-removed Editor tool.
- Preserve `.meta` sidecars/GUIDs for all retained assets.
- Remove `.meta` only when the corresponding asset is approved for deletion.
- Recheck current owner working tree before every destructive batch; the supplied ZIP is only a snapshot.
- Do not use a text search as the only proof for Unity serialized references.
- Keep all uncertain assets `UNKNOWN` until their consumers are inspected.
- Mark retired work packages and test plans `HISTORICAL`; do not erase the decision trail.

## 7. Independent validation

- **Preview type:** existing spherical demo and playable biome inspection scenes.
- **Preview entry points:** `SphericalWorldDemo.unity`, `WORLD-002_BiomeInspection.unity`, Planet Studio Editor window.
- **Fixture/profile:** existing project settings and named world profiles.
- **Seed:** use the seed serialized in the settings/profile; record the actual seed in owner evidence.
- **Visible result:** spherical terrain, movement/camera, biome surface and optional WORLD-011 placement.
- **Diagnostics:** Unity Console, Planet Studio output, placement summary, Test Runner.
- **Clear/reset:** use the existing Generate/Clear operations; confirm regeneration does not accumulate duplicate roots.
- **Planar preview:** removed only if the owner explicitly approves Batch C.

## 8. Tests and acceptance

| Test category | Expected result | Status |
|---|---|---|
| Static reference scan | No retained code/scene references to removed planar types/assets | NOT RUN after implementation |
| Unity import/compile | Zero compile errors | NOT RUN |
| EditMode | Current spherical EditMode suite passes | NOT RUN |
| PlayMode | Current spherical PlayMode suite passes | NOT RUN |
| Module preview | Spherical demo generates/clears/regenerates | NOT RUN |
| Biome inspection | Shared fields, terrain, spawn and placement work | NOT RUN |
| Serialized references | No missing GUIDs or missing custom scripts in retained scenes | NOT RUN |
| Build | Owner-confirmed launch scene builds | NOT RUN |
| Blender | Sync/round-trip remains intact | BLOCKED pending Blender source |
| QA | Independent review PASS | PENDING |

Acceptance requires evidence; compilation alone is insufficient.

## 9. Risks and rollback

- Removing the planar pipeline conflicts with WORLD-001 §9. Owner decision required.
- Deleting recovery scenes may discard historical snapshots; archive outside Unity first.
- Generated mesh outputs are large but must still be checked in Unity before removal.
- The current Build Settings contains only `Assets/Scenes/SampleScene.unity`; the playable spherical preview scenes are not build scenes.
- The old `ownership.json` ledger and `_ART_TEST` FBX imports cannot be safely reconciled without Blender source/config/manifests.
- Rollback uses exact asset + `.meta` restoration from the owner-approved backup/commit.

## 10. Documentation impact

Expected after approval and implementation:
- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md`
- `Documentation/00_GOVERNANCE/DOCUMENT_INDEX.md`
- `Documentation/01_ARCHITECTURE/PROJECT_FILE_AND_FEATURE_INDEX.md`
- `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md`
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md`
- `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-001_SPHERICAL_WORLD_FOUNDATION.md`
- `Documentation/07_Testing/COVERAGE_MATRIX.md`
- `Documentation/07_Testing/Work_Packages/PLANAR_WORLD_GENERATION_TEST_PLAN.md`
- `Documentation/03_ENGINEERING/WORK_PACKAGES/WORLD-011-A_ENVIRONMENT_ASSET_READINESS_AUDIT.md`
- `Documentation/06_AUDIT/REPOSITORY_AUDIT_REPORT.md`
- `Documentation/06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md`

## 11. Work breakdown

| Step | Output | Validation |
|---|---|---|
| A — approve scope | Owner decision on planar retirement and scene purge | Explicit approval recorded |
| B — plan milestone | CLEAN-001 plan pushed to owner branch | Git diff reviewed |
| C — scene residue purge | Recovery scenes and World01 removed | Unity scene/reference check |
| D — generated outputs purge | Eight unreferenced mesh outputs removed | GUID scan + Unity import |
| E — planar pipeline retirement | Entire planar feature removed, if approved | Source scan + Unity compile |
| F — data/package decision | External Data/Tools and unused packages resolved | Data validator + Package Manager |
| G — spherical repair/revalidation | Spherical previews and tests pass | Unity Test Runner + screenshots |
| H — docs/QA/acceptance | Queue, indexes, decisions and reports updated | QA PASS + owner acceptance |

## 12. Commit messages

### Plan milestone — owner to execute after reviewing the proposal

```text
docs(cleanup): add CLEAN-001 legacy purge plan

Add the snapshot audit, legacy inventory and bounded removal plan.
No Unity project files are deleted or modified by this plan.

Work-Package: CLEAN-001
Agent: ARCH
```

### Implementation milestone

Not yet prepared. It must reflect the exact approved files actually removed and the validation evidence obtained. Do not use a completion subject until Unity validation and acceptance criteria pass.

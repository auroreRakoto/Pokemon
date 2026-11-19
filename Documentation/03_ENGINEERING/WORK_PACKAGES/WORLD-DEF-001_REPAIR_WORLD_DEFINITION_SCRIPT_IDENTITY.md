# WORLD-DEF-001 — Repair WorldDefinition Script Identity

- **Status:** PROPOSED
- **Owner module:** World (with Tools support for Unity asset validation)
- **Priority:** P1 — serialized content integrity
- **Dependencies:** owner approval; plan milestone committed and pushed; Unity Editor import/asset inspection
- **Related action:** `ACT-010` in `Documentation/06_AUDIT/REPOSITORY_ACTION_REGISTER.md`
- **Source snapshot:** `Pokemon-main (1).zip`, SHA-256 `df3061a74844331fa80120586aff9e4c7dcdb537a0c340ff98e7f1d1a7c0c93c`

## 1. Goal

Restore one unambiguous `WorldDefinition` ScriptableObject script identity and ensure the `FirstWorld.asset` points to that script, without changing world-generation behaviour or the asset's serialized values.

## 2. Current-state audit

The supplied archive contains both:

- `Assets/Project/Runtime/World/Biomes/WorldDefinition.cs`
- `Assets/Project/Runtime/World/Biomes/WolrdDefinition.cs`

The two source files are byte-identical (SHA-256 `18f8d118b9f93b6c9f986ba903a18951f414b9407cba2326cc8bd027e9c92365`) and both declare `public class WorldDefinition : ScriptableObject`.

Both `.meta` files are byte-identical and contain the same GUID:

`83e7a11a16a9d2e48a0f9113a1ff6158`

This creates a duplicate C# type declaration and duplicate Unity GUID in the archive.

The report `Documentation/06_AUDIT/CLEANUP_EXECUTOR_001_FINAL_REPORT.md` states that `WolrdDefinition.cs` was renamed to `WorldDefinition.cs`, with its `.meta` moved unchanged. The supplied archive contradicts that report because both old and new paths are present.

Additional serialized-reference finding:

- `Assets/Project/Content/Definitions/World/FirstWorld.asset` has asset GUID `75c0e3bfab59d054bb06af43437f837c`.
- Its `m_Script` references GUID `2c36bf92410248c4c95991fb08654bcf`.
- No `.meta` in this archive owns GUID `2c36bf92410248c4c95991fb08654bcf`.
- The canonical `WorldDefinition.cs.meta` owns GUID `83e7a11a16a9d2e48a0f9113a1ff6158`.
- The `FirstWorld.asset` asset GUID is referenced by `Assets/Project/Content/Scenes/Worlds/Meadow.unity` and five `Assets/_Recovery/*.unity` scenes. These references target the asset GUID and must remain unchanged.

Code consumers found in the archive:
- `Assets/Project/Editor/ContentCatalogBuilder.cs`
- `Assets/Project/Runtime/Infrastructure/ProjectContentCatalog.cs`
- `Assets/Project/Runtime/World/Generation/WorldBiomeGenerator.cs`
- `Assets/Project/Runtime/World/Presentation/WorldBiomeMapRenderer.cs`

No C# consumer refers to the misspelled type name; the misspelling appears only in historical/audit text and the stale source filename.

## 3. Scope

### Included
- Remove the duplicate typo source `Assets/Project/Runtime/World/Biomes/WolrdDefinition.cs`.
- Remove its duplicate sidecar `Assets/Project/Runtime/World/Biomes/WolrdDefinition.cs.meta`.
- Keep `WorldDefinition.cs` and its existing `.meta` unchanged, preserving GUID `83e7a11a16a9d2e48a0f9113a1ff6158`.
- Verify the Unity import and determine whether `FirstWorld.asset` has a missing script.
- If Unity confirms the script reference is missing and the canonical class is the intended script, update only `FirstWorld.asset`'s `m_Script.guid` from `2c36bf92410248c4c95991fb08654bcf` to `83e7a11a16a9d2e48a0f9113a1ff6158`.
- Preserve all other serialized fields and the asset GUID.
- Update the audit/action register/changelog only after the implementation and Unity validation evidence exist.

### Explicitly excluded
- Changes to `WorldDefinition` fields, defaults, biome configuration or generation logic.
- Changes to `WorldBiomeGenerator`, `WorldBiomeMapRenderer`, catalog APIs or their public contracts.
- Changes to the spherical world architecture or terrain roadmap.
- Changes to `FirstWorld.asset` values other than a verified `m_Script` GUID repair.
- Changes to scenes or their references to `FirstWorld.asset`.
- Any changes to Pokémon, ecology, networking or persistence.
- Broad cleanup or deletion of unrelated assets.

## 4. Ownership and architecture

- **Authoritative owner:** World owns the world-definition type and configuration.
- **Consumers:** Content catalog, world generation and biome-map presentation consume the definition.
- **Allowed dependencies:** existing Unity `ScriptableObject` and `BiomeDefinition` references only.
- **Forbidden changes:** no new module dependency, no new package, no new global registry, no new assembly definition.
- **Data category:** authored static definition.
- **Authority:** preserve DEC-007 dedicated-server/server-authoritative direction; this repair does not change authority or networking.

## 5. Implementation sequence

1. Confirm the plan milestone is committed and pushed.
2. In Unity, inspect `WorldDefinition.cs` import and the `FirstWorld.asset` Inspector.
3. Confirm whether `FirstWorld.asset` displays `Missing (Mono Script)`.
4. If missing, verify the canonical script's GUID in its `.meta` is `83e7a11a16a9d2e48a0f9113a1ff6158`.
5. Remove only the typo `.cs` and its `.meta`.
6. If step 3 confirmed a missing script, change only `FirstWorld.asset`'s `m_Script.guid` to the canonical GUID.
7. Reimport/refresh and reopen `FirstWorld.asset`.
8. Verify `worldName`, `seed`, `resolution`, biome references, noise scale, forest threshold and transition width are unchanged.
9. Open `Meadow.unity` and the five recovery scenes; verify references to the `FirstWorld.asset` remain resolved.
10. Run Unity compilation and the world-content/catalog validation available in the project.

## 6. Validation plan

| Check | Expected result |
|---|---|
| Source declaration scan | Exactly one `WorldDefinition` class declaration remains |
| Duplicate GUID scan | Exactly one `.meta` owns GUID `83e7a11a16a9d2e48a0f9113a1ff6158` |
| Missing GUID scan | No remaining serialized reference to `2c36bf92410248c4c95991fb08654bcf` |
| Asset identity | `FirstWorld.asset` retains GUID `75c0e3bfab59d054bb06af43437f837c` |
| Serialized values | All world-definition values and biome references remain unchanged |
| Scene references | Meadow and recovery scenes still resolve the same FirstWorld asset |
| Unity import | `WorldDefinition` imports without duplicate identity warnings |
| Unity compilation | Pass |
| Content catalog | `FirstWorld` is discoverable as `WorldDefinition` |
| Runtime smoke test | Existing world generation can read the definition; no generation semantics changed |

Unity Editor validation is mandatory; static inspection alone cannot close this work package.

## 7. Rollback

- Restore the typo source and `.meta` only if the owner establishes that the old path is still required; do not restore both copies as active compilation inputs.
- Restore the original `FirstWorld.asset` from version control if the GUID repair causes import or serialization issues.
- Do not change the canonical script GUID during rollback.

## 8. Acceptance criteria

- [ ] One `WorldDefinition` source declaration remains.
- [ ] One `.meta` owns the canonical script GUID.
- [ ] `FirstWorld.asset` references the canonical script GUID if and only if Unity confirmed the prior reference was missing.
- [ ] All serialized values and asset references are preserved.
- [ ] Unity compilation passes.
- [ ] Content catalog and world-definition import checks pass.
- [ ] QA records results and the owner accepts the work package.

## 9. Commit message

Implementation commit (after checks):

`fix(world): restore unique WorldDefinition script identity`

Required footers:
- `Work-Package: WORLD-DEF-001`
- `Plan-Commit: <PLAN_COMMIT_HASH>`
- `Agent: WORLD`

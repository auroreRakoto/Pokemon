# WORLD-DEF-001 — Static Audit Evidence

- **Status:** PROPOSED
- **Snapshot:** `Pokemon-main (1).zip`
- **SHA-256:** `df3061a74844331fa80120586aff9e4c7dcdb537a0c340ff98e7f1d1a7c0c93c`

## Findings

| Item | Evidence | Result |
|---|---|---|
| Duplicate source | `WorldDefinition.cs` and `WolrdDefinition.cs` have identical SHA-256 | Confirmed |
| Duplicate type | Both files declare `public class WorldDefinition : ScriptableObject` | Confirmed |
| Duplicate script GUID | Both `.meta` files contain `83e7a11a16a9d2e48a0f9113a1ff6158` | Confirmed |
| FirstWorld script reference | `FirstWorld.asset` `m_Script.guid` is `2c36bf92410248c4c95991fb08654bcf` | Confirmed |
| Owner for referenced script GUID | No `.meta` in this archive owns `2c36bf92410248c4c95991fb08654bcf` | Not found in snapshot |
| Canonical script GUID | `WorldDefinition.cs.meta` owns `83e7a11a16a9d2e48a0f9113a1ff6158` | Confirmed |
| FirstWorld asset identity | `FirstWorld.asset.meta` GUID `75c0e3bfab59d054bb06af43437f837c` | Confirmed |
| Scene consumers | `Meadow.unity` and five `Assets/_Recovery/*.unity` files reference the FirstWorld asset GUID | Confirmed |
| C# consumers | ContentCatalogBuilder, ProjectContentCatalog, WorldBiomeGenerator, WorldBiomeMapRenderer | Confirmed |
| Unity Inspector / import | Not available from ZIP inspection | NOT RUN |
| Unity compilation | Not available from ZIP inspection | NOT RUN |

## Important distinction

The archive contradicts its own `CLEANUP_EXECUTOR_001_FINAL_REPORT.md`, which says the typo file was renamed and the `.meta` moved. The archive still contains both paths.

The proposed repair retains the correctly spelled file and its current `.meta`. It removes only the duplicate typo path. The `FirstWorld.asset` script reference is changed only after Unity confirms that its current GUID resolves to a missing script and the canonical script is the intended class.

No implementation has been performed by this audit.

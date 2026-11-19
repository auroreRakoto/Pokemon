# Corrective checkpoint — suspend legacy ecosystem telemetry

- **Status:** IMPLEMENTED — owner Unity validation required
- **Purpose:** remove compile-time dependencies on the retired/absent Ecology and Pokémon runtime types without restoring the old ecosystem.
- **Scope:** `EcosystemLogger` and the new `EcosystemCsvWriter` introduced by the previous batch only.

## Evidence

The owner ran a search under `Assets` for:
- `EcosystemDataCollector.cs`
- `EcosystemSnapshot.cs`
- `PokemonAgent.cs`
- `PokemonDefinition.cs`
- `FruitBushManager.cs`
- `FruitBush.cs`
- `FruitType.cs`

The command returned no paths.

The previous `STD-002_CSHARP_BATCH_01_ECOSYSTEM_LOGGER.zip` introduced `EcosystemCsvWriter.cs` and changed `EcosystemLogger.cs`. Both depended on types absent from the owner's current project.

The earlier source archive contains `EcosystemLogger.cs` with GUID `d7aed1f14e2f942478d014f77e199f9c`, and serialized references to that component in `Meadow.unity` and recovery scenes. Therefore this correction preserves the existing `EcosystemLogger.cs.meta` unchanged and keeps the component class identity, rather than deleting the MonoBehaviour and leaving Missing Script references.

## Apply

1. Replace `Assets/Project/Runtime/Infrastructure/Diagnostics/EcosystemLogger.cs` with the file in this ZIP.
2. Delete `Assets/Project/Runtime/Infrastructure/Diagnostics/EcosystemCsvWriter.cs`.
3. Delete `Assets/Project/Runtime/Infrastructure/Diagnostics/EcosystemCsvWriter.cs.meta`.
4. **Do not modify or regenerate** `EcosystemLogger.cs.meta`; preserve its existing GUID.
5. Let Unity recompile and send the complete Console errors if any remain.

The replacement `EcosystemLogger` is intentionally inert. Legacy telemetry collection/export is suspended until its owning Ecology/Pokémon runtime modules are reintroduced through a future approved work package. Existing serialized `EcosystemLogger` components remain attached by their script GUID but no longer collect or write data.

## Validation

- Static evidence: the replacement file has no references to `EcosystemDataCollector`, `EcosystemSnapshot`, `PokemonAgent`, or other absent legacy types.
- Unity compilation: **NOT RUN**.
- Scene/prefab import and missing-script inspection: **OWNER TO RUN**.
- Telemetry functionality: **DEFERRED**, not validated.
- Other compiler errors: **not yet known**; Unity may reveal additional errors after these are removed.

## Rollback

Restore the previous `EcosystemLogger.cs` and `EcosystemCsvWriter.cs` from version control only if the full Ecology/Pokémon telemetry dependencies are restored together. Restoring only the logger/writer will reintroduce the current compile errors.

## Checkpoint commit

`fix(diagnostics): suspend legacy ecosystem telemetry dependencies`

`Agent: TOOLS`

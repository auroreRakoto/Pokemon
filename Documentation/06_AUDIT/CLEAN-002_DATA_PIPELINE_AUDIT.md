# CLEAN-002 — Data Pipeline Audit

- **Status:** AUDIT COMPLETE — implementation not yet applied
- **Snapshot:** supplied `Assets.zip`, `Data.zip`, `Tools.zip`, `Documentation.zip` (2026-11-19)
- **Owner module:** TOOLS
- **Work package:** CLEAN-002

## Finding 1 — Root planar demo dataset is inconsistent

The external root `Data/` contains a 2 km × 2 km, 256 × 256 world definition (`Data/worlds/world_meadow_01.json`), while its four map manifests each describe a 64 × 64 grid, origin (-64, -64), world size 128 × 128 m.

The supplied `Tools/validate_data.py --data Data` reproduces 12 errors: for each of four layers, dimensions, origin and worldSize disagree with the world definition.

The generator `Tools/generate_demo_maps.py` is tied to this exact planar format: it loads `Data/worlds/world_meadow_01.json`, creates float32 X/Z layers under `Data/maps/world_meadow_01/`, and rewrites the world definition.

## Finding 2 — A separate complete dataset exists under StreamingAssets

`Assets/StreamingAssets/PokemonData/` contains a separate 64 × 64 fixture with consistent world metadata and map manifests. The same validator returns:

```text
PASS: data definitions, references and map grids are valid
```

The current supplied C# source has no reader/reference to `PokemonData`. The old planar loader/build processor that referenced a StreamingAssets world-generation JSON was removed by CLEAN-001. This package therefore preserves StreamingAssets unchanged and defers its disposition to CLEAN-003.

## Finding 3 — Retain reusable JSON definitions

Keep root:
- `Data/biomes/forest.json`
- `Data/biomes/meadow.json`
- `Data/pokemon/chenipotte.json`
- `Data/pokemon/mystherbe.json`
- `Data/schemas/*.schema.json`

These are static definition/schema examples and are not the large planar map payload. They may support later POK/ECO data-contract work. The validator can continue to validate the complete StreamingAssets dataset with an explicit `--data` path.

## Finding 4 — No Unity C# consumer found

A static scan of supplied C# files found no reference to:
- root `Data/`;
- `Data/maps/`;
- `Data/worlds/`;
- `Tools/generate_demo_maps.py`;
- `PokemonData`.

The `Tools/validate_data.py` CLI remains a manual tool. It is not called by current C# source.

## Approved deletion set

- `Data/generation/world_generation.json`
- `Data/maps/world_meadow_01/` (all 8 files)
- `Data/worlds/world_meadow_01.json`
- `Tools/generate_demo_maps.py`

## Retained set

- `Data/biomes/`
- `Data/pokemon/`
- `Data/schemas/`
- `Tools/validate_data.py`
- `Assets/StreamingAssets/PokemonData/` (CLEAN-003 scope)

## Validation

| Check | Result |
|---|---|
| ZIP integrity | PASS |
| Static C# source reference scan | No consumers found |
| Root Data validator | FAIL — 12 metadata consistency errors |
| StreamingAssets validator | PASS |
| Unity compile/import | NOT RUN |
| Unity tests/build | NOT RUN |

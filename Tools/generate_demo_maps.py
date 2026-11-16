#!/usr/bin/env python3
"""Generate deterministic float32 little-endian demo environment grids."""
from __future__ import annotations
import argparse, json, math, random, struct
from pathlib import Path

LAYERS = {
    "height": ("continuous", "normalized_0_1", 0.0, 1.0),
    "humidity": ("continuous", "normalized_0_1", 0.0, 1.0),
    "temperature": ("continuous", "normalized_0_1", 0.0, 1.0),
    "vegetation_density": ("continuous", "normalized_0_1", 0.0, 1.0),
}

def clamp(v: float) -> float:
    return max(0.0, min(1.0, v))

def generate(width: int, height: int, seed: int):
    rng = random.Random(seed)
    phase = rng.random() * math.tau
    rows = {name: [] for name in LAYERS}
    for y in range(height):
        for x in range(width):
            nx = x / max(1, width - 1)
            ny = y / max(1, height - 1)
            # Broad deterministic shapes with restrained seeded variation.
            ridge = 0.5 + 0.20 * math.sin(nx * 5.2 + phase) + 0.13 * math.cos(ny * 4.1 - phase)
            hill = math.exp(-(((nx - 0.72) ** 2) / 0.045 + ((ny - 0.30) ** 2) / 0.08))
            height_v = clamp(0.35 + 0.45 * ridge + 0.20 * hill)
            moisture_v = clamp(0.72 - 0.38 * nx + 0.12 * math.sin(ny * 7.0 + phase) + 0.04 * math.cos(nx * 12.0))
            temp_v = clamp(0.70 - 0.28 * ny - 0.12 * height_v + 0.03 * math.sin(nx * 5.0))
            forest_mask = math.exp(-(((nx - 0.76) ** 2) / 0.07 + ((ny - 0.55) ** 2) / 0.13))
            meadow_mask = math.exp(-(((nx - 0.27) ** 2) / 0.12 + ((ny - 0.45) ** 2) / 0.20))
            vegetation_v = clamp(0.12 + 0.72 * forest_mask + 0.32 * meadow_mask + 0.10 * moisture_v)
            rows["height"].append(height_v)
            rows["humidity"].append(moisture_v)
            rows["temperature"].append(temp_v)
            rows["vegetation_density"].append(vegetation_v)
    return rows

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--output", default="Data", help="Data root directory")
    p.add_argument("--world-id", default="world_meadow_01")
    p.add_argument("--width", type=int, default=64)
    p.add_argument("--height", type=int, default=64)
    p.add_argument("--seed", type=int, default=481516)
    args = p.parse_args()
    if args.width < 2 or args.height < 2:
        raise SystemExit("width and height must be >= 2")
    root = Path(args.output).resolve()
    world_path = root / "worlds" / f"{args.world_id}.json"
    if not world_path.exists():
        raise SystemExit(f"World definition not found: {world_path}")
    world = json.loads(world_path.read_text(encoding="utf-8"))
    world["seed"] = args.seed
    world["mapResolution"] = {"width": args.width, "height": args.height}
    world["layers"] = []
    layers = generate(args.width, args.height, args.seed)
    for name, values in layers.items():
        folder = root / "maps" / args.world_id / name
        folder.mkdir(parents=True, exist_ok=True)
        data_name = "values.f32"
        data_path = folder / data_name
        with data_path.open("wb") as f:
            for value in values:
                f.write(struct.pack("<f", value))
        manifest = {
            "schemaVersion": 1,
            "id": f"{args.world_id}.{name}",
            "worldId": args.world_id,
            "layerType": "continuous",
            "semantic": name,
            "dataFile": data_name,
            "encoding": "float32",
            "byteOrder": "little",
            "channels": 1,
            "width": args.width,
            "height": args.height,
            "units": "normalized_0_1",
            "valueRange": {"min": 0.0, "max": 1.0},
            "coordinateSystem": "world_xz",
            "origin": world.get("origin", {"x": -64.0, "z": -64.0}),
            "worldSize": world.get("size", {"x": 128.0, "z": 128.0}),
            "rowOrder": "south_to_north",
            "sampling": "bilinear",
            "generator": {"id": "world_environment_generator", "version": "0.1.0", "seed": args.seed}
        }
        (folder / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
        world["layers"].append({"id": name, "manifestPath": f"maps/{args.world_id}/{name}/manifest.json"})
    world_path.write_text(json.dumps(world, indent=2) + "\n", encoding="utf-8")
    print(f"Generated {len(layers)} deterministic layers at {root / 'maps' / args.world_id}")

if __name__ == "__main__":
    main()

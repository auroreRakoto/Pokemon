#!/usr/bin/env python3
"""Validate the REBOOT-001 starter data using Python's standard library only."""
from __future__ import annotations
import argparse, json, math, struct, sys
from pathlib import Path

class Validator:
    def __init__(self, root: Path):
        self.root = root.resolve(); self.errors = []; self.warnings = []
    def error(self, msg): self.errors.append(msg)
    def read_json(self, path):
        try: return json.loads(path.read_text(encoding="utf-8"))
        except Exception as exc:
            self.error(f"{path.relative_to(self.root)}: invalid JSON ({exc})"); return None
    def files(self, folder): return sorted(folder.glob("*.json")) if folder.exists() else []
    def check_version_id(self, doc, path):
        if not isinstance(doc, dict): self.error(f"{path}: root must be an object"); return
        if doc.get("schemaVersion") != 1: self.error(f"{path}: schemaVersion must be 1")
        if not isinstance(doc.get("id"), str) or not doc["id"].strip(): self.error(f"{path}: missing non-empty id")
    def check_range(self, obj, key, path):
        r = obj.get(key)
        if not isinstance(r, dict) or not isinstance(r.get("min"), (int,float)) or not isinstance(r.get("max"), (int,float)):
            self.error(f"{path}: {key} requires numeric min/max"); return
        if r["min"] > r["max"]: self.error(f"{path}: {key}.min > max")
        if r["min"] < 0.0 or r["max"] > 1.0: self.error(f"{path}: {key} must stay within normalized range 0..1")
    def run(self):
        worlds = {}; biomes = {}; species = {}
        for folder, target in (("worlds", worlds), ("biomes", biomes), ("pokemon", species)):
            for path in self.files(self.root / folder):
                doc = self.read_json(path)
                if doc is None: continue
                self.check_version_id(doc, path.relative_to(self.root))
                if doc.get("id") in target: self.error(f"duplicate id {doc.get('id')} in {folder}")
                target[doc.get("id")] = doc
        for path in self.files(self.root / "biomes"):
            doc = self.read_json(path)
            if not doc: continue
            for field in ("humidity", "temperature", "vegetationDensity", "height"):
                if field not in doc.get("conditions", {}): self.error(f"{path.relative_to(self.root)}: missing conditions.{field}")
                else: self.check_range(doc["conditions"], field, path.relative_to(self.root))
        for path in self.files(self.root / "pokemon"):
            doc = self.read_json(path)
            if not doc: continue
            preferences = doc.get("habitatPreferences", {})
            for biome_id in preferences.get("preferredBiomeIds", []):
                if biome_id not in biomes: self.error(f"{path.relative_to(self.root)}: unknown biome id {biome_id}")
            for field in ("humidity", "vegetationDensity"):
                if field not in preferences: self.error(f"{path.relative_to(self.root)}: missing habitatPreferences.{field}")
                else: self.check_range(preferences, field, path.relative_to(self.root))
        for world_id, world in worlds.items():
            refs = world.get("biomeIds", [])
            for biome_id in refs:
                if biome_id not in biomes: self.error(f"world {world_id}: unknown biome id {biome_id}")
            seen_layers = set()
            for layer in world.get("layers", []):
                layer_id = layer.get("id")
                if layer_id in seen_layers: self.error(f"world {world_id}: duplicate layer id {layer_id}")
                seen_layers.add(layer_id)
                rel = Path(layer.get("manifestPath", ""))
                if rel.is_absolute() or ".." in rel.parts: self.error(f"world {world_id}: unsafe manifestPath {rel}"); continue
                manifest_path = (self.root / rel).resolve()
                if self.root not in manifest_path.parents: self.error(f"world {world_id}: manifest escapes Data root"); continue
                manifest = self.read_json(manifest_path)
                if not manifest: continue
                self.check_version_id(manifest, rel)
                if manifest.get("worldId") != world_id: self.error(f"{rel}: worldId mismatch")
                if manifest.get("semantic") != layer_id: self.error(f"{rel}: semantic does not match layer id {layer_id}")
                if manifest.get("encoding") != "float32" or manifest.get("byteOrder") != "little" or manifest.get("channels") != 1:
                    self.error(f"{rel}: starter validator supports float32 little-endian single-channel only")
                w, h = manifest.get("width"), manifest.get("height")
                if not isinstance(w, int) or not isinstance(h, int) or w < 2 or h < 2:
                    self.error(f"{rel}: width/height must be integers >= 2"); continue
                expected_resolution = world.get("mapResolution", {})
                if w != expected_resolution.get("width") or h != expected_resolution.get("height"):
                    self.error(f"{rel}: dimensions do not match world mapResolution")
                if manifest.get("origin") != world.get("origin"):
                    self.error(f"{rel}: origin does not match world origin")
                if manifest.get("worldSize") != world.get("size"):
                    self.error(f"{rel}: worldSize does not match world size")
                data_rel = Path(manifest.get("dataFile", ""))
                if data_rel.is_absolute() or ".." in data_rel.parts: self.error(f"{rel}: unsafe dataFile {data_rel}"); continue
                data_path = (manifest_path.parent / data_rel).resolve()
                if manifest_path.parent.resolve() not in data_path.parents: self.error(f"{rel}: dataFile escapes manifest folder"); continue
                if not data_path.is_file(): self.error(f"{rel}: missing data file {data_rel}"); continue
                expected = w * h * 4
                actual = data_path.stat().st_size
                if actual != expected: self.error(f"{rel}: expected {expected} bytes, got {actual}"); continue
                lo = manifest.get("valueRange", {}).get("min", 0.0); hi = manifest.get("valueRange", {}).get("max", 1.0)
                if not isinstance(lo, (int,float)) or not isinstance(hi, (int,float)) or lo > hi:
                    self.error(f"{rel}: invalid valueRange"); continue
                with data_path.open("rb") as stream:
                    index = 0
                    while chunk := stream.read(4):
                        value = struct.unpack("<f", chunk)[0]
                        if not math.isfinite(value): self.error(f"{rel}: non-finite value at index {index}"); break
                        if value < lo - 1e-6 or value > hi + 1e-6:
                            self.error(f"{rel}: value {value} outside [{lo}, {hi}] at index {index}"); break
                        index += 1
        if not worlds: self.error("no world JSON definitions found")
        if not biomes: self.error("no biome JSON definitions found")
        if not species: self.warnings.append("no Pokémon species definitions found")
        return not self.errors

def main():
    p=argparse.ArgumentParser(); p.add_argument("--data", default="Data"); args=p.parse_args()
    root=Path(args.data)
    if not root.is_dir(): raise SystemExit(f"Data directory not found: {root}")
    v=Validator(root); ok=v.run()
    for w in v.warnings: print(f"WARNING: {w}")
    for e in v.errors: print(f"ERROR: {e}")
    if ok: print("PASS: data definitions, references and map grids are valid")
    else: print(f"FAIL: {len(v.errors)} error(s)")
    return 0 if ok else 1
if __name__ == "__main__": sys.exit(main())

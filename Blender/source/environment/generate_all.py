
"""
ART - Environment Batch Generator
Lance automatiquement tous les générateurs environment.

Usage :
Blender > Text Editor > Run Script

Options :
- GENERATE_ALL : lance tous les générateurs.
- ASSET_FILTER : permet de limiter la génération.
"""

import bpy
import sys
import traceback
import importlib.util
from pathlib import Path

# --------------------------------------------------
# CONFIGURATION
# --------------------------------------------------

GENERATE_ALL = True

# Si GENERATE_ALL = False, seuls ces assets seront lancés.
ASSET_FILTER = [
    "grass_short",
    "grass_tall",
    "wild_grass",
    "flower_yellow",
    "flower_blue",
    "mushroom_red",
    "stone_cluster_small",
    "stone_cluster_large",
    "leaf_litter",
    "reed_clump",
    "fallen_log",
    "tree_stump",
]

# --------------------------------------------------
# ROOT
# --------------------------------------------------

import _core

BLENDER_ROOT = Path(_core.__file__).resolve().parent.parent
PROJECT_ROOT = BLENDER_ROOT.parent
SOURCE_ROOT = BLENDER_ROOT / "source" / "environment"

print("\n======================================")
print(" ART ENVIRONMENT BATCH GENERATOR")
print("======================================")
print("Project:", PROJECT_ROOT)
print("Source:", SOURCE_ROOT)

if not SOURCE_ROOT.exists():
    raise RuntimeError(f"Dossier introuvable : {SOURCE_ROOT}")

# --------------------------------------------------
# DISCOVERY
# --------------------------------------------------

generators = sorted(
    SOURCE_ROOT.glob("*/generator.py"),
    key=lambda p: p.parent.name.lower()
)

if not GENERATE_ALL:
    generators = [
        p for p in generators
        if p.parent.name in ASSET_FILTER
    ]

print(f"\n{len(generators)} générateur(s) détecté(s).\n")

# --------------------------------------------------
# EXECUTION
# --------------------------------------------------

success = []
failures = []

for index, script_path in enumerate(generators, start=1):

    asset_name = script_path.parent.name

    print("\n--------------------------------------")
    print(f"[{index}/{len(generators)}] {asset_name}")
    print("--------------------------------------")

    try:
        # Exécution dans un namespace indépendant
        namespace = {
            "__name__": "__main__",
            "__file__": str(script_path),
        }

        code = compile(
            script_path.read_text(encoding="utf-8"),
            str(script_path),
            "exec"
        )

        exec(code, namespace)

        success.append(asset_name)
        print(f"[SUCCESS] {asset_name}")

    except Exception as error:
        failures.append((asset_name, str(error)))
        print(f"[FAILED] {asset_name}: {error}")
        traceback.print_exc()

# --------------------------------------------------
# REPORT
# --------------------------------------------------

print("\n======================================")
print(" ART BATCH REPORT")
print("======================================")

print(f"\nTotal   : {len(generators)}")
print(f"Success : {len(success)}")
print(f"Failed  : {len(failures)}")

print("\nSUCCESS:")
for name in success:
    print("  +", name)

print("\nFAILED:")
for name, error in failures:
    print("  -", name, ":", error)

print("\n======================================")
print(" BATCH FINISHED")
print("======================================")

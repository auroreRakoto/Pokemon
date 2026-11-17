"""ART - Environment Batch Generator.

Run this file from Blender's Text Editor after opening it from the repository.
The batch discovers environment generators, excludes test assets by default,
and prints a complete execution report.
"""

from pathlib import Path
import sys
import traceback

import bpy

# -----------------------------------------------------------------------------
# CONFIGURATION
# -----------------------------------------------------------------------------

# Batch policy takes precedence over values declared in individual generators.
# PRODUCTION: enforce canonical production destinations and test_only=False.
# TEST: force selected generators into Environment/_ART_TEST and test_only=True.
# CUSTOM: preserve each generator's declared destination and test_only value.
MODE = "PRODUCTION"

# True: run every discovered generator. False: use ASSET_FILTER.
GENERATE_ALL = True

# Used only when GENERATE_ALL is False. Names are source subfolder names.
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

# Test-named generator folders are excluded by default.
# Set True to include them in TEST or CUSTOM mode.
INCLUDE_TEST_ASSETS = False

MODELS_ROOT = "Assets/Project/Content/Models"

# Canonical production routes, relative to MODELS_ROOT.
# This table is authoritative in PRODUCTION mode.
PRODUCTION_ASSETS = {
    "fallen_log": {"category": "NaturalFeatures", "destination": "Environment/NaturalFeatures/Fallen_Log/SM_Fallen_Log.fbx"},
    "flower_blue": {"category": "Vegetation", "destination": "Environment/Vegetation/Flower_Blue/SM_Flower_Blue.fbx"},
    "flower_yellow": {"category": "Vegetation", "destination": "Environment/Vegetation/Flower_Yellow/SM_Flower_Yellow.fbx"},
    "grass_short": {"category": "Vegetation", "destination": "Environment/Vegetation/Grass_Short/SM_Grass_Short.fbx"},
    "grass_tall": {"category": "Vegetation", "destination": "Environment/Vegetation/Grass_Tall/SM_Grass_Tall.fbx"},
    "leaf_litter": {"category": "Ground", "destination": "Environment/Ground/Leaf_Litter/SM_Leaf_Litter.fbx"},
    "mushroom_red": {"category": "NaturalFeatures", "destination": "Environment/NaturalFeatures/Mushroom_Red/SM_Mushroom_Red.fbx"},
    "reed_clump": {"category": "Vegetation", "destination": "Environment/Vegetation/Reed_Clump/SM_Reed_Clump.fbx"},
    "stone_cluster_large": {"category": "Rocks", "destination": "Environment/Rocks/Stone_Cluster_Large/SM_Stone_Cluster_Large.fbx"},
    "stone_cluster_small": {"category": "Rocks", "destination": "Environment/Rocks/Stone_Cluster_Small/SM_Stone_Cluster_Small.fbx"},
    "tree_stump": {"category": "NaturalFeatures", "destination": "Environment/NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx"},
    "wild_grass": {"category": "Vegetation", "destination": "Environment/Vegetation/Wild_Grass/SM_Grass_Wild.fbx"},
}

# -----------------------------------------------------------------------------
# BLENDER PROJECT DISCOVERY (must happen before importing _core)
# -----------------------------------------------------------------------------


def _active_text_file_path() -> Path:
    """Return the saved path of the active Blender Text Editor script."""
    try:
        space = bpy.context.space_data
        text_block = getattr(space, "text", None)
        if text_block and text_block.filepath:
            path = Path(bpy.path.abspath(text_block.filepath))
            if path.is_file():
                return path
    except (AttributeError, TypeError, RuntimeError):
        pass

    # Fallback for Blender contexts where space_data is not the Text Editor.
    candidates = []
    for text_block in bpy.data.texts:
        if not text_block.filepath:
            continue
        path = Path(bpy.path.abspath(text_block.filepath))
        if path.is_file() and path.name.lower() == "generate_all.py":
            candidates.append(path.resolve())

    candidates = list(dict.fromkeys(candidates))
    if len(candidates) == 1:
        return candidates[0]
    if len(candidates) > 1:
        raise RuntimeError(
            "Several generate_all.py files are open. Activate the intended "
            "repository script in Blender's Text Editor and run it again."
        )

    raise RuntimeError(
        "Cannot determine the saved path of generate_all.py. Open it from "
        "Blender/source/environment/ with Text > Open, save it, then run it."
    )


def _find_blender_root(script_path: Path) -> Path:
    """Walk upward until the repository's Blender/_core package is found."""
    for directory in (script_path.resolve().parent, *script_path.resolve().parents):
        if (directory / "_core" / "__init__.py").is_file():
            return directory
        blender_dir = directory / "Blender"
        if (blender_dir / "_core" / "__init__.py").is_file():
            return blender_dir

    raise RuntimeError(
        "Cannot locate Blender/_core/__init__.py from the batch script: "
        f"{script_path}"
    )


BATCH_SCRIPT_PATH = _active_text_file_path()
BLENDER_ROOT = _find_blender_root(BATCH_SCRIPT_PATH)
PROJECT_ROOT = BLENDER_ROOT.parent

if str(BLENDER_ROOT) not in sys.path:
    sys.path.insert(0, str(BLENDER_ROOT))

# Shared imports are deliberately after project discovery.
import _core  # noqa: E402

SOURCE_ROOT = BLENDER_ROOT / "source" / "environment"

# -----------------------------------------------------------------------------
# DISCOVERY AND SELECTION
# -----------------------------------------------------------------------------


def _is_test_generator(script_path: Path) -> bool:
    """Use the documented folder naming convention for test generators."""
    folder_name = script_path.parent.name.lower()
    return (
        folder_name.endswith("_test")
        or folder_name.startswith("test_")
        or folder_name in {"test", "tests", "_test", "_tests"}
    )


def _discover_generators():
    if not SOURCE_ROOT.is_dir():
        raise RuntimeError(f"Environment source directory not found: {SOURCE_ROOT}")

    discovered = sorted(
        SOURCE_ROOT.glob("*/generator.py"),
        key=lambda path: path.parent.name.casefold(),
    )

    if GENERATE_ALL:
        selected = list(discovered)
    else:
        requested = {name.casefold() for name in ASSET_FILTER}
        selected = [
            path for path in discovered
            if path.parent.name.casefold() in requested
        ]
        found_names = {path.parent.name.casefold() for path in selected}
        missing = sorted(requested - found_names)
        if missing:
            print("[BATCH][WARNING] Requested asset(s) not found:", ", ".join(missing))

    skipped_by_filter = len(discovered) - len(selected)

    if MODE.upper() == "PRODUCTION":
        production_selected = [path for path in selected if not _is_test_generator(path)]
        skipped_tests = len(selected) - len(production_selected)
        selected = production_selected
    elif not INCLUDE_TEST_ASSETS:
        production_selected = [path for path in selected if not _is_test_generator(path)]
        skipped_tests = len(selected) - len(production_selected)
        selected = production_selected
    else:
        skipped_tests = 0

    return discovered, selected, skipped_by_filter, skipped_tests


# -----------------------------------------------------------------------------
# EXECUTION
# -----------------------------------------------------------------------------


def _run_generator(script_path: Path):
    """Execute one generator in an isolated namespace and propagate failures."""
    policy = {
        "mode": MODE.upper(),
        "models_root": MODELS_ROOT,
        "production_assets": PRODUCTION_ASSETS,
    }
    namespace = {
        "__name__": "__main__",
        "__file__": str(script_path.resolve()),
        "__package__": None,
        "ART_BATCH_POLICY": policy,
    }
    source = script_path.read_text(encoding="utf-8-sig")
    compiled = compile(source, str(script_path), "exec")
    exec(compiled, namespace)


def main():
    print("\n====================================================")
    print(" ART ENVIRONMENT BATCH GENERATOR")
    print("====================================================")
    print(f"[BATCH] Project root       : {PROJECT_ROOT}")
    print(f"[BATCH] Blender root      : {BLENDER_ROOT}")
    print(f"[BATCH] Source root       : {SOURCE_ROOT}")
    print(f"[BATCH] _core version     : {_core.__version__}")
    mode = MODE.upper()
    if mode not in {"PRODUCTION", "TEST", "CUSTOM"}:
        raise ValueError("MODE must be PRODUCTION, TEST or CUSTOM")
    print(f"[BATCH] Selection mode   : {'ALL' if GENERATE_ALL else 'FILTERED'}")
    print(f"[BATCH] Policy mode      : {mode}")
    print(f"[BATCH] Include tests    : {INCLUDE_TEST_ASSETS}")
    if mode == "PRODUCTION" and INCLUDE_TEST_ASSETS:
        print("[BATCH][INFO] PRODUCTION mode always excludes test-named generator folders.")
    if mode == "TEST" and not INCLUDE_TEST_ASSETS:
        print("[BATCH][INFO] Test-named generator folders remain excluded; set INCLUDE_TEST_ASSETS=True to include them.")

    discovered, selected, skipped_by_filter, skipped_tests = _discover_generators()

    success = []
    failures = []

    print(f"\n[BATCH] Generators discovered : {len(discovered)}")
    print(f"[BATCH] Generators selected   : {len(selected)}")
    print(f"[BATCH] Skipped by filter     : {skipped_by_filter}")
    print(f"[BATCH] Test assets excluded  : {skipped_tests}")

    for index, script_path in enumerate(selected, start=1):
        asset_name = script_path.parent.name
        print("\n----------------------------------------------------")
        print(f"[BATCH][{index}/{len(selected)}] START {asset_name}")
        print("----------------------------------------------------")

        try:
            _run_generator(script_path)
            success.append(asset_name)
            print(f"[BATCH][SUCCESS] {asset_name}")
        except Exception as error:
            failures.append((asset_name, repr(error)))
            print(f"[BATCH][FAILED] {asset_name}: {error}")
            traceback.print_exc()

    executed = len(success) + len(failures)
    skipped_total = skipped_by_filter + skipped_tests

    print("\n====================================================")
    print(" ART ENVIRONMENT BATCH REPORT")
    print("====================================================")
    print(f"Discovered : {len(discovered)}")
    print(f"Selected   : {len(selected)}")
    print(f"Executed   : {executed}")
    print(f"Succeeded  : {len(success)}")
    print(f"Failed     : {len(failures)}")
    print(f"Skipped    : {skipped_total} (filter: {skipped_by_filter}, tests: {skipped_tests})")

    print("\nSUCCEEDED:")
    for name in success:
        print(f"  + {name}")

    print("\nFAILED:")
    for name, error in failures:
        print(f"  - {name}: {error}")

    print("\n====================================================")
    print(" BATCH FINISHED")
    print("====================================================\n")

    return {
        "discovered": len(discovered),
        "selected": len(selected),
        "executed": executed,
        "succeeded": len(success),
        "failed": len(failures),
        "skipped": skipped_total,
        "success_assets": success,
        "failed_assets": failures,
    }


BATCH_RESULT = main()

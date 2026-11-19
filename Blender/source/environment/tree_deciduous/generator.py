"""ART environment asset generator: tree_deciduous.

A stylized broadleaf tree with a sturdy trunk and layered green crown.
Run from Blender Text Editor or through generate_all.py.
"""
from pathlib import Path
import sys
import traceback
import bpy


def _find_blender_root():
    candidates = []
    try:
        active_text = getattr(bpy.context.space_data, "text", None)
        if active_text and active_text.filepath:
            candidates.append(Path(bpy.path.abspath(active_text.filepath)))
    except (AttributeError, TypeError, RuntimeError):
        pass
    try:
        script_path = Path(__file__)
        if script_path.is_absolute() and script_path.exists():
            candidates.append(script_path)
    except (NameError, OSError, TypeError):
        pass
    if bpy.data.filepath:
        candidates.append(Path(bpy.path.abspath(bpy.data.filepath)))
    for candidate in candidates:
        candidate = candidate.resolve()
        if candidate.is_file():
            candidate = candidate.parent
        for directory in (candidate, *candidate.parents):
            if (directory / "_core" / "__init__.py").is_file():
                return directory
            blender_dir = directory / "Blender"
            if (blender_dir / "_core" / "__init__.py").is_file():
                return blender_dir
    raise RuntimeError("Cannot locate Blender/_core. Open this saved generator from the repository.")


BLENDER_ROOT = _find_blender_root()
if str(BLENDER_ROOT) not in sys.path:
    sys.path.insert(0, str(BLENDER_ROOT))

import _core
from _core.paths import ProjectPaths
from project_modules.environment.generators import (
    generate_asset, resolve_batch_policy, build_tree_deciduous
)

ASSET_KEY = "tree_deciduous"
ASSET_ID = "environment.tree_deciduous"
OBJECT_NAME = "SM_Tree_Deciduous"
UNITY_DESTINATION = "Assets/Project/Content/Models/Environment/Trees/SM_Tree_Deciduous.fbx"


def generate():
    unity_destination, test_only, asset_category = resolve_batch_policy(
        asset_key=ASSET_KEY,
        object_name=OBJECT_NAME,
        declared_unity_destination=UNITY_DESTINATION,
        declared_test_only=False,
        declared_asset_category="Trees",
        batch_policy=globals().get("ART_BATCH_POLICY"),
    )
    return generate_asset(
        asset_key=ASSET_KEY,
        asset_id=ASSET_ID,
        object_name=OBJECT_NAME,
        unity_destination=unity_destination,
        builder=build_tree_deciduous,
        kwargs={'height': 8.0, 'seed': 31},
        source_path=f"Blender/source/environment/{ASSET_KEY}/",
        test_only=test_only,
        asset_category=asset_category,
    )


try:
    print("[ART] Starting", ASSET_KEY, "Blender", bpy.app.version_string)
    print("[ART] Project root:", ProjectPaths.discover(start=Path(_core.__file__)).root)
    generate()
    print("[ART] SUCCESS:", ASSET_KEY)
except Exception:
    print("[ART] FAILED:", ASSET_KEY)
    traceback.print_exc()
    raise

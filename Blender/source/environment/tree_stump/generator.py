"""ART environment asset generator: tree_stump.

Production environment asset generator.
Run from the saved Blender Text Editor script or through generate_all.py.
Exports only to Blender/exports; Unity transfer is handled by TOOLS.
"""
from pathlib import Path
import sys
import traceback
import bpy


def _find_blender_root():
    """Locate this repository's Blender directory without machine-specific paths."""
    candidates = []

    # The active Text Editor path is the most reliable source for Run Script.
    try:
        active_text = getattr(bpy.context.space_data, "text", None)
        if active_text and active_text.filepath:
            candidates.append(Path(bpy.path.abspath(active_text.filepath)))
    except (AttributeError, TypeError, RuntimeError):
        pass

    # Batch execution supplies an absolute __file__; standalone execution may not.
    try:
        script_path = Path(__file__)
        if script_path.is_absolute() and script_path.exists():
            candidates.append(script_path)
    except (NameError, OSError, TypeError):
        pass

    # Fallback: a saved .blend file inside the repository.
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

    raise RuntimeError(
        "Cannot locate Blender/_core. Open the saved generator from this repository "
        "with Text > Open, or save the .blend file inside the repository."
    )


BLENDER_ROOT = _find_blender_root()
if str(BLENDER_ROOT) not in sys.path:
    sys.path.insert(0, str(BLENDER_ROOT))

import _core
from _core.paths import ProjectPaths
from project_modules.environment.generators import generate_asset, resolve_batch_policy, build_stump

ASSET_KEY = 'tree_stump'
ASSET_ID = 'environment.tree_stump'
OBJECT_NAME = 'SM_Tree_Stump'
UNITY_DESTINATION = "Assets/Project/Content/Models/Environment/NaturalFeatures/Tree_Stump/SM_Tree_Stump.fbx"

def generate():
    unity_destination, test_only, asset_category = resolve_batch_policy(
        asset_key=ASSET_KEY,
        object_name=OBJECT_NAME,
        declared_unity_destination=UNITY_DESTINATION,
        declared_test_only=False,
        declared_asset_category="NaturalFeatures",
        batch_policy=globals().get("ART_BATCH_POLICY"),
    )
    return generate_asset(
        asset_key=ASSET_KEY,
        asset_id=ASSET_ID,
        object_name=OBJECT_NAME,
        unity_destination=unity_destination,
        builder=build_stump,
        kwargs={'height': 0.34, 'radius': 0.17},
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

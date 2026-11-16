"""ART environment asset generator: wild_grass.

Run from Blender Text Editor. Outputs only to Blender/exports.
The Unity destination in the manifest is a proposed test mapping; TOOLS must
approve the import/sync contract before any Unity copy is performed.
"""
from pathlib import Path
import sys, traceback
import bpy
import _core
from _core.paths import ProjectPaths
from project_modules.environment.generators import generate_asset, build_grass

ASSET_KEY = 'wild_grass'
ASSET_ID = 'environment.wild_grass'
OBJECT_NAME = 'SM_Grass_Wild'
UNITY_TEST_DESTINATION = 'Assets/Project/Content/Models/Environment/_ART_TEST/wild_grass/SM_Grass_Wild.fbx'

def generate():
    return generate_asset(
        asset_key=ASSET_KEY,
        asset_id=ASSET_ID,
        object_name=OBJECT_NAME,
        unity_destination=UNITY_TEST_DESTINATION,
        builder=build_grass,
        kwargs={'blades': 12, 'height': 0.55, 'width': 0.065, 'seed': 13},
        source_path=f"Blender/source/environment/{ASSET_KEY}/",
        test_only=True,
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

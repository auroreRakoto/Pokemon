"""ART environment asset generator: grass_short.

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

ASSET_KEY = 'grass_short'
ASSET_ID = 'environment.grass_short'
OBJECT_NAME = 'SM_Grass_Short'
UNITY_TEST_DESTINATION = 'Assets/Project/Content/Models/Environment/_ART_TEST/grass_short/SM_Grass_Short.fbx'

def generate():
    return generate_asset(
        asset_key=ASSET_KEY,
        asset_id=ASSET_ID,
        object_name=OBJECT_NAME,
        unity_destination=UNITY_TEST_DESTINATION,
        builder=build_grass,
        kwargs={'blades': 7, 'height': 0.36, 'width': 0.05, 'seed': 11},
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

"""ART proof asset: deterministic low-poly short grass tuft.

Run from Blender's Text Editor after adding <repo>/Blender to sys.path.
Exports only to Blender/exports; never writes to Unity Assets or .meta files.
"""
from pathlib import Path
import sys
import math
import traceback

import bpy
import bmesh


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
from _core.collections import get_or_create_collection, move_objects_to_collection
from _core.materials import get_or_create_material, assign_material
from _core.export import ExportProfile, export_fbx
from project_modules.environment.generators import resolve_batch_policy

ASSET_KEY = "grass_short_test"
ASSET_ID = "environment.vegetation.grass_short_test"
COLLECTION_NAME = "ART_TEST_GrassShort"
OBJECT_NAME = "SM_Grass_Short_Test"
FBX_NAME = "SM_Grass_Short_Test.fbx"
UNITY_TEST_DESTINATION = (
    "Assets/Project/Content/Models/Environment/Vegetation/"
    "_ART_TEST/Grass_Short_Test/SM_Grass_Short_Test.fbx"
)


def _remove_previous_test_collection():
    old = bpy.data.collections.get(COLLECTION_NAME)
    if old is None:
        return
    # Only remove objects belonging to this dedicated test collection.
    for obj in list(old.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(old)


def _make_grass_mesh():
    """Create five closed, tapered, gently curved blades in one mesh."""
    verts, faces, material_indices = [], [], []
    ring_levels = (0.0, 0.22, 0.48, 0.73, 0.91, 1.0)
    base_widths = (0.060, 0.052, 0.039, 0.023, 0.010, 0.0025)
    half_thickness = 0.006
    blade_specs = (
        (math.radians(-90), 0.39, 0.90),
        (math.radians(-18), 0.43, 1.00),
        (math.radians(54), 0.36, 0.82),
        (math.radians(126), 0.41, 0.94),
        (math.radians(198), 0.34, 0.78),
    )

    for blade_index, (angle, height, width_scale) in enumerate(blade_specs):
        radial_x, radial_y = math.cos(angle), math.sin(angle)
        tangent_x, tangent_y = -radial_y, radial_x
        first = len(verts)

        for level_index, t in enumerate(ring_levels):
            # Each blade leans outward progressively, then curls slightly inward.
            outward = 0.015 + 0.095 * (t ** 1.45)
            inward_curl = 0.012 * (t ** 5)
            center_x = radial_x * (outward - inward_curl)
            center_y = radial_y * (outward - inward_curl)
            center_z = height * t
            half_width = base_widths[level_index] * width_scale
            depth = half_thickness * (1.0 - 0.78 * t)
            corners = (
                (-half_width, -depth),
                ( half_width, -depth),
                ( half_width,  depth),
                (-half_width,  depth),
            )
            for side, across in corners:
                verts.append((
                    center_x + tangent_x * side,
                    center_y + tangent_y * side,
                    center_z + across,
                ))

        ring_count = len(ring_levels)
        # Closed bottom cap.
        faces.append(tuple(first + i for i in (3, 2, 1, 0)))
        material_indices.append(blade_index % 2)
        # Four side faces between every adjacent pair of rings.
        for ring in range(ring_count - 1):
            lower = first + ring * 4
            upper = lower + 4
            for side in range(4):
                nxt = (side + 1) % 4
                faces.append((lower + side, lower + nxt, upper + nxt, upper + side))
                material_indices.append(blade_index % 2)
        # Small closed tip cap.
        last = first + (ring_count - 1) * 4
        faces.append(tuple(last + i for i in range(4)))
        material_indices.append(blade_index % 2)

    mesh = bpy.data.meshes.new("MD_Grass_Short_Test")
    mesh.from_pydata(verts, [], faces)
    mesh.validate(verbose=False)
    mesh.update()

    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()

    obj = bpy.data.objects.new(OBJECT_NAME, mesh)
    mat_fresh = get_or_create_material("M_Grass_Test_Fresh", (0.24, 0.52, 0.12, 1.0))
    mat_light = get_or_create_material("M_Grass_Test_Light", (0.38, 0.66, 0.18, 1.0))
    assign_material(obj, mat_fresh, 0)
    assign_material(obj, mat_light, 1)

    for polygon, material_index in zip(mesh.polygons, material_indices):
        polygon.material_index = material_index
        polygon.use_smooth = False

    return obj


def generate():
    print("[ART] Grass Short test — start")
    print("[ART] Blender:", bpy.app.version_string)
    print("[ART] _core:", _core.__version__, _core.__file__)

    paths = ProjectPaths.discover(start=Path(_core.__file__))
    print("[ART] Project root:", paths.root)

    _remove_previous_test_collection()
    collection = get_or_create_collection(COLLECTION_NAME)
    obj = _make_grass_mesh()
    move_objects_to_collection([obj], collection, unlink_others=True)

    # Keep object origin at ground level and transforms clean for a static prop.
    obj.location = (0.0, 0.0, 0.0)
    obj.rotation_euler = (0.0, 0.0, 0.0)
    obj.scale = (1.0, 1.0, 1.0)
    bpy.context.view_layer.update()

    unity_destination, test_only, asset_category = resolve_batch_policy(
        asset_key=ASSET_KEY,
        object_name=OBJECT_NAME,
        declared_unity_destination=UNITY_TEST_DESTINATION,
        declared_test_only=True,
        declared_asset_category="Vegetation",
        batch_policy=globals().get("ART_BATCH_POLICY"),
    )

    export_dir = paths.export_asset("environment", ASSET_KEY)
    destination = export_dir / FBX_NAME
    result = export_fbx(
        objects=[obj],
        destination=destination,
        asset_id=ASSET_ID,
        domain="environment",
        asset_type="static_mesh",
        unity_destination=unity_destination,
        profile=ExportProfile.static_mesh(),
        source_path="Blender/source/environment/grass_short_test/",
        allowed_export_root=paths.exports,
        require_materials=True,
        require_uvs=False,
        asset_domain="Environment",
        asset_category=asset_category,
        extra_manifest={
            "purpose": "ART-001 first integration proof",
            "test_asset": True,
            "test_only": test_only,
            "dimensions_target_m": "approx. 0.25 x 0.25 x 0.43",
            "unity_import_note": "Temporary test destination only; do not sync automatically.",
        },
    )

    print("[ART] Export result:", result.to_dict())
    if not result.success:
        raise RuntimeError(result.message)

    print("[ART] FBX:", result.export_path)
    print("[ART] Manifest:", result.manifest_path)
    print("[ART] SUCCESS — Unity Assets and .meta files were not touched.")
    return result


try:
    generate()
except Exception:
    print("[ART] FAILED — full traceback follows")
    traceback.print_exc()
    raise

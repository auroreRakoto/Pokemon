"""Reusable deterministic environment mesh generators for Pokemon ART.

Only Blender authoring utilities live here. No Unity APIs, gameplay rules,
Unity Assets writes, or .meta handling are permitted in this package.
"""
from __future__ import annotations
import math
import random
import bpy
import bmesh
from mathutils import Vector
from _core.materials import get_or_create_material, assign_material
from _core.collections import get_or_create_collection, move_objects_to_collection
from _core.paths import ProjectPaths
from _core.export import ExportProfile, export_fbx

PALETTE = {
    "leaf_dark": (0.075, 0.22, 0.045, 1),
    "leaf": (0.16, 0.39, 0.07, 1),
    "leaf_light": (0.34, 0.58, 0.12, 1),
    "flower": (0.92, 0.55, 0.12, 1),
    "flower_light": (1.0, 0.82, 0.32, 1),
    "stem": (0.18, 0.30, 0.07, 1),
    "wood": (0.30, 0.16, 0.075, 1),
    "wood_light": (0.48, 0.29, 0.12, 1),
    "stone": (0.34, 0.37, 0.35, 1),
    "stone_light": (0.48, 0.51, 0.47, 1),
    "cap": (0.65, 0.18, 0.12, 1),
    "cap_light": (0.88, 0.38, 0.22, 1),
}
GENERATOR_VERSION = "0.2.0"

def _mat(key):
    return get_or_create_material("M_ENV_" + key.upper(), PALETTE[key])

def _mesh_object(name, verts, faces, material_keys, face_mats=None):
    mesh = bpy.data.meshes.new("MD_" + name)
    mesh.from_pydata(verts, [], faces)
    mesh.validate(verbose=False)
    mesh.update()
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    for i, key in enumerate(material_keys):
        assign_material(obj, _mat(key), i)
    if face_mats:
        for poly, idx in zip(mesh.polygons, face_mats):
            poly.material_index = idx
            poly.use_smooth = False
    return obj

def _append_tapered_blade(verts, faces, mats, angle, height, width, lean, key_index=0):
    tangent = Vector((-math.sin(angle), math.cos(angle), 0))
    radial = Vector((math.cos(angle), math.sin(angle), 0))
    levels = [(0,.95),(0.22,.78),(0.48,.58),(0.72,.36),(0.9,.16),(1,.025)]
    first = len(verts)
    for t, scale in levels:
        center = radial * (lean * t * t) + Vector((0,0,height*t))
        w = width * scale
        thick = max(0.002, width * .12 * (1-t))
        for side, depth in [(-1,-1),(1,-1),(1,1),(-1,1)]:
            p = center + tangent*(side*w) + Vector((0,0,depth*thick))
            verts.append(tuple(p))
    faces.append(tuple(first+i for i in (3,2,1,0))); mats.append(key_index)
    for ring in range(5):
        lo=first+ring*4; hi=lo+4
        for s in range(4):
            n=(s+1)%4
            faces.append((lo+s,lo+n,hi+n,hi+s)); mats.append(key_index)
    last=first+20
    faces.append(tuple(last+i for i in range(4))); mats.append(key_index)

def build_grass(name, blades=7, height=.42, width=.055, seed=11, reed=False):
    rng=random.Random(seed); v=[]; f=[]; fm=[]
    for i in range(blades):
        a=2*math.pi*i/blades+rng.uniform(-.22,.22)
        h=height*rng.uniform(.72,1.18)
        _append_tapered_blade(v,f,fm,a,h,width*rng.uniform(.72,1.15),rng.uniform(.08,.22),1 if i%3==0 else 0)
    return _mesh_object(name,v,f,["leaf","leaf_light"],fm)

def build_flower(name, petals=6, height=.30, seed=3):
    rng=random.Random(seed); v=[]; f=[]; fm=[]
    _append_tapered_blade(v,f,fm,0,height,.018,.025,0)
    # stem blade is narrow and vertical; add flattened petal ellipsoids via UV spheres below
    obj=_mesh_object(name,v,f,["stem"],fm)
    parts=[obj]
    for i in range(petals):
        a=2*math.pi*i/petals
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=4,
            radius=1, location=(math.cos(a)*.065, math.sin(a)*.065, height*.91))
        p=bpy.context.object; p.name=f"{name}_Petal_{i+1:02d}"
        p.scale=(.075,.043,.022); p.rotation_euler[2]=a
        p.data.materials.append(_mat("flower" if i%2 else "flower_light"))
        parts.append(p)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=4, radius=.035, location=(0,0,height*.94))
    center=bpy.context.object; center.name=name+"_Center"; center.data.materials.append(_mat("flower_light")); parts.append(center)
    return _join(parts,name)

def build_mushroom(name, cap_color="cap"):
    bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=.035, radius2=.022, depth=.18, location=(0,0,.09))
    stem=bpy.context.object; stem.name=name+"_Stem"; stem.data.materials.append(_mat("wood_light"))
    bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=6, radius=1, location=(0,0,.19))
    cap=bpy.context.object; cap.name=name+"_Cap"; cap.scale=(.13,.13,.065); cap.data.materials.append(_mat(cap_color))
    return _join([stem,cap],name)

def build_rock_cluster(name, count=5, radius=.22, seed=4):
    rng=random.Random(seed); parts=[]
    for i in range(count):
        r=radius*rng.uniform(.45,1)
        loc=(rng.uniform(-radius,radius),rng.uniform(-radius,radius),r*.55)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=r, location=loc)
        o=bpy.context.object; o.name=f"{name}_Stone_{i+1:02d}"
        o.scale=(rng.uniform(.8,1.25),rng.uniform(.75,1.2),rng.uniform(.55,.95))
        o.rotation_euler=(rng.random()*.3,rng.random()*.3,rng.random()*math.tau)
        o.data.materials.append(_mat("stone" if i%2 else "stone_light")); parts.append(o)
    return _join(parts,name)

def build_leaf_litter(name, count=9, seed=6):
    rng=random.Random(seed); parts=[]
    for i in range(count):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=4, radius=1,
            location=(rng.uniform(-.22,.22),rng.uniform(-.22,.22),rng.uniform(.006,.018)))
        o=bpy.context.object; o.name=f"{name}_Leaf_{i+1:02d}"
        o.scale=(rng.uniform(.07,.14),rng.uniform(.035,.07),.008)
        o.rotation_euler[2]=rng.random()*math.tau
        o.data.materials.append(_mat("leaf" if i%3 else "leaf_light")); parts.append(o)
    return _join(parts,name)

def build_log(name, length=.65, radius=.12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=radius, depth=length,
        location=(0,0,radius), rotation=(0,math.pi/2,0))
    o=bpy.context.object; o.name=name; o.data.materials.append(_mat("wood"))
    return o


def _add_tree_branch(name, start, end, radius_start, radius_end, material_key, vertices=7):
    """Create one tapered branch between two local-space points."""
    start_v = Vector(start)
    end_v = Vector(end)
    direction = end_v - start_v
    length = direction.length
    if length <= 1e-5:
        raise ValueError("Tree branch length must be positive.")
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices,
        radius1=radius_start,
        radius2=radius_end,
        depth=length,
        location=(start_v + end_v) * 0.5,
    )
    branch = bpy.context.object
    branch.name = name
    branch.rotation_mode = 'QUATERNION'
    branch.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(direction.normalized())
    branch.data.materials.append(_mat(material_key))
    return branch


def _add_canopy_cluster(name, location, scale, material_key, subdivisions=1):
    bpy.ops.mesh.primitive_ico_sphere_add(
        subdivisions=subdivisions, radius=1.0, location=location
    )
    canopy = bpy.context.object
    canopy.name = name
    canopy.scale = scale
    canopy.data.materials.append(_mat(material_key))
    return canopy


def _finish_tree(parts, name):
    """Join tree parts and put the object origin at the trunk base."""
    tree = _join(parts, name)
    bpy.context.scene.cursor.location = (0.0, 0.0, 0.0)
    bpy.context.view_layer.objects.active = tree
    tree.select_set(True)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    tree.location = (0.0, 0.0, 0.0)
    tree.rotation_euler = (0.0, 0.0, 0.0)
    tree.scale = (1.0, 1.0, 1.0)
    return tree


def build_tree_deciduous(name, height=8.0, seed=31):
    """Stylized broadleaf tree: sturdy trunk and a layered, irregular crown."""
    rng = random.Random(seed)
    parts = []
    trunk_height = height * 0.62
    trunk_radius = height * 0.045

    bpy.ops.mesh.primitive_cone_add(
        vertices=9, radius1=trunk_radius, radius2=trunk_radius * 0.48,
        depth=trunk_height, location=(0, 0, trunk_height * 0.5)
    )
    trunk = bpy.context.object
    trunk.name = name + "_Trunk"
    trunk.data.materials.append(_mat("wood"))
    parts.append(trunk)

    # Four visible low-poly branches support the canopy silhouette.
    for index, angle in enumerate((0.35, 1.9, 3.45, 5.1)):
        start_z = trunk_height * rng.uniform(0.52, 0.78)
        reach = height * rng.uniform(0.19, 0.25)
        end = (math.cos(angle) * reach, math.sin(angle) * reach,
               start_z + height * rng.uniform(0.13, 0.22))
        parts.append(_add_tree_branch(
            f"{name}_Branch_{index + 1:02d}", (0, 0, start_z), end,
            trunk_radius * 0.48, trunk_radius * 0.12, "wood_light", 7
        ))

    # The overlapping faceted clusters create a readable rounded crown without textures.
    crown_base = height * 0.56
    crown_radius = height * 0.34
    clusters = [
        ((0.0, 0.0, crown_base + height * 0.20), (0.30, 0.28, 0.27), "leaf"),
        ((-0.20, 0.02, crown_base + height * 0.04), (0.25, 0.24, 0.23), "leaf_dark"),
        ((0.19, -0.08, crown_base + height * 0.06), (0.26, 0.24, 0.24), "leaf_light"),
        ((0.02, 0.21, crown_base + height * 0.08), (0.24, 0.25, 0.23), "leaf"),
        ((-0.06, -0.22, crown_base + height * 0.08), (0.25, 0.23, 0.24), "leaf_dark"),
    ]
    for index, (location, normalized_scale, material_key) in enumerate(clusters):
        # Slight deterministic asymmetry prevents a perfect lollipop silhouette.
        jitter = Vector((rng.uniform(-0.12, 0.12), rng.uniform(-0.12, 0.12),
                         rng.uniform(-0.08, 0.08))) * crown_radius
        loc = Vector(location) * (height / 8.0) + jitter
        scale = tuple(component * height for component in normalized_scale)
        parts.append(_add_canopy_cluster(
            f"{name}_Crown_{index + 1:02d}", loc, scale, material_key, 1
        ))

    return _finish_tree(parts, name)


def build_tree_birch(name, height=9.0, seed=47):
    """Stylized slender birch-like tree with a pale trunk and airy crown."""
    rng = random.Random(seed)
    parts = []
    trunk_height = height * 0.82
    trunk_radius = height * 0.026

    bpy.ops.mesh.primitive_cone_add(
        vertices=8, radius1=trunk_radius, radius2=trunk_radius * 0.42,
        depth=trunk_height, location=(0, 0, trunk_height * 0.5)
    )
    trunk = bpy.context.object
    trunk.name = name + "_Trunk"
    trunk.data.materials.append(_mat("wood_light"))
    parts.append(trunk)

    # Dark bark bands are small geometry accents so the model remains readable without textures.
    for band_index, fraction in enumerate((0.22, 0.39, 0.57, 0.72)):
        z = trunk_height * fraction
        radius = trunk_radius * (1.0 - fraction * 0.48)
        bpy.ops.mesh.primitive_cone_add(
            vertices=8, radius1=radius * 1.015, radius2=radius * 0.99,
            depth=max(0.045, height * 0.012), location=(0, 0, z)
        )
        band = bpy.context.object
        band.name = f"{name}_BarkBand_{band_index + 1:02d}"
        band.data.materials.append(_mat("wood"))
        parts.append(band)

    for index, angle in enumerate((0.15, 1.45, 2.85, 4.25, 5.55)):
        start_z = trunk_height * rng.uniform(0.48, 0.82)
        reach = height * rng.uniform(0.16, 0.22)
        end = (math.cos(angle) * reach, math.sin(angle) * reach,
               start_z + height * rng.uniform(0.10, 0.18))
        parts.append(_add_tree_branch(
            f"{name}_Branch_{index + 1:02d}", (0, 0, start_z), end,
            trunk_radius * 0.55, trunk_radius * 0.10, "wood_light", 6
        ))

    # Birch foliage is arranged in narrower, separated clusters.
    crown_positions = [
        (0.0, 0.0, height * 0.84, 0.19, 0.18, 0.18, "leaf_light"),
        (-0.13, 0.02, height * 0.69, 0.17, 0.16, 0.17, "leaf"),
        (0.13, -0.04, height * 0.70, 0.17, 0.16, 0.18, "leaf_light"),
        (0.02, 0.13, height * 0.75, 0.16, 0.16, 0.17, "leaf_dark"),
        (-0.04, -0.13, height * 0.76, 0.17, 0.16, 0.18, "leaf"),
    ]
    for index, (x, y, z, sx, sy, sz, material_key) in enumerate(crown_positions):
        jitter = Vector((rng.uniform(-0.07, 0.07), rng.uniform(-0.07, 0.07),
                         rng.uniform(-0.06, 0.06))) * height
        parts.append(_add_canopy_cluster(
            f"{name}_Crown_{index + 1:02d}",
            (x * height + jitter.x, y * height + jitter.y, z + jitter.z),
            (sx * height, sy * height, sz * height), material_key, 1
        ))

    return _finish_tree(parts, name)

def build_stump(name, height=.32, radius=.18):
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=radius, depth=height, location=(0,0,height/2))
    o=bpy.context.object; o.name=name; o.data.materials.append(_mat("wood"))
    return o

def _join(objects, name):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objects: o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join()
    result=bpy.context.object; result.name=name
    result.data.name="MD_"+name
    return result

BUILDERS = {
    "grass": build_grass,
    "flower": build_flower,
    "mushroom": build_mushroom,
    "rock_cluster": build_rock_cluster,
    "leaf_litter": build_leaf_litter,
    "log": build_log,
    "stump": build_stump,
    "tree_deciduous": build_tree_deciduous,
    "tree_birch": build_tree_birch,
}

def generate_asset(asset_key, asset_id, object_name, unity_destination, builder, kwargs=None,
                   source_path=None, seed=1, test_only=False, asset_category=None):
    """Idempotent per-owned-collection generation and export through _core."""
    from pathlib import Path
    kwargs=dict(kwargs or {})
    collection_name="ART_"+asset_key.upper()
    old=bpy.data.collections.get(collection_name)
    if old:
        for ob in list(old.objects): bpy.data.objects.remove(ob,do_unlink=True)
        bpy.data.collections.remove(old)
    collection=get_or_create_collection(collection_name)
    obj=builder(object_name, **kwargs)
    move_objects_to_collection([obj],collection,unlink_others=True)
    obj.location=(0,0,0); obj.rotation_euler=(0,0,0); obj.scale=(1,1,1)
    bpy.context.view_layer.update()
    paths=ProjectPaths.discover(start=Path(__file__))
    out=paths.export_asset("environment",asset_key)
    result=export_fbx(objects=[obj],destination=out/(object_name+".fbx"),
        asset_id=asset_id,domain="environment",asset_type="static_mesh",
        unity_destination=unity_destination,profile=ExportProfile.static_mesh(),
        source_path=source_path or f"Blender/source/environment/{asset_key}/",
        allowed_export_root=paths.exports,require_materials=True,require_uvs=False,
        asset_domain="Environment", asset_category=asset_category,
        extra_manifest={"generator_version":GENERATOR_VERSION,"seed":seed,
                        "test_only":bool(test_only),"authoring":"Blender procedural mesh"})
    print("[ART]",asset_key,result.to_dict())
    if not result.success: raise RuntimeError(result.message)
    return result


def resolve_batch_policy(
    *,
    asset_key,
    object_name,
    declared_unity_destination,
    declared_test_only,
    declared_asset_category,
    batch_policy=None,
):
    """Resolve an asset's Unity destination under an optional batch policy.

    Standalone generators keep their declared values. When called by
    generate_all.py, PRODUCTION and TEST modes override destination/status;
    CUSTOM preserves the generator declaration while validating consistency.
    """
    mode = (batch_policy or {}).get("mode", "CUSTOM").upper()
    models_root = (batch_policy or {}).get(
        "models_root", "Assets/Project/Content/Models"
    ).replace("\\", "/").rstrip("/")

    if mode == "PRODUCTION":
        production_assets = (batch_policy or {}).get("production_assets", {})
        entry = production_assets.get(asset_key)
        if not entry:
            raise ValueError(
                f"No canonical production route is registered for '{asset_key}'. "
                "Add it to generate_all.py before running the production batch."
            )
        category = entry["category"]
        relative = entry["destination"].replace("\\", "/").lstrip("/")
        destination = f"{models_root}/{relative}"
        test_only = False
    elif mode == "TEST":
        category = declared_asset_category
        destination = (
            f"{models_root}/Environment/{category}/_ART_TEST/{asset_key}/{object_name}.fbx"
        )
        test_only = True
    elif mode == "CUSTOM":
        category = declared_asset_category
        destination = declared_unity_destination
        test_only = bool(declared_test_only)
    else:
        raise ValueError(
            f"Unsupported batch mode '{mode}'. Use PRODUCTION, TEST or CUSTOM."
        )

    normalized = str(destination).replace("\\", "/")
    if not normalized.lower().endswith(".fbx"):
        raise ValueError(f"Unity destination must end in .fbx: {normalized}")
    if not normalized.startswith(models_root + "/"):
        raise ValueError(
            f"Unity destination must be inside '{models_root}/': {normalized}"
        )
    if any(part in {"", ".", ".."} for part in normalized.split("/")):
        raise ValueError(f"Unsafe Unity destination: {normalized}")

    is_test_path = "/_ART_TEST/" in normalized.upper()
    if bool(test_only) != is_test_path:
        raise ValueError(
            f"Contradictory test policy for '{asset_key}': "
            f"test_only={test_only}, destination='{normalized}'."
        )

    return normalized, bool(test_only), category

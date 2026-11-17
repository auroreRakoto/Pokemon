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

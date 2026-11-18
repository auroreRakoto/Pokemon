"""Procedural low-poly starter Pokemon authoring helpers.

These are first-pass blockout generators. All geometry, rigs, materials and
Idle actions are authored in Blender. No Unity APIs or gameplay rules.
"""
from __future__ import annotations
import math
import bpy
from mathutils import Vector
from _core.collections import get_or_create_collection, move_objects_to_collection
from _core.materials import get_or_create_material
from _core.paths import ProjectPaths
from _core.config import load_project_config
from _core.export import ExportProfile, export_fbx

GENERATOR_VERSION = "0.1.0"

def mat(name, color, roughness=0.72, emission=0.0):
    m = get_or_create_material(name, color)
    m.diffuse_color = color
    m.use_nodes = True
    bsdf = next((n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED"), None)
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Roughness"].default_value = roughness
        if "Emission Color" in bsdf.inputs and emission:
            bsdf.inputs["Emission Color"].default_value = color
            bsdf.inputs["Emission Strength"].default_value = emission
    return m

def _smooth_off(obj):
    for p in obj.data.polygons:
        p.use_smooth = False

def ico(name, loc, scale, material, subdivisions=2, rotation=(0,0,0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1, location=loc)
    o=bpy.context.object; o.name=name; o.scale=scale; o.rotation_euler=rotation
    o.data.materials.append(material); _smooth_off(o)
    return o

def uv_sphere(name, loc, scale, material, segments=12, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=1, location=loc)
    o=bpy.context.object; o.name=name; o.scale=scale; o.data.materials.append(material); _smooth_off(o)
    return o

def cone(name, loc, radius1, radius2, depth, material, vertices=8, rotation=(0,0,0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius1, radius2=radius2,
                                    depth=depth, location=loc, rotation=rotation)
    o=bpy.context.object; o.name=name; o.data.materials.append(material); _smooth_off(o)
    return o

def torus(name, loc, major, minor, material, rotation=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_segments=12, minor_segments=4,
        major_radius=major, minor_radius=minor, location=loc, rotation=rotation)
    o=bpy.context.object; o.name=name; o.data.materials.append(material); _smooth_off(o)
    return o

def make_armature(name, bone_specs):
    data=bpy.data.armatures.new(name+"_Data")
    arm=bpy.data.objects.new(name,data)
    bpy.context.scene.collection.objects.link(arm)
    bpy.context.view_layer.objects.active=arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    created={}
    for bone_name, head, tail, parent in bone_specs:
        bone=data.edit_bones.new(bone_name)
        bone.head=head; bone.tail=tail
        if parent and parent in created:
            bone.parent=created[parent]
            bone.use_connect=False
        created[bone_name]=bone
    bpy.ops.object.mode_set(mode="OBJECT")
    arm.show_in_front=True
    return arm

def bind_rigid_part(obj, arm, bone_name):
    # Rigid component weighting keeps low-poly plates/details from collapsing.
    group=obj.vertex_groups.new(name=bone_name)
    if len(obj.data.vertices):
        group.add(list(range(len(obj.data.vertices))),1.0,"REPLACE")
    mod=obj.modifiers.new("ARMATURE_Deform","ARMATURE")
    mod.object=arm
    world=obj.matrix_world.copy()
    obj.parent=arm
    obj.matrix_world=world
    return obj

def animate_idle(arm, asset_slug, tracks, start=1, end=49):
    """tracks: bone -> [(frame, Euler XYZ radians, location XYZ)]"""
    scene=bpy.context.scene
    scene.frame_start=start; scene.frame_end=end
    scene.render.fps=24
    bpy.context.view_layer.objects.active=arm
    arm.select_set(True)
    bpy.ops.object.mode_set(mode="POSE")
    for bone_name, keys in tracks.items():
        pb=arm.pose.bones.get(bone_name)
        if pb is None:
            continue
        pb.rotation_mode="XYZ"
        for frame, rotation, location in keys:
            pb.rotation_euler=rotation
            pb.location=location
            pb.keyframe_insert(data_path="rotation_euler",frame=frame,group=bone_name)
            pb.keyframe_insert(data_path="location",frame=frame,group=bone_name)
    bpy.ops.object.mode_set(mode="OBJECT")
    action=arm.animation_data.action if arm.animation_data else None
    if action:
        action.name=f"AN_{asset_slug.title()}_Idle"
        # The first and last keyed poses are authored identically by each
        # species generator. Unity's ModelImporter should enable Loop Time.
        # Avoid direct Action.fcurves access: Blender 5.x layered Actions use
        # channel bags and that legacy API is not stable across versions.
        # Put action on NLA so the _core animated_mesh profile exports the strip.
        track=arm.animation_data.nla_tracks.new()
        track.name=action.name
        strip=track.strips.new(action.name, start, action)
        arm.animation_data.action=None
    return action

def begin_asset(slug):
    collection_name="ART_POKEMON_"+slug.upper()
    old=bpy.data.collections.get(collection_name)
    if old:
        for ob in list(old.objects):
            bpy.data.objects.remove(ob,do_unlink=True)
        bpy.data.collections.remove(old)
    return get_or_create_collection(collection_name)

def configured_animated_profile(config):
    """Build the animated FBX profile from the repository's single config.

    The checked-in _core ExportProfile.animated_mesh() currently takes no
    config argument, so this adapter maps the documented config values into
    the dataclass without duplicating exporter defaults in species scripts.
    """
    fbx = config["fbx"]
    values = fbx["profiles"]["animated_mesh"]
    return ExportProfile(
        name="animated_mesh",
        object_types=tuple(values["object_types"]),
        apply_unit_scale=bool(fbx["apply_unit_scale"]),
        axis_forward=values["axis_forward"],
        axis_up=values["axis_up"],
        bake_animation=bool(values["bake_animation"]),
        use_nla_strips=bool(values["use_nla_strips"]),
        use_all_actions=bool(values["use_all_actions"]),
        add_leaf_bones=bool(values["add_leaf_bones"]),
        mesh_smooth_type=fbx["mesh_smooth_type"],
    )

def finish_asset(slug, collection, arm, objects, display_name, species_id,
                 unity_destination, source_path, notes):
    move_objects_to_collection(objects+[arm],collection,unlink_others=True)
    bpy.context.scene.frame_set(1)
    paths=ProjectPaths.discover(start=__import__("_core").__file__)
    config=load_project_config(paths)
    out=paths.export_asset("pokemon",slug)
    profile=configured_animated_profile(config)
    round_trip=config.get("round_trip", {})
    if round_trip.get("status") != "VALIDATED" or round_trip.get("validated_profile") != "animated_mesh":
        unity_destination = f"Assets/Project/Content/Models/Pokemon/_ART_TEST/{slug}/SK_Pokemon_{display_name}_Base.fbx"
    result=export_fbx(
        objects=objects+[arm],
        destination=out/f"SK_Pokemon_{display_name}_Base.fbx",
        asset_id=f"pokemon.{slug}.base",
        domain="pokemon", asset_type="animated_mesh",
        unity_destination=unity_destination,
        profile=profile,
        source_path=source_path,
        allowed_export_root=paths.exports,
        require_materials=True, require_uvs=False,
        asset_domain="Pokemon",
        asset_category="BaseSpecies",
        extra_manifest={
            "species_id": species_id,
            "export_version": GENERATOR_VERSION,
            "units": config["units"],
            "scale": {"scale_length": config["units"]["scale_length"], "object_scale": [1.0, 1.0, 1.0]},
            "generator_version":GENERATOR_VERSION,
            "rig":"rigid_component_skinning",
            "animation_clips":[f"AN_{display_name}_Idle"],
            "animation_loop":"cyclic",
            "visual_status":"first_pass_low_poly_blockout",
            "design_notes":notes,
        })
    print("[ART][POKEMON]",slug,result.to_dict())
    if not result.success:
        raise RuntimeError(result.message)
    return result

def clear_selection():
    bpy.ops.object.select_all(action="DESELECT")

def set_active(obj):
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)

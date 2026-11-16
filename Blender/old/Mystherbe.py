import bpy
import math
from mathutils import Vector

# ------------------------------------------------------------
# Shared helpers
# Run this file from Blender's Scripting workspace.
# The model is created at the origin, facing -Y, Z up.
# ------------------------------------------------------------

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.materials, bpy.data.curves, bpy.data.meshes):
        # Keep datablocks Blender may still be using; unused ones are removed.
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)

def make_material(name, color, roughness=0.78):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Roughness"].default_value = roughness
    return mat

def finish_obj(obj, name, material, smooth=True):
    obj.name = name
    if material:
        obj.data.materials.append(material)
    if obj.type == 'MESH' and smooth:
        for poly in obj.data.polygons:
            poly.use_smooth = True
    return obj

def uv_sphere(name, loc, scale, mat, segments=24, rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    return finish_obj(obj, name, mat)

def cone(name, loc, radius1, radius2, depth, mat, rotation=(0,0,0), vertices=16):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius1, radius2=radius2,
                                    depth=depth, location=loc, rotation=rotation)
    return finish_obj(bpy.context.object, name, mat)

def cylinder_between(name, a, b, radius, mat, vertices=12):
    a, b = Vector(a), Vector(b)
    direction = b - a
    midpoint = (a + b) / 2
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius,
                                       depth=direction.length, location=midpoint)
    obj = bpy.context.object
    obj.rotation_mode = 'QUATERNION'
    obj.rotation_quaternion = direction.to_track_quat('Z', 'Y')
    return finish_obj(obj, name, mat)

def curve_tube(name, points, bevel, mat, cyclic=False):
    curve = bpy.data.curves.new(name + "_Curve", type='CURVE')
    curve.dimensions = '3D'
    curve.resolution_u = 16
    curve.bevel_depth = bevel
    curve.bevel_resolution = 3
    spline = curve.splines.new('BEZIER')
    spline.bezier_points.add(len(points)-1)
    for bp, co in zip(spline.bezier_points, points):
        bp.co = co
        bp.handle_left_type = 'AUTO'
        bp.handle_right_type = 'AUTO'
    spline.use_cyclic_u = cyclic
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mat)
    return obj

def set_origin_and_export(model_name, export_fbx=True):
    # Keep all generated objects together in one named collection.
    collection = bpy.data.collections.get(model_name) or bpy.data.collections.new(model_name)
    if collection.name not in bpy.context.scene.collection.children:
        bpy.context.scene.collection.children.link(collection)
    for obj in list(bpy.context.scene.objects):
        if obj.type in {'MESH', 'CURVE'}:
            for col in list(obj.users_collection):
                col.objects.unlink(obj)
            collection.objects.link(obj)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in collection.objects:
        obj.select_set(True)
    if collection.objects:
        bpy.context.view_layer.objects.active = next(iter(collection.objects))
    # Export beside this script, or to Blender's current working directory.
    if export_fbx:
        import os
        try:
            folder = os.path.dirname(os.path.abspath(__file__))
        except NameError:
            folder = bpy.path.abspath("//")
        path = os.path.join(folder, model_name + ".fbx")
        bpy.ops.export_scene.fbx(filepath=path, use_selection=True,
                                 apply_unit_scale=True, axis_forward='-Z',
                                 axis_up='Y', object_types={'MESH', 'EMPTY', 'ARMATURE'},
                                 bake_space_transform=False)
        print("FBX exported:", path)
    print(model_name, "created successfully.")

# ============================================================
# MYSTHERBE (ODDISH) - stylized procedural model
# ============================================================
clear_scene()
blue = make_material("Mystherbe_Blue", (0.12, 0.35, 0.88))
blue_light = make_material("Mystherbe_Blue_Light", (0.20, 0.48, 1.0))
green = make_material("Leaf_Green", (0.12, 0.48, 0.12))
green_light = make_material("Leaf_Light", (0.25, 0.68, 0.16))
white = make_material("Eye_White", (1.0, 0.98, 0.88))
red = make_material("Eye_Red", (0.83, 0.035, 0.055))
black = make_material("Eye_Pupil", (0.025, 0.018, 0.025))

# Round blue body, slightly flattened at the bottom.
uv_sphere("Main_Body", (0, 0, 0.78), (0.68, 0.62, 0.69), blue, 32, 24)
uv_sphere("Body_Light_Patch", (0, -0.39, 0.54), (0.36, 0.19, 0.24), blue_light)

# Five broad leaves growing from the top. Each is a tapered, softly rounded leaf.
leaf_specs = [
    ((-0.28, 0.02, 1.47), (-0.72, 0.05, 2.12), 0.22),
    ((-0.10, 0.04, 1.48), (-0.28, -0.05, 2.28), 0.23),
    (( 0.10, 0.04, 1.48), ( 0.25, -0.04, 2.30), 0.23),
    (( 0.28, 0.02, 1.46), ( 0.72, 0.05, 2.12), 0.22),
    (( 0.00, 0.12, 1.48), ( 0.02, 0.42, 2.13), 0.21),
]
for i, (base, tip, radius) in enumerate(leaf_specs):
    # A tapered cone with its wide end at the base, tilted toward the tip.
    a, b = Vector(base), Vector(tip)
    direction = b - a
    obj = cone("Leaf_%02d" % (i+1), (a+b)/2, radius, 0.012,
               direction.length, green if i % 2 == 0 else green_light)
    obj.rotation_mode = 'QUATERNION'
    obj.rotation_quaternion = direction.to_track_quat('Z', 'Y')

# Eyes on the front (-Y).
for x in (-0.25, 0.25):
    uv_sphere("Eye_White", (x, -0.565, 0.88), (0.145, 0.075, 0.18), white)
    uv_sphere("Eye_Red_Iris", (x, -0.63, 0.875), (0.105, 0.045, 0.135), red)
    uv_sphere("Eye_Pupil", (x, -0.666, 0.87), (0.052, 0.025, 0.085), black)
    uv_sphere("Eye_Glint", (x-0.025, -0.69, 0.92), (0.023, 0.014, 0.03), white, 12, 8)

# Small feet.
for x in (-0.34, 0.34):
    uv_sphere("Foot", (x, -0.02, 0.17), (0.14, 0.20, 0.13), blue)
    # Toe bumps point forward.
    for dx in (-0.055, 0.055):
        uv_sphere("Toe", (x+dx, -0.18, 0.12), (0.055, 0.11, 0.07), blue_light, 16, 10)

set_origin_and_export("Mystherbe")

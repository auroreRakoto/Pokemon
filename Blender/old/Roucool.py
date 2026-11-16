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
# ROUCOOL (PIDGEY) - stylized procedural model
# ============================================================
clear_scene()
brown = make_material("Roucool_Brown", (0.48, 0.25, 0.12))
brown_dark = make_material("Roucool_Dark_Brown", (0.28, 0.13, 0.065))
brown_light = make_material("Roucool_Feather_Light", (0.68, 0.40, 0.20))
cream = make_material("Roucool_Cream", (0.91, 0.82, 0.65))
beak_mat = make_material("Beak_Gold", (0.95, 0.58, 0.08))
feet_mat = make_material("Feet_Orange", (0.82, 0.34, 0.06))
white = make_material("Eye_White", (1.0, 0.98, 0.88))
black = make_material("Eye_Black", (0.018, 0.012, 0.01))

# Torso and chest. Bird faces -Y.
uv_sphere("Body", (0, 0.02, 0.92), (0.53, 0.70, 0.65), brown, 32, 24)
uv_sphere("Cream_Chest", (0, -0.49, 0.86), (0.39, 0.27, 0.47), cream, 28, 20)
uv_sphere("Head", (0, -0.48, 1.62), (0.43, 0.42, 0.43), brown, 32, 24)

# Face: large dark eyes, tiny highlight, pointed beak.
for x in (-0.19, 0.19):
    uv_sphere("Eye", (x, -0.845, 1.68), (0.105, 0.055, 0.13), black)
    uv_sphere("Eye_Glint", (x-0.025, -0.894, 1.725), (0.025, 0.018, 0.03), white, 12, 8)
cone("Upper_Beak", (0, -0.99, 1.48), 0.13, 0.008, 0.30, beak_mat,
     rotation=(math.radians(90), 0, 0), vertices=16)
cone("Lower_Beak", (0, -0.95, 1.40), 0.095, 0.008, 0.18, beak_mat,
     rotation=(math.radians(90), math.pi, 0), vertices=16)

# Three feather crest spikes on top of head.
for i, (x, z, tilt) in enumerate([(-0.20, 2.02, -18), (0, 2.12, 0), (0.20, 2.02, 18)]):
    cone("Crest_Feather_%d" % (i+1), (x, -0.42, z), 0.13, 0.012, 0.43,
         brown_light, rotation=(math.radians(-18), math.radians(tilt), 0))

# Wings: separate flattened feather forms, so they can be rigged later.
for side in (-1, 1):
    wing = uv_sphere("Wing_Left" if side < 0 else "Wing_Right",
                     (side*0.48, -0.02, 1.02), (0.20, 0.47, 0.39), brown_dark, 24, 16)
    wing.rotation_euler[1] = side * math.radians(-18)
    for j in range(3):
        feather = uv_sphere("Wing_Feather", (side*(0.52+0.035*j), -0.24+0.20*j, 0.90-0.10*j),
                            (0.105, 0.30, 0.12), brown_light if j == 0 else brown, 20, 12)
        feather.rotation_euler[1] = side * math.radians(-12)

# Tail feathers fan out behind the body (+Y).
for i, x in enumerate((-0.22, 0, 0.22)):
    feather = uv_sphere("Tail_Feather", (x, 0.78, 1.10 + (0.10 if i == 1 else 0)),
                        (0.12, 0.48, 0.105), brown_dark if i == 1 else brown_light, 20, 12)
    feather.rotation_euler[0] = math.radians(-12)

# Two legs and three toes per foot.
for x in (-0.22, 0.22):
    cylinder_between("Leg", (x, 0.00, 0.48), (x, -0.02, 0.20), 0.045, feet_mat)
    uv_sphere("Foot", (x, -0.06, 0.18), (0.09, 0.13, 0.055), feet_mat, 16, 10)
    for dx in (-0.09, 0, 0.09):
        cylinder_between("Toe", (x, -0.08, 0.17), (x+dx, -0.34, 0.13), 0.027, feet_mat, 10)

set_origin_and_export("Roucool")

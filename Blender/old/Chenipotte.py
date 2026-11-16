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
# CHENIPOTTE (WURMPLE) - stylized procedural model
# ============================================================
clear_scene()
red = make_material("Chenipotte_Red", (0.78, 0.055, 0.075))
red_light = make_material("Chenipotte_Red_Light", (0.95, 0.12, 0.13))
yellow = make_material("Chenipotte_Yellow", (1.0, 0.72, 0.08))
cream = make_material("Chenipotte_Underside", (1.0, 0.83, 0.36))
white = make_material("Eye_White", (1.0, 0.97, 0.88))
black = make_material("Eye_Black", (0.025, 0.018, 0.025))

# Body: overlapping rounded segments, front faces -Y.
segments = [
    ((0, -0.62, 0.67), (0.43, 0.48, 0.43)),
    ((0, -0.02, 0.62), (0.39, 0.43, 0.39)),
    ((0,  0.48, 0.57), (0.34, 0.38, 0.34)),
    ((0,  0.91, 0.52), (0.29, 0.32, 0.29)),
]
for i, (loc, scale) in enumerate(segments):
    uv_sphere("Body_Segment_%02d" % (i+1), loc, scale, red if i % 2 == 0 else red_light)

# Pale yellow underside visible from the front and sides.
uv_sphere("Yellow_Belly", (0, -0.12, 0.40), (0.31, 0.83, 0.19), cream)

# Face and eyes.
for x in (-0.19, 0.19):
    uv_sphere("Eye_White", (x, -1.005, 0.79), (0.105, 0.055, 0.125), white)
    uv_sphere("Eye_Pupil", (x, -1.052, 0.785), (0.045, 0.027, 0.073), black, 16, 12)
    uv_sphere("Eye_Glint", (x-0.018, -1.076, 0.82), (0.018, 0.012, 0.022), white, 12, 8)

# Two characteristic yellow head feelers.
cone("Head_Feeler_Left", (-0.25, -0.78, 1.10), 0.105, 0.018, 0.43, yellow,
     rotation=(math.radians(-18), math.radians(-20), math.radians(-12)))
cone("Head_Feeler_Right", (0.25, -0.78, 1.10), 0.105, 0.018, 0.43, yellow,
     rotation=(math.radians(-18), math.radians(20), math.radians(12)))

# Rear horn.
cone("Tail_Horn", (0, 1.18, 0.69), 0.16, 0.015, 0.48, yellow,
     rotation=(math.radians(-22), 0, 0))

# Tiny feet along both sides.
for side in (-1, 1):
    for y, z in [(-0.35, 0.36), (0.25, 0.32), (0.78, 0.29)]:
        x = side * 0.31
        uv_sphere("Tiny_Foot", (x, y, z), (0.105, 0.13, 0.075), yellow, 16, 10)
        cone("Foot_Tip", (side*0.37, y-0.035, z-0.025), 0.055, 0.015, 0.12,
             yellow, rotation=(0, side*math.radians(70), 0), vertices=12)

set_origin_and_export("Chenipotte")

import bpy
import math
import os
import random
from mathutils import Vector

# ============================================================
# POKEMON GRASS ASSETS - redesigned from the supplied reference
# Blender 5.x
#
# Generates 3 distinct models:
#   Grass_Short   : 3-5 broad, angular blades
#   Grass_Tall    : dense tuft of 6-10 long, narrow blades
#   Grass_Wild    : irregular dry/wild grass with small flowers
#
# Each model is a separate collection and a separate FBX.
# No ground plane. Origin at ground level. Z up.
# ============================================================

EXPORT_FBX = True
EXPORT_FOLDER = os.path.join(os.path.expanduser("~"), "PokemonEnvironmentExports")
RANDOM_SEED = 2026
random.seed(RANDOM_SEED)

def make_mat(name, color, roughness=0.86):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    return mat

MAT = {
    "short_1": make_mat("Grass_Short_Deep_Green", (0.20, 0.34, 0.055)),
    "short_2": make_mat("Grass_Short_Olive", (0.34, 0.49, 0.075)),
    "short_3": make_mat("Grass_Short_Lime", (0.48, 0.61, 0.12)),
    "tall_1": make_mat("Grass_Tall_Dark_Green", (0.035, 0.22, 0.075)),
    "tall_2": make_mat("Grass_Tall_Green", (0.12, 0.39, 0.10)),
    "tall_3": make_mat("Grass_Tall_Light_Green", (0.34, 0.57, 0.13)),
    "wild_1": make_mat("Grass_Wild_Dry_Ochre", (0.48, 0.29, 0.075)),
    "wild_2": make_mat("Grass_Wild_Golden", (0.70, 0.46, 0.12)),
    "wild_3": make_mat("Grass_Wild_Dark_Brown", (0.30, 0.19, 0.065)),
    "flower_pink": make_mat("Wild_Flower_Pink", (0.92, 0.12, 0.43)),
    "flower_yellow": make_mat("Wild_Flower_Yellow", (0.98, 0.66, 0.08)),
    "flower_purple": make_mat("Wild_Flower_Purple", (0.40, 0.12, 0.75)),
    "flower_center": make_mat("Wild_Flower_Center", (1.0, 0.78, 0.18)),
}

ROOT = bpy.context.scene.collection
CURRENT = None

def begin_collection(name):
    global CURRENT
    CURRENT = bpy.data.collections.new(name)
    ROOT.children.link(CURRENT)

def link_current(obj, name, material=None, smooth=False):
    obj.name = name
    for old_collection in list(obj.users_collection):
        old_collection.objects.unlink(obj)
    CURRENT.objects.link(obj)
    if material:
        obj.data.materials.append(material)
    if obj.type == "MESH" and smooth:
        for poly in obj.data.polygons:
            poly.use_smooth = True
    return obj

def make_blade(name, base, tip, width, material, thickness=0.018, bend=None):
    """Faceted pointed leaf: broad at the base, sharp at the tip."""
    base = Vector(base)
    tip = Vector(tip)
    axis = tip - base
    side = axis.cross(Vector((0.0, 1.0, 0.0)))
    if side.length < 0.001:
        side = axis.cross(Vector((1.0, 0.0, 0.0)))
    side.normalize()

    if bend is None:
        bend = Vector((0.0, 0.0, 0.035))
    else:
        bend = Vector(bend)

    p1 = base.lerp(tip, 0.34) + bend * 0.35
    p2 = base.lerp(tip, 0.72) + bend
    verts = [
        base,
        p1 - side * width * 0.50,
        p1 + Vector((0, 0, 0.035)),
        p1 + side * width * 0.50,
        p2 - side * width * 0.24,
        p2 + Vector((0, 0, 0.025)),
        p2 + side * width * 0.24,
        tip,
    ]
    faces = [
        (0, 1, 2), (0, 2, 3),
        (1, 4, 5, 2), (2, 5, 6, 3),
        (4, 7, 5), (5, 7, 6)
    ]
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(material)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    CURRENT.objects.link(obj)

    solidify = obj.modifiers.new("Blade_Thickness", "SOLIDIFY")
    solidify.thickness = thickness
    bevel = obj.modifiers.new("Tiny_Edge_Bevel", "BEVEL")
    bevel.width = 0.008
    bevel.segments = 1
    return obj

def make_stem(name, start, end, radius, material):
    a, b = Vector(start), Vector(end)
    direction = b - a
    bpy.ops.mesh.primitive_cone_add(
        vertices=6, radius1=radius, radius2=radius * 0.55,
        depth=direction.length, location=(a + b) * 0.5
    )
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = direction.to_track_quat("Z", "Y")
    return link_current(obj, name, material, False)

def make_flower(name, position, petal_material, scale=1.0):
    """Tiny stylized five-petal flower at the tip of a wild stem."""
    x, y, z = position
    # Short flower head, with five separate oval petals.
    for i in range(5):
        angle = math.tau * i / 5.0
        px = x + math.cos(angle) * 0.055 * scale
        py = y + math.sin(angle) * 0.055 * scale
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=8, ring_count=6,
            location=(px, py, z)
        )
        petal = bpy.context.object
        petal.scale = (0.052 * scale, 0.030 * scale, 0.018 * scale)
        petal.rotation_euler[2] = angle
        link_current(petal, name + "_Petal_%02d" % i, petal_material, True)

    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=8, ring_count=6, radius=1,
        location=(x, y, z + 0.012 * scale)
    )
    center = bpy.context.object
    center.scale = (0.025 * scale, 0.025 * scale, 0.018 * scale)
    link_current(center, name + "_Center", MAT["flower_center"], True)

def export_collection(name):
    global CURRENT
    if EXPORT_FBX:
        os.makedirs(EXPORT_FOLDER, exist_ok=True)
        bpy.ops.object.select_all(action="DESELECT")
        objects = [o for o in CURRENT.all_objects if o.type in {"MESH", "EMPTY", "ARMATURE"}]
        for obj in objects:
            obj.select_set(True)
        if objects:
            bpy.context.view_layer.objects.active = objects[0]
            filepath = os.path.join(EXPORT_FOLDER, name + ".fbx")
            bpy.ops.export_scene.fbx(
                filepath=filepath,
                use_selection=True,
                apply_unit_scale=True,
                axis_forward="-Z",
                axis_up="Y",
                object_types={"MESH", "EMPTY", "ARMATURE"}
            )
            print("FBX exported:", filepath)
    CURRENT = None

# ------------------------------------------------------------
# MODEL 1: SHORT GRASS - compact tuft, 3 to 5 broad blades
# ------------------------------------------------------------
def create_short_grass():
    begin_collection("Grass_Short")
    rng = random.Random(RANDOM_SEED + 1)
    count = 4
    for i in range(count):
        angle = math.tau * i / count + rng.uniform(-0.28, 0.28)
        height = rng.uniform(0.72, 1.00)
        radius = rng.uniform(0.10, 0.22)
        base = (math.cos(angle) * radius, math.sin(angle) * radius, 0.035)
        lean = rng.uniform(0.24, 0.43)
        tip = (
            math.cos(angle) * (radius + lean),
            math.sin(angle) * (radius + lean),
            height
        )
        width = rng.uniform(0.30, 0.40)
        make_blade(
            "Short_Broad_Blade_%02d" % (i + 1),
            base, tip, width,
            [MAT["short_1"], MAT["short_2"], MAT["short_3"], MAT["short_2"]][i],
            thickness=0.045,
            bend=(0, 0, rng.uniform(0.02, 0.08))
        )
    export_collection("Grass_Short")

# ------------------------------------------------------------
# MODEL 2: TALL GRASS - dense tuft, 6 to 10 narrow blades
# ------------------------------------------------------------
def create_tall_grass():
    begin_collection("Grass_Tall")
    rng = random.Random(RANDOM_SEED + 2)
    count = 8
    for i in range(count):
        angle = math.tau * i / count + rng.uniform(-0.22, 0.22)
        height = rng.uniform(1.45, 2.15)
        radius = rng.uniform(0.035, 0.15)
        base = (math.cos(angle) * radius, math.sin(angle) * radius, 0.025)
        lean = rng.uniform(0.10, 0.34)
        tip = (
            math.cos(angle) * (radius + lean),
            math.sin(angle) * (radius + lean),
            height
        )
        width = rng.uniform(0.095, 0.16)
        make_blade(
            "Tall_Narrow_Blade_%02d" % (i + 1),
            base, tip, width,
            [MAT["tall_1"], MAT["tall_2"], MAT["tall_3"]][i % 3],
            thickness=0.018,
            bend=(0, 0, rng.uniform(0.01, 0.05))
        )
    export_collection("Grass_Tall")

# ------------------------------------------------------------
# MODEL 3: WILD GRASS - irregular twiggy tufts + flower accents
# ------------------------------------------------------------
def create_wild_grass():
    begin_collection("Grass_Wild")
    rng = random.Random(RANDOM_SEED + 3)
    dry_mats = [MAT["wild_1"], MAT["wild_2"], MAT["wild_3"]]

    # Several thin, uneven stems create the airy, branching silhouette.
    stem_count = 15
    flower_indices = {2, 5, 8, 11, 13}
    flower_mats = [
        MAT["flower_pink"], MAT["flower_yellow"], MAT["flower_purple"],
        MAT["flower_pink"], MAT["flower_yellow"]
    ]

    for i in range(stem_count):
        angle = rng.uniform(0, math.tau)
        radius = rng.uniform(0.02, 0.34)
        height = rng.uniform(0.75, 1.55)
        lean = rng.uniform(0.18, 0.58)
        base = Vector((math.cos(angle) * radius, math.sin(angle) * radius, 0.025))
        tip = Vector((
            base.x + math.cos(angle) * lean + rng.uniform(-0.12, 0.12),
            base.y + math.sin(angle) * lean + rng.uniform(-0.12, 0.12),
            height
        ))

        # Main wiry stem, visibly thinner and more irregular than the two grass tufts.
        make_stem(
            "Wild_Stem_%02d" % i, base, tip,
            rng.uniform(0.012, 0.022), dry_mats[i % 3]
        )

        # Two or three small twig branches near the upper half.
        direction = (tip - base).normalized()
        for branch_index in range(rng.choice([2, 2, 3])):
            t = rng.uniform(0.42, 0.82)
            branch_base = base.lerp(tip, t)
            side_angle = angle + rng.choice([-1, 1]) * rng.uniform(0.55, 1.15)
            branch_length = rng.uniform(0.16, 0.32)
            branch_tip = branch_base + Vector((
                math.cos(side_angle) * branch_length,
                math.sin(side_angle) * branch_length,
                rng.uniform(0.10, 0.24)
            ))
            make_stem(
                "Wild_Twig_%02d_%02d" % (i, branch_index),
                branch_base, branch_tip,
                rng.uniform(0.008, 0.014), dry_mats[(i + branch_index) % 3]
            )

        # A few stems carry small colorful flowers.
        if i in flower_indices:
            make_flower(
                "Wild_Flower_%02d" % i,
                (tip.x, tip.y, tip.z),
                flower_mats[list(sorted(flower_indices)).index(i)],
                scale=rng.uniform(0.72, 1.0)
            )

    export_collection("Grass_Wild")

# Remove only previous versions of these three generated collections.
for collection in list(bpy.data.collections):
    if collection.name in {"Grass_Short", "Grass_Tall", "Grass_Wild"}:
        for obj in list(collection.objects):
            bpy.data.objects.remove(obj, do_unlink=True)
        bpy.data.collections.remove(collection)

create_short_grass()
create_tall_grass()
create_wild_grass()

print("Three redesigned grass assets generated.")
print("FBX folder:", EXPORT_FOLDER)

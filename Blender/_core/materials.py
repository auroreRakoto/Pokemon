"""Material utilities that avoid imposing a project shader implementation."""
import bpy


def get_or_create_material(name, color=(0.8, 0.8, 0.8, 1.0)):
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name=name)
    material.diffuse_color = color
    material.use_nodes = True
    return material


def assign_material(obj, material, slot_index=0):
    if obj.type != "MESH":
        raise TypeError(f"{obj.name} n'est pas un mesh.")
    while len(obj.data.materials) <= slot_index:
        obj.data.materials.append(None)
    obj.data.materials[slot_index] = material

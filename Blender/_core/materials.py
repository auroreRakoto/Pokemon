"""Material utilities with explicit Principled BSDF colour for FBX export."""
import bpy


def get_or_create_material(name, color=(0.8, 0.8, 0.8, 1.0)):
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name=name)

    rgba = tuple(float(channel) for channel in color)
    material.diffuse_color = rgba
    material.use_nodes = True

    # FBX exporters read node-based material data more reliably when the
    # Principled BSDF Base Color is explicitly populated. diffuse_color alone
    # is primarily a viewport/display colour and may arrive grey in Unity.
    nodes = material.node_tree.nodes if material.node_tree else None
    if nodes is not None:
        principled = next((node for node in nodes if node.type == "BSDF_PRINCIPLED"), None)
        if principled is None:
            principled = nodes.new(type="ShaderNodeBsdfPrincipled")
            output = next((node for node in nodes if node.type == "OUTPUT_MATERIAL"), None)
            if output is None:
                output = nodes.new(type="ShaderNodeOutputMaterial")
            material.node_tree.links.new(principled.outputs["BSDF"], output.inputs["Surface"])
        base_color = principled.inputs.get("Base Color")
        if base_color is not None:
            base_color.default_value = rgba

    return material


def assign_material(obj, material, slot_index=0):
    if obj.type != "MESH":
        raise TypeError(f"{obj.name} n'est pas un mesh.")
    while len(obj.data.materials) <= slot_index:
        obj.data.materials.append(None)
    obj.data.materials[slot_index] = material

"""Basic armature and skinning inspection helpers."""
def find_armatures(objects):
    return [obj for obj in objects if obj.type == "ARMATURE"]


def find_skinned_meshes(objects):
    return [
        obj for obj in objects
        if obj.type == "MESH" and any(mod.type == "ARMATURE" for mod in obj.modifiers)
    ]


def validate_armature_bindings(objects):
    issues = []
    armatures = find_armatures(objects)
    for mesh in [obj for obj in objects if obj.type == "MESH"]:
        modifiers = [mod for mod in mesh.modifiers if mod.type == "ARMATURE"]
        if modifiers and not any(mod.object in armatures for mod in modifiers):
            issues.append(f"{mesh.name}: modificateur Armature pointant hors du lot exporté.")
        if modifiers and not mesh.vertex_groups:
            issues.append(f"{mesh.name}: armature présente mais aucun vertex group.")
    return issues

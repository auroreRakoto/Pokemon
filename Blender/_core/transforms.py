"""Transform inspection and normalization helpers."""
import bpy


def apply_transforms(objects, location=False, rotation=True, scale=True):
    objects = [obj for obj in objects if obj and obj.type in {"MESH", "CURVE", "EMPTY", "ARMATURE"}]
    if not objects:
        return
    # Operator requires a valid view layer and active selection.
    from .scene import select_only
    previous_active = bpy.context.view_layer.objects.active
    previous_selected = list(bpy.context.selected_objects)
    try:
        select_only(objects)
        bpy.ops.object.transform_apply(
            location=location, rotation=rotation, scale=scale
        )
    finally:
        select_only(previous_selected, previous_active)


def transforms_are_identity(obj, tolerance=1e-4, check_location=False):
    values = []
    if check_location:
        values.extend(abs(v) for v in obj.location)
    values.extend(abs(v) for v in obj.rotation_euler)
    values.extend(abs(v - 1.0) for v in obj.scale)
    return all(value <= tolerance for value in values)

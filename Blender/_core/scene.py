"""Scene helpers. These functions never delete user data implicitly."""
import bpy


def get_or_create_scene(name):
    scene = bpy.data.scenes.get(name)
    if scene is None:
        scene = bpy.data.scenes.new(name)
    return scene


def set_active_scene(scene):
    # Scene switching is not available in every background/context mode.
    window = bpy.context.window
    if window is None:
        raise RuntimeError("Impossible de changer de scène sans fenêtre Blender active.")
    window.scene = scene


def deselect_all():
    for obj in bpy.context.selected_objects:
        obj.select_set(False)


def select_only(objects, active=None):
    deselect_all()
    valid = [obj for obj in objects if obj and obj.name in bpy.context.view_layer.objects]
    for obj in valid:
        obj.select_set(True)
    chosen = active if active in valid else (valid[0] if valid else None)
    bpy.context.view_layer.objects.active = chosen
    return valid

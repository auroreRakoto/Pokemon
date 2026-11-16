"""Animation action discovery and clip metadata helpers."""
import bpy


def actions_for_objects(objects):
    actions = {}
    for obj in objects:
        animation_data = obj.animation_data
        if animation_data and animation_data.action:
            action = animation_data.action
            actions[action.name] = action
        if obj.type == "ARMATURE" and obj.data.animation_data and obj.data.animation_data.action:
            action = obj.data.animation_data.action
            actions[action.name] = action
    return [actions[name] for name in sorted(actions)]


def action_metadata(action):
    start, end = action.frame_range
    return {
        "name": action.name,
        "frame_start": float(start),
        "frame_end": float(end),
        "frame_count": max(0, int(round(end - start)) + 1),
    }


def set_scene_frame_range(scene, start, end):
    if int(start) > int(end):
        raise ValueError("La frame de début doit être inférieure ou égale à la frame de fin.")
    scene.frame_start = int(start)
    scene.frame_end = int(end)

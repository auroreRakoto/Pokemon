"""Collision mesh naming and validation helpers."""
COLLISION_PREFIX = "COL"


def collision_name(base_name, suffix=""):
    tail = f"_{suffix}" if suffix else ""
    return f"{COLLISION_PREFIX}_{base_name}{tail}"


def find_collision_objects(objects):
    return [
        obj for obj in objects
        if obj.name.startswith("COL_") or obj.name.startswith("UCX_")
    ]

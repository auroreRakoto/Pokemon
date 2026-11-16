"""LOD naming and inspection conventions; actual simplification stays asset-driven."""
LOD_PREFIX = "LOD"


def lod_name(base_name, level):
    if int(level) < 0:
        raise ValueError("Le niveau LOD doit être positif ou nul.")
    return f"{base_name}_{LOD_PREFIX}{int(level)}"


def find_lods(objects):
    return sorted(
        [obj for obj in objects if obj.type == "MESH" and "_LOD" in obj.name],
        key=lambda obj: obj.name
    )

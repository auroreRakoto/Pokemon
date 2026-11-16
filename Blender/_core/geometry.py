"""Generic mesh inspection helpers."""
def mesh_stats(objects):
    meshes = [obj for obj in objects if obj.type == "MESH"]
    return {
        "object_count": len(meshes),
        "vertex_count": sum(len(obj.data.vertices) for obj in meshes),
        "edge_count": sum(len(obj.data.edges) for obj in meshes),
        "polygon_count": sum(len(obj.data.polygons) for obj in meshes),
    }


def mesh_has_faces(obj):
    return obj.type == "MESH" and len(obj.data.polygons) > 0


def dimensions(obj):
    return tuple(float(value) for value in obj.dimensions)

"""UV layer inspection and creation."""
def ensure_uv_layer(obj, name="UVMap"):
    if obj.type != "MESH":
        raise TypeError(f"{obj.name} n'est pas un mesh.")
    layer = obj.data.uv_layers.get(name)
    if layer is None:
        layer = obj.data.uv_layers.new(name=name)
    return layer


def validate_uvs(obj):
    if obj.type != "MESH":
        return False
    if len(obj.data.polygons) == 0:
        return False
    return len(obj.data.uv_layers) > 0

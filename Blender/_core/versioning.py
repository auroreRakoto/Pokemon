"""Pipeline version metadata."""
from . import __version__, MANIFEST_SCHEMA_VERSION


def pipeline_versions():
    import bpy
    return {
        "core": __version__,
        "manifest_schema": MANIFEST_SCHEMA_VERSION,
        "blender": bpy.app.version_string,
    }

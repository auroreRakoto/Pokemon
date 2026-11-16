"""Deterministic sidecar manifest writer. Unity .meta files are never touched."""
import json
import os
import tempfile
from pathlib import Path
from . import __version__, MANIFEST_SCHEMA_VERSION
from .geometry import mesh_stats
from .animation import actions_for_objects, action_metadata


def build_manifest(asset_id, domain, asset_type, source_path, export_path,
                   unity_destination, objects, export_profile,
                   validation, dependencies=None, extra=None):
    objects = list(objects)
    actions = actions_for_objects(objects)
    manifest = {
        "schema_version": MANIFEST_SCHEMA_VERSION,
        "asset_id": asset_id,
        "domain": domain,
        "asset_type": asset_type,
        "source": {"path": str(source_path).replace("\\", "/")},
        "pipeline": {"core_version": __version__},
        "export": {
            "format": "fbx",
            "profile": export_profile,
            "path": str(export_path).replace("\\", "/"),
        },
        "unity": {"destination": str(unity_destination).replace("\\", "/")},
        "geometry": mesh_stats(objects),
        "animations": [action_metadata(action) for action in actions],
        "dependencies": sorted(set(dependencies or [])),
        "validation": validation.to_dict(),
    }
    if extra:
        manifest["extra"] = extra
    return manifest


def write_manifest(path, data):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    payload = json.dumps(data, indent=2, ensure_ascii=False, sort_keys=True) + "\n"
    fd, temporary = tempfile.mkstemp(prefix=path.name + ".", suffix=".tmp", dir=str(path.parent))
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as stream:
            stream.write(payload)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    except Exception:
        try:
            os.unlink(temporary)
        except OSError:
            pass
        raise
    return path

"""Loading and validation of the shared Blender project configuration."""
import json
from pathlib import Path


class ConfigError(ValueError):
    pass


def load_json(path, required=True):
    path = Path(path)
    if not path.exists():
        if required:
            raise ConfigError(f"Fichier de configuration introuvable : {path}")
        return {}
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ConfigError(f"Configuration JSON invalide ({path}) : {exc}") from exc
    if not isinstance(data, dict):
        raise ConfigError(f"La configuration doit être un objet JSON : {path}")
    return data


def load_project_config(paths):
    """Load the single runtime config at Blender/config/project.json."""
    config_path = Path(paths.config) / "project.json"
    data = load_json(config_path, required=True)
    fbx = data.get("fbx")
    if not isinstance(fbx, dict) or not isinstance(fbx.get("profiles"), dict):
        raise ConfigError(f"Section 'fbx.profiles' absente ou invalide : {config_path}")
    for name in ("static_mesh", "skinned_mesh", "animated_mesh"):
        profile = fbx["profiles"].get(name)
        if not isinstance(profile, dict):
            raise ConfigError(f"Profil FBX '{name}' absent : {config_path}")
        for axis in ("axis_forward", "axis_up"):
            if not isinstance(profile.get(axis), str) or not profile[axis]:
                raise ConfigError(f"{name}.{axis} absent ou invalide : {config_path}")
    return data

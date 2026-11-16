"""JSON configuration loading (compatible with Blender's bundled Python)."""
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

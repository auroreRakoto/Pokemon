"""Repository-relative path resolution and safe output boundaries."""
from dataclasses import dataclass
from pathlib import Path


class PathConfigurationError(RuntimeError):
    pass


def find_project_root(start=None):
    """Find the repository root by walking upward until Blender/ is found."""
    current = Path(start or __file__).resolve()
    if current.is_file():
        current = current.parent
    for candidate in (current, *current.parents):
        if (candidate / "Blender").is_dir():
            return candidate
    raise PathConfigurationError(
        "Impossible de trouver la racine du projet : dossier 'Blender/' absent. "
        "Placez ce package dans <repo>/Blender/_core/."
    )


def _is_inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


@dataclass(frozen=True)
class ProjectPaths:
    root: Path

    @classmethod
    def discover(cls, start=None):
        return cls(find_project_root(start).resolve())

    @property
    def blender(self):
        return self.root / "Blender"

    def _configured_path(self, key, default):
        from .config import load_project_config
        data = load_project_config(self)
        relative = data.get("paths", {}).get(key, default)
        path = (self.root / relative).resolve()
        if not _is_inside(path, self.root):
            raise PathConfigurationError(f"Le chemin configuré '{key}' sort du projet : {relative}")
        return path

    @property
    def source(self):
        return self._configured_path("source_root", "Blender/source")

    @property
    def exports(self):
        return self._configured_path("exports_root", "Blender/exports")

    @property
    def config(self):
        return self.blender / "config"

    @property
    def unity_models(self):
        return self._configured_path("unity_models_root", "Assets/Project/Content/Models")

    def source_asset(self, domain, asset):
        return self._safe(self.source / _safe_name(domain) / _safe_name(asset), self.source)

    def export_asset(self, domain, asset):
        path = self._safe(self.exports / _safe_name(domain) / _safe_name(asset), self.exports)
        path.mkdir(parents=True, exist_ok=True)
        return path

    def _safe(self, path, boundary):
        path = path.resolve()
        if not _is_inside(path, boundary):
            raise PathConfigurationError(f"Chemin hors de la zone autorisée : {path}")
        return path


def _safe_name(value):
    value = str(value).strip()
    if not value or value in {".", ".."} or "/" in value or "\\" in value:
        raise PathConfigurationError(f"Identifiant de chemin invalide : {value!r}")
    return value

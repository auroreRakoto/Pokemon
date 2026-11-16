"""Blocking pre-export checks; budgets and domain rules are supplied by callers."""
from pathlib import Path
from .result import ValidationReport
from .naming import find_duplicate_names, validate_identifier


def validate_export_path(path, allowed_root):
    path = Path(path).resolve()
    allowed_root = Path(allowed_root).resolve()
    try:
        path.relative_to(allowed_root)
    except ValueError:
        raise ValueError(f"Chemin d'export hors du dossier autorisé : {path}")
    return path


def validate_objects(objects, asset_id=None, require_materials=True,
                     require_uvs=False, require_faces=True):
    report = ValidationReport()
    objects = list(objects or [])
    if not objects:
        report.add("EMPTY_EXPORT_SET", "Aucun objet n'a été fourni pour l'export.")
        return report

    if asset_id and not validate_identifier(asset_id):
        report.add("INVALID_ASSET_ID", f"Identifiant asset invalide : {asset_id!r}")

    for name in find_duplicate_names(objects):
        report.add("DUPLICATE_OBJECT_NAME", f"Nom d'objet dupliqué : {name}")

    for obj in objects:
        if obj.type == "MESH":
            if require_faces and len(obj.data.polygons) == 0:
                report.add("MESH_WITHOUT_FACES", "Le mesh ne contient aucun polygone.", object_name=obj.name)
            if require_materials and len(obj.data.materials) == 0:
                report.add("MISSING_MATERIAL", "Aucun slot matériau sur le mesh.", "WARNING", obj.name)
            if require_uvs and len(obj.data.uv_layers) == 0:
                report.add("MISSING_UV", "Aucune UV map sur le mesh.", object_name=obj.name)

        if obj.type == "MESH":
            for modifier in obj.modifiers:
                if modifier.type == "ARMATURE" and modifier.object is None:
                    report.add("EMPTY_ARMATURE_MODIFIER", "Modificateur Armature sans armature.", object_name=obj.name)

    return report

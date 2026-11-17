"""FBX export orchestration with validation and safe staging."""
from dataclasses import dataclass
from pathlib import Path
import os
import tempfile
import bpy

from .result import ExportResult, ValidationReport
from .validation import validate_objects, validate_export_path
from .scene import select_only
from .manifest import build_manifest, write_manifest


@dataclass(frozen=True)
class ExportProfile:
    name: str
    object_types: tuple = ("MESH", "ARMATURE", "EMPTY")
    apply_unit_scale: bool = True
    axis_forward: str = "-Z"
    axis_up: str = "Y"
    apply_scale_options: str = "FBX_SCALE_UNITS"
    mesh_smooth_type: str = "OFF"
    use_mesh_modifiers: bool = True
    bake_animation: bool = False
    use_nla_strips: bool = False
    use_all_actions: bool = False
    add_leaf_bones: bool = False

    @classmethod
    def from_config(cls, config, name):
        fbx = config.get("fbx", {})
        values = dict(fbx.get("profiles", {}).get(name, {}))
        if not values:
            raise ValueError(f"Profil FBX inconnu ou absent de project.json : {name}")
        values["object_types"] = tuple(values.get("object_types", ("MESH", "ARMATURE", "EMPTY")))
        values.setdefault("apply_unit_scale", fbx.get("apply_unit_scale", True))
        values.setdefault("apply_scale_options", fbx.get("apply_scale_options", "FBX_SCALE_UNITS"))
        values.setdefault("mesh_smooth_type", fbx.get("mesh_smooth_type", "OFF"))
        values.setdefault("use_mesh_modifiers", fbx.get("use_mesh_modifiers", True))
        return cls(name=name, **values)

    @classmethod
    def static_mesh(cls, config=None):
        return cls.from_config(config, "static_mesh") if config else cls(name="static_mesh", object_types=("MESH", "EMPTY"))

    @classmethod
    def skinned_mesh(cls, config=None):
        return cls.from_config(config, "skinned_mesh") if config else cls(name="skinned_mesh", bake_animation=False)

    @classmethod
    def animated_mesh(cls, config=None):
        return cls.from_config(config, "animated_mesh") if config else cls(name="animated_mesh", bake_animation=True, use_nla_strips=True)


def export_fbx(objects, destination, asset_id, domain, asset_type,
               unity_destination, profile=None, source_path="",
               allowed_export_root=None, require_materials=True,
               require_uvs=False, dependencies=None, extra_manifest=None):
    """Export a validated object set to FBX and emit its manifest.

    This function does not copy files into Unity and never creates/modifies .meta.
    """
    objects = list(objects or [])
    profile = profile or ExportProfile.static_mesh()
    destination = Path(destination).resolve()
    allowed_root = Path(allowed_export_root).resolve() if allowed_export_root else destination.parent.resolve()
    report = ValidationReport()

    try:
        validate_export_path(destination, allowed_root)
    except ValueError as exc:
        report.add("INVALID_EXPORT_PATH", str(exc))
        return ExportResult(False, asset_id, report=report, message="Export refusé : chemin invalide.")

    report = validate_objects(
        objects, asset_id=asset_id,
        require_materials=require_materials,
        require_uvs=require_uvs,
    )
    if not report.passed:
        return ExportResult(False, asset_id, report=report, message="Validation bloquante.")

    destination.parent.mkdir(parents=True, exist_ok=True)
    fd, temp_name = tempfile.mkstemp(prefix=destination.stem + ".", suffix=".fbx", dir=str(destination.parent))
    os.close(fd)
    temporary = Path(temp_name)

    previous_selected = list(bpy.context.selected_objects)
    previous_active = bpy.context.view_layer.objects.active

    try:
        selected = select_only(objects)
        if not selected:
            raise RuntimeError("Aucun objet exportable après sélection.")

        bpy.ops.export_scene.fbx(
            filepath=str(temporary),
            use_selection=True,
            object_types=set(profile.object_types),
            apply_unit_scale=profile.apply_unit_scale,
            apply_scale_options=profile.apply_scale_options,
            axis_forward=profile.axis_forward,
            axis_up=profile.axis_up,
            mesh_smooth_type=profile.mesh_smooth_type,
            use_mesh_modifiers=profile.use_mesh_modifiers,
            add_leaf_bones=profile.add_leaf_bones,
            bake_anim=profile.bake_animation,
            bake_anim_use_nla_strips=profile.use_nla_strips,
            bake_anim_use_all_actions=profile.use_all_actions,
        )

        if not temporary.is_file() or temporary.stat().st_size == 0:
            raise RuntimeError("Blender n'a pas produit de FBX valide.")

        # Promote only after export success; old FBX survives a failed export.
        os.replace(temporary, destination)

        manifest_data = build_manifest(
            asset_id=asset_id,
            domain=domain,
            asset_type=asset_type,
            source_path=source_path,
            export_path=destination.name,
            unity_destination=unity_destination,
            objects=objects,
            export_profile=profile.name,
            validation=report,
            dependencies=dependencies,
            extra=extra_manifest,
        )
        manifest_path = destination.with_suffix(".manifest.json")
        write_manifest(manifest_path, manifest_data)

        return ExportResult(
            True, asset_id, str(destination), str(manifest_path), report,
            "Export FBX et manifeste terminés. Aucun fichier Unity .meta n'a été modifié."
        )
    except Exception as exc:
        report.add("EXPORT_FAILED", str(exc))
        return ExportResult(False, asset_id, report=report, message=f"Échec export : {exc}")
    finally:
        try:
            if temporary.exists():
                temporary.unlink()
        except OSError:
            pass
        select_only(previous_selected, previous_active)

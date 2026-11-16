"""Shared Blender asset-pipeline primitives for the Pokemon project."""

__version__ = "0.1.0"
MANIFEST_SCHEMA_VERSION = 1

from .result import ValidationIssue, ValidationReport, ExportResult
from .paths import ProjectPaths, find_project_root
from .validation import validate_objects, validate_export_path
from .manifest import write_manifest
from .export import ExportProfile, export_fbx

__all__ = [
    "__version__", "MANIFEST_SCHEMA_VERSION",
    "ValidationIssue", "ValidationReport", "ExportResult",
    "ProjectPaths", "find_project_root",
    "validate_objects", "validate_export_path",
    "write_manifest", "ExportProfile", "export_fbx",
]

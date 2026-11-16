"""Small shared result types; independent of bpy."""
from dataclasses import dataclass, field
from typing import Any, Dict, List


@dataclass
class ValidationIssue:
    code: str
    message: str
    severity: str = "ERROR"
    object_name: str = ""

    def to_dict(self) -> Dict[str, Any]:
        return {
            "code": self.code,
            "message": self.message,
            "severity": self.severity,
            "object": self.object_name or None,
        }


@dataclass
class ValidationReport:
    issues: List[ValidationIssue] = field(default_factory=list)

    @property
    def errors(self):
        return [item for item in self.issues if item.severity == "ERROR"]

    @property
    def warnings(self):
        return [item for item in self.issues if item.severity == "WARNING"]

    @property
    def passed(self):
        return not self.errors

    def add(self, code, message, severity="ERROR", object_name=""):
        self.issues.append(ValidationIssue(code, message, severity, object_name))

    def to_dict(self):
        return {
            "status": "passed" if self.passed else "failed",
            "errors": [item.to_dict() for item in self.errors],
            "warnings": [item.to_dict() for item in self.warnings],
        }


@dataclass
class ExportResult:
    success: bool
    asset_id: str
    export_path: str = ""
    manifest_path: str = ""
    report: ValidationReport = field(default_factory=ValidationReport)
    message: str = ""

    def to_dict(self):
        return {
            "success": self.success,
            "asset_id": self.asset_id,
            "export_path": self.export_path or None,
            "manifest_path": self.manifest_path or None,
            "message": self.message,
            "validation": self.report.to_dict(),
        }

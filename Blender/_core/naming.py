"""Stable asset and Blender object naming helpers."""
import re


def slugify(value):
    value = re.sub(r"[^a-zA-Z0-9]+", "_", str(value).strip())
    return value.strip("_").lower()


def validate_identifier(value):
    text = str(value or "")
    if not text or not re.fullmatch(r"[a-z0-9]+(?:[._-][a-z0-9]+)*", text):
        return False
    return True


def find_duplicate_names(objects):
    names = [obj.name for obj in objects]
    return sorted({name for name in names if names.count(name) > 1})

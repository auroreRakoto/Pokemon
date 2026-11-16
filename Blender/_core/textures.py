"""Portable image loading and packing helpers."""
import bpy
from pathlib import Path


def load_image(path, check_existing=True):
    path = Path(path).resolve()
    if not path.is_file():
        raise FileNotFoundError(f"Texture introuvable : {path}")
    return bpy.data.images.load(str(path), check_existing=check_existing)


def pack_image(image):
    if image is None:
        raise ValueError("Image manquante.")
    image.pack()


def image_info(image):
    return {
        "name": image.name,
        "filepath": bpy.path.abspath(image.filepath) if image.filepath else "",
        "width": int(image.size[0]),
        "height": int(image.size[1]),
        "packed": bool(image.packed_file),
    }

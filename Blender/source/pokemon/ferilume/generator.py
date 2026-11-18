"""ART Pokemon generator — Ferilume.

Open this saved file with Blender Text > Open, save it, then Run Script.
"""
# --- Canonical Blender Text Editor bootstrap (must precede _core imports) ---
from pathlib import Path
import sys
import math
import traceback

import bpy


def _current_text_file_path() -> Path:
    """Return the saved path of the active Blender Text Editor script."""
    try:
        space = bpy.context.space_data
        text_block = getattr(space, "text", None)
        if text_block and text_block.filepath:
            path = Path(bpy.path.abspath(text_block.filepath))
            if path.is_file():
                return path
    except (AttributeError, TypeError, RuntimeError):
        pass

    candidates = []
    for text_block in bpy.data.texts:
        if not text_block.filepath:
            continue
        path = Path(bpy.path.abspath(text_block.filepath))
        if path.is_file() and path.name.lower() == "generator.py":
            candidates.append(path)

    unique_candidates = list(dict.fromkeys(path.resolve() for path in candidates))
    if len(unique_candidates) == 1:
        return unique_candidates[0]
    if len(unique_candidates) > 1:
        raise RuntimeError(
            "Plusieurs generator.py sont ouverts. Active le bon script dans "
            "l'éditeur de texte avant Run Script."
        )
    raise RuntimeError(
        "Le script Blender n'a pas de chemin enregistré. Utilise Text > Open "
        "pour ouvrir le generator.py depuis le dépôt, puis sauvegarde-le."
    )


def _find_blender_root(generator_path: Path) -> Path:
    for directory in (generator_path.resolve().parent, *generator_path.resolve().parents):
        if (directory / "_core" / "__init__.py").is_file():
            return directory
        candidate = directory / "Blender"
        if (candidate / "_core" / "__init__.py").is_file():
            return candidate
    raise RuntimeError(
        f"Impossible de trouver Blender/_core depuis le générateur : {generator_path}"
    )


GENERATOR_PATH = _current_text_file_path()
BLENDER_ROOT = _find_blender_root(GENERATOR_PATH)
if str(BLENDER_ROOT) not in sys.path:
    sys.path.insert(0, str(BLENDER_ROOT))

# Shared imports are intentionally after repository discovery.
import _core
from _core.paths import ProjectPaths
from project_modules.pokemon.procedural import *
# --- End bootstrap ---

def generate():
    c=begin_asset("ferilume")
    fire=mat("MAT_Ferilume_Flame",(1.0,.24,.025,1),.48,1.8)
    gold=mat("MAT_Ferilume_FlameLight",(1.0,.68,.08,1),.52,1.0)
    soot=mat("MAT_Ferilume_Soot",(0.12,.075,.12,1))
    cream=mat("MAT_Ferilume_Face",(1.0,.78,.42,1))
    black=mat("MAT_Ferilume_Eyes",(.012,.009,.018,1),.25)
    white=mat("MAT_Ferilume_Highlight",(1,1,1,1),.2)
    arm=make_armature("RIG_Ferilume",[
        ("root",(0,0,.05),(0,0,.28),None),
        ("body",(0,0,.28),(0,0,.72),"root"),
        ("flame",(0,0,.72),(0,0,1.12),"body"),
        ("tail",(0,-.12,.26),(0,-.48,.22),"root"),
        ("eye.L",(-.17,-.27,.53),(-.17,-.27,.68),"body"),
        ("eye.R",(.17,-.27,.53),(.17,-.27,.68),"body"),
    ])
    parts=[]
    parts.append((ico("Ferilume_Body",(0,0,.43),(.39,.34,.34),soot,2), "body"))
    parts.append((ico("Ferilume_Face",(0,-.245,.48),(.29,.12,.25),cream,2), "body"))
    # flame crown: stacked low-poly lobes, with asymmetric tips
    parts.append((ico("Ferilume_FlameCore",(0,0,.78),(.28,.25,.31),fire,1), "flame"))
    parts.append((cone("Ferilume_FlameTip",(-.06,0,1.03),.18,.015,.48,gold,7), "flame"))
    parts.append((cone("Ferilume_FlameSide",(.19,.01,.91),.12,.01,.30,fire,7,(0,-.35,-.2)), "flame"))
    parts.append((ico("Ferilume_Tail",(0,.30,.25),(.12,.32,.10),soot,1), "tail"))
    for side,bone in [(-1,"eye.L"),(1,"eye.R")]:
        parts.append((ico(f"Ferilume_Eye_{side}",(side*.145,-.355,.54),(.065,.035,.09),black,2),bone))
        parts.append((ico(f"Ferilume_EyeShine_{side}",(side*.16,-.385,.58),(.018,.012,.024),white,1),bone))
    # little ember collar
    parts.append((ico("Ferilume_Ember",(0,-.02,.18),(.16,.13,.07),gold,1),"root"))
    for obj,bone in parts: bind_rigid_part(obj,arm,bone)
    animate_idle(arm,"Ferilume",{
        "body":[(1,(0,0,-.025),(0,0,0)),(13,(0,0,.025),(0,0,.025)),(25,(0,0,-.025),(0,0,0)),(37,(0,0,.025),(0,0,.025)),(49,(0,0,-.025),(0,0,0))],
        "flame":[(1,(0,0,-.06),(0,0,0)),(13,(0,.10,.07),(0,0,.015)),(25,(0,0,-.06),(0,0,0)),(37,(0,-.10,.06),(0,0,.015)),(49,(0,0,-.06),(0,0,0))],
        "tail":[(1,(0,0,-.12),(0,0,0)),(25,(0,0,.12),(0,0,0)),(49,(0,0,-.12),(0,0,0))],
    })
    return finish_asset("ferilume",c,arm,[o for o,b in parts],"Ferilume","pokemon.ferilume",
        "Assets/Project/Content/Models/Pokemon/_ART_TEST/Ferilume/SK_Pokemon_Ferilume_Base.fbx",
        "Blender/source/pokemon/ferilume/",
        "Floating soot-sprite body, flame crown and ember tail; no evolution-stage geometry.")

try:
    print("[ART] Generating Ferilume — Blender",bpy.app.version_string)
    generate()
    print("[ART] SUCCESS Ferilume")
except Exception:
    traceback.print_exc()
    raise

"""ART Pokemon generator — Nacoryn.

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
    c=begin_asset("nacoryn")
    blue=mat("MAT_Nacoryn_Azure",(.025,.36,.78,1))
    pale=mat("MAT_Nacoryn_Cream",(.83,.91,.94,1))
    cyan=mat("MAT_Nacoryn_Cyan",(.08,.78,.91,1),.5)
    coral=mat("MAT_Nacoryn_Coral",(1,.22,.48,1),.58)
    violet=mat("MAT_Nacoryn_Violet",(.48,.20,.82,1),.55)
    black=mat("MAT_Nacoryn_Eyes",(.008,.025,.06,1),.22)
    white=mat("MAT_Nacoryn_EyeShine",(1,1,1,1),.2)
    specs=[
        ("root",(0,0,.12),(0,0,.34),None),
        ("body.front",(0,0,.34),(0,0,.68),"root"),
        ("neck",(0,0,.68),(0,-.10,.98),"body.front"),
        ("head",(0,-.10,.98),(0,-.28,1.22),"neck"),
        ("body.mid",(0,0,.34),(0,.28,.28),"root"),
        ("body.rear",(0,.28,.28),(0,.55,.42),"body.mid"),
        ("tail",(0,.55,.42),(0,.75,.70),"body.rear"),
        ("fin.L",(-.12,0,.55),(-.34,0,.70),"body.front"),
        ("fin.R",(.12,0,.55),(.34,0,.70),"body.front"),
        ("coral.L",(-.10,-.05,1.10),(-.24,-.05,1.35),"head"),
        ("coral.R",(.10,-.05,1.10),(.24,-.05,1.35),"head"),
    ]
    arm=make_armature("RIG_Nacoryn",specs)
    parts=[]
    # A chain of overlapping body segments creates one continuous eel-like silhouette.
    body_segments=[
        ((0,.34,.34),(.25,.30,.28),"body.front"),
        ((0,.12,.34),(.24,.28,.25),"body.front"),
        ((0,-.08,.48),(.22,.27,.23),"neck"),
        ((0,.53,.39),(.20,.24,.22),"body.mid"),
        ((0,.70,.52),(.17,.22,.18),"body.rear"),
        ((0,.81,.68),(.13,.19,.15),"tail"),
    ]
    for i,(loc,scale,bone) in enumerate(body_segments):
        parts.append((ico(f"Nacoryn_BodySegment_{i:02d}",loc,scale,blue if i%2==0 else pale,2),"body.front" if i<2 else bone))
    parts.append((ico("Nacoryn_Head",(0,-.25,1.02),(.30,.32,.25),blue,2),"head"))
    parts.append((ico("Nacoryn_Muzzle",(0,-.48,.96),(.21,.17,.13),pale,2),"head"))
    for side,bone in [(-1,"head"),(1,"head")]:
        parts.append((ico(f"Nacoryn_Eye_{side}",(side*.19,-.49,1.08),(.055,.035,.075),black,2),bone))
        parts.append((ico(f"Nacoryn_EyeShine_{side}",(side*.20,-.522,1.11),(.016,.01,.02),white,1),bone))
    # Coral antlers and side fins: no legs/feet are created.
    for side,bone in [(-1,"coral.L"),(1,"coral.R")]:
        parts.append((cone(f"Nacoryn_CoralStem_{side}",(side*.12,-.06,1.28),.035,.012,.32,coral,6,(0,side*.35,side*.2)),bone))
        parts.append((cone(f"Nacoryn_CoralBranch_{side}",(side*.23,-.06,1.34),.026,.008,.22,coral,6,(0,side*.9,side*.2)),bone))
    for side,bone in [(-1,"fin.L"),(1,"fin.R")]:
        parts.append((ico(f"Nacoryn_SideFin_{side}",(side*.27,.02,.62),(.20,.07,.15),cyan,1,(0,side*.3,side*.25)),bone))
    parts.append((ico("Nacoryn_TailFin",(0,.91,.79),(.20,.07,.24),violet,1,(.4,0,0)),"tail"))
    for obj,bone in parts: bind_rigid_part(obj,arm,bone)
    animate_idle(arm,"Nacoryn",{
        "body.front":[(1,(0,0,-.035),(0,0,0)),(13,(0,.06,.035),(0,0,.018)),(25,(0,0,-.035),(0,0,0)),(37,(0,-.06,.035),(0,0,.018)),(49,(0,0,-.035),(0,0,0))],
        "neck":[(1,(0,0,-.025),(0,0,0)),(13,(.035,0,.025),(0,0,0)),(25,(0,0,-.025),(0,0,0)),(37,(-.035,0,.025),(0,0,0)),(49,(0,0,-.025),(0,0,0))],
        "head":[(1,(0,0,-.025),(0,0,0)),(13,(0,.035,.025),(0,0,0)),(25,(0,0,-.025),(0,0,0)),(37,(0,-.035,.025),(0,0,0)),(49,(0,0,-.025),(0,0,0))],
        "body.mid":[(1,(0,.04,0),(0,0,0)),(25,(0,-.04,0),(0,0,0)),(49,(0,.04,0),(0,0,0))],
        "body.rear":[(1,(0,-.06,0),(0,0,0)),(25,(0,.06,0),(0,0,0)),(49,(0,-.06,0),(0,0,0))],
        "tail":[(1,(0,0,-.14),(0,0,0)),(13,(0,.10,.10),(0,0,0)),(25,(0,0,.14),(0,0,0)),(37,(0,-.10,-.10),(0,0,0)),(49,(0,0,-.14),(0,0,0))],
        "fin.L":[(1,(0,0,-.12),(0,0,0)),(25,(0,0,.12),(0,0,0)),(49,(0,0,-.12),(0,0,0))],
        "fin.R":[(1,(0,0,.12),(0,0,0)),(25,(0,0,-.12),(0,0,0)),(49,(0,0,.12),(0,0,0))],
    })
    return finish_asset("nacoryn",c,arm,[o for o,b in parts],"Nacoryn","pokemon.nacoryn",
        "Assets/Project/Content/Models/Pokemon/_ART_TEST/Nacoryn/SK_Pokemon_Nacoryn_Base.fbx",
        "Blender/source/pokemon/nacoryn/",
        "Legless aquatic serpent: continuous segmented trunk, coral crests and fins; deliberately no limbs.")

try:
    print("[ART] Generating Nacoryn (legless serpent) — Blender",bpy.app.version_string)
    generate()
    print("[ART] SUCCESS Nacoryn — no legs generated")
except Exception:
    traceback.print_exc()
    raise

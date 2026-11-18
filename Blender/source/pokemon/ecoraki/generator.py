"""Ecoraki — approved visual V2, geometry-first Blender generator.

Open this saved file via Blender Text > Open, save, then Run Script.
This pass creates the model only. Rigging and Idle are deliberately deferred
until the silhouette and materials are approved in Blender.
"""
from pathlib import Path
import sys
import math
import traceback
import bpy

def _current_text_file_path() -> Path:
    try:
        space = bpy.context.space_data
        text_block = getattr(space, "text", None)
        if text_block and text_block.filepath:
            path = Path(bpy.path.abspath(text_block.filepath))
            if path.is_file():
                return path
    except (AttributeError, TypeError, RuntimeError):
        pass
    candidates=[]
    for text_block in bpy.data.texts:
        if text_block.filepath:
            path=Path(bpy.path.abspath(text_block.filepath))
            if path.is_file() and path.name.lower()=="generator.py":
                candidates.append(path.resolve())
    candidates=list(dict.fromkeys(candidates))
    if len(candidates)==1: return candidates[0]
    if len(candidates)>1:
        raise RuntimeError("Plusieurs generator.py sont ouverts. Active le bon script dans l'éditeur.")
    raise RuntimeError("Script non enregistré. Ouvre le fichier depuis le dépôt avec Text > Open.")

def find_blender_root(generator_path: Path) -> Path:
    for directory in (generator_path.resolve().parent,*generator_path.resolve().parents):
        if (directory/"_core"/"__init__.py").is_file(): return directory
        candidate=directory/"Blender"
        if (candidate/"_core"/"__init__.py").is_file(): return candidate
    raise RuntimeError(f"Blender/_core introuvable depuis {generator_path}")

GENERATOR_PATH=_current_text_file_path()
BLENDER_ROOT=find_blender_root(GENERATOR_PATH)
if str(BLENDER_ROOT) not in sys.path: sys.path.insert(0,str(BLENDER_ROOT))

import _core
from _core.paths import ProjectPaths
from _core.config import load_project_config
from _core.export import ExportProfile, export_fbx
from _core.collections import get_or_create_collection
from project_modules.pokemon.ecoraki_geometry import (
    make_material, ellipsoid, tube, leaf, ear, curved_mark, parent_to_collection
)

ASSET_ID="pokemon.ecoraki"
SPECIES_ID="ecoraki"
GENERATOR_VERSION="0.2.0"
OBJECT_NAME="SK_Pokemon_Ecoraki_Base"
UNITY_DESTINATION="Assets/Project/Content/Models/Pokemon/_ART_TEST/Ecoraki/SK_Pokemon_Ecoraki_Base.fbx"

def configured_profile(config):
    fbx=config["fbx"]
    p=fbx["profiles"]["static_mesh"]
    return ExportProfile(
        name="static_mesh",
        object_types=tuple(p["object_types"]),
        apply_unit_scale=bool(fbx["apply_unit_scale"]),
        axis_forward=p["axis_forward"],
        axis_up=p["axis_up"],
        bake_animation=bool(p["bake_animation"]),
        use_nla_strips=bool(p["use_nla_strips"]),
        use_all_actions=bool(p["use_all_actions"]),
        add_leaf_bones=bool(p["add_leaf_bones"]),
        mesh_smooth_type=fbx["mesh_smooth_type"],
    )

def clear_owned_collection():
    name="ART_POKEMON_ECORAKI"
    old=bpy.data.collections.get(name)
    if old:
        for obj in list(old.objects): bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.collections.remove(old)
    return get_or_create_collection(name)

def generate():
    paths=ProjectPaths.discover(start=BLENDER_ROOT)
    config=load_project_config(paths)
    collection=clear_owned_collection()
    # Palette: warm cream, charcoal-brown, leaf greens and amber-gold.
    cream=make_material("MAT_Ecoraki_Cream",(0.94,.84,.69,1))
    pale=make_material("MAT_Ecoraki_Paw",(0.99,.86,.69,1))
    charcoal=make_material("MAT_Ecoraki_Charcoal",(.105,.082,.075,1))
    dark=make_material("MAT_Ecoraki_Mask",(.075,.052,.047,1))
    inner=make_material("MAT_Ecoraki_Ear_Inner",(.84,.43,.32,1))
    leaf_green=make_material("MAT_Ecoraki_Leaf_Green",(.28,.62,.045,1))
    leaf_light=make_material("MAT_Ecoraki_Leaf_Light",(.56,.79,.075,1))
    vein=make_material("MAT_Ecoraki_Leaf_Vein",(.18,.39,.035,1))
    gold=make_material("MAT_Ecoraki_Gold_Mark",(1,.48,.018,1),.52,.45)
    iris=make_material("MAT_Ecoraki_Iris",(1,.61,.025,1),.32)
    pupil=make_material("MAT_Ecoraki_Pupil",(.018,.012,.009,1),.2)
    white=make_material("MAT_Ecoraki_Eye_Highlight",(1,1,1,1),.18)

    objects=[]
    # Torso: broad shoulder/hip volume with a slightly lifted rump.
    objects.append(ellipsoid("Ecoraki_Torso",(0,.015,.66),(.39,.56,.34),charcoal,32,20))
    objects.append(ellipsoid("Ecoraki_Belly",(0,-.16,.57),(.30,.40,.245),cream,28,18))
    objects.append(ellipsoid("Ecoraki_Chest",(0,-.37,.72),(.30,.18,.25),cream,24,16))
    # Neck ruff under the leaf collar; tapered silhouette rather than a sphere stack.
    objects.append(ellipsoid("Ecoraki_Neck",(0,-.31,.88),(.255,.25,.27),charcoal,24,16))

    # Head is deliberately oversized and rounded, with a cream face.
    objects.append(ellipsoid("Ecoraki_Head",(0,-.49,1.12),(.39,.34,.36),cream,32,24))
    # Dark mask is a shallow custom ellipsoid patch on the front of the face.
    objects.append(ellipsoid("Ecoraki_Face_Mask",(0,-.787,1.15),(.305,.045,.235),dark,28,18))
    # Cream muzzle and cheek tufts.
    objects.append(ellipsoid("Ecoraki_Muzzle",(0,-.827,1.005),(.17,.065,.105),cream,24,14))
    for side in (-1,1):
        objects.append(ellipsoid(f"Ecoraki_Cheek_{side}",(side*.265,-.72,1.03),(.105,.075,.12),cream,18,12))
        # Eyes: amber iris, dark pupil, two highlights.
        x=side*.165
        objects.append(ellipsoid(f"Ecoraki_EyeSocket_{side}",(x,-.823,1.20),(.115,.038,.145),dark,20,14))
        objects.append(ellipsoid(f"Ecoraki_Iris_{side}",(x,-.855,1.20),(.083,.029,.112),iris,20,14))
        objects.append(ellipsoid(f"Ecoraki_Pupil_{side}",(x,-.881,1.20),(.039,.018,.078),pupil,16,12))
        objects.append(ellipsoid(f"Ecoraki_EyeGlint_{side}",(x-.022,-.899,1.245),(.021,.012,.028),white,12,8))
        objects.append(ellipsoid(f"Ecoraki_EyeGlintSmall_{side}",(x+.022,-.897,1.17),(.009,.008,.012),white,10,6))
        # Large triangular lemur ears, pink inner ear facing forward.
        ear_obj,ear_inner=ear(f"Ecoraki_Ear_{side}",(side*.27,-.43,1.30),.53,.19,charcoal,inner)
        objects.extend([ear_obj,ear_inner])
    # Nose and mouth details.
    objects.append(ellipsoid("Ecoraki_Nose",(0,-.895,1.015),(.035,.018,.025),dark,12,8))
    objects.append(curved_mark("Ecoraki_Mouth_Left",[(0,-.891,1.0),(-.018,-.895,.974),(-.07,-.884,.968)],dark,.008))
    objects.append(curved_mark("Ecoraki_Mouth_Right",[(0,-.891,1.0),(.018,-.895,.974),(.07,-.884,.968)],dark,.008))

    # Four short but substantial quadruped legs with rounded paws.
    for side,label in [(-1,"L"),(1,"R")]:
        for front in (True,False):
            y=-.30 if front else .39
            x=side*.245
            ztop=.60 if front else .58
            zbottom=.19
            legmat=charcoal
            leg=ellipsoid(f"Ecoraki_Leg_{'Front' if front else 'Rear'}_{label}",
                          (x,y,(ztop+zbottom)/2),(.105,.13,(ztop-zbottom)/2+.035),legmat,20,14)
            objects.append(leg)
            paw=ellipsoid(f"Ecoraki_Paw_{'Front' if front else 'Rear'}_{label}",
                          (x,y-.055,.145),(.135,.18,.09),pale,20,12)
            objects.append(paw)
            # Three tiny toes per paw, facing forward (-Y).
            for toe in range(3):
                tx=x+(toe-1)*.067
                objects.append(ellipsoid(f"Ecoraki_Toe_{'F' if front else 'R'}_{label}_{toe}",
                    (tx,y-.185,.13),(.038,.065,.045),cream,12,8))

    # Signature thick ringed lemur tail. A continuous swept tube, not a chain of blobs.
    tail_points=[
        (0,.43,.58),(.02,.68,.53),(.06,.91,.58),(.10,1.12,.78),
        (.11,1.17,1.03),(.08,1.06,1.24),(.04,.83,1.32),(.02,.62,1.22)
    ]
    tail_radii=[.19,.205,.21,.215,.205,.19,.16,.105]
    tail=tube("Ecoraki_Tail_Ringed",tail_points,tail_radii,[cream,charcoal],sides=14,steps_per_span=7)
    objects.append(tail)

    # Layered leaf collar: 9 broad pointed leaves radiating from the neck.
    for i in range(9):
        angle=2*math.pi*i/9
        base=(.18*math.sin(angle),-.36+.10*math.cos(angle),.91)
        tip=(.43*math.sin(angle),-.39+.16*math.cos(angle),.69+.045*math.cos(angle*2))
        obj,vein_obj=leaf(f"Ecoraki_CollarLeaf_{i:02d}",base,tip,.145,leaf_light if i%3==0 else leaf_green,vein)
        objects.extend([obj,vein_obj])
    # Three swept crown leaves, clearly visible above the forehead.
    for i,(base,tip,w) in enumerate([
        ((-.10,-.48,1.40),(-.25,-.49,1.69),.105),
        ((0,-.49,1.42),(0,-.51,1.77),.12),
        ((.10,-.48,1.40),(.25,-.49,1.68),.105),
    ]):
        obj,vein_obj=leaf(f"Ecoraki_CrownLeaf_{i:02d}",base,tip,w,leaf_light if i==1 else leaf_green,vein)
        objects.extend([obj,vein_obj])

    # Golden botanical glyphs on outer forelegs and haunches.
    for side,label in [(-1,"L"),(1,"R")]:
        x=side*.337
        objects.append(curved_mark(f"Ecoraki_Mark_Fore_{label}",
            [(x,-.37,.39),(x*1.04,-.39,.46),(x,-.37,.54),(x*.96,-.36,.59)],gold,.018))
        objects.append(curved_mark(f"Ecoraki_Mark_Hind_{label}",
            [(x,.29,.35),(x*1.05,.27,.44),(x,.29,.53),(x*.96,.30,.58)],gold,.018))

    # Put all generated objects into the owned collection; keep origin at ground center.
    for obj in objects:
        for old_collection in list(obj.users_collection):
            old_collection.objects.unlink(obj)
        collection.objects.link(obj)
    bpy.context.scene.unit_settings.system=config["units"]["system"]
    bpy.context.scene.unit_settings.length_unit=config["units"]["length"]
    bpy.context.scene.unit_settings.scale_length=config["units"]["scale_length"]
    bpy.context.scene.frame_set(1)

    profile=configured_profile(config)
    export_dir=paths.export_asset("pokemon","ecoraki")
    result=export_fbx(
        objects=objects,
        destination=export_dir/"SK_Pokemon_Ecoraki_Base.fbx",
        asset_id=ASSET_ID,
        domain="pokemon",
        asset_type="static_mesh",
        unity_destination=UNITY_DESTINATION,
        profile=profile,
        source_path="Blender/source/pokemon/ecoraki/",
        allowed_export_root=paths.exports,
        require_materials=True,
        require_uvs=False,
        asset_domain="Pokemon",
        asset_category="BaseSpecies",
        extra_manifest={
            "species_id":SPECIES_ID,
            "export_version":GENERATOR_VERSION,
            "generator_version":GENERATOR_VERSION,
            "units":config["units"],
            "scale":{"scale_length":config["units"]["scale_length"],"object_scale":[1,1,1]},
            "design_reference":"Approved Ecoraki V2 concept",
            "model_status":"geometry_blockout_for_visual_review",
            "rig_status":"deferred_until_model_approval",
            "animation_status":"deferred_until_model_approval",
            "notes":["Lemur-like quadruped","Thick continuous ringed curled tail","Layered leaf collar and crown","Golden botanical markings"]
        }
    )
    print("[ART][ECORAKI]",result.to_dict())
    if not result.success:
        raise RuntimeError(result.message)
    return result

try:
    print("[ART] Ecoraki geometry V2 — Blender",bpy.app.version_string)
    print("[ART] Project root:",ProjectPaths.discover(start=BLENDER_ROOT).root)
    generate()
    print("[ART] SUCCESS: Ecoraki V2 geometry exported")
except Exception:
    print("[ART] FAILED: Ecoraki V2")
    traceback.print_exc()
    raise

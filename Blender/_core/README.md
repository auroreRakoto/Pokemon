# ART Blender `_core` — v0.1.0

Bibliothèque commune de base pour les générateurs Blender du projet Pokémon.

## Installation

Copier le dossier `_core/` dans :

```text
<racine-du-depot>/Blender/_core/
```

Le dossier doit contenir `__init__.py`. Ne pas copier le ZIP lui-même dans Blender.

## Utilisation depuis un générateur

Les générateurs d'assets restent dans `Blender/source/<domain>/<asset>/`.
Ils importent `_core` ; ils ne contiennent pas leur propre logique générique d'export.

```python
from pathlib import Path
import bpy

from _core.paths import ProjectPaths
from _core.export import ExportProfile, export_fbx

paths = ProjectPaths.discover()
asset_id = "environment.vegetation.grass_short"
objects = list(bpy.context.selected_objects)

result = export_fbx(
    objects=objects,
    destination=paths.export_asset("environment", "grass_short") / "SM_Grass_Short.fbx",
    asset_id=asset_id,
    domain="environment",
    asset_type="static_mesh",
    unity_destination="Assets/Project/Content/Models/Environment/Vegetation/Grass_Short.fbx",
    profile=ExportProfile.static_mesh(),
    source_path="Blender/source/environment/grass_short/",
    allowed_export_root=paths.exports,
)
print(result.to_dict())
if not result.success:
    raise RuntimeError(result.message)
```

## Profils

- `ExportProfile.static_mesh()` : objets statiques ; aucune animation.
- `ExportProfile.skinned_mesh()` : mesh + armature ; export d'animation désactivé.
- `ExportProfile.animated_mesh()` : export d'animation FBX et NLA strips.

Les valeurs d'axes `-Z / Y` sont des valeurs initiales à confirmer par un round-trip dans Unity 6000.4.11f1. Ne pas considérer le profil comme définitivement validé avant ce test.

## Règles de sécurité

- `_core` écrit uniquement sous `Blender/exports/`.
- Il ne copie jamais les FBX vers `Assets/`.
- Il ne crée, copie, modifie ou supprime jamais de `.meta`.
- Un FBX existant n'est remplacé qu'après réussite de l'export temporaire.
- Les validations bloquantes empêchent l'export.
- Les avertissements de matériau sont consignés mais ne bloquent pas par défaut.
- Les règles de budgets et conventions propres à chaque domaine sont fournies par les générateurs, pas inventées par `_core`.

## Limites v0.1.0

- Le package n'a pas encore été exécuté dans Blender : tests de syntaxe Python uniquement.
- Les profils FBX doivent être vérifiés dans le Unity 6 du projet.
- Les clips d'animation dépendent de la configuration Action/NLA du fichier Blender.
- Les fonctions LOD/collision/sockets sont des helpers de conventions et d'inspection, pas des générateurs automatiques complets.
- L'import/synchronisation Unity reste sous responsabilité TOOLS.

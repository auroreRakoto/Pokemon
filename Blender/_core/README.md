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

## Configuration et profils

La configuration runtime unique est `Blender/config/project.json` (modèle : `project.example.json`). Elle centralise les chemins du projet, les unités et tous les paramètres FBX communs : axes, échelle, modificateurs, lissage et options d'animation.

Un générateur charge cette configuration avec `load_project_config(paths)`, puis utilise `ExportProfile.static_mesh(config)`, `ExportProfile.skinned_mesh(config)` ou `ExportProfile.animated_mesh(config)`. Les paramètres propres à un asset restent dans son générateur.

Les axes `-Z / Y` sont des valeurs candidates, pas encore validées. La procédure complète Blender → FBX → Unity 6000.4.11f1 et ses critères d'acceptation sont documentés dans `Documentation/02_DOMAINS/ART_AND_ASSET_PIPELINE.md`. Chaque profil doit réussir séparément avant d'être déclaré validé.

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
- Les profils FBX doivent être vérifiés individuellement dans le Unity 6 du projet selon la procédure de round-trip.
- Les clips d'animation dépendent de la configuration Action/NLA du fichier Blender.
- Les fonctions LOD/collision/sockets sont des helpers de conventions et d'inspection, pas des générateurs automatiques complets.
- L'import/synchronisation Unity reste sous responsabilité TOOLS.

# REBOOT-001 — Starter kit d'implémentation

Ce kit ajoute une fondation de données sans remplacer les anciens systèmes du projet.

## 1. Contenu

- `Data/` : source canonique hors de `Assets/`.
- `Tools/generate_demo_maps.py` : génère quatre couches float32 déterministes.
- `Tools/validate_data.py` : valide les définitions, références et grilles avec Python standard.
- `Assets/Project/Runtime/Data/` : DTO et chargeur JSON/grilles Runtime.
- `Assets/Project/Runtime/World/Environment/EnvironmentGrid.cs` : échantillonnage bilinéaire en coordonnées X/Z.
- `Assets/Project/Editor/DataPipeline/` : commandes de copie vers StreamingAssets et de génération des projections ScriptableObject.
- `Assets/Project/Runtime/Data/GeneratedDefinitionAssets.cs` : types de projections générées (mondes, biomes, espèces).

## 2. Installation dans ton projet Unity

1. Fais une sauvegarde/commit de ton projet actuel.
2. Décompresse le contenu du kit dans la racine de ton projet Unity, au même niveau que `Assets/`, `Packages/` et `ProjectSettings/`.
3. Vérifie que tu obtiens `Data/`, `Tools/` et les fichiers sous `Assets/Project/...`.
4. Ne supprime et ne déplace aucun ancien script, prefab, asset ou `.meta`.
5. Laisse Unity terminer la compilation.

Si des dossiers `Assets/Project/Runtime/Data`, `Assets/Project/Runtime/World/Environment` ou `Assets/Project/Editor/DataPipeline` existent déjà, compare les fichiers avant de fusionner ; ne remplace pas silencieusement un fichier existant.

## 3. Générer et valider les cartes

Depuis la racine du projet :

```bash
python Tools/generate_demo_maps.py --output Data
python Tools/validate_data.py --data Data
```

Résultat attendu :

```text
PASS: data definitions, references and map grids are valid
```

Pour générer une résolution de test différente :

```bash
python Tools/generate_demo_maps.py --output Data --width 128 --height 128 --seed 481516
python Tools/validate_data.py --data Data
```

Les valeurs du dataset sont uniquement des données techniques de démonstration. Les plages de biomes et préférences d'habitat ne sont pas des décisions gameplay finales.

## 4. Synchroniser les données dans Unity

Après compilation, utilise le menu Unity :

`Tools > Pokemon Data > Sync Data to StreamingAssets`

La commande copie le dossier canonique `Data/` vers :

`Assets/StreamingAssets/PokemonData/`

Ce dossier de destination est réservé aux copies générées par cette commande. Ne stocke pas de données éditées manuellement dedans. Relance la commande après toute modification des sources `Data/`.

## 5. Générer les projections ScriptableObject (optionnel)

Menu Unity :

`Tools > Pokemon Data > Generate ScriptableObject Projections`

La commande lit `Data/worlds/`, `Data/biomes/` et `Data/pokemon/`, puis crée/met à jour les projections dans :

- `Assets/Project/Generated/Data/Worlds/`
- `Assets/Project/Generated/Data/Biomes/`
- `Assets/Project/Generated/Data/Pokemon/`

Ces `.asset` sont des artefacts dérivés. **Ne les édite pas à la main** : toute modification doit être faite dans le JSON puis régénérée. La commande n'efface pas automatiquement les projections devenues orphelines ; leur suppression doit être auditée séparément.

## 6. Utiliser le chargeur Runtime

Le chargement se fait depuis un `MonoBehaviour` ou un service qui possède une coroutine :

```csharp
using System.Collections;
using Pokemon.Project.Data;
using Pokemon.Project.World.Environment;
using UnityEngine;

public class DataLoadingExample : MonoBehaviour
{
    private IEnumerator Start()
    {
        WorldDefinitionData world = null;
        string error = null;

        yield return WorldDataRuntimeLoader.LoadJson<WorldDefinitionData>(
            "worlds/world_meadow_01.json",
            value => world = value,
            message => error = message);

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
            yield break;
        }

        Debug.Log($"Loaded world: {world.id} ({world.mapResolution.width}x{world.mapResolution.height})");

        EnvironmentGrid humidity = null;
        yield return WorldDataRuntimeLoader.LoadGrid(
            "maps/world_meadow_01/humidity/manifest.json",
            value => humidity = value,
            message => error = message);

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(error);
            yield break;
        }

        if (humidity.TrySampleBilinear(0f, 0f, out float humidityAtOrigin))
            Debug.Log($"Humidity at world origin: {humidityAtOrigin}");
    }
}
```

Cet exemple est un usage de l'API, pas un composant à ajouter automatiquement à une scène. Ne l'attache pas à un GameObject tant que tu n'as pas besoin de tester le chargement.

## 7. Limites connues de cette première tranche

- Seules les grilles continues `float32` little-endian à un canal sont prises en charge par le chargeur initial.
- Les grilles catégorielles `uint16`, masques, compression et streaming par chunks ne sont pas encore implémentés.
- Le validateur Python vérifie les invariants de ce starter kit ; il n'est pas un moteur JSON Schema complet.
- Les DTO utilisent `JsonUtility`, donc des classes simples avec champs publics et tableaux. Pas de dictionnaires, polymorphisme ou propriétés complexes.
- Le choix de terrain Unity Terrain vs mesh n'est pas fait.
- Les JSON de biomes et espèces sont des exemples de contrat, pas une définition exhaustive des domaines WORLD/ECO/POK.
- Les projections ScriptableObject sont fournies pour les mondes, biomes et espèces ; elles restent optionnelles, dérivées et non éditables comme source de vérité.
- Le kit n'a pas été compilé dans l'éditeur Unity : l'environnement de génération ne contient pas ton installation Unity.

## 8. Références Unity consultées

- `JsonUtility.FromJson` : la désérialisation Unity cible des classes/structures simples compatibles avec le sérialiseur Unity ; les classes dérivées de `UnityEngine.Object` ne sont pas prises en charge par `FromJson`.
- `Application.streamingAssetsPath` : chemin de StreamingAssets ; Android/WebGL nécessitent une approche URL/UnityWebRequest plutôt qu'un accès synchrone `System.IO`.

Documentation officielle :
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/JsonUtility.FromJson.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-streamingAssetsPath.html

## 9. Git

Le Holy Grail exige un commit/push du plan avant le commit/push d'implémentation sur la même branche. Le dépôt `.git` et le remote n'étaient pas accessibles dans l'environnement de création du kit. Le plan est fourni séparément dans `REBOOT-001_IMPLEMENTATION_PLAN.md` ; ajoute-le et pousse-le avant d'intégrer le code, puis commit/push l'implémentation avec le footer `Agent: ARCH` et le Work-Package `REBOOT-001`.

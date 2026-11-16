# REBOOT-001 — Plan d'implémentation initiale

- Agent : ARCH
- Work package : REBOOT-001
- Statut : plan d'implémentation préparé ; push Git non effectué (aucun dépôt Git distant/local fourni dans l'environnement de travail)
- Base : `ARCH-REBOOT-001_PIPELINE_ARCHITECTURE_PROPOSAL.md` et Holy Grail du snapshot `Pokemon-main (2).zip`

## Objectif de cette première tranche

Mettre en place une fondation non destructive permettant de définir les données canoniques hors de `Assets/`, de générer des cartes numériques hybrides de démonstration, de les valider indépendamment de Unity, puis de synchroniser les sources vers `StreamingAssets` pour leur chargement runtime.

## Inclus

1. Structure canonique `Data/`.
2. Contrats JSON versionnés de départ pour monde, biome, espèce Pokémon et manifeste de couche.
3. Quatre grilles de démonstration déterministes : hauteur, humidité, température, densité végétale.
4. Format de grille continu `float32`, little-endian, un canal.
5. Validateur Python standard-library : structure, références, dimensions, taille binaire, plage et valeurs finies.
6. DTO C# sérialisables et indépendants des `UnityEngine.Object`.
7. Lecteur runtime asynchrone basé sur `UnityWebRequest`, compatible avec le mode d'accès URL de `StreamingAssets`.
8. Commande Editor idempotente de synchronisation de `Data/` vers `Assets/StreamingAssets/PokemonData/`.
9. Projections ScriptableObject optionnelles et régénérables pour mondes, biomes et espèces, dans `Assets/Project/Generated/Data/`.
10. Documentation d'installation et limites connues.

## Exclusions explicites

- Aucun remplacement ou suppression des anciens systèmes Runtime.
- Aucun déplacement d'assets existants ni de `.meta`.
- Aucun choix définitif entre Unity Terrain et mesh terrain.
- Aucune classification finale de biomes ni règle écologique définitive.
- Aucun spawn, comportement ou simulation Pokémon.
- Aucun ScriptableObject n'est une source canonique ou une donnée à éditer manuellement. Un menu Editor optionnel produit des projections régénérables depuis les JSON ; le chargement JSON Runtime reste disponible.
- Aucun pipeline Blender ou changement d'assets artistiques dans cette tranche.

## Dépendances et contraintes

- Unity observé dans le ZIP : `6000.4.11f1`.
- `JsonUtility` impose des classes/structures simples, champs supportés et objet JSON racine ; les DTO utilisent des tableaux et champs publics, sans dictionnaires ni polymorphisme.
- Les données runtime sont synchronisées dans `StreamingAssets`. Sur Android/WebGL, les fichiers ne doivent pas être lus via `System.IO`; le lecteur utilise donc `UnityWebRequest`.
- La validation Python utilise uniquement la bibliothèque standard et ne prétend pas implémenter la totalité de JSON Schema.

## Critères d'acceptation

- `python Tools/generate_demo_maps.py --output Data` génère les quatre couches de manière reproductible.
- `python Tools/validate_data.py --data Data` retourne exit code 0 pour le dataset fourni.
- Modifier la taille d'une grille sans modifier le manifeste doit provoquer une erreur.
- Une valeur NaN/Infinity ou hors de la plage déclarée doit provoquer une erreur.
- La commande Unity `Tools/Pokemon Data/Sync Data to StreamingAssets` copie les données sans modifier les sources canoniques.
- Le Runtime peut charger un JSON et une grille binaire depuis `StreamingAssets` via callbacks/coroutines.
- Aucun fichier Runtime ne référence `UnityEditor`.

## Vérifications prévues

- Vérification statique des scripts Python par `py_compile`.
- Tests du générateur et du validateur sur le dataset de démonstration.
- Vérification de reproductibilité des octets des grilles.
- Inspection statique des scripts C# et de leurs dépendances.
- Compilation et test Play Mode à effectuer dans le projet Unity local : impossible dans l'environnement de génération de ce livrable.

## Documentation à mettre à jour après validation

- `Documentation/01_ARCHITECTURE/DATA_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md`
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md`
- `Documentation/02_DOMAINS/WORLD_AND_BIOMES.md`
- `Documentation/02_DOMAINS/TERRAIN_AND_RENDERING.md`
- `Documentation/04_AI_INSTRUCTIONS/instructions/unity-csharp.instructions.md`

Ces mises à jour ne sont pas appliquées dans cette tranche : elles doivent refléter les choix et résultats réellement validés, pas une proposition non testée.

## Workflow Git

Le Holy Grail exige un commit et push du plan avant l'implémentation, puis un commit/push d'implémentation sur la même branche. L'environnement ne contient pas le dépôt `.git` ni ses remotes. Le plan est livré séparément pour permettre au propriétaire de l'ajouter et de le pousser avant d'intégrer les scripts. Ne pas inventer de hash ou prétendre qu'un push a été fait.

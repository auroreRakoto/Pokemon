# NEXT HANDOFF

## 1. Rapport produit
Nom : `CODE_ARCHITECTURE_COMPLIANCE_REPORT.md`

## 2. Agent suivant
Agent : `QA`

## 3. Documents à lui transmettre
- `CODE_ARCHITECTURE_COMPLIANCE_REPORT.md`
- `Documentation/06_AUDIT/CODE_FUNCTION_OWNERSHIP_INVENTORY.md`
- `Documentation/01_ARCHITECTURE/MODULE_BOUNDARIES.md`
- `Documentation/01_ARCHITECTURE/INTERFACE_CONTRACTS.md`
- `Documentation/01_ARCHITECTURE/UNITY_ARCHITECTURE.md`
- `Documentation/02_DOMAINS/POKEMON_SIMULATION.md`
- `Documentation/02_DOMAINS/ECOLOGY_SIMULATION.md`
- Le projet Unity complet fourni dans `Pokemon-main_ARCHITECTURE_COMPLIANT.zip`

## 4. Travail attendu du prochain agent
- Ouvrir le projet avec Unity `6000.4.11f1` ou la version exacte déclarée dans `ProjectSettings/ProjectVersion.txt`.
- Laisser Unity réimporter les scripts, `.meta`, prefabs, ScriptableObjects et la scène Meadow.
- Compiler et relever toutes les erreurs/avertissements pertinents; corriger les erreurs sans réintroduire de dépendances contraires au Holy Grail.
- Vérifier les références Inspector du `PokemonSpawnManager`, `EcosystemLogger`, `EcosystemDataCollector`, `FruitBushManager`, `ResourceManager`, `WorldBiomeGenerator` et `GrassVegetationSpawner`.
- Tester la génération du monde, le placement des ressources, le spawn des Caterpie/Oddish, le déplacement, le foraging, le cycle jour/nuit, la mortalité et la reproduction.
- Vérifier la création des visuels de fruits et la mise à jour des statistiques/CSV.
- Vérifier les prefabs Caterpie/Oddish/Pidgey et les nouveaux `PokemonPopulationProfile`.
- Vérifier le choix de scène dans Build Settings; `SampleScene.unity` est encore la scène configurée, Meadow n'a pas été promue automatiquement.
- Produire `QA_CODE_COMPLIANCE_REVIEW.md` avec PASS / FAIL / BLOCKED, versions, étapes, preuves et limitations. Ne jamais déclarer un test exécuté sans preuve Unity.

## 5. Livrable attendu
Nom : `QA_CODE_COMPLIANCE_REVIEW.md`

## 6. Instructions au propriétaire
1. Télécharger `Pokemon-main_ARCHITECTURE_COMPLIANT.zip`.
2. Faire une sauvegarde/branche du projet actuel avant remplacement.
3. Décompresser et ouvrir le dossier `Pokemon-main` dans Unity.
4. Attendre la fin de l'import et de la compilation.
5. Ouvrir `Assets/Project/Content/Scenes/Worlds/Meadow.unity` et lancer les tests demandés.
6. Transmettre le projet, le rapport de conformité et l'inventaire des fonctions à l'agent QA.
7. Récupérer `QA_CODE_COMPLIANCE_REVIEW.md`; ne considérer la conformité finale comme validée qu'après les PASS Unity et Play Mode.

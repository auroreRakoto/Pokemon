# MOD-001-E — Placement des assets environnementaux optionnel

- **Statut:** PROPOSED
- **Owner module:** WORLD / Environment
- **Agent:** WORLD
- **Dépendances:** MOD-001-B validé; MOD-001-C et MOD-001-D validés.
- **Objectif:** n'instancier aucun asset environnemental quand le module est désactivé; quand activé, ne dépendre que des données réellement requises par la politique de placement.

## Scope inclus
- Brancher le résultat d'activation sur `GenerateEnvironmentPlacement`.
- Ne créer la racine `WORLD-011_GeneratedEnvironment` que si le module est actif.
- Ne charger/valider la configuration ni exiger le catalogue si le module est désactivé.
- Introduire une distinction explicite entre règles de placement nécessitant un biome et règles génériques basées sur World Core (eau, pente, altitude, humidité, température).
- Refuser proprement une règle biome si la classification est absente; ne jamais convertir `None` en Plains.
- Conserver déterminisme, seed, filtres, limites et contraintes WORLD-011-A; scope de contenu reste Plains/Forest conformément à la décision WORLD-011-A.

## Fichiers candidats
- `Environment/SphericalEnvironmentPlacementGenerator.cs`
- contrats/config JSON `SphericalEnvironmentPlacementJson` et `SphericalEnvironmentPlacementConfig`
- `Inspection/SphericalBiomeInspectionBootstrap.cs` ou futur point de composition.
- tests de placement existants et tests d'indépendance avec/sans assets.

## Exclusions
- Pas d'extension du scope de contenu à Desert/Mountains.
- Pas de changement de pivots, échelles, bounds ou prefabs (sauf défaut bloquant constaté et autorisé).
- Pas de streaming, canopy complète ou optimisation avancée.
- Pas de dépendance au joueur, à sa caméra ou au renderer.

## Contrats et invariants
- Assets désactivés = zéro instance et zéro racine générée.
- Absence de catalogue quand assets désactivés = aucune erreur.
- Placement activé ne modifie jamais le field set.
- Les règles biome requièrent la classification; les règles génériques ne la requièrent pas.
- Même seed, field set, catalogue et profil = mêmes placements.
- L'échec du placement bloque uniquement EnvironmentAssets.

## Preview indépendante
- Preview placement sans joueur ni caméra, avec World Core et les providers nécessaires.
- Modes sans assets, assets génériques et assets biome-dépendants.
- Capture des compteurs candidats/placés/rejetés.

## Tests / critères d'acceptation
- Module off: aucune racine et aucune instance.
- Module on + catalogue absent: rapport Blocked, aucune exception non gérée, autres modules actifs.
- Module on + règles génériques: fonctionne sans classification biome.
- Module on + règle biome sans classification: blocage précis.
- Déterminisme et limites globales/espacement conservés.
- Tests EditMode/PlayMode et preview validés par le propriétaire.

## Risques
- `SphericalEnvironmentPlacementGenerator.Generate` appelle toujours `SampleBiome`.
- Le document de placement possède `ResolvedRule.Biome`; il faut vérifier le schéma et les règles réelles avant d'ajouter le mode générique.
- Le contexte de validation MOD-001-B a déjà un booléen `environmentPlacementRequiresBiomes`; il devra être alimenté par l'analyse réelle du document résolu, pas une valeur codée en dur.

## Handoff
- Commit proposé: `feat(world): make environment asset placement optional`
- Footer: `Work-Package: MOD-001-E` / `Agent: WORLD`

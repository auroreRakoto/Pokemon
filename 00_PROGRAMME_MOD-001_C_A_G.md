# MOD-001 — Programme de découplage des modules

- **Programme parent:** MOD-001_OPTIONAL_WORLD_MODULE_COMPOSITION
- **Statut de ce document:** PROPOSED — soumis à validation du propriétaire
- **Principe confirmé:** sélection des modules à la création du monde uniquement; configuration immuable pendant la session.
- **Politique d'échec confirmée:** une dépendance manquante bloque uniquement le module optionnel concerné; World Core invalide bloque la création du monde.
- **Norme C#:** maximum 10 fonctions par fichier, 50 lignes logiques par fonction, commentaire descriptif au-dessus de chaque fonction.
- **Ordre d'exécution:** un seul incrément implémenté à la fois; tests Unity et retour propriétaire avant l'incrément suivant.

## Source et limites de l'audit

Base de code examinée: `Assets.zip` fourni dans la conversation, complété par les dernières livraisons MOD-001-A et MOD-001-B générées ensuite. L'archive `Assets.zip` est un snapshot de sources, pas le working tree Git courant.

Fichiers directement examinés:
- `SphericalGeographicFieldSet.cs`
- `SphericalGeographicFieldGenerator.cs`
- `SphericalBiomeInspectionBootstrap.cs`
- `SphericalEnvironmentPlacementGenerator.cs`
- `SphericalGroundProbe.cs`
- `SphericalPlayerController.cs`
- `SphericalBiomeSurfaceMeshGenerator.cs`
- `SphericalPlanetStudioGenerator.cs`
- `SphericalWorldModuleOptions.cs`
- `SphericalWorldModuleValidator.cs`

Limite: l'archive `Assets.zip` ne contient pas les documents gouvernance/architecture complets ni l'état courant de Git. Les présents plans détaillent les incréments techniques de MOD-001 déjà approuvé; ils ne prétendent pas remplacer ni réviser les documents d'autorité absents. Avant chaque implémentation, le dernier snapshot de code et les décisions/contrats courants devront être comparés.

## Séquence proposée

| Incrément | Objectif | Dépendance | Livrable de validation |
|---|---|---|---|
| MOD-001-C | Séparer les données World Core de la classification des biomes | MOD-001-B | Tests géographiques avec/sans biomes |
| MOD-001-D | Rendre la composition et la présentation du terrain optionnelles | C | Preview World Core seul, terrain neutre, rendu biomes |
| MOD-001-E | Rendre le placement d'assets réellement optionnel | D | Preview avec/sans assets, déterminisme |
| MOD-001-F | Rendre le joueur et sa caméra réellement optionnels | D | Preview observation et preview jouable |
| MOD-001-G | Définir l'intégration Pokémon sans imposer Pokémon au monde | C, D, F et runtime Pokémon existant/approuvé | Contrats et tests d'indépendance; implémentation seulement quand le module Pokémon existe |

Chaque incrément conserve les invariants:
- une seule génération des données géographiques autoritaires par monde;
- les modules de présentation et de gameplay ne régénèrent pas World Core;
- aucun module optionnel ne dépend du joueur, de la caméra ou du renderer, sauf dépendance explicitement validée;
- aucun objet factice pour masquer une capacité absente;
- opérations Generate/Clear idempotentes;
- preview indépendant du monde jouable complet.

# MOD-001-D — Composition de monde et présentation du terrain optionnelles

- **Statut:** PROPOSED
- **Owner module:** WORLD / Presentation / World Composition
- **Agent:** WORLD
- **Dépendances:** MOD-001-C validé; MOD-001-B validateur disponible.
- **Objectif:** composer un monde sans terrain rendu, avec terrain neutre opaque, ou avec terrain et présentation par biomes.

## Scope inclus
- Introduire un point de composition runtime explicite qui lit une configuration validée avant de créer des objets.
- Faire générer World Core une seule fois; transmettre le même field set aux modules actifs.
- Si terrain 3D désactivé: ne créer aucun mesh, renderer, matériau ou collider de présentation; conserver les données et requêtes géographiques.
- Si terrain activé et biomes désactivés: utiliser un matériau URP neutre/classique opaque, jamais un shader manquant ou transparent.
- Si présentation biomes désactivée: conserver terrain neutre même si la classification existe pour d'autres consommateurs.
- Garder le bootstrap d'inspection comme fixture/route de preview, mais extraire les responsabilités nécessaires au point de composition sans transformer ce bootstrap temporaire en système de jeu global.

## Fichiers candidats
- `Inspection/SphericalBiomeInspectionBootstrap.cs`
- `Presentation/SphericalBiomeSurfaceMeshGenerator.cs`
- `Presentation` terrain mesh/material construction existants
- `Studio/SphericalPlanetStudioSettings.cs` et `Studio/SphericalWorldProfile.cs` uniquement si un adaptateur de composition manque.
- Tests Editor/PlayMode du rendu sphérique et nouveau test de composition.

## Exclusions
- Pas de placement d'assets (MOD-001-E).
- Pas de création/suppression du joueur/caméra (MOD-001-F).
- Pas de modification des données World Core ou classification (MOD-001-C).
- Pas de refonte URP/shader ni nouvelle dépendance de package.
- Pas d'assemblies Unity supplémentaires.

## Contrats et invariants
- La présentation consomme le field set; elle n'en est jamais propriétaire.
- Désactiver le terrain n'empêche ni la génération ni les requêtes World Core.
- Terrain sans biome = matériau opaque neutre explicite.
- La désactivation du rendu biome ne doit pas désactiver la classification si un autre module la demande.
- Le rapport MOD-001-B est la seule source de décisions d'activation.

## Preview indépendante
- Route A: World Core seul, sans GameObjects de présentation.
- Route B: terrain neutre opaque.
- Route C: terrain + présentation biome.
- Les trois routes utilisent le même profil/seed et vérifient l'identité des champs.

## Tests / critères d'acceptation
- Aucun objet terrain/presentation créé si terrain désactivé.
- Terrain neutre visible et opaque quand terrain activé mais rendu biome désactivé.
- Terrain biome inchangé en mode historique.
- Les modes sans joueur et sans assets ne sont pas affectés.
- Generate/Clear répétés ne laissent pas de racines ni meshes dupliqués.
- Tests EditMode pour la décision de composition, tests PlayMode/preview par le propriétaire.

## Risques
- Le bootstrap courant appelle `BuildPlanet()` sans branche optionnelle.
- La résolution actuelle du matériau biome échoue si mapping/shader absent; cette erreur ne doit survenir que si la présentation biome est active.
- Le preview Planet Studio et le bootstrap d'inspection ont des responsabilités distinctes à préserver.

## Handoff
- Commit proposé: `feat(world): make spherical terrain presentation optional`
- Footer: `Work-Package: MOD-001-D` / `Agent: WORLD`

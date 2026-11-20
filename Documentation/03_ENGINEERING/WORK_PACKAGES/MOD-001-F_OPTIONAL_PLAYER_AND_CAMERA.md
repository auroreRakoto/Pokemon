# MOD-001-F — Présence du joueur et caméra optionnelles

- **Statut:** PROPOSED
- **Owner module:** WORLD / Movement / World Composition
- **Agent:** WORLD
- **Dépendances:** MOD-001-B et MOD-001-D validés; MOD-001-C validé pour garantir les queries sans biomes.
- **Objectif:** créer un monde d'observation sans player, contrôleur, input adapter ni caméra joueur, tout en conservant le World Core.

## Scope inclus
- Brancher `playerEnabled` sur le point de composition.
- Quand désactivé, ne créer aucun player, contrôleur, caméra troisième personne, AudioListener ni overlay attaché au joueur.
- Quand activé, utiliser le provider de spawn validé et les contrats de déplacement sphérique existants.
- Séparer la recherche de spawn du rendu terrain; le spawn doit fonctionner avec le renderer désactivé.
- Définir le comportement sans biomes: spawn à partir de critères World Core génériques (terre, pente, altitude) ou refuser explicitement un `spawnBiome` spécifique. Le choix doit être validé avant implémentation.
- Préserver l'architecture réseau/dédié serveur existante: ce bootstrap d'inspection ne doit pas introduire d'autorité client ou de logique réseau nouvelle.

## Fichiers candidats
- `Inspection/SphericalBiomeInspectionBootstrap.cs`
- `Movement/SphericalGroundProbe.cs`
- `Movement/SphericalPlayerController.cs`
- `Movement/SphericalThirdPersonCamera.cs`
- tests de movement et tests PlayMode de composition.

## Exclusions
- Pas de refonte du contrôleur ni de la caméra.
- Pas de changement des paramètres de locomotion.
- Pas de dépendance du joueur au rendu terrain, aux assets ou aux Pokémon.
- Pas d'intégration réseau nouvelle.
- Pas de modification du gameplay Pokémon.

## Contrats et invariants
- Joueur désactivé = aucun objet joueur/caméra/input/overlay.
- Joueur activé = World Core + spawn provider + movement provider.
- Déplacement et probes restent valides sans classification des biomes.
- Le joueur ne devient jamais une dépendance du World Core, des assets ou de Pokémon.
- Mode observation peut exister sans caméra joueur; une caméra d'observation éventuelle est une capacité distincte et non incluse ici.

## Preview indépendante
- Preview observation sans joueur.
- Preview jouable avec joueur.
- Les deux utilisent la même génération World Core et le même seed.

## Tests / critères d'acceptation
- Player off: aucun GameObject/player component/camera/audio listener/input créé.
- Player on: contrôleur et caméra fonctionnent avec terrain rendu off.
- Player on + biomes off: spawn générique explicite fonctionne ou la configuration est bloquée avec diagnostic; aucun faux biome.
- Clear/Generate répétés ne laissent aucun objet joueur/caméra orphelin.
- EditMode/PlayMode et inspection manuelle validées par le propriétaire.

## Risques
- Le bootstrap courant trouve le spawn à partir de `spawnBiome` et construit toujours joueur + caméra.
- `SphericalGroundProbe` lit actuellement un biome; le découplage doit être fait en MOD-001-C avant ce WP.
- Le plan ne doit pas modifier les décisions réseau confirmées.

## Handoff
- Commit proposé: `feat(world): make spherical player presence optional`
- Footer: `Work-Package: MOD-001-F` / `Agent: WORLD`

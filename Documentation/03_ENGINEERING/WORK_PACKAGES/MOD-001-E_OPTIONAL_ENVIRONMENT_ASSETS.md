# MOD-001-E — Placement des assets environnementaux optionnel

- **Statut:** PROPOSED — validation du propriétaire requise.
- **Milestone:** A — plan uniquement; aucune implémentation autorisée avant commit/push et approbation.
- **Owner module:** WORLD
- **Dépendances:** MOD-001-B VALIDATED; MOD-001-C et MOD-001-D VALIDATED; résultat de l'audit WORLD-011-A et règles de placement actuellement approuvées.
- **Décisions applicables:** DEC-007, DEC-019, DEC-020, DEC-044, DEC-047, DEC-048 et DEC-049.
- **Norme de code:** maximum 10 fonctions par fichier; maximum 50 lignes logiques par fonction; commentaire descriptif au-dessus de chaque fonction.
- **Périmètre:** Faire du placement d'assets un consommateur optionnel de World Core, sans dépendance au terrain rendu, au player ou à la caméra.

## 1. Résultat attendu

Faire du placement d'assets un consommateur optionnel de World Core, sans dépendance au terrain rendu, au player ou à la caméra.

## 2. Scope inclus

- Brancher le rapport d'activation sur le générateur de placement et le point de composition.
- Module off: ne pas charger/valider le catalogue ni les règles de placement; ne créer aucune racine générée et aucune instance.
- Module on: vérifier le catalogue et la configuration avant toute instanciation; un défaut bloque uniquement EnvironmentAssets.
- Auditer le schéma actuel pour distinguer les règles génériques fondées sur World Core (eau, pente, altitude, humidité/température) des règles nécessitant explicitement un biome.
- Autoriser le placement générique sans classification biome; bloquer une règle explicitement biome-dépendante si la classification est absente.
- Préserver le déterminisme, le seed, les limites, les filtres et les règles d'espacement.
- Respecter WORLD-011-A: placement déterministe Plains/Forest uniquement, sans étendre le contenu à Desert/Mountains.

## 3. Exclusions

- Pas d'extension du contenu à Desert/Mountains.
- Pas de changement opportuniste des prefabs, pivots, échelles, bounds ou modèles; tout défaut est consigné séparément.
- Pas de canopy complète, streaming, pooling ou optimisation avancée.
- Pas de dépendance au player, à la caméra ou au renderer.
- Pas de nouvelle politique biome/générique implicite; toute règle est explicite dans les données.

## 4. Fichiers candidats

Les chemins ci-dessous sont des candidats identifiés dans le snapshot fourni. Ils doivent être confirmés contre le dernier working tree/snapshot du propriétaire avant le milestone d'implémentation.

- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementConfig.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementJson.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPrefabCatalog.cs`
- `point de composition (livré en D) et tests placement`

## 5. Contrats et invariants

- World Core reste l'unique propriétaire des champs géographiques autoritaires.
- Le même field set est partagé; les modules optionnels ne régénèrent ni ne mutent les données World Core.
- Le module absent ou bloqué ne doit pas arrêter les modules indépendants.
- Aucun fallback silencieux ni objet factice.
- Sélection des modules à la création du monde; aucune hot-toggle dans la session.
- Les défauts d'un module optionnel sont signalés localement; seul World Core invalide bloque la création.
- Les GUID, `.meta`, scènes, prefabs et données sérialisées sont préservés sauf migration explicitement prévue et auditée.
- DEC-007 et les décisions réseau existantes ne sont pas modifiées.

## 6. Preview indépendante

Preview d'assets autonome sans player, caméra ou renderer; trois cas: module off, règles génériques, règles biome.

## 7. Tests et critères d'acceptation

1. Assets off: aucune racine/instance et aucune lecture du catalogue/configuration.
2. Catalogue absent avec assets on: EnvironmentAssets est bloqué; World Core et autres modules restent actifs.
3. Règles génériques: placement possible sans classification biome.
4. Règle biome sans classification: diagnostic précis et zéro placement partiel.
5. Même seed/field set/catalogue/config: mêmes candidats et placements.
6. Les contraintes WORLD-011-A (Plains/Forest, déterminisme, transformations prefabs) restent respectées.
7. Clear/Generate répétés ne laissent pas d'objets résiduels.

### Critères d'acceptation

- [ ] Module off = zéro objet et aucune dépendance aux assets.
- [ ] Module on ne modifie jamais World Core.
- [ ] Placement générique ne requiert pas les biomes; règles biome le déclarent explicitement.
- [ ] Tests et preview indépendants passent.

## 8. Risques et points d'audit

Le générateur actuel appelle SampleBiome pour filtrer chaque candidat. Le booléen environmentPlacementRequiresBiomes du contexte MOD-001-B doit être dérivé du document résolu réel, pas d'une constante.

## 9. Rollback

- Conserver le chemin historique tant que le nouveau chemin n'a pas passé les tests de régression.
- Ne supprimer aucune API, scène, prefab ou asset avant audit des références et plan de migration.
- Les modifications de composition doivent pouvoir être désactivées/rétablies sans migration destructive.
- Toute modification de schéma JSON doit être versionnée et testée avec les profils historiques.

## 10. Documentation à mettre à jour après implémentation

- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` si une décision confirmée doit être enregistrée.
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md` et `INTERFACE_CONTRACTS.md` pour les contrats réellement modifiés.
- `Documentation/02_DOMAINS/WORLD_AND_BIOMES.md` et/ou `TERRAIN_AND_RENDERING.md` selon l'étape.
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` et `DOCUMENT_INDEX.md`.
- Rapport d'implémentation, plan de test et changelog.

## 11. Milestones et statut

- **Milestone A — Plan:** PROPOSED; à approuver, committer et pousser par le propriétaire.
- **Milestone B — Implémentation:** NOT STARTED; autorisé seulement après Milestone A poussé, confirmation du hash et approbation explicite de démarrer cette étape.
- **Validation:** Unity compilation, tests, preview et intégration sont NOT RUN tant que le propriétaire ne les exécute pas.

## 12. Commit proposé

```text
feat(world): make environment asset placement optional

Work-Package: MOD-001-E
Agent: WORLD
```

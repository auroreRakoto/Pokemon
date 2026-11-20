# MOD-001-F — Présence du joueur et caméra optionnelles

- **Statut:** PROPOSED — validation du propriétaire requise.
- **Milestone:** A — plan uniquement; aucune implémentation autorisée avant commit/push et approbation.
- **Owner module:** WORLD
- **Dépendances:** MOD-001-B VALIDATED; MOD-001-C et MOD-001-D VALIDATED; composition root audité.
- **Décisions applicables:** DEC-007, DEC-019, DEC-020, DEC-044, DEC-047, DEC-048 et DEC-049.
- **Norme de code:** maximum 10 fonctions par fichier; maximum 50 lignes logiques par fonction; commentaire descriptif au-dessus de chaque fonction.
- **Périmètre:** Créer des mondes d'observation/simulation sans player, input, contrôleur, caméra joueur, AudioListener ni HUD joueur, tout en permettant un player optionnel.

## 1. Résultat attendu

Créer des mondes d'observation/simulation sans player, input, contrôleur, caméra joueur, AudioListener ni HUD joueur, tout en permettant un player optionnel.

## 2. Scope inclus

- Brancher playerEnabled au point de composition et au rapport d'activation.
- Player off: ne créer ni player, contrôleur/motor, input adapter, caméra troisième personne, AudioListener ni overlay joueur.
- Player on: utiliser le provider de spawn et les contrats de mouvement existants; le spawn ne doit pas dépendre du renderer.
- Autoriser un spawn générique sans biome à partir de critères explicites World Core (classe terre/eau, pente, altitude et contraintes validées).
- Les règles de spawn qui exigent un biome sont bloquées si la classification est désactivée; aucune substitution implicite vers Plains.
- Conserver les paramètres de locomotion et la caméra existants; ne pas refondre le mouvement.
- Préserver DEC-007: aucune nouvelle autorité client ou logique réseau n'est introduite par l'inspection.

## 3. Exclusions

- Pas de refonte du contrôleur, du motor ou de la caméra.
- Pas de modification des paramètres de locomotion, gravité, vitesse ou dimensions.
- Pas de dépendance du player au terrain rendu, aux assets environnementaux ou aux Pokémon.
- Pas de caméra d'observation autonome dans ce WP; si nécessaire, capacité séparée.
- Pas de nouvelle intégration réseau.

## 4. Fichiers candidats

Les chemins ci-dessous sont des candidats identifiés dans le snapshot fourni. Ils doivent être confirmés contre le dernier working tree/snapshot du propriétaire avant le milestone d'implémentation.

- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs ou successeur`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalGroundProbe.cs (si reliquat après C)`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerController.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerMotor.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalThirdPersonCamera.cs`
- `tests movement et composition PlayMode`

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

Preview observation sans player/caméra joueur et preview jouable avec player. Même profil/seed/field set.

## 7. Tests et critères d'acceptation

1. Player off: aucun player, controller, input, player camera, AudioListener ou HUD joueur.
2. Player on + terrain off: player fonctionne à partir des queries World Core, sans renderer.
3. Player on + biomes off: spawn générique explicite fonctionne.
4. Spawn biome-spécifique + biomes off: player module localement bloqué avec diagnostic.
5. Provider spawn ou mouvement manquant: seul Player est bloqué.
6. Generate/Clear répétés ne laissent aucun player/caméra/input orphelin.
7. Les 10 tests de mouvement existants et les tests de composition passent.

### Critères d'acceptation

- [ ] Le monde peut être créé sans aucun objet joueur/caméra.
- [ ] Le player activé fonctionne indépendamment du rendu terrain et des assets.
- [ ] Le spawn générique fonctionne sans biomes.
- [ ] Les previews observation et jouable sont indépendantes.

## 8. Risques et points d'audit

Le bootstrap actuel choisit un spawnBiome puis crée player, caméra et HUD dans le même flux. Le player controller utilise SphericalGroundProbe; l'indépendance biome doit être garantie en C avant F.

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
feat(world): make spherical player presence optional

Work-Package: MOD-001-F
Agent: WORLD
```

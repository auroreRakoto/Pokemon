# MOD-001-D — Composition du monde et présentation terrain optionnelles

- **Statut:** PROPOSED — validation du propriétaire requise.
- **Milestone:** A — plan uniquement; aucune implémentation autorisée avant commit/push et approbation.
- **Owner module:** WORLD
- **Dépendances:** MOD-001-C VALIDATED; MOD-001-B VALIDATED; dernière version des settings/profils MOD-001-A et bootstrap actuel.
- **Décisions applicables:** DEC-007, DEC-019, DEC-020, DEC-044, DEC-047, DEC-048 et DEC-049.
- **Norme de code:** maximum 10 fonctions par fichier; maximum 50 lignes logiques par fonction; commentaire descriptif au-dessus de chaque fonction.
- **Périmètre:** Composer World Core avec zéro présentation 3D, un terrain neutre opaque, ou le terrain avec présentation biome, selon le profil.

## 1. Résultat attendu

Composer World Core avec zéro présentation 3D, un terrain neutre opaque, ou le terrain avec présentation biome, selon le profil.

## 2. Scope inclus

- Introduire/adapter un point de composition explicite qui valide la configuration avant de créer les objets de présentation.
- Créer World Core une seule fois et transmettre le même field set aux présentations actives.
- Terrain presentation off: ne créer aucun mesh planétaire, renderer, matériau, collider de présentation ni racine visuelle de planète; garder les données et requêtes.
- Terrain presentation on + biome ground presentation off: créer un terrain opaque URP neutre/classique.
- Biome ground presentation on: utiliser le shader opaque partagé DEC-048 et les données de classification disponibles.
- Déplacer les responsabilités nécessaires hors de SphericalBiomeInspectionBootstrap sans transformer le bootstrap de test en service global ni introduire de service locator.
- Conserver l'outil Editor explicite Generate/Apply-to-Scene exigé par DEC-019; la sélection runtime sans terrain ne supprime pas cette capacité Editor.

## 3. Exclusions

- Ne pas modifier les champs World Core ni les règles de classification (MOD-001-C).
- Ne pas placer les assets environnementaux (MOD-001-E).
- Ne pas créer/supprimer le player ou sa caméra (MOD-001-F).
- Ne pas modifier les règles URP, remplacer le shader DEC-048 ni introduire de package.
- Pas de hot-toggle pendant une session.

## 4. Fichiers candidats

Les chemins ci-dessous sont des candidats identifiés dans le snapshot fourni. Ils doivent être confirmés contre le dernier working tree/snapshot du propriétaire avant le milestone d'implémentation.

- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeSurfaceMeshGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetPresenter.cs`
- `matériau neutre et mapping biome existants (audit GUID/asset avant modification)`
- `tests de présentation/composition`

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

Trois previews indépendantes avec le même profil/seed: World Core seul; terrain neutre opaque; terrain biome. Les previews ne doivent pas exiger le player ou les assets.

## 7. Tests et critères d'acceptation

1. Terrain off: zéro GameObject de terrain, mesh, renderer, matériau ou collider de présentation.
2. Terrain neutre: rendu URP opaque visible sans dépendance à la classification biome.
3. Terrain biome: rendu existant conservé et opaque avec DEC-048.
4. Mapping/shader biome absent: pas d'erreur si la présentation biome est désactivée; module biome presentation localement bloqué si activé.
5. Les trois modes consomment le même field set et les mêmes champs géographiques.
6. Generate/Clear répétés ne laissent pas de racines, meshes ou matériaux dupliqués.
7. Editor Generate/Apply-to-Scene existant reste fonctionnel.

### Critères d'acceptation

- [ ] Les trois configurations sont indépendamment prévisualisables.
- [ ] Terrain off ne crée aucun objet de présentation.
- [ ] Terrain neutre est opaque et n'exige pas de biomes.
- [ ] Le chemin biome existant est conservé.
- [ ] Les tests EditMode/PlayMode et les inspections visuelles passent.

## 8. Risques et points d'audit

Le bootstrap actuel génère le field set, construit la présentation, choisit le spawn biome, place les assets et crée player/caméra. Les responsabilités doivent être séparées par propriétaire, sans retirer par accident les capacités réservées à E/F.

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
feat(world): make spherical terrain presentation optional

Work-Package: MOD-001-D
Agent: WORLD
```

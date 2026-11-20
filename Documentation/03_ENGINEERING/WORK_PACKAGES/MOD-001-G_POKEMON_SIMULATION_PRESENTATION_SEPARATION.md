# MOD-001-G — Simulation Pokémon indépendante de la présentation visuelle

- **Statut:** PROPOSED — validation du propriétaire requise.
- **Milestone:** A — plan uniquement; aucune implémentation autorisée avant commit/push et approbation.
- **Owner module:** POK / ARCH; WORLD fournit le contrat spatial
- **Dépendances:** MOD-001-C, D, E et F VALIDATED; archive récente contenant le runtime Pokémon réel; contrats POK/ECO/WORLD/NETWORK et décision de l'owner sur les contrats manquants.
- **Décisions applicables:** DEC-007, DEC-019, DEC-020, DEC-044, DEC-047, DEC-048 et DEC-049.
- **Norme de code:** maximum 10 fonctions par fichier; maximum 50 lignes logiques par fonction; commentaire descriptif au-dessus de chaque fonction.
- **Périmètre:** Séparer simulation/état/comportements Pokémon des prefabs, meshes, matériaux et autres assets visuels, afin que les Pokémon puissent exister et évoluer sans player ni présentation Pokémon.

## 1. Résultat attendu

Séparer simulation/état/comportements Pokémon des prefabs, meshes, matériaux et autres assets visuels, afin que les Pokémon puissent exister et évoluer sans player ni présentation Pokémon.

## 2. Scope inclus

- Phase G0 obligatoire: audit du runtime Pokémon fourni, callers, assets, prefabs, scènes, données sérialisées, tests, contrats de placement/mouvement et propriétaires des états mutables.
- Définir le contrat entre simulation Pokémon et adaptateur de présentation; la simulation ne doit pas instancier ou exiger un prefab.
- Définir les modes: Pokémon off; simulation active sans présentation; simulation + présentation.
- Définir le provider de placement World Core et les dépendances habitat/biomes explicitement déclarées.
- Vérifier l'indépendance du player et de la caméra; l'observation autonome ne requiert aucun player.
- Vérifier la séparation autorité serveur selon DEC-007 avant toute exécution partagée.
- Phase G1 d'implémentation ultérieure uniquement après audit G0, validation des contrats et approbation d'un WP POK dédié.

## 3. Exclusions

- Aucun placeholder Pokémon, runtime fictif ou réintroduction de code supprimé/archivé.
- Ne pas coder à partir des seuls noms de classes ou de la documentation de domaine.
- Pas de refonte de PokemonAgent sans WP POK/ARCH séparé et tests de régression.
- Pas de nouvelle décision sur serveur, protocole, combat, reproduction, besoins ou écologie.
- Pas de dépendance du runtime Pokémon au player, à la caméra, au renderer ou aux assets environnementaux.
- Ne pas assimiler absence d'assets visuels à absence de simulation.

## 4. Fichiers candidats

Les chemins ci-dessous sont des candidats identifiés dans le snapshot fourni. Ils doivent être confirmés contre le dernier working tree/snapshot du propriétaire avant le milestone d'implémentation.

- `Phase G0: documents POK/ECO/WORLD/NETWORK actuels et tous les fichiers runtime Pokémon réels fournis ultérieurement; aucun fichier Runtime n'est autorisé à changer dans G0.`
- `Phase G1 candidate après audit: contrats simulation/presentation, composition Pokémon, factory/adapters et tests; chemins exacts à définir à partir du snapshot réel.`

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

Preview simulation autonome sans player/caméra et sans prefab visuel; inspection des états/données via diagnostics ou outil de test. La preview ne doit pas inventer de runtime absent.

## 7. Tests et critères d'acceptation

1. G0: inventaire complet des types, appels, assets, GUID, scènes/prefabs, sérialisations, tests et responsabilités.
2. G0: diagramme de dépendances prouvant qu'aucun état Pokémon n'est possédé par la présentation.
3. G0: contrat de placement World Core et dépendances biome/habitat explicités.
4. G0: matrice client/serveur conforme à DEC-007.
5. G1: simulation active sans assets visuels, player ou caméra.
6. G1: Pokémon off = zéro simulation, instance, abonnement ou présentation.
7. G1: même seed et données d'entrée produisent les mêmes décisions déterministes lorsque applicable.
8. G1: tests serveur et contrats autoritaires passés.

### Critères d'acceptation

- [ ] G0 est accepté uniquement avec runtime source réel, audit des consommateurs et contrats sans contradiction.
- [ ] G0 ne modifie aucun code.
- [ ] G1 n'est pas autorisé par ce milestone; il exige un work package POK/ARCH dédié et une approbation séparée.
- [ ] Le système peut conceptuellement fonctionner sans assets visuels, sans joueur et sans caméra, sans violer DEC-007.

## 8. Risques et points d'audit

Les snapshots Assets.zip et Pokemon-clean.zip disponibles pour ce travail ne contiennent pas le runtime Pokémon annoncé par certains documents de domaine. Ces documents décrivent des responsabilités mais ne prouvent pas les fichiers/API actuels. Le WP G est donc bloqué pour l'implémentation tant qu'un snapshot actuel du Runtime Pokémon et les contrats correspondants ne sont pas fournis.

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
docs(pokemon): plan asset-independent pokemon simulation audit

Work-Package: MOD-001-G
Agent: POK / ARCH; WORLD fournit le contrat spatial
```

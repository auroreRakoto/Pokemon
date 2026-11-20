# MOD-001-G — Test plan: Simulation Pokémon indépendante de la présentation visuelle

- **Statut:** PROPOSED — tests à exécuter après implémentation.
- **Owner:** POK / ARCH; WORLD fournit le contrat spatial
- **WP:** `MOD-001-G_G_PLAN.md`
- **Exécution locale:** Unity Test Runner; les previews et vérifications PlayMode sont exécutées par le propriétaire.
- **Déterminisme:** seeds/profils fixes; les comparaisons doivent porter sur les données produites et les objets créés, pas seulement sur l'absence d'exception.

## Cas requis

1. G0: inventaire complet des types, appels, assets, GUID, scènes/prefabs, sérialisations, tests et responsabilités.
2. G0: diagramme de dépendances prouvant qu'aucun état Pokémon n'est possédé par la présentation.
3. G0: contrat de placement World Core et dépendances biome/habitat explicités.
4. G0: matrice client/serveur conforme à DEC-007.
5. G1: simulation active sans assets visuels, player ou caméra.
6. G1: Pokémon off = zéro simulation, instance, abonnement ou présentation.
7. G1: même seed et données d'entrée produisent les mêmes décisions déterministes lorsque applicable.
8. G1: tests serveur et contrats autoritaires passés.

## Régression

- Exécuter les suites World/Spherical existantes pertinentes avant et après le changement.
- Conserver les quatre échecs précédemment observés dans `SphericalBiomeTextureGeneratorTests` comme baseline connue jusqu'à diagnostic distinct; ne pas les masquer ni les attribuer sans preuve au WP.
- Exécuter les tests de movement lorsque le contrat de surface/probe est touché.
- Exécuter les tests d'environnement et les vérifications WORLD-011-A lorsque le placement est touché.

## Preview et intégration

Preview simulation autonome sans player/caméra et sans prefab visuel; inspection des états/données via diagnostics ou outil de test. La preview ne doit pas inventer de runtime absent.

## Rapport exigé

- Unity version et commit de code testé.
- Liste exacte des tests lancés, nombre Passed/Failed/Skipped.
- Logs complets pour chaque échec.
- Captures de chaque mode de preview.
- Confirmation Generate/Clear idempotent.
- NOT RUN / BLOCKED / N/A avec raison; aucune validation ne doit être supposée.

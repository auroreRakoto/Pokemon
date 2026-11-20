# MOD-001-E — Test plan: Placement des assets environnementaux optionnel

- **Statut:** PROPOSED — tests à exécuter après implémentation.
- **Owner:** WORLD
- **WP:** `MOD-001-E_E_PLAN.md`
- **Exécution locale:** Unity Test Runner; les previews et vérifications PlayMode sont exécutées par le propriétaire.
- **Déterminisme:** seeds/profils fixes; les comparaisons doivent porter sur les données produites et les objets créés, pas seulement sur l'absence d'exception.

## Cas requis

1. Assets off: aucune racine/instance et aucune lecture du catalogue/configuration.
2. Catalogue absent avec assets on: EnvironmentAssets est bloqué; World Core et autres modules restent actifs.
3. Règles génériques: placement possible sans classification biome.
4. Règle biome sans classification: diagnostic précis et zéro placement partiel.
5. Même seed/field set/catalogue/config: mêmes candidats et placements.
6. Les contraintes WORLD-011-A (Plains/Forest, déterminisme, transformations prefabs) restent respectées.
7. Clear/Generate répétés ne laissent pas d'objets résiduels.

## Régression

- Exécuter les suites World/Spherical existantes pertinentes avant et après le changement.
- Conserver les quatre échecs précédemment observés dans `SphericalBiomeTextureGeneratorTests` comme baseline connue jusqu'à diagnostic distinct; ne pas les masquer ni les attribuer sans preuve au WP.
- Exécuter les tests de movement lorsque le contrat de surface/probe est touché.
- Exécuter les tests d'environnement et les vérifications WORLD-011-A lorsque le placement est touché.

## Preview et intégration

Preview d'assets autonome sans player, caméra ou renderer; trois cas: module off, règles génériques, règles biome.

## Rapport exigé

- Unity version et commit de code testé.
- Liste exacte des tests lancés, nombre Passed/Failed/Skipped.
- Logs complets pour chaque échec.
- Captures de chaque mode de preview.
- Confirmation Generate/Clear idempotent.
- NOT RUN / BLOCKED / N/A avec raison; aucune validation ne doit être supposée.

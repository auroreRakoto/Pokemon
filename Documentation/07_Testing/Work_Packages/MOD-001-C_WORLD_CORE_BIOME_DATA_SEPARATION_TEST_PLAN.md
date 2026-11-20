# MOD-001-C — Test plan: World Core sans classification des biomes

- **Statut:** PROPOSED — tests à exécuter après implémentation.
- **Owner:** WORLD
- **WP:** `MOD-001-C_C_PLAN.md`
- **Exécution locale:** Unity Test Runner; les previews et vérifications PlayMode sont exécutées par le propriétaire.
- **Déterminisme:** seeds/profils fixes; les comparaisons doivent porter sur les données produites et les objets créés, pas seulement sur l'absence d'exception.

## Cas requis

1. Le chemin historique Generate(settings) conserve les résultats biome et les tests existants.
2. Le chemin sans biomes expose HasBiomeClassification=false; TrySampleBiome retourne false; aucune valeur Plains artificielle n'est fournie.
3. Les champs altitude, température, humidité, pente, classe d'eau et couverture océanique sont identiques pour un même seed/config entre les deux modes.
4. SphericalGroundProbe et les requêtes de surface fonctionnent sans classification.
5. Les tests de topologie, coutures, déterminisme, drainage, océan et movement restent verts.
6. La génération sans biomes n'alloue pas les buffers de classification biome.
7. Preview indépendante exécutable sans terrain rendu ni joueur.

## Régression

- Exécuter les suites World/Spherical existantes pertinentes avant et après le changement.
- Conserver les quatre échecs précédemment observés dans `SphericalBiomeTextureGeneratorTests` comme baseline connue jusqu'à diagnostic distinct; ne pas les masquer ni les attribuer sans preuve au WP.
- Exécuter les tests de movement lorsque le contrat de surface/probe est touché.
- Exécuter les tests d'environnement et les vérifications WORLD-011-A lorsque le placement est touché.

## Preview et intégration

Preview géographique isolée sans renderer, player ni assets. Même seed/profil dans les deux modes; inspection des six faces, coutures, eau et champs continus.

## Rapport exigé

- Unity version et commit de code testé.
- Liste exacte des tests lancés, nombre Passed/Failed/Skipped.
- Logs complets pour chaque échec.
- Captures de chaque mode de preview.
- Confirmation Generate/Clear idempotent.
- NOT RUN / BLOCKED / N/A avec raison; aucune validation ne doit être supposée.

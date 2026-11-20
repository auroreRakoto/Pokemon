# MOD-001-D — Test plan: Composition du monde et présentation terrain optionnelles

- **Statut:** PROPOSED — tests à exécuter après implémentation.
- **Owner:** WORLD
- **WP:** `MOD-001-D_D_PLAN.md`
- **Exécution locale:** Unity Test Runner; les previews et vérifications PlayMode sont exécutées par le propriétaire.
- **Déterminisme:** seeds/profils fixes; les comparaisons doivent porter sur les données produites et les objets créés, pas seulement sur l'absence d'exception.

## Cas requis

1. Terrain off: zéro GameObject de terrain, mesh, renderer, matériau ou collider de présentation.
2. Terrain neutre: rendu URP opaque visible sans dépendance à la classification biome.
3. Terrain biome: rendu existant conservé et opaque avec DEC-048.
4. Mapping/shader biome absent: pas d'erreur si la présentation biome est désactivée; module biome presentation localement bloqué si activé.
5. Les trois modes consomment le même field set et les mêmes champs géographiques.
6. Generate/Clear répétés ne laissent pas de racines, meshes ou matériaux dupliqués.
7. Editor Generate/Apply-to-Scene existant reste fonctionnel.

## Régression

- Exécuter les suites World/Spherical existantes pertinentes avant et après le changement.
- Conserver les quatre échecs précédemment observés dans `SphericalBiomeTextureGeneratorTests` comme baseline connue jusqu'à diagnostic distinct; ne pas les masquer ni les attribuer sans preuve au WP.
- Exécuter les tests de movement lorsque le contrat de surface/probe est touché.
- Exécuter les tests d'environnement et les vérifications WORLD-011-A lorsque le placement est touché.

## Preview et intégration

Trois previews indépendantes avec le même profil/seed: World Core seul; terrain neutre opaque; terrain biome. Les previews ne doivent pas exiger le player ou les assets.

## Rapport exigé

- Unity version et commit de code testé.
- Liste exacte des tests lancés, nombre Passed/Failed/Skipped.
- Logs complets pour chaque échec.
- Captures de chaque mode de preview.
- Confirmation Generate/Clear idempotent.
- NOT RUN / BLOCKED / N/A avec raison; aucune validation ne doit être supposée.

# MOD-001-F — Test plan: Présence du joueur et caméra optionnelles

- **Statut:** PROPOSED — tests à exécuter après implémentation.
- **Owner:** WORLD
- **WP:** `MOD-001-F_F_PLAN.md`
- **Exécution locale:** Unity Test Runner; les previews et vérifications PlayMode sont exécutées par le propriétaire.
- **Déterminisme:** seeds/profils fixes; les comparaisons doivent porter sur les données produites et les objets créés, pas seulement sur l'absence d'exception.

## Cas requis

1. Player off: aucun player, controller, input, player camera, AudioListener ou HUD joueur.
2. Player on + terrain off: player fonctionne à partir des queries World Core, sans renderer.
3. Player on + biomes off: spawn générique explicite fonctionne.
4. Spawn biome-spécifique + biomes off: player module localement bloqué avec diagnostic.
5. Provider spawn ou mouvement manquant: seul Player est bloqué.
6. Generate/Clear répétés ne laissent aucun player/caméra/input orphelin.
7. Les 10 tests de mouvement existants et les tests de composition passent.

## Régression

- Exécuter les suites World/Spherical existantes pertinentes avant et après le changement.
- Conserver les quatre échecs précédemment observés dans `SphericalBiomeTextureGeneratorTests` comme baseline connue jusqu'à diagnostic distinct; ne pas les masquer ni les attribuer sans preuve au WP.
- Exécuter les tests de movement lorsque le contrat de surface/probe est touché.
- Exécuter les tests d'environnement et les vérifications WORLD-011-A lorsque le placement est touché.

## Preview et intégration

Preview observation sans player/caméra joueur et preview jouable avec player. Même profil/seed/field set.

## Rapport exigé

- Unity version et commit de code testé.
- Liste exacte des tests lancés, nombre Passed/Failed/Skipped.
- Logs complets pour chaque échec.
- Captures de chaque mode de preview.
- Confirmation Generate/Clear idempotent.
- NOT RUN / BLOCKED / N/A avec raison; aucune validation ne doit être supposée.

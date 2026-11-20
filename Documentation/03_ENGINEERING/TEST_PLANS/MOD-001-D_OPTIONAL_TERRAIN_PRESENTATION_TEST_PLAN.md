# MOD-001-D OPTIONAL TERRAIN PRESENTATION TEST PLAN

- **Statut:** PROPOSED — exécution après implémentation du WP correspondant.
- **Mode:** EditMode + PlayMode/preview selon le cas.
- **Déterminisme:** fixtures et seeds fixes; comparer les sorties, pas seulement l'absence d'exception.

## Cas requis

1. Terrain presentation off creates no terrain meshes, renderers or owned materials.
2. Terrain presentation on + biome ground presentation off produces opaque neutral URP terrain.
3. Biome presentation on preserves current blend shader/material mapping behavior.
4. Missing biome shader/mapping is ignored when biome presentation is disabled.
5. All modes consume the same World Core field set.
6. Generate/Clear cycles leave no duplicate roots or meshes.

## Exécution

Le propriétaire exécute ces tests dans Unity Test Runner. Toute compilation, preview ou intégration non exécutée doit rester marquée NOT RUN; les échecs doivent être rapportés avec les logs complets.

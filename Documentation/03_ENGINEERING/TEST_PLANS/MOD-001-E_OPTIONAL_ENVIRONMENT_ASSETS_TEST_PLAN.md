# MOD-001-E OPTIONAL ENVIRONMENT ASSETS TEST PLAN

- **Statut:** PROPOSED — exécution après implémentation du WP correspondant.
- **Mode:** EditMode + PlayMode/preview selon le cas.
- **Déterminisme:** fixtures et seeds fixes; comparer les sorties, pas seulement l'absence d'exception.

## Cas requis

1. Environment assets off creates no environment root or prefab instances and does not load placement config.
2. Environment assets on with missing catalog blocks only EnvironmentAssets.
3. Generic placement works without biome classification when its rules do not require biomes.
4. Biome-dependent placement blocks with an exact diagnostic when classification is unavailable.
5. Same seed, field set, rules and catalog produce identical placement results.
6. Existing WORLD-011-A scope and prefab transform invariants remain unchanged.

## Exécution

Le propriétaire exécute ces tests dans Unity Test Runner. Toute compilation, preview ou intégration non exécutée doit rester marquée NOT RUN; les échecs doivent être rapportés avec les logs complets.

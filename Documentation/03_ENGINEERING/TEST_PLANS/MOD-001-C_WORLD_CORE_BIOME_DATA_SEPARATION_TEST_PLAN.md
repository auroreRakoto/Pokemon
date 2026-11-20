# MOD-001-C WORLD CORE BIOME DATA SEPARATION TEST PLAN

- **Statut:** PROPOSED — exécution après implémentation du WP correspondant.
- **Mode:** EditMode + PlayMode/preview selon le cas.
- **Déterminisme:** fixtures et seeds fixes; comparer les sorties, pas seulement l'absence d'exception.

## Cas requis

1. World Core continuous/water fields match with biome classification on/off for identical settings and seed.
2. Biome APIs explicitly report unavailable data; no implicit Plains fallback.
3. Movement ground probe succeeds without biome classification.
4. Existing seam, determinism, ocean coverage and biome tests remain green in legacy mode.
5. Studio biome thematic output handles unavailable classification explicitly.
6. Independent geography preview works without renderer/player.

## Exécution

Le propriétaire exécute ces tests dans Unity Test Runner. Toute compilation, preview ou intégration non exécutée doit rester marquée NOT RUN; les échecs doivent être rapportés avec les logs complets.

# MOD-001-G OPTIONAL POKEMON TEST PLAN

- **Statut:** PROPOSED — exécution après implémentation du WP correspondant.
- **Mode:** EditMode + PlayMode/preview selon le cas.
- **Déterminisme:** fixtures et seeds fixes; comparer les sorties, pas seulement l'absence d'exception.

## Cas requis

1. Pokémon disabled creates no Pokémon runtime or presentation objects.
2. Pokémon enabled without runtime is locally blocked and does not block other modules.
3. Pokémon simulation/observation works without player or player camera.
4. Biome-dependent habitat rules declare their dependency; generic placement does not assume biome data.
5. Dedicated server remains authoritative for shared Pokémon state.
6. Server/client contract tests and autonomous observation preview pass.

## Exécution

Le propriétaire exécute ces tests dans Unity Test Runner. Toute compilation, preview ou intégration non exécutée doit rester marquée NOT RUN; les échecs doivent être rapportés avec les logs complets.

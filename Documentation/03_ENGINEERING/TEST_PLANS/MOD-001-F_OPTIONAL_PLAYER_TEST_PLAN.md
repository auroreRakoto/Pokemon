# MOD-001-F OPTIONAL PLAYER TEST PLAN

- **Statut:** PROPOSED — exécution après implémentation du WP correspondant.
- **Mode:** EditMode + PlayMode/preview selon le cas.
- **Déterminisme:** fixtures et seeds fixes; comparer les sorties, pas seulement l'absence d'exception.

## Cas requis

1. Player off creates no player, movement controller, player camera, AudioListener, input or player overlay.
2. Player on works with terrain presentation disabled.
3. Player on with biome classification disabled uses explicit generic spawn criteria or is locally blocked.
4. Player provider failure does not block World Core or environment placement.
5. Generate/Clear cycles do not leave player/camera objects.
6. Existing spherical movement tests remain green.

## Exécution

Le propriétaire exécute ces tests dans Unity Test Runner. Toute compilation, preview ou intégration non exécutée doit rester marquée NOT RUN; les échecs doivent être rapportés avec les logs complets.

# MOD-001-G — Intégration future du module Pokémon indépendant

- **Statut:** PROPOSED / BLOCKED — dépend de la disponibilité et de l'approbation du runtime Pokémon.
- **Owner module:** POK / Architecture / World Composition
- **Agent:** POK
- **Dépendances:** MOD-001-C, D et F validés; architecture Pokémon, contrats de placement et runtime Pokémon courants fournis et approuvés.
- **Objectif:** permettre à Pokémon d'être activé ou absent sans dépendre du joueur, de la caméra, du rendu terrain, des assets environnementaux ou des biomes visuels.

## Scope inclus (conditionnel)
- Auditer le runtime Pokémon réel, ses contrats, appels, assets, scènes, prefabs et sérialisations.
- Définir un provider de placement utilisant World Core; biome classification reste une dépendance optionnelle seulement pour les règles d'habitat qui la déclarent.
- Séparer simulation/état d'entité et présentation visuelle.
- Ajouter la validation des providers réels au contexte MOD-001-B.
- Ajouter preview autonome de simulation/observation sans joueur.
- Respecter DEC-007: autorité serveur dédiée pour l'état partagé et les conséquences gameplay.

## Exclusions
- Aucun placeholder Pokémon ni simulation fictive.
- Aucune restauration spéculative de Chenipotte, Mystherbe ou baies.
- Pas de refonte de `PokemonAgent` dans ce WP; son éventuel découpage exige son propre WP POK/ARCH et tests de régression.
- Pas de modification du modèle réseau approuvé.
- Pas de changement des systèmes de combat, reproduction, besoins ou écologie sans WP distinct.

## Contrats et invariants
- Pokémon off = aucune instance, simulation, abonnement ou ressource Pokémon.
- Pokémon on sans runtime valide = seulement Pokémon est bloqué; le monde continue.
- Pokémon n'exige jamais le player, la caméra ou les assets environnementaux.
- Les clients n'imposent jamais les résultats autoritaires; respecter DEC-007.
- Le monde peut être généré et observé sans Pokémon.

## Preview et tests
- Preview simulation/observation sans player.
- Tests d'activation avec World Core seul; avec classification; avec présentation off; avec player off.
- Tests serveur/dédié et contrats réseau selon le plan approuvé.
- Vérifier l'absence totale d'objets/updates Pokémon lorsque désactivé.

## Blocage explicite
L'archive de sources examinée ne contient pas le runtime Pokémon. Ce plan ne donne donc pas autorisation d'inventer ou de coder son implémentation. Il sera révisé à partir du dernier projet et des documents POK/NETWORK lorsqu'ils seront fournis.

## Handoff
- Commit proposé après déblocage et validation: `feat(pokemon): integrate optional world pokemon module`
- Footer: `Work-Package: MOD-001-G` / `Agent: POK`

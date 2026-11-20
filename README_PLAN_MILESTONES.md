# MOD-001 — Milestones de plan C à G

- **Statut global:** plans PROPOSED; aucun code autorisé par ce bundle.
- **Owner approvals reçues pour préparer les plans:** C, D, E, F, G.
- **Décisions de conception confirmées:** spawn générique sans biomes = oui; monde sans terrain 3D = oui; simulation Pokémon sans assets visuels = oui.
- **Failure policy:** blocage local du module optionnel; World Core invalide bloque le monde.
- **Ordre:** C → D → E → F → G.
- **Règle:** approbation du programme ne vaut pas approbation d'implémentation. Chaque WP doit être approuvé et son plan milestone poussé; l'implémentation se fait un seul WP à la fois.

## Matrice des dépendances

| WP | Dépendances | Nature |
|---|---|---|
| MOD-001-C | MOD-001-B | séparation des données géographiques et biomes |
| MOD-001-D | C + B | composition World Core / terrain / présentation |
| MOD-001-E | B + C + D + WORLD-011-A | assets génériques ou biome-dépendants |
| MOD-001-F | B + C + D | player optionnel et spawn générique |
| MOD-001-G | C + D + E + F + sources/runtime Pokémon réels | audit et découplage simulation/présentation; implementation BLOCKED tant que le runtime n'est pas fourni |

## Limite spécifique MOD-001-G

Les documents de domaine Pokémon décrivent des responsabilités architecturales, mais les snapshots de code fournis pour ce programme ne contiennent pas le Runtime Pokémon. G0 est donc un audit/contrat sans code. G1 demandera un nouveau WP POK/ARCH après réception du runtime, des callers, assets, prefabs, scènes et contrats réseau actuels.

## Commits de plan

Chaque dossier WP est conçu pour être poussé séparément. Cela conserve un historique et permet d'arrêter ou d'amender une étape sans approuver implicitement les suivantes.

# MOD-001-C — World Core sans classification des biomes

- **Statut:** PROPOSED — validation du propriétaire requise.
- **Milestone:** A — plan uniquement; aucune implémentation autorisée avant commit/push et approbation.
- **Owner module:** WORLD
- **Dépendances:** MOD-001-B validé; dernière version appliquée de MOD-001-A; snapshot courant des consommateurs et tests.
- **Décisions applicables:** DEC-007, DEC-019, DEC-020, DEC-044, DEC-047, DEC-048 et DEC-049.
- **Norme de code:** maximum 10 fonctions par fichier; maximum 50 lignes logiques par fonction; commentaire descriptif au-dessus de chaque fonction.
- **Périmètre:** Rendre la classification des biomes facultative sans rendre facultatifs les champs géographiques autoritaires (topologie, altitude, eau, température, humidité, pente et requêtes de surface).

## 1. Résultat attendu

Rendre la classification des biomes facultative sans rendre facultatifs les champs géographiques autoritaires (topologie, altitude, eau, température, humidité, pente et requêtes de surface).

## 2. Scope inclus

- Auditer tous les consommateurs de SphericalGeographicFieldSet et classer chaque lecture de biome comme obligatoire, optionnelle ou purement visuelle.
- Ajouter une voie explicite de génération sans classification, en conservant Generate(settings) et son comportement historique pour compatibilité.
- Représenter explicitement l'absence de classification; ajouter HasBiomeClassification et des API TryGet/TrySample adaptées. Les API historiques GetBiome/SampleBiome ne doivent pas retourner implicitement Plains lorsque les données manquent.
- Ne pas allouer/calculer les tableaux et classifications de biome lorsque le module est désactivé.
- Rendre SphericalGroundProbe et les requêtes de déplacement indépendants de la classification.
- Définir le comportement de la carte thématique Planet Studio lorsque les biomes sont absents: diagnostic explicite ou mode de visualisation non-biome déjà supporté, sans fallback trompeur.
- Ajouter fixtures déterministes pour comparer les champs World Core avec classification activée et désactivée.

## 3. Exclusions

- Ne pas changer les formules de terrain, hydrologie, niveau marin, couverture océanique, climat, humidité ou seuils de classification.
- Ne pas modifier les shaders, matériaux, meshes, placement d'assets, spawn joueur ou Pokémon.
- Ne pas changer les définitions DEC-044 ni ajouter de nouveaux biomes.
- Ne pas introduire d'asmdef, framework ou dépendance.

## 4. Fichiers candidats

Les chemins ci-dessous sont des candidats identifiés dans le snapshot fourni. Ils doivent être confirmés contre le dernier working tree/snapshot du propriétaire avant le milestone d'implémentation.

- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldSet.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalGroundProbe.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioGenerator.cs (uniquement si son contrat biome l'exige)`
- `tests géographiques et movement concernés, à confirmer par recherche exhaustive des appels`

## 5. Contrats et invariants

- World Core reste l'unique propriétaire des champs géographiques autoritaires.
- Le même field set est partagé; les modules optionnels ne régénèrent ni ne mutent les données World Core.
- Le module absent ou bloqué ne doit pas arrêter les modules indépendants.
- Aucun fallback silencieux ni objet factice.
- Sélection des modules à la création du monde; aucune hot-toggle dans la session.
- Les défauts d'un module optionnel sont signalés localement; seul World Core invalide bloque la création.
- Les GUID, `.meta`, scènes, prefabs et données sérialisées sont préservés sauf migration explicitement prévue et auditée.
- DEC-007 et les décisions réseau existantes ne sont pas modifiées.

## 6. Preview indépendante

Preview géographique isolée sans renderer, player ni assets. Même seed/profil dans les deux modes; inspection des six faces, coutures, eau et champs continus.

## 7. Tests et critères d'acceptation

1. Le chemin historique Generate(settings) conserve les résultats biome et les tests existants.
2. Le chemin sans biomes expose HasBiomeClassification=false; TrySampleBiome retourne false; aucune valeur Plains artificielle n'est fournie.
3. Les champs altitude, température, humidité, pente, classe d'eau et couverture océanique sont identiques pour un même seed/config entre les deux modes.
4. SphericalGroundProbe et les requêtes de surface fonctionnent sans classification.
5. Les tests de topologie, coutures, déterminisme, drainage, océan et movement restent verts.
6. La génération sans biomes n'alloue pas les buffers de classification biome.
7. Preview indépendante exécutable sans terrain rendu ni joueur.

### Critères d'acceptation

- [ ] World Core est généré une seule fois et ne dépend pas de la classification.
- [ ] Tous les consommateurs ont été audités et les usages biome absents sont traités explicitement.
- [ ] Pas de fallback silencieux vers Plains.
- [ ] Tests EditMode et preview indépendante passent; intégration Planet Studio validée.

## 8. Risques et points d'audit

SphericalGroundProbe lit actuellement SampleBiome; SphericalPlanetStudioGenerator expose une carte Biomes; SphericalEnvironmentPlacementGenerator et SphericalBiomeInspectionBootstrap utilisent SampleBiome. Les deux derniers consommateurs sont à rendre sûrs par leurs work packages respectifs, sans élargir C à leur composition.

## 9. Rollback

- Conserver le chemin historique tant que le nouveau chemin n'a pas passé les tests de régression.
- Ne supprimer aucune API, scène, prefab ou asset avant audit des références et plan de migration.
- Les modifications de composition doivent pouvoir être désactivées/rétablies sans migration destructive.
- Toute modification de schéma JSON doit être versionnée et testée avec les profils historiques.

## 10. Documentation à mettre à jour après implémentation

- `Documentation/00_GOVERNANCE/DECISION_REGISTER.md` si une décision confirmée doit être enregistrée.
- `Documentation/01_ARCHITECTURE/WORLD_ARCHITECTURE.md` et `INTERFACE_CONTRACTS.md` pour les contrats réellement modifiés.
- `Documentation/02_DOMAINS/WORLD_AND_BIOMES.md` et/ou `TERRAIN_AND_RENDERING.md` selon l'étape.
- `Documentation/03_ENGINEERING/DEVELOPMENT_QUEUE.md` et `DOCUMENT_INDEX.md`.
- Rapport d'implémentation, plan de test et changelog.

## 11. Milestones et statut

- **Milestone A — Plan:** PROPOSED; à approuver, committer et pousser par le propriétaire.
- **Milestone B — Implémentation:** NOT STARTED; autorisé seulement après Milestone A poussé, confirmation du hash et approbation explicite de démarrer cette étape.
- **Validation:** Unity compilation, tests, preview et intégration sont NOT RUN tant que le propriétaire ne les exécute pas.

## 12. Commit proposé

```text
refactor(world): separate biome classification from world core

Work-Package: MOD-001-C
Agent: WORLD
```

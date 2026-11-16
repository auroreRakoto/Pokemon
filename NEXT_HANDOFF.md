# NEXT HANDOFF

## 1. Rapport produit
Nom : `ARCH-REBOOT-001_IMPLEMENTATION_REPORT.md`

## 2. Agent suivant
Agent : `TOOLS`

## 3. Documents à lui transmettre
- `REBOOT-001_PIPELINE_RESTART_BRIEF.md`
- `ARCH-REBOOT-001_PIPELINE_ARCHITECTURE_PROPOSAL.md`
- `REBOOT-001_IMPLEMENTATION_PLAN.md`
- `README_INSTALLATION.md`
- `Data/` et `Assets/Project/Runtime/` du starter kit

## 4. Travail attendu du prochain agent
- Vérifier l'intégration des scripts C# avec Unity 6000.4.11f1.
- Compiler le projet et corriger les erreurs de compilation éventuelles.
- Tester la synchronisation `Data/` vers `StreamingAssets`.
- Tester le chargement JSON et binaire en Play Mode, au minimum dans l'Editor.
- Vérifier le chargement `StreamingAssets` sur les plateformes visées lorsqu'elles seront connues.
- Ne pas introduire de règles métier WORLD/ECO/POK dans le pipeline TOOLS.
- Ne pas déplacer ni supprimer les anciens systèmes ou `.meta`.
- Rendre compte des écarts entre plan, implémentation et tests réellement exécutés.

## 5. Livrable attendu
Nom : `TOOLS-REBOOT-001_INTEGRATION_REVIEW.md`

Le rapport doit distinguer PASS / FAIL / BLOCKED et inclure les versions, commandes, preuves et limitations.

## 6. Instructions au propriétaire
1. Télécharger `REBOOT-001_STARTER_KIT.zip` et `REBOOT-001_IMPLEMENTATION_PLAN.md`.
2. Faire un commit/sauvegarde du projet actuel.
3. Ajouter et pousser le plan d'implémentation avant l'intégration du code, conformément à `GIT_WORKFLOW.md`.
4. Décompresser le starter kit à la racine du projet Unity en fusionnant prudemment les dossiers.
5. Exécuter les deux commandes Python indiquées dans `README_INSTALLATION.md`.
6. Ouvrir Unity, attendre la compilation et lancer la synchronisation depuis le menu Tools.
7. Transmettre le kit, le plan et le dépôt courant à TOOLS pour la revue d'intégration.
8. Récupérer `TOOLS-REBOOT-001_INTEGRATION_REVIEW.md`, puis faire effectuer une QA indépendante avant approbation finale.

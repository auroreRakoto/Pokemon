# Unity Architecture and Script-Driven Workflow

## Philosophy

Unity should primarily be the runtime engine, renderer and authoring environment for automated tools. Prefer script-driven setup and repeatable pipelines over repetitive Inspector configuration.

## Runtime/editor separation

### Runtime code
- simulation and gameplay;
- world/session runtime;
- rendering controllers;
- input and UI;
- runtime asset/data loading;
- network clients/server runtime as appropriate.

### Editor-only code
- AssetDatabase operations;
- create/update ScriptableObjects;
- create materials and TerrainLayers;
- configure prefabs and scenes;
- import processors;
- previews, validation windows and generation menus.

Editor code must be isolated under Editor folders or Editor-only assemblies. Runtime assemblies must not reference `UnityEditor`.

## Script-driven asset setup

A repeatable editor command should be able to:
1. find source textures/assets;
2. validate file presence and import settings;
3. create/update materials and terrain layers;
4. configure prefab components/references;
5. create/update data assets;
6. run validation;
7. produce a report.

Operations should be idempotent: running them repeatedly should update the same intended assets rather than create duplicates.

## Inspector policy

Inspector references are acceptable when they are stable, few, and genuinely useful to tune manually. Do not require users to drag dozens of references into fields to make the game work.

Prefer centralized catalogs, stable IDs, addressable/resource registries or generated references where appropriate. Select the loading technology only after evaluating build and platform constraints.

## Scene policy

Avoid essential gameplay setup hidden in manually configured scenes. Use a bootstrap process and validation that checks required scene objects and configuration. Keep scenes small and purposeful.

## Runtime scaling

For individual Pokémon size:
- stable root contains identity, networking and gameplay components;
- child `VisualRoot` contains mesh/rig/visual attachments;
- apply individual size factor to `VisualRoot`;
- define collider and interaction scaling deliberately;
- do not scale network root or movement coordinate system accidentally.

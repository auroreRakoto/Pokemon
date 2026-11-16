# Art, Blender and Asset Pipeline

## Scope

Owns visual/audio asset production standards and reliable transfer from source creation to Unity.

## Source/export separation

- Blender source files and scripts are editable sources.
- Exports are generated outputs.
- Unity imported assets are runtime project content.
- Never edit an export as the only source of truth.
- Use explicit output directories; avoid writing to drive roots or arbitrary absolute paths.
- Export operations should be repeatable and report failures clearly.

## Suggested Blender structure

```text
Blender/
  _core/
  project_modules/
    pokemon/
    environment/
    collectibles/
    props/
  source/
    pokemon/<species>/
    environment/<category>/
    collectibles/<category>/
    props/<category>/
    materials/<category>/
  exports/
    pokemon/
    environment/
    collectibles/
    props/
  documentation/
```

`project_modules` contains package directories/modules, not unrelated root-level asset scripts. Individual asset generators belong under `source/<domain>/<asset>/`.

## Pokémon asset requirements

For each species, define as relevant:
- base mesh;
- material slots and texture references;
- armature/rig;
- skin weights;
- animation set;
- attachment sockets;
- colliders/interaction bounds metadata;
- LODs if needed;
- optional forms/dimorphism;
- audio/VFX references.

Not every species needs a bespoke rig if compatible rigs can be shared, but this must be validated.

## Size variants

Ordinary individual size variation uses one base mesh and a scale factor. Keep a stable gameplay root and scale `VisualRoot`. Ensure colliders, sockets, interaction points, navigation and camera framing are handled consistently.

## Environment asset requirements

Environment objects may expose interaction metadata:
- berry slots;
- shelter/rest points;
- nest points;
- perch points;
- hollow/log entrance;
- cover zones;
- resource harvest points.

Decorative objects do not need interaction sockets unless gameplay uses them.

## Texture/material standards

Define texture resolution, format, colour space, compression, tiling scale, naming, channel packing and shader compatibility per asset class. Do not assume every asset needs every PBR map. Stylized hand-painted/albedo-first workflows are valid.

## Naming

Use stable, readable identifiers. Suggested patterns:
- `SM_Pokemon_Chenipotte_Base`
- `MAT_Ground_Grass`
- `TEX_Ground_Grass_Albedo`
- `AN_Pokemon_Chenipotte_Idle`
- `VFX_Ability_<Name>_Impact`

Final naming convention must be consolidated with Unity import tooling before mass production.

## Validation

Before export/import, validate:
- object names and collection structure;
- transforms/origin/scale;
- normals and mesh integrity;
- material slots;
- rig and bone naming if applicable;
- socket naming;
- polygon/texture budgets;
- export path;
- manifest completeness.

## Manifest

Each export should ideally include a manifest with asset ID, source path, export version, generator version, dependencies, units/scale, material references and intended Unity destination.

# World, Terrain and Biomes

## Scope

Owns spatial world generation, terrain representation, biome placement, environment objects, navigation-relevant spatial queries and world-side interaction locations.

## Visual layers

1. Terrain base: grass, dirt, rock, sand, mud, snow, leaf litter and similar.
2. Model materials: bark, leaves, stone, Pokémon surfaces, berries and props.
3. Placed details: grass tufts, flowers, fallen leaves, stones, mushrooms, branches.
4. Interactive environment: berry bushes, water sources, shelters, nests, hollow logs.
5. Dynamic effects: puddles, wetness, wind response, footprints and temporary traces.

Do not confuse terrain texture painting with placement of actual 3D objects.

## Terrain rendering

Unity Terrain generated through scripts is a candidate initial solution. A custom mesh/chunk approach remains possible. Keep biome data independent of the rendering backend.

If Unity Terrain is selected:
- generate TerrainData through editor/runtime-appropriate APIs;
- create TerrainLayers via editor tooling where asset creation is required;
- generate alphamaps/splat weights by scripts;
- centralize texture references and import settings;
- validate layer count, texture tiling, dimensions and missing references.

If mesh terrain is selected:
- define chunk topology, normals, UVs, vertex colours/material blending and collision policy;
- support deterministic generation and LOD where needed.

## Texture/material architecture

Separate:
- source texture files;
- imported Unity textures;
- material definitions;
- shader implementations;
- biome-to-material mappings;
- runtime terrain painting/assignment.

Stylized art does not require full PBR maps on every surface. Use albedo/base colour first; add normal, roughness/smoothness, AO, height or transparency only when visually justified.

## Biome definitions

May contain:
- climate ranges;
- terrain material palette and blend rules;
- vegetation profiles/density;
- rock/log/prop distribution;
- resource spawn profiles;
- habitat availability;
- suitable species and constraints;
- ambient audio/VFX;
- generation parameters.

## Procedural generation

Use explicit seeds and versioned generation settings. Separate stages: terrain shape, biome regions, material assignment, object placement, resource placement, habitat setup, validation.

Generation must be reproducible enough for debugging. Avoid uncontrolled global random state.

## Spatial services

Expose queries such as:
- environment context at position;
- biome at position;
- nearby resource candidates;
- reachable interaction point;
- suitable shelter/rest location;
- ground/slope/water condition.

Other domains should not inspect private terrain-generation data structures directly.

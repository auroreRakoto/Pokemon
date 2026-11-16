# Terrain, Textures and Rendering

## Rendering principles

- Stylized 3D is the target visual direction.
- Prefer coherent art direction over unnecessary material complexity.
- Keep biome logic separate from rendering implementation.
- Automate material, texture and terrain setup where possible.

## Terrain options

### Unity Terrain
Advantages: built-in heightmap terrain, terrain layers, alphamaps, detail/tree integration and familiar tooling.

Risks/constraints: terrain-specific limits, layer/shader constraints, runtime editing/streaming complexity and possible mismatch with custom topology.

### Custom mesh terrain
Advantages: full control over topology, chunks, vertex colours, mesh generation and custom data.

Risks/constraints: must implement more infrastructure: LOD, collision, normals, UVs, chunk seams, painting and tooling.

Decision remains OPEN. Start with a small technical prototype and compare based on project needs.

## Texture maps

- Base Color/Albedo: visible colour.
- Normal: lighting detail.
- Roughness/Smoothness: surface response.
- AO: local occlusion.
- Height: parallax/displacement or blending where supported.
- Alpha: cutout/transparency where required.

Use only maps that materially improve the stylized result.

## Procedural texture distribution

Potential inputs:
- biome ID/weights;
- height;
- slope;
- moisture;
- distance to paths/water/objects;
- authored masks;
- seeded noise.

Texture weights should be normalized and validated. Avoid hard transitions unless intentionally stylized.

## Runtime and editor responsibilities

Editor tooling creates reusable assets (materials, TerrainLayers, configuration assets) and validates imports. Runtime code applies/generates terrain state supported by the chosen representation. Keep UnityEditor APIs out of builds.

## Dynamic ground

Potential effects: wetness, mud, snow, footprints, flattened grass, leaf litter, resource traces. Prefer decals, overlays, instanced details or material parameters over modifying huge terrain textures for every small event. Choose implementation based on persistence, multiplayer visibility and performance.

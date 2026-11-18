# WORLD-010-DIAG — High-Contrast Biome Texture Pipeline Test

- **Status:** DIAGNOSTIC BUILD — deliberately ugly temporary textures; do not commit as final art
- **Purpose:** Determine whether generated biome textures are actually sampled by the playable spherical terrain shader.

## Diagnostic appearance

- Plains: fluorescent magenta / near-black checkerboard.
- Forest: cyan / near-black checkerboard.
- Desert: bright yellow / near-black checkerboard.
- Mountains: white / red checkerboard.

The shader temporarily displays raw sampled texture colours, bypassing biome tint multiplication. The texture repeat size is 4 metres. Generated textures use point filtering, no mipmaps and uncompressed import to prevent subtle filtering/compression from hiding the test pattern.

## Runtime evidence

On material configuration, the playable bootstrap logs:
- selected shader name;
- diagnostic mode and texture world size;
- each biome material name;
- each assigned texture name and dimensions, or an explicit missing-texture message.

## Installation / test

1. Stop Play Mode and back up the project.
2. Extract the patch at the project root.
3. Wait for Unity compilation.
4. Run `Tools > World > Generate or Refresh Biome Materials (URP)`.
5. Run `Tools > World > Assign Biome Mapping to Current Inspection Scene`.
6. Enter Play Mode and inspect a broad Plains area, then Forest, Desert and Mountains.
7. Check the Console for `[WORLD-010 TEXTURE DIAGNOSTIC]`.

## Interpretation

- **Checkerboards visible:** texture generation, material assignment, mesh biome weights and shader sampling are connected. The normal art textures were simply too subtle or too small; proceed to redesign them.
- **Checkerboards absent and log says texture missing:** generated material/texture asset pipeline is broken.
- **Checkerboards absent but all textures are logged:** inspect shader compilation/selection, runtime material instance and mesh biome weights. Do not proceed with art tuning.
- **Only one biome palette appears everywhere:** inspect vertex biome weights and mesh colour channel data.
- **Water remains blue:** expected; ocean uses a separate mask and is excluded from terrestrial diagnostic textures.

## Exit criteria

All four terrestrial checkerboard palettes appear in their corresponding biomes, with stable transitions and no cube-face seams. Once confirmed, revert diagnostic mode and replace the checker textures with final stylized textures before any feature-completion commit.

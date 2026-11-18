# WORLD-008 — JSON World Profiles

- **Status:** IMPLEMENTATION CHECKPOINT — profile model, editor workflow and initial JSON profiles authored; Unity validation pending
- **Depends on:** WORLD-007 configurable planet radius and ocean range; WORLD-002 geographic fields; DEC-020 JSON data location
- **Owner-approved direction:** use multiple JSON files to describe different world characteristics.

## Goal

Allow the developer to select a named world profile in Planet Studio, load its complete world-generation characteristics, edit the resulting settings, and save custom profiles as additional JSON files.

Profiles use the existing spherical generation pipeline. They are not separate generators, test modes, or production modes.

## Data location and format

All source JSON profiles live under `Assets/Project/Data/WorldProfiles/`, following DEC-020.

Each profile is schema-versioned (`schemaVersion: 1`) and includes:
- identity: `id`, `displayName`, `description`;
- planet: radius in kilometres, seed, face resolution and planisphere dimensions;
- terrain: enabled layers, amplitudes, frequencies and continent threshold;
- climate/biomes: temperature influences, ocean target, moisture model parameters, mountain classification and existing desert/forest thresholds.

Runtime settings continue to store radius in metres. The JSON profile expresses it in kilometres for readability; applying a profile converts at the boundary.

## Initial profiles

1. Temperate Balanced — 50 km, 65% ocean.
2. Continental — 80 km, 35% ocean.
3. Archipelago — 25 km, 82% ocean.
4. Arid — 40 km, 30% ocean.
5. Rugged Highlands — 60 km, 55% ocean.

These are parameter presets for the currently implemented plains, forest, desert and mountains classification. They do not claim to add snow, tundra, biome-specific assets, vegetation, hydrology or weather systems.

## Planet Studio workflow

- Discover valid `*.json` profiles in the profile directory.
- Display their `displayName` values in a preset dropdown.
- Load the selected profile into the existing settings asset.
- Save the current world-generation settings as a new JSON profile selected by the user.
- Do not overwrite the built-in profiles implicitly.
- Do not alter editor preview settings, drainage diagnostics, player dimensions, movement, gravity or camera settings when loading a profile.

## Explicit non-goals

- Runtime player-facing world selection menu.
- Automatic scaling of terrain relief, player size, movement speed, gravity or camera based on radius.
- New biome types or biome textures.
- Weather, seasons, vegetation placement or Pokémon spawning.
- Separate generation algorithms per profile.

## Acceptance criteria

- All five supplied JSON files parse with Unity `JsonUtility`.
- Profile load applies all declared world-generation fields to the settings asset.
- Profile save produces a new JSON file in the approved data directory.
- Applying a profile leaves preview-only and drainage-only settings unchanged.
- Radius is converted between profile kilometres and runtime metres exactly once.
- Ocean targets below 50% and above 80% survive profile load and settings validation.
- Unknown schema versions are rejected with a clear error.
- Unity compilation and EditMode tests pass; Planet Studio can load each profile and generate its world.

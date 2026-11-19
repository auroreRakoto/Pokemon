# Current C# Source Inventory — 2026-11-19

- **Status:** STATIC INVENTORY — owner archive snapshot, not live filesystem access.
- **Archive:** `Pokemon-clean.zip`
- **SHA-256:** `829ea7a6abec16886f0133825b376f91f2eece6092125b306fb3447aea3b7a57`
- **Total C# files:** 56
- **Runtime:** 33
- **Editor:** 8
- **Tests:** 15

## Summary

- No Pokémon simulation Runtime source is present.
- No declarations/usages of the former `PokemonAgent`, `PokemonNeeds`, `PokemonDefinition`, `PokemonScheduleCalculator` or related simulation types were found in C# source.
- The only Pokémon-named assembly in Runtime is the spherical World assembly `Pokemon.Project.World.Spherical`; this is a namespace/assembly label, not Pokémon gameplay code.
- Existing EditMode/PlayMode tests are for spherical World systems.
- Unity compilation was not run by this audit. The owner separately supplied a Console screenshot showing 0 errors and 0 warnings.

## Runtime C# files

- `Assets/Project/Runtime/Infrastructure/Diagnostics/EcosystemLogger.cs`
- `Assets/Project/Runtime/World/Spherical/CubeSphereTopology.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementConfig.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPlacementJson.cs`
- `Assets/Project/Runtime/World/Spherical/Environment/SphericalEnvironmentPrefabCatalog.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalDistanceBenchmark.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalDrainage.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldSet.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGeographicFieldType.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalGraphDistance.cs`
- `Assets/Project/Runtime/World/Spherical/Geography/SphericalNeighbourGraph.cs`
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalBiomeInspectionBootstrap.cs`
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalGeographicPlayerController.cs`
- `Assets/Project/Runtime/World/Spherical/Inspection/SphericalWorldDemoOverlay.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalGroundProbe.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerController.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalPlayerMotor.cs`
- `Assets/Project/Runtime/World/Spherical/Movement/SphericalThirdPersonCamera.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeMaterialMapping.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeSurfaceMeshGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalBiomeTextureGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetMeshGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalPlanetPresenter.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalWorldDemoBootstrap.cs`
- `Assets/Project/Runtime/World/Spherical/Presentation/SphericalWorldDemoOverlay.cs`
- `Assets/Project/Runtime/World/Spherical/SphericalHeightField.cs`
- `Assets/Project/Runtime/World/Spherical/SphericalSurfaceQuery.cs`
- `Assets/Project/Runtime/World/Spherical/SphericalWorldSettings.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioGenerator.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalPlanetStudioSettings.cs`
- `Assets/Project/Runtime/World/Spherical/Studio/SphericalWorldProfile.cs`

## Editor C# files

- `Assets/Project/Editor/Tools/Blender/BlenderFbxSync.cs`
- `Assets/Project/Editor/Tools/Prefabs/EnvironmentPrefabGeneratorWindow.cs`
- `Assets/Project/Editor/Tools/World/EnvironmentPlacementAssetGenerator.cs`
- `Assets/Project/Editor/World/Spherical/Biomes/SphericalBiomeMaterialGenerator.cs`
- `Assets/Project/Editor/World/Spherical/SphericalBiomeInspectionSceneBuilder.cs`
- `Assets/Project/Editor/World/Spherical/SphericalDrainageStudioPreview.cs`
- `Assets/Project/Editor/World/Spherical/SphericalPlanetGenerationStudioWindow.cs`
- `Assets/Project/Editor/World/Spherical/SphericalWorldDemoSceneTool.cs`

## Test C# files

- `Assets/Project/Tests/EditMode/World/Spherical/CubeSphereTopologyRegressionTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/CubeSphereTopologyTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/Environment/SphericalEnvironmentPlacementSettingsTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/Movement/SphericalPlayerMotorTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalBiomeSurfaceRenderingTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalBiomeTextureGeneratorTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalDrainageLogicIsolationTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalDrainageTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalGeographicFieldGeneratorTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalHeightFieldTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalNeighbourGraphTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalPlanetStudioTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalPresentationTests.cs`
- `Assets/Project/Tests/EditMode/World/Spherical/SphericalWorldProfileTests.cs`
- `Assets/Project/Tests/PlayMode/World/Spherical/SphericalPlanetStudioPlayModeTests.cs`

## Interpretation

This inventory is a reproducible path list for the supplied archive only. It does not prove what exists in the owner's working tree after the archive was created. Re-run the inventory after future source changes.

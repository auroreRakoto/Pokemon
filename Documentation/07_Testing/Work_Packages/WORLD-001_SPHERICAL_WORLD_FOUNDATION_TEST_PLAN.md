# WORLD-001 — Spherical World Foundation: Detailed Test Plan

**Purpose:** map the already implemented spherical-world foundation to reproducible EditMode, PlayMode and manual acceptance tests.  
**Source classes inspected:** `CubeSphereTopology`, `SphericalHeightField`, `SphericalSurfaceQuery`, `SphericalWorldSettings`, `SphericalPlanetMeshGenerator`, `SphericalPlanetPresenter`, `SphericalPlayerController`, `SphericalThirdPersonCamera`, `SphericalWorldDemoBootstrap`, `SphericalWorldDemoOverlay`.

## A. Cube-Sphere topology

### Existing EditMode tests
- `CubeSphereTopologyTests.FaceGridProducesFiniteNormalizedDirections`
- `CubeSphereTopologyTests.DirectionAndFaceCoordinateConversionRoundTrips`
- `CubeSphereTopologyTests.FaceEdgeSamplesAreSharedAcrossAdjacentFaces`
- `CubeSphereTopologyRegressionTests.EveryFaceCentreMapsToItsOwnFace`
- `CubeSphereTopologyRegressionTests.FaceGridDirectionsAreFiniteAndNormalized`
- `CubeSphereTopologyRegressionTests.ZeroDirectionIsRejected`

### Additional tests required
| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-TOP-01 | `AllTwelveFaceSeamsShareEquivalentDirections` | Each adjacent face-pair edge maps to the same unit direction within the documented direction tolerance | EditMode |
| W001-TOP-02 | `AllEightCubeCornersHaveDeterministicFaceOwnership` | Every corner resolves to the documented canonical face/UV tie-break consistently | EditMode |
| W001-TOP-03 | `InvalidFaceAndNonFiniteCoordinatesAreRejectedOrClampedByContract` | Invalid inputs follow an explicit documented policy; no NaN direction is emitted | EditMode |
| W001-TOP-04 | `DirectionMappingIsStableAcrossSupportedResolutions` | Shared geometric directions remain equivalent at low, medium and high supported resolutions | EditMode |

## B. Spherical height field and settings

### Existing EditMode tests
- `SphericalHeightFieldTests.SameSeedAndSettingsProduceSameHeight`
- `SphericalHeightFieldTests.DifferentSeedsProduceDifferentHeightField`
- `SphericalHeightFieldTests.HeightRemainsWithinConfiguredRange`

### Additional tests required
| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-HGT-01 | `HeightSamplingIsFiniteForRepresentativeSphereDirections` | Cardinal, diagonal, near-pole and seam directions produce finite heights | EditMode |
| W001-HGT-02 | `HeightSamplingIsContinuousAcrossCubeFaceSeams` | Paired directions on opposite representations of a seam differ by no more than the approved height tolerance | EditMode |
| W001-HGT-03 | `SettingsValidationRejectsInvalidPlanetDimensions` | Invalid radius/resolution/noise settings are rejected or normalized according to the settings contract | EditMode |
| W001-HGT-04 | `HeightFieldChangesWhenEachConfiguredNoiseParameterChanges` | Each parameter documented as active has an observable, isolated effect | EditMode |

## C. Surface query and tangent frame

### Existing EditMode tests
- `SphericalPresentationTests.SurfaceQueryReturnsConfiguredBaseRadiusPlusElevation`
- `SphericalPresentationTests.TangentFrameIsOrthonormalForRepresentativeDirections`

### Additional tests required
| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-SUR-01 | `SurfaceRadiusMatchesSampledHeightForAllRepresentativeDirections` | Surface query radius equals configured base radius plus sampled elevation within metre tolerance | EditMode |
| W001-SUR-02 | `TangentFrameRemainsFiniteNearBothPoles` | North/east vectors remain finite, normalized and tangent near both poles | EditMode |
| W001-SUR-03 | `TangentFrameOrientationIsConsistent` | Cross-product/handedness follows the documented local-frame convention | EditMode |
| W001-SUR-04 | `ZeroDirectionHasExplicitFailureBehaviour` | Zero direction does not silently produce a misleading surface location | EditMode |

## D. Spherical mesh generation

### Existing EditMode tests
- `SphericalPresentationTests.MeshGeneratorBuildsSixFiniteFaceMeshes`
- `SphericalPlanetStudioTests.DisabledLayersProducePerfectBaseSphere`

### Additional tests required
| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-MSH-01 | `EveryTriangleIndexIsWithinVertexRange` | All triangle indices are valid for every generated face | EditMode |
| W001-MSH-02 | `TriangleWindingFacesOutward` | Face triangle winding and normals are consistently outward | EditMode |
| W001-MSH-03 | `AdjacentFaceSeamVerticesCoincide` | Shared seam samples have matching world-space positions within tolerance | EditMode |
| W001-MSH-04 | `MeshCountsMatchResolution` | Six face meshes have expected vertex/index counts for several resolutions | EditMode |
| W001-MSH-05 | `MeshBoundsContainAllVertices` | Every generated vertex lies within mesh bounds | EditMode |
| W001-MSH-06 | `MeshGenerationDoesNotLeakOnRepeatedBuild` | Repeated build/clear cycles release prior generated meshes | PlayMode |

## E. Presenter and demo scene lifecycle

No automated tests for `SphericalPlanetPresenter` or `SphericalWorldDemoBootstrap` were found in the inspected snapshot.

| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-PRE-01 | `BuildCreatesOneOwnedRootWithSixFaceMeshes` | Presenter creates the expected root and six generated faces, with no duplicate roots | PlayMode |
| W001-PRE-02 | `BuildTwiceReplacesPreviousGeneratedContent` | A second build replaces only presenter-owned content | PlayMode |
| W001-PRE-03 | `ClearGeneratedContentIsIdempotent` | Calling clear repeatedly leaves no generated content and causes no exception | PlayMode |
| W001-PRE-04 | `TerrainMaterialIsAppliedToAllGeneratedFaces` | All generated renderers use the configured material | PlayMode |
| W001-BOOT-01 | `GenerateDemoThenClearDemoLeavesNoGeneratedObjects` | Bootstrap generation and clearing are reversible and leave unrelated scene objects intact | PlayMode |
| W001-BOOT-02 | `GenerateDemoTwiceDoesNotDuplicateRuntimeSystems` | Repeated generation creates one active demo setup | PlayMode |

## F. Player movement and third-person camera

No automated tests for `SphericalPlayerController` or `SphericalThirdPersonCamera` were found in the inspected snapshot.

| ID | Test | Acceptance criterion | Layer |
|---|---|---|---|
| W001-MOV-01 | `ControllerInitializationRequiresValidDependencies` | Missing settings/camera/required references fail explicitly and diagnostically | PlayMode |
| W001-MOV-02 | `MovementRemainsTangentToLocalSphericalSurface` | Movement changes surface position without drifting radially away from the planet beyond tolerance | PlayMode |
| W001-MOV-03 | `MovementUpdatesLocalUpAcrossTheGlobe` | Local up follows the current radial direction as the player moves | PlayMode |
| W001-MOV-04 | `MovementSpeedIsStableAcrossFrameRates` | Equivalent simulated input over equivalent elapsed time yields comparable displacement | PlayMode |
| W001-CAM-01 | `CameraTracksInitializedTarget` | Camera follows the configured target and remains at the requested orbit distance | PlayMode |
| W001-CAM-02 | `MovementBasisIsTangentAndOrthonormal` | Camera-derived forward/right basis lies in the tangent plane and remains orthonormal | EditMode or PlayMode |
| W001-CAM-03 | `CameraRemainsStableNearPoles` | Camera basis and orientation remain finite during near-pole target movement | PlayMode |

## G. Manual visual acceptance

Run in the spherical demo scene and record screenshots/video:
- globe has six continuous faces without visible cracks;
- relief is visible and consistent at face seams;
- player remains attached to the spherical surface while moving around the globe;
- camera remains readable and follows the player without sudden flips;
- demo overlay reports the expected seed/radius and does not obscure essential gameplay;
- generate/clear cycles leave no orphan meshes or duplicate roots.

Manual visual QA supplements, but does not replace, the automated tests above.

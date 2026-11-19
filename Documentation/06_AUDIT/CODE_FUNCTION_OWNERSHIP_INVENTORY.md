> **HISTORICAL — NOT THE CURRENT SOURCE INVENTORY.** This inventory describes the 2026-11-17 post-refactor snapshot (59 C# files / 196 method declarations). The owner-supplied `Pokemon-clean.zip` snapshot dated 2026-11-19 contains 56 C# files (33 Runtime / 8 Editor / 15 Tests) and no Pokémon simulation Runtime source. See `CURRENT_SOURCE_INVENTORY_2026-11-19.md` and `POK-AGENT-001_CURRENT_SOURCE_RECONCILIATION.md`.

# C# Function Ownership Inventory

Snapshot reviewed: 2026-11-17
Source files: **59**
Method declarations: **196** (signature-level inventory; property accessors are not counted; Unity lifecycle methods are counted when explicitly declared).

The inventory records where each function lives after the refactor. It is not a substitute for Unity compilation or runtime tests.

## `Assets/Project/Editor/Content/ContentCatalogBuilder.cs`

- Types: ContentCatalogBuilder
- Methods: 8
  - `20` — `public static void BuildOrUpdateCatalog()`
  - `129` — `public static void ValidateCatalog()`
  - `269` — `private static GameObject FindUniquePrefabByName(`
  - `293` — `private static GameObject FindPreferredGrassPrefab(`
  - `339` — `private static List<string> FindPrefabPaths()`
  - `349` — `private static void SetObject(`
  - `364` — `private static void SetObjectArray(`
  - `386` — `private static void EnsureFolderExists(string folderPath)`

## `Assets/Project/Editor/Data/Pipeline/JsonToScriptableObjectProjection.cs`

- Types: JsonToScriptableObjectProjection
- Methods: 12
  - `19` — `public static void GenerateAll()`
  - `51` — `private static int GenerateWorlds(string sourceFolder)`
  - `72` — `private static int GenerateBiomes(string sourceFolder)`
  - `93` — `private static int GeneratePokemon(string sourceFolder)`
  - `114` — `private static string[] GetJsonFiles(string folder)`
  - `137` — `private static bool IsValidWorld(WorldDefinitionData data, string sourceFile)`
  - `146` — `private static bool IsValidBiome(BiomeDefinitionData data, string sourceFile)`
  - `155` — `private static bool IsValidPokemon(PokemonSpeciesData data, string sourceFile)`
  - `164` — `private static bool IsValidIdentity(string id, string sourceFile)`
  - `171` — `private static bool IsSafeId(string id)`
  - `180` — `private static string EnsureOutputPath(string category, string id)`
  - `187` — `private static void EnsureAssetFolder(string assetFolder)`

## `Assets/Project/Editor/Data/Pipeline/PokemonDataSyncMenu.cs`

- Types: PokemonDataSyncMenu
- Methods: 2
  - `14` — `public static void SyncDataToStreamingAssets()`
  - `48` — `private static void CopyDirectory(string source, string destination)`

## `Assets/Project/Editor/Tools/Blender/BlenderEnvironmentSync.cs`

- Types: BlenderEnvironmentSync, Manifest, ExportData, UnityData, ValidationData, ExtraData, OwnershipLedger, OwnershipEntry, Candidate
- Methods: 14
  - `98` — `public static void DryRun()`
  - `104` — `public static void ImportTestAssets()`
  - `122` — `private static void Execute(bool sync)`
  - `235` — `private static List<Candidate> DiscoverCandidates(`
  - `292` — `private static void ValidateManifest(Manifest manifest, string path)`
  - `330` — `private static void ValidateDestination(string assetPath)`
  - `346` — `private static void ImportCandidate(`
  - `412` — `private static OwnershipLedger LoadLedger(string projectRoot)`
  - `430` — `private static void SaveLedger(string projectRoot, OwnershipLedger ledger)`
  - `437` — `private static string NormalizeAssetPath(string path)`
  - `442` — `private static string ToAbsolute(string projectRoot, string assetPath)`
  - `452` — `private static void EnsureInsideRoot(string candidate, string root, string label)`
  - `465` — `private static string MakeProjectRelative(string projectRoot, string absolutePath)`
  - `475` — `private static string ComputeSha256(string path)`

## `Assets/Project/Runtime/Composition/MeadowGenerator.cs`

- Types: MeadowGenerator
- Methods: 1
  - `13` — `public void Generate()`

## `Assets/Project/Runtime/Composition/PokemonSpawnManager.cs`

- Types: PokemonSpawnManager
- Methods: 4
  - `29` — `private void Start() => GeneratePopulation();`
  - `32` — `public void GeneratePopulation()`
  - `93` — `private void ForwardPokemonEvent(PokemonSimulationEvent simulationEvent)`
  - `96` — `private void OnDestroy()`

## `Assets/Project/Runtime/Composition/PokemonUnityFactory.cs`

- Types: PokemonUnityFactory
- Methods: 4
  - `6` — `public PokemonAgent Instantiate(PokemonDefinition definition, Vector3 position)`
  - `19` — `public IWorldMovement GetMovement(PokemonAgent agent)`
  - `27` — `public void SetName(PokemonAgent agent, string instanceName)`
  - `32` — `public void Destroy(PokemonAgent agent)`

## `Assets/Project/Runtime/Composition/WorldBiomeGenerator.cs`

- Types: WorldBiomeGenerator
- Methods: 1
  - `15` — `public void Generate()`

## `Assets/Project/Runtime/Data/Contracts/WorldDataModels.cs`

- Types: WorldDefinitionData, WorldSizeData, WorldOriginData, MapResolutionData, WorldLayerReferenceData, BiomeDefinitionData, BiomeConditionsData, WeightedRangeData, BiomePresentationData, PokemonSpeciesData, PokemonHabitatPreferencesData, NumericRangeData, PokemonPresentationData, MapLayerManifestData, GeneratorMetadataData
- Methods: 0

## `Assets/Project/Runtime/Data/Loading/WorldDataRuntimeLoader.cs`

- Types: intentionally, WorldDataRuntimeLoader
- Methods: 5
  - `45` — `public static IEnumerator LoadGrid(string manifestRelativePath,`
  - `122` — `private static bool TryDecodeGrid(byte[] bytes, MapLayerManifestData manifest, out float[] values, out string error)`
  - `161` — `private static bool ValidateManifest(MapLayerManifestData manifest, out string error)`
  - `182` — `private static string CombineRelative(string folder, string file)`
  - `188` — `private static bool TryBuildUri(string relativePath, out string uri, out string error)`

## `Assets/Project/Runtime/Data/Projections/GeneratedDefinitionAssets.cs`

- Types: WorldDefinitionAsset, BiomeDefinitionAsset, PokemonSpeciesDefinitionAsset
- Methods: 0

## `Assets/Project/Runtime/Ecology/Contracts/IEcologyFoodService.cs`

- Types: IEcologyFoodService, IEcologyFoodSource
- Methods: 0

## `Assets/Project/Runtime/Ecology/Contracts/IEcologyResourceReadModel.cs`

- Types: IEcologyResourceReadModel, IEcologyResourceInstance
- Methods: 0

## `Assets/Project/Runtime/Ecology/Generation/BiomeEcologyGenerator.cs`

- Types: BiomeEcologyGenerator
- Methods: 10
  - `6` — `public static MeadowData Generate(`
  - `28` — `private static void GenerateWater(`
  - `55` — `private static void GenerateRocks(`
  - `70` — `private static void GenerateHumidity(`
  - `157` — `private static void GenerateVegetation(`
  - `213` — `private static void GenerateFlora(`
  - `253` — `private static void GenerateBushes(`
  - `306` — `private static void GeneratePoints(`
  - `328` — `private static void DrawCircle(`
  - `355` — `private static float FractalNoise(`

## `Assets/Project/Runtime/Ecology/Generation/WorldEcologyComposer.cs`

- Types: WorldEcologyComposer
- Methods: 1
  - `7` — `public static WorldEcologyData Compose(BiomeMapData biomeMap,`

## `Assets/Project/Runtime/Ecology/Resources/Definitions/FruitType.cs`

- Types: FruitType
- Methods: 0

## `Assets/Project/Runtime/Ecology/Resources/Instances/FruitBush.cs`

- Types: FruitBush
- Methods: 3
  - `34` — `public void Initialize(FruitType type, int capacity, int startingFruit, float productionTime)`
  - `48` — `private void Update()`
  - `60` — `public bool TryConsumeFruit(FruitType requestedType)`

## `Assets/Project/Runtime/Ecology/Resources/Systems/FruitBushManager.cs`

- Types: FruitBushManager
- Methods: 8
  - `79` — `public void Initialize(Transform groundTransform)`
  - `84` — `public void Initialize(Transform groundTransform, int seed)`
  - `119` — `private void GenerateBushes(`
  - `159` — `public bool TryFindNearestFood(Vector3 position, FruitType foodType, float searchRadius, out IEcologyFoodSource source)`
  - `166` — `public bool TryConsume(IEcologyFoodSource source, FruitType foodType)`
  - `173` — `private bool FindNearestBush(`
  - `211` — `private bool ConsumeFruit(`
  - `222` — `private void ClearBushes()`

## `Assets/Project/Runtime/Ecology/Resources/Systems/ResourceManager.cs`

- Types: ResourceManager
- Methods: 10
  - `145` — `public float GetAvailableFoodAtIndex(int index)`
  - `155` — `public float GetInitialFoodAtIndex(int index)`
  - `169` — `public void Initialize(`
  - `207` — `private void Update()`
  - `223` — `private void RegenerateFood(float deltaTime)`
  - `244` — `public bool FindNearestFood(`
  - `333` — `public float ConsumeFood(`
  - `358` — `public float GetAvailableFood(Vector3 worldPosition)`
  - `372` — `private int GetWorldIndex(Vector3 worldPosition)`
  - `401` — `private Vector3 GetCellWorldPosition(int x, int y)`

## `Assets/Project/Runtime/Ecology/World/EcologySpawnLocationProvider.cs`

- Types: EcologySpawnLocationProvider
- Methods: 3
  - `22` — `public bool TryFindSpawnLocation(HabitatSpawnRequirements requirements, out Vector3 position)`
  - `43` — `public Vector3 GetOffspringLocation(Vector3 parentPosition, float radius)`
  - `53` — `private bool IsSuitable(HabitatSpawnRequirements requirements, int index)`

## `Assets/Project/Runtime/Ecology/World/MeadowData.cs`

- Types: MeadowData
- Methods: 1
  - `27` — `public int GetIndex(int x, int y)`

## `Assets/Project/Runtime/Ecology/World/WorldEcologyData.cs`

- Types: WorldEcologyData
- Methods: 1
  - `29` — `public int GetIndex(int x, int y)`

## `Assets/Project/Runtime/Ecology/World/WorldEcologyLayer.cs`

- Types: WorldEcologyLayer
- Methods: 0

## `Assets/Project/Runtime/Infrastructure/Content/ProjectContentCatalog.cs`

- Types: ProjectContentCatalog
- Methods: 3
  - `47` — `public PokemonDefinition FindPokemonDefinition(string pokemonName)`
  - `66` — `public BiomeDefinition FindBiomeDefinition(string biomeName)`
  - `85` — `public WorldDefinition FindWorldDefinition(string worldName)`

## `Assets/Project/Runtime/Infrastructure/Telemetry/EcosystemDataCollector.cs`

- Types: EcosystemDataCollector
- Methods: 5
  - `23` — `public void Initialize(IEcologyResourceReadModel ecology, IPokemonPopulationReadModel population)`
  - `33` — `public EcosystemSnapshot Collect(float simulationTime)`
  - `54` — `private void CollectPokemonStatistics(`
  - `156` — `private void CollectSpeciesStatistics(`
  - `250` — `private void CollectFruitStatistics(`

## `Assets/Project/Runtime/Infrastructure/Telemetry/EcosystemLogger.cs`

- Types: EcosystemLogger
- Methods: 17
  - `56` — `private void OnEnable()`
  - `61` — `private void OnDisable()`
  - `74` — `private void Start()`
  - `97` — `private void Update()`
  - `134` — `private void TrySubscribeEventSource()`
  - `150` — `private bool CreateSession()`
  - `281` — `private void RecordSnapshot(bool includeIndividuals)`
  - `300` — `private void RecordIndividualSnapshot()`
  - `312` — `private void WriteWorldStatistics(`
  - `328` — `private void WritePopulationStatistics(`
  - `369` — `private void WriteResourceStatistics(`
  - `394` — `private void WriteFruitBushStatistics(`
  - `438` — `private void WriteBehaviorStatistics(`
  - `503` — `private void WriteIndividualStatistics(`
  - `550` — `private void HandleEcosystemEvent(PokemonSimulationEvent simulationEvent)`
  - `570` — `private string Format(float value)`
  - `578` — `private string EscapeCsv(string value)`

## `Assets/Project/Runtime/Infrastructure/Telemetry/EcosystemSnapshot.cs`

- Types: SpeciesSnapshot, IndividualSnapshot, FruitTypeSnapshot, EcosystemSnapshot
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Behaviour/Actions/PokemonForagingBehaviour.cs`

- Types: PokemonForagingBehaviour
- Methods: 5
  - `31` — `public void SetSleeping(bool sleeping)`
  - `39` — `public void Tick(float deltaTime, float satiety, float foodCooldownDuration)`
  - `72` — `private void Seek(float deltaTime, float satiety)`
  - `98` — `private void Eat(float satiety, float cooldownDuration)`
  - `114` — `private void SetState(PokemonActionState state)`

## `Assets/Project/Runtime/Pokemon/Behaviour/Actions/PokemonWanderBehaviour.cs`

- Types: PokemonWanderBehaviour
- Methods: 3
  - `13` — `public void Tick(float speed, float deltaTime)`
  - `19` — `public void SetTarget(Vector3 position) => target = position;`
  - `21` — `public void ChooseDestination()`

## `Assets/Project/Runtime/Pokemon/Behaviour/PokemonActionState.cs`

- Types: PokemonActionState, PokemonActivityPattern
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Behaviour/PokemonAgent.cs`

- Types: PokemonAgent, ActivityPattern
- Methods: 3
  - `64` — `public void Initialize(PokemonDefinition pokemonDefinition, Vector3 center,`
  - `97` — `private void Update()`
  - `126` — `private void EmitEvent(string eventName)`

## `Assets/Project/Runtime/Pokemon/Behaviour/Scheduling/PokemonScheduleCalculator.cs`

- Types: PokemonScheduleCalculator
- Methods: 3
  - `4` — `public static bool TryCalculateCurrentHour(float timeNow, float secondsPerDay,`
  - `14` — `public static bool IsActivePeriod(PokemonActivityPattern pattern,`
  - `22` — `public static bool IsWithinHours(float hour, float startHour, float endHour)`

## `Assets/Project/Runtime/Pokemon/Contracts/IPokemonFactory.cs`

- Types: IPokemonFactory
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Contracts/IPokemonPopulationReadModel.cs`

- Types: IPokemonPopulationReadModel
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Contracts/IPokemonReproductionService.cs`

- Types: IPokemonReproductionService
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Definitions/PokemonDefinition.cs`

- Types: PokemonDefinition
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Individual/Identity/PokemonIndividualId.cs`

- Types: PokemonIndividualId
- Methods: 5
  - `12` — `public static PokemonIndividualId NewId() => new PokemonIndividualId(Guid.NewGuid().ToString("N"));`
  - `14` — `public override string ToString() => value ?? string.Empty;`
  - `15` — `public bool Equals(PokemonIndividualId other) => string.Equals(value, other.value, StringComparison.Ordinal);`
  - `16` — `public override bool Equals(object obj) => obj is PokemonIndividualId other && Equals(other);`
  - `17` — `public override int GetHashCode() => value == null ? 0 : value.GetHashCode();`

## `Assets/Project/Runtime/Pokemon/Individual/Identity/PokemonSimulationEvent.cs`

- Types: PokemonSimulationEvent
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Individual/Needs/PokemonNeeds.cs`

- Types: PokemonNeeds
- Methods: 3
  - `12` — `public void Initialize(float initialSatiety)`
  - `17` — `public void UpdateSatiety(float deltaTime, float satietyDrain)`
  - `25` — `public void RestoreSatiety(float foodAmount, float recoveryPerFood)`

## `Assets/Project/Runtime/Pokemon/Individual/Systems/PokemonDailyRoutine.cs`

- Types: PokemonDailyRoutine
- Methods: 2
  - `29` — `public void Tick(float simulationTime)`
  - `36` — `public bool ShouldSleep(float satiety) => !IsActivePeriod && satiety > emergencyWakeSatiety;`

## `Assets/Project/Runtime/Pokemon/Individual/Systems/PokemonMortalitySystem.cs`

- Types: PokemonMortalitySystem
- Methods: 1
  - `16` — `public bool Tick(float deltaTime, float satiety)`

## `Assets/Project/Runtime/Pokemon/Individual/Systems/PokemonReproductionSystem.cs`

- Types: PokemonReproductionSystem
- Methods: 2
  - `18` — `public void Tick(float deltaTime, float satiety, float totalFoodConsumed, Vector3 position,`
  - `41` — `private void Cancel(System.Action<string> emit)`

## `Assets/Project/Runtime/Pokemon/Population/PokemonPopulationProfile.cs`

- Types: PokemonPopulationProfile
- Methods: 0

## `Assets/Project/Runtime/Pokemon/Population/PokemonPopulationService.cs`

- Types: PokemonPopulationService
- Methods: 9
  - `36` — `public void GeneratePopulation()`
  - `56` — `public bool TrySpawnOffspring(PokemonDefinition definition, Vector3 parentPosition)`
  - `72` — `public void Clear()`
  - `83` — `private void SpawnSpecies(PokemonPopulationProfile profile)`
  - `103` — `private bool CreatePokemon(PokemonDefinition definition, Vector3 position, int individualNumber)`
  - `129` — `private PokemonPopulationProfile FindProfile(PokemonDefinition definition)`
  - `136` — `private static HabitatSpawnRequirements ToHabitatRequirements(PokemonDefinition definition)`
  - `143` — `private void ForwardEvent(PokemonSimulationEvent simulationEvent)`
  - `146` — `private void RemoveDestroyedEntries()`

## `Assets/Project/Runtime/Presentation/Ecology/ResourceMinimap.cs`

- Types: ResourceMinimap
- Methods: 5
  - `42` — `private void Start()`
  - `77` — `private void Update()`
  - `97` — `private void UpdateMinimap()`
  - `149` — `private Color GetResourceColor(`
  - `186` — `private void OnDestroy()`

## `Assets/Project/Runtime/Presentation/Ecology/Resources/FruitBushVisuals.cs`

- Types: FruitBushVisuals
- Methods: 6
  - `17` — `private void OnEnable()`
  - `24` — `private void OnDisable()`
  - `30` — `private void Refresh()`
  - `40` — `private void CreateVisuals()`
  - `71` — `private GameObject GetVisualPrefab()`
  - `81` — `private void ClearVisuals()`

## `Assets/Project/Runtime/Presentation/Simulation/SimulationTimeController.cs`

- Types: SimulationTimeController
- Methods: 9
  - `20` — `private void Start()`
  - `32` — `private void Update()`
  - `53` — `public void IncreaseSpeed()`
  - `63` — `public void DecreaseSpeed()`
  - `73` — `public void TogglePause()`
  - `84` — `public void SetNormalSpeed()`
  - `95` — `public void SetSpeed(int index)`
  - `106` — `private void ApplySpeed()`
  - `113` — `private void OnDestroy()`

## `Assets/Project/Runtime/Presentation/World/Biomes/WorldBiomeMapRenderer.cs`

- Types: WorldBiomeMapRenderer
- Methods: 6
  - `21` — `public void Render(`
  - `82` — `private Color GetPixelColor(`
  - `117` — `private Color GetGrayscale(float value)`
  - `129` — `private void ApplyTexture(Texture2D texture)`
  - `144` — `private void DestroyGeneratedTextures()`
  - `167` — `private void OnDestroy()`

## `Assets/Project/Runtime/Presentation/World/Maps/MapRenderer.cs`

- Types: MapRenderer
- Methods: 6
  - `13` — `public void Render(MeadowData data, BiomeDefinition definition)`
  - `48` — `private Color GetPixelColor(`
  - `99` — `private Color GetGrayscaleColor(`
  - `111` — `private Color GetCombinedColor(`
  - `139` — `private void ApplyTexture(Texture2D texture)`
  - `154` — `private void OnDestroy()`

## `Assets/Project/Runtime/World/Biomes/Data/BiomeLayer.cs`

- Types: BiomeLayer
- Methods: 0

## `Assets/Project/Runtime/World/Biomes/Data/BiomeMapData.cs`

- Types: BiomeMapData
- Methods: 1
  - `15` — `public int GetIndex(int x, int y)`

## `Assets/Project/Runtime/World/Biomes/Definitions/BiomeDefinition.cs`

- Types: BiomeDefinition
- Methods: 0

## `Assets/Project/Runtime/World/Contracts/IWorldMovement.cs`

- Types: IWorldMovement
- Methods: 0

## `Assets/Project/Runtime/World/Contracts/IWorldSpawnLocationQuery.cs`

- Types: IWorldSpawnLocationQuery, HabitatSpawnRequirements
- Methods: 0

## `Assets/Project/Runtime/World/Environment/EnvironmentGrid.cs`

- Types: EnvironmentGrid
- Methods: 3
  - `22` — `public float GetValue(int x, int y)`
  - `30` — `public bool TrySampleBilinear(float worldX, float worldZ, out float value)`
  - `52` — `private static float Lerp(float a, float b, float t) => a + (b - a) * t;`

## `Assets/Project/Runtime/World/Generation/Biomes/WorldBiomeFieldGenerator.cs`

- Types: WorldBiomeFieldGenerator
- Methods: 1
  - `7` — `public static BiomeMapData Generate(WorldDefinition definition)`

## `Assets/Project/Runtime/World/Movement/PokemonMovementController.cs`

- Types: PokemonMovementController
- Methods: 3
  - `14` — `public Vector3 ConstrainToWorld(Vector3 position)`
  - `22` — `public void Configure(Vector3 worldCenter, float width, float depth)`
  - `30` — `public void MoveTowards(Vector3 target, float speed, float deltaTime)`

## `Assets/Project/Runtime/World/Placement/Vegetation/GrassVegetationSpawner.cs`

- Types: GrassVegetationSpawner
- Methods: 7
  - `30` — `public void Initialize(Texture2D vegetationMap, Transform ground)`
  - `108` — `private void SpawnGrass(`
  - `156` — `private GameObject GetPrefabForPosition(int x, int z)`
  - `168` — `private void CacheValidPrefabs()`
  - `176` — `private float GetDeterministicValue(int x, int z, int offset)`
  - `193` — `private void CreateContainer(Transform ground)`
  - `205` — `public void Clear()`

## `Assets/Project/Runtime/World/Worlds/Definitions/WorldDefinition.cs`

- Types: WorldDefinition
- Methods: 0

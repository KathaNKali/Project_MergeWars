# SYS_Generator.md - Generator Variant

_Synced with code: Generator.cs, GeneratorConfig.cs, GeneratorSpawner.cs, GeneratorLoadout.cs. `GeneratorSetupTool.cs` was not re-read this pass._

## Confirmed
- CONFIRMED: Generator is a 3D world object on the battlefield (not UI),
  positioned outside/below the grid, tapped via Physics.Raycast -
  consistent with MergeSystem's drag/drop input model and Touchdown-style
  camera framing.
- CONFIRMED: spawned hero is instantiated via PoolManager (see
  SYS_PoolManager.md), not raw Instantiate(). Generator requests an object
  from the pool, does not own or track it after handing it to GridManager.
- CONFIRMED: spawned hero is parented to a champions container (or the
  grid), never to the Generator itself - Generator's involvement ends at
  spawn.

## Depends on
- ManaEconomy (CanAfford, Spend) - the real implementation (see
  SYS_ManaEconomy.md)
- GridManager (TryGetOpenSlot, PlaceOccupant, GetWorldPosition) - the
  Merge grid instance
- PoolManager (Get)
- GeneratorConfig (ScriptableObject - manaCost, heroPool of
  HeroDefinition[])
- HeroDefinition (per-hero data asset - see SYS_HeroDefinition.md) and
  HeroInstance (MonoBehaviour set on the spawned hero at spawn time)
- MergeSystem (optional - see SYS_MergeSystem.md) - if assigned, Generator
  also adds/initializes a HeroMergeInput on the spawned hero so it becomes
  mergeable

## Implementation notes
- Location: `Assets/Scripts/Generator/Generator.cs` (namespace
  `MergeWars.Generators`), MonoBehaviour,
  `[RequireComponent(typeof(Collider))]` so it is raycast-hittable.
- `TryTap()` order: (0) `config`, `gridManager`, `manaEconomy`,
  `poolManager` must all be assigned, else no-op; (1)
  `ManaEconomy.CanAfford(config.manaCost)` - false is a no-op, no mana
  spent; (2) `GridManager.TryGetOpenSlot()` - none is a no-op, no mana
  spent; (3) `config.GetRandomHeroDefinition()` - null definition or null
  `definition.prefab` is a no-op, no mana spent; (4) spend mana,
  `PoolManager.Get(definition.prefab)`, reparent to `championsContainer`
  (never the Generator's own transform), position at
  `GridManager.GetWorldPosition(slot) + Vector3.up *
  definition.spawnHeightOffset`, get-or-add `HeroInstance` and set
  `definition` + `starLevel = 1`, optionally get-or-add and initialize
  `HeroMergeInput`, then `GridManager.PlaceOccupant(slot, hero)`.
- KNOWN EDGE (not changed, flagged): mana is spent before
  `PoolManager.Get` returns; if `Get` returns null, `TryTap()` returns
  false after mana was already spent.
- Tap entry point is `OnMouseDown()` (Unity's built-in Physics.Raycast-
  based mouse dispatch against the required Collider). `TryTap()` is
  public so a future input dispatcher can call it directly.
- No unlock check exists in code, per spec - not even a stub.
- If the optional `mergeSystem` field is assigned, spawned heroes get a
  `HeroMergeInput` wired via `Initialize(gridManager, mergeSystem)`
  (tap-tap-select and drag-and-drop - see SYS_MergeSystem.md). If left
  unassigned, spawned heroes have no merge input; the spawn flow is
  unaffected.
- **GeneratorConfig** (`Assets/Scripts/Generator/GeneratorConfig.cs`):
  - `int manaCost` (default 10 - ASSUMED PLACEHOLDER; stays on this
    generator-side wrapper, not per-hero)
  - `HeroDefinition[] heroPool` - one chosen at random each spawn via
    `GetRandomHeroDefinition()`; empty/null pool means `TryTap()` no-ops
    before spending mana.
  - `string barracksID`, `string barracksName`, and
    `GeneratorType classType` (nested enum: Tank, Assassin, Support,
    Controller, DamageDealer, Marksman, Artillery). These are metadata
    only - nothing in the current code reads them. Their relationship to
    `HeroRole` is undocumented (see Open Unknowns).
- **RETIRED:** `HeroClassConfig` / `HeroClassId` (Ground/Air/Vehicles) were
  replaced by `GeneratorConfig` + `HeroDefinition`/`HeroRole`. Heroes are
  authored individually as `HeroDefinition` assets. The old per-class mana
  costs (Ground 10 / Air 15 / Vehicles 20) no longer apply.
- `Assets/Editor/GeneratorSetupTool.cs` (menu: `MergeWars/Setup/Create
  Placeholder Hero Assets`) creates placeholder `HeroDefinition` assets
  and a sample `GeneratorConfig`. NOT re-read this pass; it may need
  updating for the current `HeroStats` fields and `GeneratorConfig`
  fields (see Not verified).

## Wiring scene-object dependencies when Generator is spawned from a prefab
- CONFIRMED: `gridManager`, `manaEconomy`, `poolManager`,
  `championsContainer`, and `mergeSystem` are scene objects. A prefab
  asset cannot hold a reference to a scene object, so a Generator
  instantiated from a prefab always starts with these fields empty.
- `Generator.Initialize(GridManager gridManager, ManaEconomy manaEconomy,
  PoolManager poolManager, Transform championsContainer, MergeSystem
  mergeSystem = null)` wires them at runtime - same pattern as
  `HeroMergeInput.Initialize(...)`. Whoever instantiates a Generator
  prefab must call it immediately after `Instantiate()`.
- Hand-placed scene Generators can instead assign these in the Inspector
  and skip `Initialize`.

## GeneratorSpawner (game-start placement of Generators themselves)
- CONFIRMED: separate from `Generator.TryTap()` (which spawns heroes).
  GeneratorSpawner places Generator *prefabs* into slots on a dedicated
  Generator grid (a second GridManager instance, separate from the Merge
  grid) once, at game start.
- Location: `Assets/Scripts/Generator/GeneratorSpawner.cs`,
  `GeneratorLoadout.cs` (namespace `MergeWars.Generators`).
- `GeneratorLoadout` (ScriptableObject) - authored config only: an
  ordered `Generator[] generatorPrefabs`. Read at `Start()`, never
  written back. Not a player-selection system.
- `GeneratorSpawner` (MonoBehaviour): serialized refs to the Generator
  grid's `GridManager` (`generatorGridManager`), a `GeneratorLoadout`,
  `generatorsContainer`, and the Merge-side scene objects each spawned
  Generator needs (`mergeGridManager`, `manaEconomy`, `poolManager`,
  `championsContainer`, optional `mergeSystem`).
  - `Start()` calls public `SpawnLoadout()`. Logs a warning and no-ops if
    the grid manager, loadout, or prefab list is missing/empty.
  - Resizes the Generator grid via `GridManager.SetDimensions(1, count)`
    (ASSUMED PLACEHOLDER layout), then per prefab in order: skips null
    entries, gets an open slot (warns and stops if none),
    `Instantiate()`s directly (no PoolManager - one-time spawn), sets
    position from `GetWorldPosition(slot)`, calls `Generator.Initialize(
    mergeGridManager, manaEconomy, poolManager, championsContainer,
    mergeSystem)`, and `PlaceOccupant(slot, instance.gameObject)`.
  - Note: `SpawnLoadout()` is public and can be called twice; it does not
    clear previously spawned generators or reset the Generator grid's
    occupants. Not guarded - flagged.
- `GridManager.SetDimensions(int rows, int columns)` only resizes and
  rebuilds; sizing policy stays outside GridManager.

## Open Unknowns
- Generator grid layout beyond a single row (wrapping to multiple rows) is
  not designed.
- How "whatever the player has chosen" maps onto `GeneratorLoadout` is
  unresolved (swap the assigned asset vs. a runtime-filtered list).
- `GeneratorConfig.manaCost` default (10) and any per-generator values are
  ASSUMED PLACEHOLDERS - no balance doc specifies real numbers.
- `GeneratorConfig.GeneratorType` / `classType` vs `HeroRole`: how they
  relate, and whether `classType` should constrain `heroPool`, is
  undocumented.
- `barracksID` uniqueness/usage is undefined (no validator, no consumer).

## Not verified
- `GeneratorSetupTool.cs` was not shared this pass. Nothing was compiled
  or run in Unity.
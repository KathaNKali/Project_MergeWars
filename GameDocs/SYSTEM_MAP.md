# SYSTEM_MAP.md — Generator Variant

Dependency edges between systems. An arrow (A → B) means A depends on /
calls into B; B has no knowledge of A. This file only lists systems that
currently exist (as real implementations or explicit stubs) in the
codebase — see PROJECT_INDEX.md for build status.

## Current systems

- **GridManager** (`Assets/Scripts/Grid/`) — foundational. No dependencies
  on other gameplay systems. Per-instance grid (not a singleton) — a thin
  MonoBehaviour wrapper around a `GridData` instance (plain C# class
  owning the slot array and slot lookup/mutation logic). Multiple
  GridManager components can coexist in-scene as independent grids (e.g.
  Merge grid, Generator grid), each with its own dimensions/anchor/cell
  size, wired to consumers via explicit serialized references. See
  SYS_GridManager.md.

- **PoolManager** (`Assets/Scripts/Pooling/PoolManager.cs`) — foundational.
  No dependencies on other gameplay systems. See SYS_PoolManager.md.

- **CurrencyEconomy** (`Assets/Scripts/Economy/CurrencyEconomy.cs`) — new
  shared base class (current amount, max cap, afford/spend, cap-upgrade
  tiers). Foundational. No dependencies. Not used directly — only via its
  subclasses below.

- **ManaEconomy** (`Assets/Scripts/Economy/ManaEconomy.cs`) — real
  implementation (no longer a stub), subclass of CurrencyEconomy.
  Foundational. No dependencies. See SYS_ManaEconomy.md.

- **CoinEconomy** (`Assets/Scripts/Economy/CoinEconomy.cs`) — new, same
  pattern as ManaEconomy, subclass of CurrencyEconomy. Foundational. No
  dependencies. No consumer/spender exists yet (expected). See
  SYS_CoinEconomy.md.

- **HeroRole / HeroStats / HeroDefinition / HeroInstance**
  (`Assets/Scripts/Heroes/`) — new Hero Character Data Architecture layer.
  `HeroRole` (enum) and `HeroStats` (struct) are foundational, no
  dependencies. `HeroDefinition` (ScriptableObject, one asset per
  individual hero) is foundational config data, no runtime dependencies.
  `HeroInstance` (MonoBehaviour) depends only on `HeroDefinition` (its own
  `definition` reference) — attached to spawned hero GameObjects by
  Generator. Also scaffolded-only placeholder types with no real
  dependencies or behavior: `HeroSubclass`, `HeroSpecies`,
  `HeroBackground`, `HeroFeat`, `HeroEquipmentSpell`. See
  SYS_HeroDefinition.md.

- **GeneratorConfig** (`Assets/Scripts/Generator/GeneratorConfig.cs`) —
  ScriptableObject config, replaces the retired `HeroClassConfig`.
  Depends on `HeroDefinition` (data reference, `heroPool` array).
  Referenced by Generator.

- **Generator** (`Assets/Scripts/Generator/Generator.cs`) — depends on:
  - GridManager (TryGetOpenSlot, PlaceOccupant, GetWorldPosition)
  - PoolManager (Get)
  - ManaEconomy (CanAfford, Spend) — now the real implementation
  - GeneratorConfig (data reference) — replaces HeroClassConfig
  - HeroInstance (adds/sets on spawned hero at spawn time)
  - HeroDefinition (prefab, `spawnHeightOffset`, via GeneratorConfig.heroPool)
  - MergeSystem (optional — adds/initializes HeroMergeInput on spawned hero if assigned)
  See SYS_Generator.md.

- **MergeSystem** (`Assets/Scripts/Merge/MergeSystem.cs`) — depends on:
  - GridManager (GetOccupant, RemoveOccupant)
  - PoolManager (Release)
  - HeroInstance (reads definition.heroId + starLevel, mutates starLevel)
  No dependency on ManaEconomy/CoinEconomy or combat. See SYS_MergeSystem.md.

- **HeroMergeInput** (`Assets/Scripts/Merge/HeroMergeInput.cs`) — attached
  to spawned heroes by Generator (or pre-authored on a hero prefab).
  Requires Collider + HeroInstance on the same GameObject. Depends on:
  - GridManager (TryGetSlotIndexForOccupant, TryGetNearestOpenSlotIndex,
    RemoveOccupant, PlaceOccupant, GetWorldPosition, CellSizeX/CellSizeZ)
  - MergeSystem (TryMerge)
  - HeroInstance (GetComponent on the drop target)
  Also uses Camera.main and the legacy Input API for pointer position.
  Implements both confirmed merge triggers (tap-tap-select and
  drag-and-drop) in a single component. See SYS_MergeSystem.md.

- **GeneratorSpawner / GeneratorLoadout** (`Assets/Scripts/Generator/`) —
  `GeneratorSpawner` depends on:
  - GridManager (the Generator grid instance — `SetDimensions`,
    `TryGetOpenSlot`, `GetWorldPosition`, `PlaceOccupant`)
  - GeneratorLoadout (data reference — ordered `Generator[]` list)
  - Generator (Instantiate + `Initialize(...)` on each spawned prefab)
  - ManaEconomy, PoolManager, MergeSystem, and the Merge grid's
    GridManager (scene references handed on to each Generator via
    `Initialize`; GeneratorSpawner itself never calls them)
  Places Generator prefabs themselves into the Generator grid once at
  game start; distinct from Generator.TryTap() (which spawns heroes).
  See SYS_Generator.md.

## Dependency edges (summary)

```
Generator → GridManager
Generator → PoolManager
Generator → ManaEconomy (real)
Generator → GeneratorConfig (data)
Generator → HeroInstance (adds/sets on spawn)
Generator → HeroDefinition (prefab, spawnHeightOffset)
Generator → MergeSystem (optional, adds/initializes HeroMergeInput)
GeneratorConfig → HeroDefinition (data, heroPool array)
HeroInstance → HeroDefinition (definition reference)
MergeSystem → GridManager (GetOccupant, RemoveOccupant)
MergeSystem → PoolManager (Release)
MergeSystem → HeroInstance (reads/mutates)
HeroMergeInput → GridManager (TryGetSlotIndexForOccupant, TryGetNearestOpenSlotIndex, RemoveOccupant, PlaceOccupant, GetWorldPosition, CellSizeX/Z)
HeroMergeInput → HeroInstance (drop-target lookup)
HeroMergeInput → MergeSystem (TryMerge)
GeneratorSpawner → GridManager (Generator grid — SetDimensions, TryGetOpenSlot, GetWorldPosition, PlaceOccupant)
GeneratorSpawner → GeneratorLoadout (data)
GeneratorSpawner → Generator (Instantiate, Initialize)
GeneratorSpawner → ManaEconomy / PoolManager / MergeSystem / Merge-grid GridManager (references passed through to Generator.Initialize only)
ManaEconomy → CurrencyEconomy (base class, not a runtime dependency edge)
CoinEconomy → CurrencyEconomy (base class, not a runtime dependency edge)
```

Beyond the edges listed above, no other system depends on GridManager,
PoolManager, ManaEconomy, or CoinEconomy — they are foundational/
leaf-consumed only.
CoinEconomy currently has no consumer at all (no spender/shop system
exists yet) — expected, not a gap.

## Retired
`HeroClassConfig` and `HeroClassId` (Ground/Air/Vehicles) have been
removed and replaced by `GeneratorConfig` + `HeroDefinition`/`HeroRole`
(Tank/MeleeDps/RangedDps/Support/Controller) — see SYS_HeroDefinition.md.

## Not yet built
EnemyBaseManager, LevelProgression/base-transition logic, unlock system,
discard mechanic, save/load wiring, combat, and the reward hook that
grants Mana/Coins on base destruction (both CurrencyEconomy subclasses
intentionally have no `Add`-style method yet). These are referenced as
future dependents/dependencies in the SYS_*.md "Depends on" sections but
do not exist in code yet.

_Synced with code in the doc-sync pass (edges above were reconciled against the C# source; HeroDefinitionValidator and GeneratorSetupTool were not re-read)._
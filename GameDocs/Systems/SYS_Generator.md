## Confirmed this pass
- CONFIRMED: Generator is a 3D world object on the battlefield (not UI),
  positioned outside/below the grid, tapped via Physics.Raycast — consistent
  with MergeSystem's drag/drop input model and Touchdown-style camera framing.
- CONFIRMED: spawned hero is instantiated via PoolManager (see
  SYS_PoolManager.md), not raw Instantiate(). Generator requests an object
  from the pool, does not own or track it after handing it to GridManager.
- CONFIRMED: spawned hero is parented to a champions container (or the grid),
  never to the Generator itself — Generator's involvement ends at spawn.

## Depends on (updated)
- ManaEconomy (CanAfford, spend)
- GridManager (TryGetOpenSlot, PlaceOccupant, GetWorldPosition)
- PoolManager (request pooled hero instance by prefab)
- Config: GeneratorConfig (ScriptableObject — manaCost, heroPool of HeroDefinition[]) — replaces the retired HeroClassConfig
- HeroDefinition (per-hero data asset — see SYS_HeroDefinition.md) and HeroInstance (MonoBehaviour set on the spawned hero at spawn time)
- MergeSystem (optional — see SYS_MergeSystem.md) — if assigned, Generator also adds/initializes a HeroMergeInput component on the spawned hero so it becomes mergeable

## Implementation notes (added after PoolManager + Generator build)
- Location: `Assets/Scripts/Generator/Generator.cs` (namespace
  `MergeWars.Generators`), MonoBehaviour, `[RequireComponent(typeof(Collider))]`
  so it's guaranteed raycast-hittable.
- `TryTap()` implements the confirmed order exactly: (1)
  `ManaEconomy.CanAfford(config.manaCost)` — false is a no-op, no mana
  spent; (2) `GridManager.TryGetOpenSlot()` — none is a no-op, no mana
  spent; (3) both pass → spend mana, `PoolManager.Get(config.heroPrefab)`,
  reparent to a serialized `championsContainer` (never the Generator's own
  transform), position via `GridManager.GetWorldPosition(slot)`, then
  `GridManager.PlaceOccupant(slot, hero)`.
- Tap entry point is `OnMouseDown()` (Unity's built-in
  Physics.Raycast-based mouse dispatch against the required Collider).
  `TryTap()` itself is public so a future dedicated input dispatcher can
  call it directly instead, without changing Generator's internals.
- No unlock check exists in code, per spec — not even a stub.
- **RETIRED:** `HeroClassConfig`/`HeroClassId` (Ground/Air/Vehicles) have
  been replaced — see SYS_HeroDefinition.md. Heroes are now authored
  individually as `HeroDefinition` assets (one per hero), each carrying its
  own `heroId`, `HeroRole` (Tank/MeleeDps/RangedDps/Support/Controller),
  `HeroStats`, and `prefab` reference.
- `GeneratorConfig` implemented at `Assets/Scripts/Generator/GeneratorConfig.cs`
  with `int manaCost` (stays on this generator-side wrapper, not per-hero)
  and `HeroDefinition[] heroPool` — a generator can hold multiple different
  heroes; `GeneratorConfig.GetRandomHeroDefinition()` picks one at random
  each time the generator spawns (same random-selection behavior as
  before, now at the individual-hero level). If the pool is empty/null,
  `TryTap()` no-ops before spending mana.
- `Generator.TryTap()` now also gets-or-adds a `HeroInstance` component on
  the spawned hero GameObject and sets `definition` (the chosen
  `HeroDefinition`) and `starLevel = 1` before placing it into the grid —
  this is the occupant identity component MergeSystem reads/mutates.
- If `Generator`'s optional `mergeSystem` field is assigned, `TryTap()`
  also gets-or-adds a `HeroMergeInput` component on the spawned hero and
  calls `Initialize(gridManager, mergeSystem)` on it, wiring up both
  confirmed merge input triggers (tap-tap-select and drag-and-drop — see
  SYS_MergeSystem.md). If `mergeSystem` is left unassigned, spawned heroes
  simply have no merge input; Generator's spawn flow is unaffected either
  way.
- `Assets/Editor/GeneratorSetupTool.cs` (menu: `MergeWars/Setup/Create
  Placeholder Hero Assets`) updated accordingly — creates placeholder
  `HeroDefinition` assets (one per placeholder prefab, each with a unique
  `heroId` and `HeroRole`) plus a sample `GeneratorConfig` referencing them,
  at `Assets/Configs/Heroes/*.asset` and
  `Assets/Configs/Generators/GeneratorConfig_Placeholder.asset`. Uses the
  same real Unity APIs as before (`GameObject.CreatePrimitive`,
  `PrefabUtility.SaveAsPrefabAsset`, `ScriptableObject.CreateInstance`).
- `// TODO(design)` markers left in code:
  - `HeroClassConfig.manaCost` default (10) and the per-class values the
    setup tool assigns (Ground 10 / Air 15 / Vehicles 20) are ASSUMED
    PLACEHOLDER — no balance doc specifies real numbers.
  - `ManaEconomy` (see below) is a stub, not the real system.

## Dependency stub note
- No `SYS_ManaEconomy.md` exists yet anywhere in `/GameDocs/`, so
  `ManaEconomy` does not exist as a real system. A minimal stub was added
  at `Assets/Scripts/Economy/ManaEconomy.cs` covering only
  `CanAfford(int)` / `Spend(int)` and a serialized starting mana value —
  marked `// TODO(design): replace with real ManaEconomy per
  SYS_ManaEconomy.md` once that doc and system exist. Generator depends on
  this stub's public interface only, so swapping in the real
  implementation later should not require Generator changes.
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
- PoolManager (request pooled hero instance by class/prefab)
- Config: HeroClassConfig (ScriptableObject — classId, manaCost, heroPrefab)

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
- `HeroClassConfig` implemented at `Assets/Scripts/Generator/HeroClassConfig.cs`
  with `HeroClassId` enum (`Ground`, `Air`, `Vehicles`), `int manaCost`,
  `GameObject[] heroPrefabs` — a generator can hold multiple heroes of the
  same class; `Generator.TryTap()` picks one at random
  (`Random.Range(0, heroPrefabs.Length)`) each time it spawns. If the
  array is empty/null, `TryTap()` no-ops before spending mana. Three asset
  instances plus placeholder prefabs (primitive capsule/sphere/cube) are
  created via an editor-only utility,
  `Assets/Editor/GeneratorSetupTool.cs` (menu: `MergeWars/Setup/Create
  Placeholder Hero Assets`) — run this once in the Unity Editor to
  generate `Assets/Prefabs/Heroes/*.prefab` and
  `Assets/Configs/HeroClasses/HeroClassConfig_*.asset` (each seeded with a
  single-entry `heroPrefabs` array; add more prefabs to the array in the
  Inspector for actual variety). This was necessary because asset/prefab
  binary-ish files can't be safely hand-authored outside the Editor; the
  tool uses real Unity APIs (`GameObject.CreatePrimitive`,
  `PrefabUtility.SaveAsPrefabAsset`, `ScriptableObject.CreateInstance`) so
  the result is guaranteed valid.
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
# CURRENT_TASK.md

## Active task
Generator tap input switched from 3D raycast to UI Button (reverses the
earlier "3D world object, not UI" decision — CONFIRMED this pass, user
had already set up the UI separately). `Generator.cs`:
`[RequireComponent(typeof(Collider))]` + `OnMouseDown()` replaced with
`[RequireComponent(typeof(Button))]`; `Awake()` wires
`button.onClick.AddListener(() => TryTap())` in code, removed in
`OnDestroy()` for subscribe/unsubscribe symmetry. `TryTap()` itself is
UNCHANGED (mana spend, hero spawn, grid placement). `GeneratorSpawner.cs`
and the Generator-grid placement/positioning logic are UNCHANGED per
explicit instruction — Generator prefabs are still instantiated and
positioned via `GridManager.GetWorldPosition(slotIndex)`; this assumes
the UI Canvas is World Space mode (not verified by the AI — user's
existing UI setup).

No next task has been scoped yet — confirm with the project owner
before starting one.

## Active variant
Generator & Continuous Conquest Variant

## Reference docs (read only these)
- /GameDocs/Systems/SYS_Generator.md
- /GameDocs/Systems/SYS_LaneSystem.md
- /GameDocs/Systems/SYS_MergeSystem.md
- /GameDocs/Systems/SYS_HeroDefinition.md
- /GameDocs/Systems/SYS_GridManager.md
- /GameDocs/Systems/SYS_PoolManager.md
- /GameDocs/Systems/SYS_ManaEconomy.md
- /GameDocs/Implementation/PROGRESS_LOG.md

## Out of scope for this task
- Redesigning GeneratorSpawner's placement/positioning model (kept as-is
  per explicit instruction)
- The movement/march/attack system that consumes LaneBounds.GetLaneIndex
  to actually move a hero down its lane (Combat/movement is still "Not
  built")
- Per-lane generators/economy (Generators stay shared/global, confirmed)
- Per-lane enemy bases (single shared base, confirmed)
- Building-type pools (EnemyBaseManager doesn't exist yet)
- Unlock system, discard
- Combat/damage resolution consuming HeroStats
- Real content for Subclass/Species/Background/Feats/Equipment-Spells
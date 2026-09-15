# SYS_GridManager.md — Generator Variant

## Responsibility
Owns an M×N grid data structure (dimensions configurable, not fixed) and
slot↔world-position mapping for a grid zone. The grid's world
origin/anchor is placeable (author sets a transform; grid builds
relative to it — not hardcoded to a scene position). Grid starts EMPTY on
first play. Grid state carries over between bases (not reset on
level-clear, per variant design).

**GridManager is a per-instance grid, not a singleton.** Multiple
GridManager components can exist in the scene simultaneously, each an
independent grid with its own dimensions/anchor/cell size — e.g. a Merge
grid and a Generator grid. Consumers (Generator, MergeSystem,
HeroMergeInput) each hold an explicit serialized reference to the
specific GridManager instance they operate on; there is no shared/global
grid lookup.

The slot-array data structure and its lookup/mutation logic
(TryGetOpenSlot, PlaceOccupant, RemoveOccupant, GetOccupant,
TryGetSlotIndexForOccupant, GetWorldPosition, and slot-array
construction) live in a plain C# `GridData` class
(`Assets/Scripts/Grid/GridData.cs`), not in GridManager itself.
GridManager is a thin MonoBehaviour wrapper: it holds the
Inspector-facing configuration (rows, columns, anchor, cell size, merge
area reference bounds, axis convention, debug gizmo) and an internal
`GridData` instance, and forwards all slot API calls to it. This split
exists so the grid data/logic is reusable across independently
configured grid instances without duplicating slot-array code per grid
type, and so gameplay data stays separate from
MonoBehaviour/presentation concerns.

## Cell generation order (CONFIRMED)
Cells are indexed starting at the top-right corner, filling right-to-left
across the top row first, then proceeding downward row by row. This is an
indexing/generation convention only — it does not imply any gameplay rule
about slot priority (e.g. TryGetOpenSlot() is not required to search in
this order unless a future system needs it to).

## Default configuration
- Default instantiation: 4×4 (16 slots) — matches original confirmed design
  and current variant default. M and N are otherwise runtime-configurable
  per grid instance; nothing hardcodes 4×4 into the class itself.

## Depends on
- Generators (fill an open slot on tap — read-access to find an open slot)
- LevelProgression / base-transition logic (does NOT reset grid on base
  clear — only reads current state to carry it into next base)

## Does NOT know about
- Merge/swap validity rules (MergeSystem's job)
- Mana cost of spawning (ManaEconomy's job)
- Combat, base layout
- Whether a slot's occupant is "unlocked" (generator/unlock system's concern)

## Confirmed / Assumed / Changed this pass
- CONFIRMED (this pass): grid dimensions are M×N, configurable per instance,
  not fixed at 4×4 — supersedes the earlier fixed-4×4 note. 4×4 remains the
  default.
- CONFIRMED (this pass): grid origin is placeable (author-positioned
  transform), not a fixed world position.
- CONFIRMED (this pass): cell generation order is top-right start,
  right-to-left across top row, then downward.
- CONFIRMED (carried forward): starts empty, persists base-to-base, empty
  during combat.
- UNKNOWN: whether any future system (targeting, visual scan effects) will
- UNKNOWN: which local axis relative to the anchor transform represents
  "column increases to the right" / "row increases downward" — current
  implementation uses ASSUMED PLACEHOLDER serialized axis vectors
  (`columnAxis` = anchor.right, `rowAxis` = anchor -forward), flagged with
  `// TODO(design)` in `GridManager.cs`.

## Implementation notes (added after flexible M×N GridManager task)
- Location: `Assets/Scripts/Grid/GridManager.cs`, `GridData.cs`,
  `GridSlotData.cs` (namespace `MergeWars.Grid`).
- Grid data is a plain array of `GridSlotData` (row, col, slotIndex,
  worldPosition, occupant, isOccupied) — no per-cell GameObjects, since
  cells are data + anchor-relative position only. If future raycast/
  drag-drop targeting requires per-cell colliders, add a pooled
  GameObject layer on top of this data rather than instantiating on tap.
- `TryGetOpenSlot` does a plain linear scan over the slot array; it does
  not treat generation order as a selection priority rule.
- Occupant reference type is `GameObject` as a placeholder
  (`// TODO(design)` in `GridSlotData.cs`) since MergeSystem/Generators
  have not yet defined an actual occupant data contract.
- API surface implemented on GridManager (all delegate to an internal
  `GridData`): `TryGetOpenSlot`, `PlaceOccupant`, `RemoveOccupant`,
  `GetOccupant`, `TryGetSlotIndexForOccupant`, `GetWorldPosition`,
  `TryGetNearestOpenSlotIndex` (nearest unoccupied slot to a world
  position within a distance threshold, compared on the grid's
  horizontal plane \u2014 used by HeroMergeInput for drag-to-open-slot
  repositioning; see SYS_MergeSystem.md), plus `BuildGrid()` (rebuilds
  the `GridData` slot array from current rows/columns/anchor),
  `SetDimensions(int rows, int columns)` (resizes and rebuilds \u2014 added so
  an external owner, e.g. GeneratorSpawner, can size this grid from its
  own data without GridManager needing to know why; see
  SYS_Generator.md), and `Rows`/`Columns`/`TotalSlots` accessors.
- `GridData` (plain C# class, no MonoBehaviour) owns the slot array
  itself and the slot-index?world-position computation (`Build`,
  `TryGetOpenSlot`, `PlaceOccupant`, `RemoveOccupant`, `GetOccupant`,
  `TryGetSlotIndexForOccupant`, `GetWorldPosition`, `IsValidIndex`). A
  GridManager owns exactly one `GridData` instance; nothing else
  currently constructs a `GridData` directly, but any future owner
  (e.g. a non-MonoBehaviour test harness) could.
- Added an editor-only debug gizmo (`OnDrawGizmos`, toggleable via
  `drawDebugGizmo`) that draws a wire cube per slot (cyan = empty, orange
  = occupied) so the grid is visible in the Scene view without any
  per-cell GameObjects. Reads from `GridData.Slots`. Purely a
  visualization aid; not part of the data API.

## Multiple grid instances (this pass)
- Confirmed pattern for separate grids (e.g. Merge grid, Generator grid):
  place one `GridManager` component per grid in the scene, each with its
  own rows/columns/anchor/cell size configured independently. Wire each
  consumer (Generator ? its grid, MergeSystem/HeroMergeInput ? their
  grid) to the specific `GridManager` instance it should operate on via
  the existing serialized-reference fields — no registry/lookup-by-id
  layer exists or is currently planned; add one only if a future system
  needs to resolve "the X grid" without a direct scene reference.
- A Building grid (for a future EnemyBaseManager) is explicitly out of
  scope for this pass — not built, no SYS doc exists for it yet.
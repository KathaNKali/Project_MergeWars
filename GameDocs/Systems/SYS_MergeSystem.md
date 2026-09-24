# SYS_GridManager.md - Generator Variant

_Synced with code: GridManager.cs, GridData.cs, GridSlotData.cs._

## Responsibility
Owns an M x N grid data structure (dimensions configurable, not fixed) and
slot <-> world-position mapping for a grid zone. The grid's world
origin/anchor is placeable (author sets a transform; grid builds
relative to it - not hardcoded to a scene position). Grid starts EMPTY on
first play. Grid state carries over between bases (not reset on
level-clear, per variant design).

**GridManager is a per-instance grid, not a singleton.** Multiple
GridManager components can exist in the scene simultaneously, each an
independent grid with its own dimensions/anchor/cell size - e.g. a Merge
grid and a Generator grid. Consumers (Generator, GeneratorSpawner,
MergeSystem, HeroMergeInput) each hold an explicit serialized reference to
the specific GridManager instance they operate on; there is no
shared/global grid lookup.

The slot-array data structure and its lookup/mutation logic
(TryGetOpenSlot, PlaceOccupant, RemoveOccupant, GetOccupant,
TryGetSlotIndexForOccupant, TryGetNearestOpenSlotIndex, GetWorldPosition,
and slot-array construction) live in a plain C# `GridData` class
(`Assets/Scripts/Grid/GridData.cs`), not in GridManager itself.
GridManager is a thin MonoBehaviour wrapper: it holds the Inspector-facing
configuration (rows, columns, anchor, cell size, merge area reference
bounds, axis convention, debug gizmo) and an internal `GridData`
instance, and forwards slot API calls to it. This split keeps grid
data/logic reusable across independently configured grid instances and
separate from MonoBehaviour/presentation concerns.

## Cell generation order (CONFIRMED)
Cells are indexed starting at the top-right corner, filling right-to-left
across the top row first, then proceeding downward row by row
(`slotIndex = row * columns + colFromRight`). This is an indexing
convention only - it does not imply any gameplay rule about slot priority
(`TryGetOpenSlot()` is a plain linear scan and is not required to follow
this order).

## Default configuration
- Default: 4x4 (16 slots). Rows/columns are runtime-configurable per
  instance; nothing hardcodes 4x4 into the class itself.

## Anchor, cell size, and merge area (matches code)
- The `anchor` transform represents the CENTER of the grid/merge area (not
  a corner). The grid is always built centered on it (offsets computed
  from the center index `(count-1)/2`), regardless of row/column count. If
  no anchor is assigned, the GridManager's own transform is used.
- Cell size is INDEPENDENT of the merge area (`cellSizeX` along the column
  axis, `cellSizeZ` along the row axis; exposed as `CellSizeX`/`CellSizeZ`).
  The grid does not auto-fit or clamp.
- `mergeAreaWidth` / `mergeAreaDepth` are reference bounds only. If the
  grid's footprint (`columns * cellSizeX`, `rows * cellSizeZ`) exceeds
  them, `BuildGrid()` logs a warning (`CheckMergeAreaOverflow`) - nothing
  is resized. Defaults: 4 x 4.
- `OnValidate()` rebuilds the grid in the editor whenever rows/columns are
  positive, so Inspector edits are reflected immediately.

## Depends on
- Nothing at runtime. (Generators, GeneratorSpawner, MergeSystem and
  HeroMergeInput depend on it.)
- Future: LevelProgression / base-transition logic will read current grid
  state to carry it into the next base (does NOT reset grid on base
  clear).

## Does NOT know about
- Merge/swap validity rules (MergeSystem's job)
- Mana cost of spawning (ManaEconomy's job)
- Combat, base layout
- Whether a slot's occupant is "unlocked" (generator/unlock system's
  concern)
- Why a caller wants a given size (`SetDimensions` only resizes/rebuilds)

## Confirmed / Assumed
- CONFIRMED: grid dimensions are M x N, configurable per instance; 4x4 is
  the default.
- CONFIRMED: grid origin is placeable (author-positioned transform).
- CONFIRMED: cell generation order is top-right start, right-to-left
  across the top row, then downward.
- CONFIRMED (carried forward): starts empty, persists base-to-base, empty
  during combat.
- ASSUMED PLACEHOLDER: which local axis relative to the anchor represents
  "column increases to the right" / "row increases downward" -
  serialized `columnAxis` (default `Vector3.right`) and `rowAxis` (default
  `Vector3.back`), applied through `anchor.TransformDirection`. Flagged
  `// TODO(design)` in `GridManager.cs`.
- UNKNOWN: whether any future system (targeting, visual scan effects)
  will need per-cell colliders or per-cell GameObjects. (This line was
  truncated in the previous version of this doc; the original
  continuation is not recoverable from the sources provided.)

## Implementation notes
- Location: `Assets/Scripts/Grid/GridManager.cs`, `GridData.cs`,
  `GridSlotData.cs` (namespace `MergeWars.Grid`).
- Grid data is a plain array of `GridSlotData` (slotIndex, row, col,
  worldPosition, occupant, isOccupied) - no per-cell GameObjects. If
  future raycast/drag-drop targeting requires per-cell colliders, add a
  pooled GameObject layer on top of this data rather than instantiating on
  tap.
- Occupant reference type is a bare `GameObject` (`// TODO(design)` in
  `GridSlotData.cs`) since no richer occupant contract exists.
- API surface on GridManager (all delegate to `GridData`, except where
  noted): `TryGetOpenSlot`, `PlaceOccupant`, `RemoveOccupant`,
  `GetOccupant`, `TryGetSlotIndexForOccupant`, `GetWorldPosition`
  (falls back to the anchor/own position if the index is invalid),
  `TryGetNearestOpenSlotIndex(worldPosition, maxDistance, out slotIndex)`
  (nearest unoccupied slot on the horizontal X/Z plane within
  `maxDistance`; used by HeroMergeInput for drag-to-open-slot
  repositioning - see SYS_MergeSystem.md), `BuildGrid()` (rebuilds the
  slot array; note this DISCARDS occupant state), `SetDimensions(int
  rows, int columns)` (resizes and rebuilds; used by GeneratorSpawner),
  plus `Rows` / `Columns` / `TotalSlots` / `CellSizeX` / `CellSizeZ`
  accessors.
- Caution: `BuildGrid()` is called from `Awake`, `SetDimensions`,
  `OnValidate`, and (in edit mode) `OnDrawGizmos`. In play mode the gizmo
  path only rebuilds when the slot count is out of date, so live occupant
  state is not clobbered by gizmo redraws. `SetDimensions` on a grid that
  already holds occupants will drop them from the data (the GameObjects
  themselves are untouched).
- Editor-only debug gizmo (`OnDrawGizmos`, toggle `drawDebugGizmo`): a
  wire cube per slot (cyan = empty, orange = occupied), plus an optional
  wireframe of the merge-area reference bounds (`drawMergeAreaBounds`;
  yellow normally, red when the grid footprint overflows the bounds).
  Purely a visualization aid.

## Multiple grid instances
- Confirmed pattern for separate grids (Merge grid, Generator grid): one
  `GridManager` component per grid in the scene, each with independent
  rows/columns/anchor/cell size. Wire each consumer to its specific
  instance via serialized-reference fields - no registry/lookup-by-id
  layer exists or is planned; add one only if a future system needs to
  resolve "the X grid" without a direct scene reference.
- A Building grid (for a future EnemyBaseManager) is not built and has no
  SYS doc yet.
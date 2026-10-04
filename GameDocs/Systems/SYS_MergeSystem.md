# SYS_MergeSystem.md - Generator Variant

_Synced with code: MergeSystem.cs, HeroMergeInput.cs. Regenerated from
source - the previous version of this file was corrupted (contained
duplicated SYS_GridManager.md content)._

## Responsibility
`MergeSystem`: central merge validation/execution logic, decoupled from
input - input triggers call `TryMerge(slotIndexA, slotIndexB)` and it does
not know how the merge was triggered. `HeroMergeInput`: single input
component that resolves both confirmed merge triggers (tap-tap-select,
drag-and-drop) into slot indices and calls into `MergeSystem` - it does
not implement merge validity itself.

## Confirmed
- CONFIRMED: Match rule - two heroes merge if
  `HeroInstance.definition.heroId` matches exactly AND `starLevel`
  matches (not prefab/reference identity). No adjacency requirement.
- CONFIRMED: Max star level is 4 (hardcoded `const` in `MergeSystem` - no
  shared config exists yet). Merging two max-star heroes is a no-op.
- CONFIRMED: Destination of a merge is always `slotIndexB` (the
  second-tapped hero, or the drag's drop target) - it receives the +1
  `starLevel`. The source (`slotIndexA`) is removed from the grid and
  released back to `PoolManager`. The destination hero's GameObject/
  position never changes during a merge - only its `starLevel` (and, if
  `LaneBounds` is assigned, its re-queried `laneIndex`) is mutated.
- CONFIRMED: tap-tap-select and drag-and-drop are handled by the SAME
  component (`HeroMergeInput`) because Unity's `OnMouseDown`/
  `OnMouseDrag`/`OnMouseUp` dispatch is per-GameObject; tap vs. drag is
  disambiguated by total mouse movement distance during the press
  (`dragThresholdPixels`).
- CONFIRMED: Drag release priority order (strategic hero repositioning
  pass): (1) drop on another hero -> attempt merge; (2) drop near an open
  (unoccupied) slot -> move there instead (updates `GridManager` occupant
  data, snaps to the slot's exact world position, not the raw drop
  point); (3) otherwise -> snap back to the original position, unchanged
  (no tween - DoTween not yet integrated).
- CONFIRMED: tap-tap-select is merge-only - empty grid space has no
  component to receive a tap, so there is no tap-to-move equivalent of
  strategic repositioning.
- CONFIRMED: while dragging, the hero's own collider is switched to a
  trigger so it can pass over other heroes without physical collision,
  and restored to its original trigger state on release/disable.
- CONFIRMED: drag movement is resolved against a flat virtual plane at a
  fixed height (`originalWorldPosition.y + dragHeightOffset`), not real
  ground geometry, so the hero never bobs vertically while dragged.

## Depends on
- GridManager (`GetOccupant`, `RemoveOccupant`, `PlaceOccupant`,
  `TryGetSlotIndexForOccupant`, `TryGetNearestOpenSlotIndex`,
  `GetWorldPosition`, `CellSizeX`/`CellSizeZ`)
- PoolManager (`Release`)
- HeroInstance (reads `definition.heroId` + `starLevel`, mutates
  `starLevel`; `HeroMergeInput` also caches its own `HeroInstance` to
  write `laneIndex`)
- LaneBounds (optional, see /GameDocs/Systems/SYS_LaneSystem.md) -
  `MergeSystem` re-queries and updates the surviving hero's `laneIndex`
  after a successful merge; `HeroMergeInput` re-queries and updates
  `laneIndex` after a drag-to-open-slot reposition. Both no-op if
  unassigned.

## Does NOT know about
- Economy/mana cost (ManaEconomy's job)
- Combat, base layout
- Generator (how/where a hero was spawned)
- How the player selected the two heroes (tap vs. drag) - `MergeSystem`
  only receives two slot indices
- The movement/march system (not built) that would consume a hero's
  `laneIndex`

## Implementation notes
- Location: `Assets/Scripts/Merge/MergeSystem.cs`,
  `Assets/Scripts/Merge/HeroMergeInput.cs` (namespace `MergeWars.Merge`).
- `HeroMergeInput` requires `[RequireComponent(typeof(Collider))]`
  (Physics.Raycast-based mouse dispatch, same input model as Generator)
  and `[RequireComponent(typeof(HeroInstance))]`.
- `MergeSystem.TryMerge(int slotIndexA, int slotIndexB)`: no-op (false)
  if `gridManager`/`poolManager` unassigned, `slotIndexA == slotIndexB`,
  either slot is empty, either occupant lacks a `HeroInstance`/
  `definition`, or `CanMerge` fails. On success: `instanceB.starLevel +=
  1`; if `laneBounds` assigned, re-queries `instanceB.laneIndex`; removes
  occupantA from the grid and releases it to `PoolManager`.
- `MergeSystem.CanMerge(HeroInstance, HeroInstance)`: exposed separately
  so input systems (e.g. drag hover highlighting - not built) can preview
  validity without executing a merge.
- `HeroMergeInput.Initialize(GridManager, MergeSystem, LaneBounds =
  null)`: wires dependencies at runtime - used by Generator when adding
  this component to a freshly spawned hero. Prefab-authored instances can
  instead assign fields directly in the Inspector.
- `selectedForTap` is a `static` field - tap-select state is shared
  across all heroes (only one hero can be "pending" at a time), not
  per-instance.
- `TryRaycastIgnoringSelf`: uses `Physics.RaycastAll` and filters out this
  hero's own collider, finding the closest remaining hit - without this,
  dragging a hero could hit its own collider first and snap it toward the
  camera.
- `OnDisable()` guards against a pooled/deactivated hero leaving stale
  selection/press/drag state behind (clears `selectedForTap` if it was
  this instance, restores collider trigger state, resets press/drag
  flags).
- Uses the legacy `Input` API and `Camera.main` for pointer position/
  raycasting - the project's active input backend beyond this is
  undocumented (see PROJECT_INDEX.md Open Unknowns).

## Assumed / Placeholder
- ASSUMED PLACEHOLDER: `dragHeightOffset` (0.5) - no spec for how far
  above ground a hero should visually lift while dragged/selected;
  purely cosmetic, GridManager still controls the final settled position.
- ASSUMED PLACEHOLDER: `dragSmoothTime` (0.06) - no spec for how "snappy"
  vs. "floaty" the drag glide should feel.
- ASSUMED PLACEHOLDER: `dragThresholdPixels` (10) - no spec for how far
  the mouse must move before a press counts as a drag instead of a tap.
- ASSUMED PLACEHOLDER: `MergeSystem.MaxStarLevel` (4, hardcoded const) -
  CONFIRMED as the correct value, but not sourced from a shared
  constant/config since none exists yet.

## Not verified
- Not run/tested in the Unity Editor as part of this doc regeneration
  pass - this file was rewritten from source (MergeSystem.cs,
  HeroMergeInput.cs) only, since the previous version was corrupted
  (duplicated SYS_GridManager.md content).

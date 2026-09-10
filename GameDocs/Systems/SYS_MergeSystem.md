# SYS_MergeSystem.md — Merge System (Generator Variant)

## Responsibility
Validates and executes merges between two heroes already placed on the
grid, given two slot indices. Builds on top of the Hero Character Data
Architecture (see SYS_HeroDefinition.md) — reads
`HeroInstance.definition.heroId` and `HeroInstance.starLevel`, mutates
`starLevel` on the destination hero on a successful merge, and releases
the source hero back to the pool.

## Does NOT know about
- Mana/economy — merging has no cost, no `CurrencyEconomy` dependency.
- Combat/damage resolution — no combat system exists yet.
- Generator/spawning logic — MergeSystem never spawns heroes, only
  merges existing occupants.
- How the merge was triggered — input components (`HeroMergeInput`) call
  `MergeSystem.TryMerge(slotIndexA, slotIndexB)`; MergeSystem has no
  knowledge of tap/drag/mouse/UI at all.
- Adjacency — CONFIRMED not required; any two matching heroes anywhere on
  the grid can merge.

## Match rule (CONFIRMED)
Two heroes are mergeable if and only if:
1. Both slots are occupied and both occupants have a `HeroInstance` with
   a non-null `definition`.
2. `instanceA.definition.heroId == instanceB.definition.heroId` (exact
   string match, non-empty). This is the unique per-hero identifier from
   `HeroDefinition` — **not** prefab/reference identity. An earlier draft
   of this design assumed prefab-identity matching before `heroId` was
   introduced; `heroId` supersedes that.
3. `instanceA.starLevel == instanceB.starLevel`.
4. The destination's `starLevel` is below the max (4) — merging two
   max-star heroes is a no-op (CONFIRMED, not a "convert to coins"
   mechanic or any other fallback).

On a successful merge, the **destination** slot (slotIndexB — the target
of the drag drop or the second tap) has its `HeroInstance.starLevel`
incremented by 1. The **source** slot (slotIndexA) is cleared via
`GridManager.RemoveOccupant` and its GameObject is released back to
`PoolManager.Release`. Stats update automatically on the destination via
`HeroInstance.CurrentStats` (computed on demand from
`HeroDefinition.GetStatsForStar`) — MergeSystem does not touch stats
directly.

## Key types
- **MergeSystem** (`Assets/Scripts/Merge/MergeSystem.cs`) — MonoBehaviour,
  depends on `GridManager` (occupant lookup/removal) and `PoolManager`
  (release). Public API: `TryMerge(int slotIndexA, int slotIndexB)`,
  `CanMerge(HeroInstance, HeroInstance)` (validity check without
  execution, for future preview/highlight use).
- **HeroMergeInput** (`Assets/Scripts/Merge/HeroMergeInput.cs`) —
  MonoBehaviour attached to each spawned hero (added by `Generator` at
  spawn time via `Initialize(gridManager, mergeSystem)`, since placeholder
  prefabs have no pre-authored Inspector references). Implements BOTH
  confirmed input triggers in a single component (a single component is
  required, not two, because Unity's `OnMouseDown`/`OnMouseDrag`/
  `OnMouseUp` dispatch is per-GameObject and two independent components
  would double-handle the same mouse events):
  - **Tap-tap-select**: tap a hero to select it (static `selectedForTap`
	field, shared across all hero instances so only one can be pending);
	tap a different hero to attempt `MergeSystem.TryMerge`; tap the same
	selected hero again to deselect.
  - **Drag-and-drop**: press and move the hero (follows the mouse via
	raycast against a `dropTargetMask`, no tween — DoTween is not yet
	integrated per PROJECT_INDEX.md); on release, if the cursor is over
	another hero, attempts `MergeSystem.TryMerge`; otherwise snaps the
	hero's transform back to its pre-drag position.
  - Tap vs. drag is disambiguated by total mouse-movement distance during
	the press (`dragThresholdPixels`, ASSUMED PLACEHOLDER value, default
	10 pixels — no spec exists for the exact threshold).
- **GridManager.TryGetSlotIndexForOccupant** (added to
  `Assets/Scripts/Grid/GridManager.cs`) — reverse lookup from an occupant
  GameObject back to its slot index, used by `HeroMergeInput` to resolve
  slot indices from the GameObjects it sees via raycast/selection.

## Generator integration
`Generator` has an optional `mergeSystem` field. If assigned, `TryTap()`
adds a `HeroMergeInput` component to the spawned hero and calls
`Initialize(gridManager, mergeSystem)` on it, in addition to the existing
`HeroInstance` setup. If `mergeSystem` is left unassigned on a Generator,
spawned heroes simply have no merge input — Generator's core spawn flow
is unaffected either way.

## Open Unknowns
- `dragThresholdPixels` (10) is an ASSUMED PLACEHOLDER — no design spec
  for tap-vs-drag sensitivity.
- Visual feedback on merge (scale pop, particle burst, sound) is
  unimplemented — FEEL/DoTween integration is still unwired project-wide
  (see PROJECT_INDEX.md Tech Stack section).
- Whether a hero mid-drag should be excluded from being a valid merge
  *source* for a concurrent tap-select elsewhere is unresolved (not
  expected in single-touch/mouse play, flagged for multi-touch mobile
  input if that becomes relevant).
- No UI/highlight exists yet to preview `CanMerge` validity before commit
  — `CanMerge` is exposed publicly for this purpose but unused by any
  visual system this pass.

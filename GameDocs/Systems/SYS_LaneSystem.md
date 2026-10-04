eps to integrate the lanesytem in unity and setup the lanes.# SYS_LaneSystem.md - Generator Variant

_Synced with code: LaneBounds.cs. First version of this doc — created
alongside the initial LaneBounds implementation._

## Responsibility
Answers a single question on demand: "which lane is this world position
in?" (`GetLaneIndex(Vector3 worldPosition)`). `LaneBounds` is a standalone,
foundational MonoBehaviour — it holds no per-hero state and does not track
occupants; it is a pure world-position-to-lane-index query, re-evaluated
live every time a caller invokes it (never cached internally).

Lane boundaries are modeled as plain world-space corridors (evenly spaced,
centered on an author-placed `anchor` transform), NOT physics
triggers/colliders. Boundaries conceptually span the full depth from the
hero side to the enemy side of the battlefield, so the same boundary data
can later double as a march corridor for a future attack/movement system
— but `GetLaneIndex` itself has no depth/Z bound; only the lane axis
(width) is checked.

## Why decoupled from GridManager/GridData (CONFIRMED design decision)
A hero's lane is decided by WHERE it is placed (world position), not by
which grid slot/column it occupies. This was an explicit design decision:
deriving lanes from the Merge grid's column data was rejected because
`GridManager.BuildGrid()`/`SetDimensions()` discard and rebuild slot data
when the grid is resized, which would make a column-derived lane
assignment fragile. Repurposing `GridManager` itself to represent lanes
was also rejected — `GridManager`'s core responsibility is single-occupant
slot tracking (`PlaceOccupant`/`TryGetOpenSlot`), but a lane has many
heroes sharing it simultaneously with no occupancy concept, which would
blur `GridManager`'s documented responsibility. `LaneBounds` mirrors
`GridManager`'s authoring conventions (serialized anchor transform,
axis convention, debug gizmo) for consistency, but shares no code or data
with it.

## Depends on
- Nothing at runtime. Foundational, like `PoolManager`/`CurrencyEconomy`.

## Does NOT know about
- GridManager/GridData, grid occupancy, or slot indices.
- Merge/swap validity rules (MergeSystem's job).
- Combat, base layout, or how/when a movement system walks a hero along
  its lane — `LaneBounds` only answers the lane-membership query; it does
  not move anything itself.
- Mana cost of spawning, economy.

## Confirmed / Assumed
- CONFIRMED: lane membership is a LIVE query, not a cached/stored value —
  `GetLaneIndex` is re-evaluated every time a caller needs it (at spawn,
  after a drag reposition, after a merge resolves). `LaneBounds` itself
  stores nothing between calls.
- CONFIRMED: lanes are side-by-side (columns-style along `laneAxis`), not
  stacked front-to-back.
- CONFIRMED: a world position outside every lane's range CLAMPS to the
  nearest lane — `GetLaneIndex` always returns a valid index in
  `[0, LaneCount - 1]` and never "no lane".
- CONFIRMED: default `laneCount` = 2; configurable to 1, 2, or 3 (or more
  — nothing hardcodes an upper bound in code).
- ASSUMED PLACEHOLDER: `laneWidth` (default 2) — no balance/layout doc
  specifies a real per-lane width yet.
- ASSUMED PLACEHOLDER: lanes are evenly spaced/centered on the anchor only
  — no manual per-lane uneven-width authoring in this pass.
- ASSUMED PLACEHOLDER: `laneAxis` (default `Vector3.right`), applied
  through `anchor.TransformDirection` — mirrors `GridManager.columnAxis`'s
  same open "which local direction is which" caveat. Flagged
  `// TODO(design)` in `LaneBounds.cs`.

## Implementation notes
- Location: `Assets/Scripts/Lanes/LaneBounds.cs` (namespace
  `MergeWars.Lanes`).
- `GetLaneIndex(Vector3 worldPosition)`: projects the position onto
  `laneAxis` relative to `anchor`, centers the result on the total lane
  span (`laneWidth * laneCount`), floor-divides by `laneWidth` to bucket
  into a lane index, then clamps to `[0, laneCount - 1]`.
- `anchor` falls back to this GameObject's own transform if unassigned
  (same fallback pattern as `GridManager.anchor`).
- Editor-only debug gizmo (`OnDrawGizmos`, toggle `drawDebugGizmo`): draws
  one wireframe corridor per lane, colored via an HSV hue spread by lane
  index (not a hardcoded palette, since lane count isn't fixed at 3). The
  gizmo's depth (`gizmoCorridorDepth`) is cosmetic only and does not affect
  `GetLaneIndex`.

## Consumers (optional, null-safe wiring — mirrors the existing MergeSystem-on-Generator pattern)
- **Generator** (`Assets/Scripts/Generator/Generator.cs`): optional
  `[SerializeField] LaneBounds laneBounds`. In `TryTap()`, after the newly
  spawned hero is placed via `GridManager.PlaceOccupant(...)`, if assigned,
  sets `heroInstance.laneIndex = laneBounds.GetLaneIndex(hero.transform.position)`.
  No-op (spawn flow unaffected) if left unassigned. Also accepted as an
  optional trailing parameter on `Generator.Initialize(...)` for
  prefab-instantiated Generators.
- **HeroMergeInput** (`Assets/Scripts/Merge/HeroMergeInput.cs`): optional
  `[SerializeField] LaneBounds laneBounds`. After a drag-to-open-slot
  reposition completes (`TryMoveToOpenSlot()`), if assigned, re-queries and
  updates `HeroInstance.laneIndex` from the hero's final slot position.
  Also accepted as an optional trailing parameter on
  `HeroMergeInput.Initialize(...)`.
- **MergeSystem** (`Assets/Scripts/Merge/MergeSystem.cs`): optional
  `[SerializeField] LaneBounds laneBounds`. After a successful merge
  (`TryMerge()`), if assigned, re-queries and updates the SURVIVING hero's
  (`instanceB`'s) `laneIndex` from its current position — CONFIRMED by
  project owner: the merge survivor's lane must reflect its position once
  it is back in the grid. Note: in the current merge implementation the
  destination slot (`slotIndexB`) never actually changes position during a
  merge (only the source occupant is removed/released), so this re-query
  is currently a no-op in practice, but is still performed for
  correctness/future-proofing if that ever changes.
- **HeroInstance** (`Assets/Scripts/Heroes/HeroInstance.cs`): holds the
  resulting `public int laneIndex = -1` field (-1 = unassigned / no
  LaneBounds wired). `HeroInstance` itself does not know about
  `LaneBounds` — it is purely a data holder set by the consumers above.

## Not yet built
- Any movement/march system that walks a hero down its lane toward the
  (single, shared) enemy base. `LaneBounds.GetLaneIndex` is built to
  support this (lane boundaries span the full hero-to-enemy depth), but
  the movement/combat system itself is out of scope here — Combat/damage
  resolution and EnemyBaseManager are both still "Not built" per
  PROJECT_INDEX.md.
- Mid-battle continuous lane re-checking (re-querying `GetLaneIndex` every
  movement tick) — depends on the movement system above.

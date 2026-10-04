# CURRENT_TASK.md

## Active task
Lane System (LaneBounds) built (see SYS_LaneSystem.md): a standalone,
world-position-based lane query, fully decoupled from GridManager/GridData
(a hero's lane is decided by WHERE it is placed, not by grid
slot/column, so lane assignment survives grid resizes). Lane membership
is a LIVE query (GetLaneIndex), never cached by LaneBounds itself — a
position outside every lane's range clamps to the nearest lane. Lane
boundaries are plain world-space corridors (no colliders), evenly spaced
and centered on an author-placed anchor, spanning conceptually from the
hero side to the enemy side (no depth/Z bound in the query itself).
Default laneCount = 2 (configurable). Wired (optionally, null-safe) into
the three existing hero-placement points: Generator.TryTap() (spawn),
HeroMergeInput.TryMoveToOpenSlot() (drag reposition), and
MergeSystem.TryMerge() (surviving hero re-queried after a merge resolves,
per project owner confirmation that this must reflect the survivor's
position once it's back in the grid). HeroInstance gained a new
laneIndex field (-1 = unassigned). The actual movement/march system that
walks a hero down its lane toward the enemy base is NOT built — this task
only lays the query-API groundwork for that future system.

No next task has been scoped yet — confirm with the project owner
before starting one.

## Active variant
Generator & Continuous Conquest Variant

## Reference docs (read only these)
- /GameDocs/Systems/SYS_LaneSystem.md
- /GameDocs/Systems/SYS_MergeSystem.md
- /GameDocs/Systems/SYS_HeroDefinition.md
- /GameDocs/Systems/SYS_Generator.md
- /GameDocs/Systems/SYS_GridManager.md
- /GameDocs/Systems/SYS_PoolManager.md
- /GameDocs/Systems/SYS_ManaEconomy.md
- /GameDocs/Implementation/PROGRESS_LOG.md

## Out of scope for this task
- The movement/march/attack system that consumes LaneBounds.GetLaneIndex
  to actually move a hero down its lane (Combat/movement is still "Not
  built")
- Per-lane generators/economy (Generators stay shared/global, confirmed)
- Per-lane enemy bases (single shared base, confirmed)
- Manual/uneven per-lane width authoring (even-split only, this pass)
- Building-type pools (EnemyBaseManager doesn't exist yet)
- Unlock system, discard
- Combat/damage resolution consuming HeroStats
- Real content for Subclass/Species/Background/Feats/Equipment-Spells
- UI generator variant (confirmed as 3D world object — don't build a UI version)
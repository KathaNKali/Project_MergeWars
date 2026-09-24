# CURRENT_TASK.md

## Active task
Strategic hero repositioning built (see SYS_MergeSystem.md): dragging a
hero and releasing near an OPEN (unoccupied) slot now moves it there
instead of only merging or snapping back — CONFIRMED this pass, to let
the player place heroes tactically before combat. Priority order on drag
release: (1) drop on another hero → attempt merge; (2) drop near an open
slot → move (updates GridManager occupant data, snaps to the slot's
exact world position); (3) otherwise → snap back, unchanged. Drag-only —
tap-tap-select remains merge-only (empty grid space has no component to
receive a tap). Added `GridData`/`GridManager.TryGetNearestOpenSlotIndex`
to support this.

No next task has been scoped yet — confirm with the project owner
before starting one.

## Active variant
Generator & Continuous Conquest Variant

## Reference docs (read only these)
- /GameDocs/Systems/SYS_MergeSystem.md
- /GameDocs/Systems/SYS_HeroDefinition.md
- /GameDocs/Systems/SYS_Generator.md
- /GameDocs/Systems/SYS_GridManager.md
- /GameDocs/Systems/SYS_PoolManager.md
- /GameDocs/Systems/SYS_ManaEconomy.md
- /GameDocs/Implementation/PROGRESS_LOG.md

## Out of scope for this task
- Building-type pools (EnemyBaseManager doesn't exist yet)
- Unlock system, discard
- Combat/damage resolution consuming HeroStats
- Real content for Subclass/Species/Background/Feats/Equipment-Spells
- UI generator variant (confirmed as 3D world object — don't build a UI version)
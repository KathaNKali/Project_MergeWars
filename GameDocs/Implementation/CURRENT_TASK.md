# CURRENT_TASK.md
# CURRENT_TASK.md

## Active task
MergeSystem is complete (MergeSystem, HeroMergeInput, GridManager
reverse-lookup — see SYS_MergeSystem.md). Heroes merge by
HeroDefinition.heroId + HeroInstance.starLevel match, via both
tap-tap-select and drag-and-drop input, no adjacency requirement, max
star level 4 (no-op at max). No next task has been scoped yet — likely
candidates are EnemyBaseManager or combat/damage resolution consuming
HeroStats, but confirm with the project owner before starting either.

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

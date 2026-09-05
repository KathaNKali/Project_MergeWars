# CURRENT_TASK.md
# CURRENT_TASK.md

## Active task
Build PoolManager (champion-tier pools only) and Generator system together:
Generator spawns via PoolManager.Get(), checks mana, places into grid's
next open slot in generation order. 3D world object, raycast-tapped.

## Active variant
Generator & Continuous Conquest Variant

## Reference docs (read only these)
- /GameDocs/Systems/SYS_PoolManager.md
- /GameDocs/Systems/SYS_Generator.md
- /GameDocs/Systems/SYS_GridManager.md
- /GameDocs/Systems/SYS_ManaEconomy.md
- /GameDocs/Implementation/PROGRESS_LOG.md

## Out of scope for this task
- Building-type pools (EnemyBaseManager doesn't exist yet)
- Unlock system, discard, merge logic
- UI generator variant (confirmed as 3D world object — don't build a UI version)

# PROJECT_INDEX.md — Merge Wars (Generator Variant)

Last updated: this pass (PoolManager + Generator system build).

## Note on this file's origin
This file did not exist before this pass. It was created because the task
brief instructed keeping it current, but no prior version existed to
diff against — treat everything below as a first snapshot of reality, not
a history. `GAME_DESIGN_GENERATOR_VARIANT.md`, `GAMEPLAY_LOOPS.md`, and a
`DECISIONS_LOG.md` are referenced by other docs/prompts but do not exist
anywhere in `/GameDocs/` at this time — flagged for human follow-up, not
fabricated here.

## Systems status

| System | Status | Location | Doc |
|---|---|---|---|
| GridManager | Built (real) | `Assets/Scripts/Grid/` | SYS_GridManager.md |
| PoolManager | Built (real) | `Assets/Scripts/Pooling/PoolManager.cs` | SYS_PoolManager.md |
| ManaEconomy | ⚠️ Stub only | `Assets/Scripts/Economy/ManaEconomy.cs` | SYS_ManaEconomy.md |
| HeroClassConfig | Built (real, data-only) | `Assets/Scripts/Generator/HeroClassConfig.cs` | SYS_Generator.md |
| Generator | Built (real) | `Assets/Scripts/Generator/Generator.cs` | SYS_Generator.md |
| MergeSystem | Not built | — | — |
| EnemyBaseManager | Not built | — | — |
| Unlock system | Not built (intentionally deferred) | — | — |
| Discard mechanic | Not built | — | — |
| Save/load | Not built | — | — |

## Open Unknowns (surfaced for next read, not buried in code)
- ManaEconomy has no real design (starting mana, regen, cap) — see
  SYS_ManaEconomy.md. Everything there is ASSUMED PLACEHOLDER.
- Real per-class mana costs are unknown; setup tool assigns placeholder
  values (Ground 10 / Air 15 / Vehicles 20) via
  `Assets/Editor/GeneratorSetupTool.cs`.
- GridManager's `columnAxis`/`rowAxis` world-direction convention is still
  an ASSUMED PLACEHOLDER (carried over from the GridManager task).
- `GameDocs/GAME_DESIGN_GENERATOR_VARIANT.md`, `GameDocs/GAMEPLAY_LOOPS.md`,
  and `GameDocs/DECISIONS_LOG.md` are referenced by task prompts but do
  not exist in the repo — a human should confirm whether they exist
  elsewhere or need to be authored.

## Logical next step
GridManager, PoolManager, and Generator are real; ManaEconomy is a stub
and should be the next real system built (per SYS_ManaEconomy.md), since
Generator already depends on its interface. After that, MergeSystem is
the next unbuilt piece referenced by multiple existing SYS docs
(SYS_Generator.md, SYS_PoolManager.md).

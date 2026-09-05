# SYSTEM_MAP.md — Generator Variant

Dependency edges between systems. An arrow (A → B) means A depends on /
calls into B; B has no knowledge of A. This file only lists systems that
currently exist (as real implementations or explicit stubs) in the
codebase — see PROJECT_INDEX.md for build status.

## Current systems

- **GridManager** (`Assets/Scripts/Grid/`) — foundational. No dependencies
  on other gameplay systems. See SYS_GridManager.md.

- **PoolManager** (`Assets/Scripts/Pooling/PoolManager.cs`) — foundational.
  No dependencies on other gameplay systems. See SYS_PoolManager.md.

- **ManaEconomy** (`Assets/Scripts/Economy/ManaEconomy.cs`) — ⚠️ STUB, not
  a real design. Foundational. No dependencies. See SYS_ManaEconomy.md.

- **HeroClassConfig** (`Assets/Scripts/Generator/HeroClassConfig.cs`) —
  ScriptableObject config, not a system with behavior. Referenced by
  Generator.

- **Generator** (`Assets/Scripts/Generator/Generator.cs`) — depends on:
  - GridManager (TryGetOpenSlot, PlaceOccupant, GetWorldPosition)
  - PoolManager (Get)
  - ManaEconomy (CanAfford, Spend) — currently the stub
  - HeroClassConfig (data reference)
  See SYS_Generator.md.

## Dependency edges (summary)

```
Generator → GridManager
Generator → PoolManager
Generator → ManaEconomy (stub)
Generator → HeroClassConfig (data)
```

No other system currently depends on GridManager, PoolManager, or
ManaEconomy — they are foundational/leaf-consumed only.

## Not yet built
MergeSystem, EnemyBaseManager, LevelProgression/base-transition logic,
unlock system, discard mechanic, save/load wiring, real ManaEconomy,
combat. These are referenced as future dependents/dependencies in the
SYS_*.md "Depends on" sections but do not exist in code yet.

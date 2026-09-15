# PROJECT_INDEX.md — Merge Wars (Generator Variant)

Last updated: this pass (MergeSystem build — tap-tap-select + drag-and-drop merge input, heroId + starLevel matching, max 4-star cap).

## Note on this file's origin
This file did not exist before the PoolManager + Generator task. It was
created because that task brief instructed keeping it current, but no
prior version existed to diff against — treat everything below as a
running snapshot of reality, not a full history. `GAME_DESIGN_GENERATOR_VARIANT.md`,
`GAMEPLAY_LOOPS.md`, and a `DECISIONS_LOG.md` are referenced by other
docs/prompts but do not exist anywhere in `/GameDocs/` at this time —
flagged for human follow-up, not fabricated here.

## Systems status

| System | Status | Location | Doc |
|---|---|---|---|
| GridManager | Built (real, split into GridManager + GridData; multi-instance) | `Assets/Scripts/Grid/` | SYS_GridManager.md |
| GeneratorSpawner / GeneratorLoadout | Built (real, prototype layout) | `Assets/Scripts/Generator/` | SYS_Generator.md |
| PoolManager | Built (real) | `Assets/Scripts/Pooling/PoolManager.cs` | SYS_PoolManager.md |
| CurrencyEconomy | Built (real, shared base) | `Assets/Scripts/Economy/CurrencyEconomy.cs` | SYS_ManaEconomy.md / SYS_CoinEconomy.md |
| ManaEconomy | Built (real) | `Assets/Scripts/Economy/ManaEconomy.cs` | SYS_ManaEconomy.md |
| CoinEconomy | Built (real, no consumer yet) | `Assets/Scripts/Economy/CoinEconomy.cs` | SYS_CoinEconomy.md |
| HeroDefinition / HeroInstance | Built (real, Class + Stats layers only) | `Assets/Scripts/Heroes/` | SYS_HeroDefinition.md |
| HeroRole | Built (real) | `Assets/Scripts/Heroes/HeroRole.cs` | SYS_HeroDefinition.md |
| HeroSubclass / HeroSpecies / HeroBackground / HeroFeat / HeroEquipmentSpell | Scaffolded placeholder only | `Assets/Scripts/Heroes/` | SYS_HeroDefinition.md |
| GeneratorConfig | Built (real, replaces HeroClassConfig) | `Assets/Scripts/Generator/GeneratorConfig.cs` | SYS_Generator.md |
| Generator | Built (real) | `Assets/Scripts/Generator/Generator.cs` | SYS_Generator.md |
| MergeSystem | Built (real) | `Assets/Scripts/Merge/MergeSystem.cs` | SYS_MergeSystem.md |
| HeroMergeInput | Built (real, tap-tap-select + drag-and-drop) | `Assets/Scripts/Merge/HeroMergeInput.cs` | SYS_MergeSystem.md |
| EnemyBaseManager | Not built | — | — |
| Unlock system | Not built (intentionally deferred) | — | — |
| Discard mechanic | Not built | — | — |
| Save/load | Not built | — | — |
| Reward hook (base destruction → Mana/Coins) | Not built (intentionally deferred) | — | SYS_ManaEconomy.md / SYS_CoinEconomy.md |

## Open Unknowns (surfaced for next read, not buried in code)
- `GeneratorSpawner`'s grid layout (1 row × generator-count columns) is an
  ASSUMED PLACEHOLDER — no design for wrapping to multiple rows once
  generator count grows. `GeneratorLoadout` currently just lists every
  generator to spawn; how "whatever the player has chosen" actually maps
  onto it (swap assigned asset vs. runtime-filtered list) is UNRESOLVED.
- GridManager's slot-array logic was extracted into a plain C# `GridData`
  class (`Assets/Scripts/Grid/GridData.cs`) this pass; GridManager is now
  a thin MonoBehaviour wrapper. GridManager was never a singleton, so
  multiple independent grid instances (e.g. Merge grid + Generator grid)
  already coexist by placing multiple GridManager components in-scene
  with independent config, each wired to its consumer via existing
  serialized-reference fields. No registry/lookup-by-id layer exists.
  A Building grid (for a future EnemyBaseManager) remains unbuilt.
- `dragThresholdPixels` (10, on `HeroMergeInput`) is an ASSUMED
  PLACEHOLDER — no design spec for tap-vs-drag sensitivity. See
  SYS_MergeSystem.md.
- No visual/audio feedback exists on a successful merge — FEEL/DoTween
  are still unintegrated project-wide (see Tech Stack section below).
- `HeroClassConfig`/`HeroClassId` (Ground/Air/Vehicles) are RETIRED —
  replaced by `GeneratorConfig` + `HeroDefinition`/`HeroRole`
  (Tank/MeleeDps/RangedDps/Support/Controller). See SYS_HeroDefinition.md.
- All `HeroStats` field values and `HeroDefinition.perStarMultiplier`
  (default 1.5) are ASSUMED PLACEHOLDERS — no balance doc specifies real
  numbers yet.
- Subclass, Species, Background, Feats, and Equipment/Spells layers are
  scaffolded-only placeholders with no real content or rules — flagged
  for a future task once designed.
- Whether `HeroDefinitionValidator` (duplicate `heroId` check) should run
  automatically (e.g. on asset import) vs. manual-only is unresolved —
  currently manual-run only, per explicit request.
- Whether Mana/Coins should be a single global pool or per-player/
  per-base is UNRESOLVED — currently implemented as a single global pool
  per currency. See SYS_ManaEconomy.md / SYS_CoinEconomy.md.
- Whether a currency's cap-upgrade cost is paid in that same currency
  (assumed) or a different one is unspecified.
- Starting amounts, starting max caps, and all upgrade tier cost/newMaxCap
  values for both currencies are ASSUMED PLACEHOLDERS — no balance doc
  specifies real numbers.
- Real per-class hero mana costs are unknown; setup tool assigns
  placeholder values (Ground 10 / Air 15 / Vehicles 20) via
  `Assets/Editor/GeneratorSetupTool.cs`.
- GridManager's `columnAxis`/`rowAxis` world-direction convention is still
  an ASSUMED PLACEHOLDER (carried over from the GridManager task).
- `GameDocs/GAME_DESIGN_GENERATOR_VARIANT.md`, `GameDocs/GAMEPLAY_LOOPS.md`,
  and `GameDocs/DECISIONS_LOG.md` are referenced by task prompts but do
  not exist in the repo — a human should confirm whether they exist
  elsewhere or need to be authored.
- TopDown Engine / FEEL / DoTween integration points are UNKNOWN (see Tech
  Stack section below) — none of these packages are referenced in code
  yet; confirm scope per-system as each comes up.

## Logical next step
GridManager, PoolManager, ManaEconomy, CoinEconomy, the Hero Character
Data Architecture (HeroRole, HeroStats, HeroDefinition, HeroInstance,
GeneratorConfig), and MergeSystem (with HeroMergeInput for tap-tap-select
and drag-and-drop input) are all real now. Generator optionally wires
spawned heroes into MergeSystem via an assigned `mergeSystem` field. The
next unbuilt pieces are EnemyBaseManager and combat/damage resolution
(nothing consumes `HeroStats` yet). The reward hook connecting
combat/base-destruction to Mana/Coins gains is also still open, deferred
until a combat/base system exists.

## Tech Stack & Dependencies (confirmed by project owner)
- **TopDown Engine** (Unity asset, More Mountains) — intended for
  champion movement/character-controller needs going forward. Integration
  point (which systems call into it, whether spawned Generator heroes use
  its controller automatically) is UNKNOWN — not yet wired into any
  existing script. Confirm scope when the first movement-related task
  comes up.
- **FEEL** (Feedback Handling System, More Mountains) — intended for
  game-feel/feedback (VFX, camera shake, haptics, etc.). Not yet
  integrated into Generator, GridManager, PoolManager, or the economy
  systems. UNKNOWN which events (tap, spawn, merge, currency gain) should
  trigger feedback — confirm per-system when relevant.
- **DoTween** — intended for tweened motion/UI animation. Not yet used by
  any existing script (`Generator` currently sets `hero.transform.position`
  directly, no tween). UNKNOWN whether slot-placement movement should be
  tweened — flag as a follow-up if/when Generator or MergeSystem is
  revisited.
- **Cinemachine** — a camera prefab has been added at
  `Assets/Prefabs/Camera/MergeWars3DCameras.prefab`. Not yet referenced by
  any gameplay script; GridManager's debug gizmo assumes a Scene-view-
  like perspective but has no dependency on a specific camera setup.
- None of the above are referenced in code yet. This section exists so
  future tasks know these packages are expected to be available and can
  reference them instead of re-deriving/asking whether they're allowed.

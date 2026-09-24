# PROGRESS_LOG.md

Append one entry per work session. Do not edit or delete prior entries.

## Format
### [date] — [task name]
- Status: started / in-progress / blocked / complete
- What was done
- What's left
- Any TODO(design) markers left in code

### Flexible M×N GridManager (Generator Variant)
- Status: complete
- What was done: Implemented `GridManager` (MonoBehaviour) and
  `GridSlotData` (plain serializable data class) in
  `Assets/Scripts/Grid/`. Grid dimensions (rows/columns) and world anchor
  are inspector-configurable, not hardcoded; default remains 4×4. Cells
  are data-only (no per-cell GameObjects). Slot index 0 is top-right,
  filling right-to-left across the top row then downward, via a
  `slotIndex = row * columns + colFromRight` mapping — `TryGetOpenSlot`
  does a plain linear scan and does not treat this as a priority rule.
  Implemented API: `TryGetOpenSlot`, `PlaceOccupant`, `RemoveOccupant`,
  `GetOccupant`, `GetWorldPosition`, plus `BuildGrid()` and
  `Rows`/`Columns`/`TotalSlots`. Updated `SYS_GridManager.md` with an
  "Implementation notes" section.
- What's left: nothing for this task; merge logic, mana cost, generator
  taps, combat, and real save/load wiring are intentionally out of scope
  (stub-free — no extension points were added since none were requested).
- TODO(design) markers left in code: occupant reference type is a bare
  `GameObject` placeholder in `GridSlotData.cs` (no MergeSystem/Generator
  occupant contract exists yet); `columnAxis`/`rowAxis` direction vectors
  in `GridManager.cs` are an ASSUMED PLACEHOLDER for which local axis is
  "right"/"downward" relative to the anchor transform.

### PoolManager + Generator System (Generator Variant)
- Status: complete
- What was done: Implemented `PoolManager` (`Assets/Scripts/Pooling/`),
  `HeroClassConfig` ScriptableObject + `Generator` (both
  `Assets/Scripts/Generator/`). `Generator.TryTap()` follows the confirmed
  order: CanAfford check → TryGetOpenSlot check (no-op, no spend, if
  either fails) → spend mana → `PoolManager.Get` → position via
  `GridManager.GetWorldPosition` → parent to a champions container
  (never the Generator) → `GridManager.PlaceOccupant`. No unlock check
  exists, per spec. Added an editor-only utility,
  `Assets/Editor/GeneratorSetupTool.cs` (menu:
  `MergeWars/Setup/Create Placeholder Hero Assets`), to generate the 3
  placeholder hero prefabs (capsule/sphere/cube primitives) and 3
  `HeroClassConfig` assets (Ground/Air/Vehicles) using real Unity APIs,
  since these binary-ish assets can't be safely hand-authored outside the
  Editor. **This menu item must be run once inside the Unity Editor**
  before Generator can be wired up with real config/prefab references in
  a scene.
- Stub added: no `SYS_ManaEconomy.md` or ManaEconomy implementation
  existed anywhere in `/GameDocs/` or the codebase, so a minimal
  `ManaEconomy` stub (`CanAfford`/`Spend`/`CurrentMana` only) was added at
  `Assets/Scripts/Economy/ManaEconomy.cs`, with a new
  `GameDocs/Systems/SYS_ManaEconomy.md` explicitly marked STUB ONLY.
  Generator depends only on this stub's public interface, so it should
  not need changes when the real ManaEconomy is built.
- Docs created (didn't exist before this pass): `GameDocs/SYSTEM_MAP.md`,
  `GameDocs/PROJECT_INDEX.md`, `GameDocs/Systems/SYS_ManaEconomy.md`.
  Updated `SYS_PoolManager.md` and `SYS_Generator.md` with
  "Implementation notes" sections.
- What's left / logical next step: ManaEconomy is the next system that
  should get a real design + implementation (Generator already depends on
  its interface, so swapping the stub should be low-risk). After that,
  MergeSystem is the next unbuilt piece referenced by existing SYS docs.
  Someone should also run `GeneratorSetupTool`'s menu item in the Unity
  Editor at least once to materialize the placeholder prefab/config
  assets on disk.
- TODO(design) markers left in code: `ManaEconomy` starting mana (100)
  and lack of regen/cap logic (`ManaEconomy.cs`); per-class mana costs
  assigned by the setup tool (Ground 10 / Air 15 / Vehicles 20) and the
  `HeroClassConfig.manaCost` default (10) — none of these numbers are
  specified anywhere in `/GameDocs/`.

### Conflicts / gaps surfaced (not resolved silently)
- The task prompt asked to read `PROJECT_INDEX.md`,
  `GAME_DESIGN_GENERATOR_VARIANT.md`, `GAMEPLAY_LOOPS.md`, `SYSTEM_MAP.md`,
  and `DECISIONS_LOG.md` as pre-existing context. Only
  `SYS_Generator.md`, `SYS_PoolManager.md`, `SYS_GridManager.md`,
  `CURRENT_TASK.md`, and this log existed in `/GameDocs/` at the start of
  this pass — the other files did not exist and were not fabricated with
  invented design content. `PROJECT_INDEX.md` and `SYSTEM_MAP.md` were
  created fresh as first snapshots (see their own "origin" notes).
  `GAME_DESIGN_GENERATOR_VARIANT.md`, `GAMEPLAY_LOOPS.md`, and
  `DECISIONS_LOG.md` still do not exist — a human should confirm whether
  they exist elsewhere or need authoring.

### Generator: multiple heroes per class, random selection on spawn
- Status: complete
- What was done: Changed `HeroClassConfig.heroPrefab` (single) to
  `HeroClassConfig.heroPrefabs` (`GameObject[]`) so a generator can hold
  multiple heroes of the same class. `Generator.TryTap()` now picks one
  prefab at random (`Random.Range`) before spending mana; if the array is
  empty/null it no-ops without spending mana, same as the existing
  afford/slot checks. Updated `GeneratorSetupTool.cs` to seed each
  generated config's `heroPrefabs` with a single-entry array (add more
  prefabs manually in the Inspector for actual variety per class).
  Updated `SYS_Generator.md`'s implementation notes accordingly.
- Source: explicit user request (not an inferred/guessed design value —
  no `// TODO(design)` needed for the random-selection behavior itself).
- What's left: none for this change. Balance/variety of actual hero
  prefabs per class is a content task, not a code task.

### Real Mana + Coin Economy (Generator Variant)
- Status: complete
- What was done: Created a shared `CurrencyEconomy` base
  (`Assets/Scripts/Economy/CurrencyEconomy.cs`) covering current amount,
  max cap, `CanAfford`/`Spend`, and a serialized `CurrencyUpgradeTier`
  list with `HasNextUpgradeTier()`/`TryUpgradeCap()` for raising the cap
  by spending the currency's own points. Refactored `ManaEconomy` from
  the earlier bare stub into a thin `CurrencyEconomy` subclass
  (kept `CurrentMana` as a compatibility alias so `Generator` needed no
  changes). Added `CoinEconomy` as a second subclass with a
  `CurrentCoins` alias and no consumer yet. Rewrote `SYS_ManaEconomy.md`
  from "STUB ONLY" to a real-implementation doc and authored a new
  `SYS_CoinEconomy.md` with matching structure. Updated `SYSTEM_MAP.md`
  and `PROJECT_INDEX.md` (systems table, open unknowns, logical next
  step) to reflect both currencies as real, plus a new Tech Stack &
  Dependencies section recording FEEL / TopDown Engine / DoTween /
  Cinemachine as confirmed-but-not-yet-integrated dependencies.
- What's left: no reward-hook API (`AddMana`/`AddCoins`) exists yet —
  intentionally deferred until a combat/base-destruction system is
  built. No spender/shop consumes coins yet either. MergeSystem remains
  the next unbuilt system referenced by other docs.

### Hero Character Data Architecture (Generator Variant)
- Status: complete
- What was done: Built the foundational per-hero data layer ahead of
  MergeSystem. Created `HeroRole` (enum: Tank, MeleeDps, RangedDps,
  Support, Controller — the "Class" layer), replacing the retired
  `HeroClassId` (Ground/Air/Vehicles). Created `HeroStats` (serializable
  struct: Health, Attack, AttackSpeed, Range, Armor, CritChance,
  CritDamage — the "Stats" layer) with a `Multiply` helper for star
  scaling. Created `HeroDefinition` (ScriptableObject, one asset per
  individual hero — not per class) with `heroId` (unique string, the key
  the future MergeSystem will use for match validation), `displayName`,
  `role`, `baseStats`, `perStarMultiplier` (default 1.5), `prefab`, and
  `GetStatsForStar(int starLevel)` which compounds the multiplier per
  star above 1. Created `HeroInstance` (MonoBehaviour, attached to
  spawned heroes) holding `definition` + `starLevel` with a computed
  `CurrentStats` property. Scaffolded the five remaining architecture
  layers (Subclass, Species, Background, Feats, Equipment/Spells) as
  empty placeholder classes with a single `id` field each — no real
  content, per explicit scope decision to build only Class + Stats for
  real this pass. Retired `HeroClassConfig.cs`, replaced by
  `GeneratorConfig.cs` (`manaCost` + `heroPool` of `HeroDefinition[]`,
  `GetRandomHeroDefinition()`). Updated `Generator.TryTap()` to consume
  `GeneratorConfig`/`HeroDefinition` and set up `HeroInstance` on spawn.
  Rewrote `GeneratorSetupTool.cs` to seed placeholder `HeroDefinition`
  assets + a sample `GeneratorConfig` instead of the old
  `HeroClassConfig` assets. Added `HeroDefinitionValidator.cs` (editor
  menu `MergeWars/Validate/Check Duplicate Hero IDs`) to warn on
  duplicate `heroId` values across all `HeroDefinition` assets
  (non-blocking, manual-run). Authored `SYS_HeroDefinition.md`, updated
  `SYS_Generator.md`, `SYSTEM_MAP.md`, and `PROJECT_INDEX.md`
  accordingly.
- Source: explicit user request/design brief (Class/Subclass/Species/
  Background/Stats/Feats/Equipment-Spells layering, HeroRole values,
  per-hero authoring, unique heroId for future merge-matching, editor
  duplicate-ID validation) — clarified via Q&A before implementation.
- What's left: MergeSystem itself (the immediate next task) — will read
  `HeroInstance.definition.heroId` + `starLevel` to validate merges (max
  star level 4, confirmed), and mutate `starLevel` on the destination
  hero on success. No combat/damage resolution consumes `HeroStats` yet.
- TODO(design) markers left in code: every `HeroStats` field value and
  `HeroDefinition.perStarMultiplier` (1.5) are ASSUMED PLACEHOLDERS
  pending a real balance pass; `HeroSubclass`/`HeroSpecies`/
  `HeroBackground`/`HeroFeat`/`HeroEquipmentSpell` are explicitly marked
  scaffolded-only with no real content or rules.
- TODO(design)/open unknowns left: whether currencies are a single
  global pool vs. per-player/per-base is UNRESOLVED (implemented as
  global for now); whether cap-upgrade cost is paid in the same currency
  is assumed, not confirmed; starting amounts, max caps, and all
  upgrade-tier cost/newMaxCap values are ASSUMED PLACEHOLDERS pending a
  real balance pass.

### MergeSystem (Generator Variant)
- Status: complete
- What was done: Built `MergeSystem` (`Assets/Scripts/Merge/MergeSystem.cs`),
  which validates and executes merges between two occupied grid slots.
  `CanMerge(HeroInstance, HeroInstance)` checks: both instances non-null
  with a non-null `definition`, `definition.heroId` matches exactly
  (CONFIRMED as the match key — not prefab identity), `starLevel` matches,
  and the destination is below the max star level (4, CONFIRMED — merging
  two max-star heroes is a no-op, not a sell/convert mechanic).
  `TryMerge(slotIndexA, slotIndexB)` increments the destination
  (`slotIndexB`) hero's `starLevel`, then removes the source
  (`slotIndexA`) occupant via `GridManager.RemoveOccupant` and releases it
  via `PoolManager.Release`. No adjacency requirement (CONFIRMED). Added
  `GridManager.TryGetSlotIndexForOccupant(GameObject)` — reverse lookup
  needed by merge input to resolve a hero GameObject back to its slot
  index (previously only the forward `GetOccupant(slotIndex)` existed).
  Built both confirmed input triggers as a single component,
  `HeroMergeInput` (`Assets/Scripts/Merge/HeroMergeInput.cs`) — not two
  separate components, since Unity's `OnMouseDown`/`OnMouseDrag`/
  `OnMouseUp` dispatch is per-GameObject and two independent components
  would double-handle the same mouse events. It disambiguates tap vs.
  drag by total mouse-movement distance during the press
  (`dragThresholdPixels`, default 10, ASSUMED PLACEHOLDER). Tap-tap-select
  uses a static `selectedForTap` field shared across all hero instances.
  Drag-and-drop repositions the hero via raycast while held and snaps back
  to its original position (no tween) if released over an invalid target.
  `Generator` gained an optional `mergeSystem` field — if assigned,
  `TryTap()` gets-or-adds a `HeroMergeInput` on the spawned hero and calls
  `Initialize(gridManager, mergeSystem)`; if left unassigned, spawned
  heroes simply have no merge input, with no effect on Generator's core
  spawn flow. Authored `SYS_MergeSystem.md`; updated `SYS_Generator.md`,
  `SYSTEM_MAP.md`, and `PROJECT_INDEX.md` accordingly.
- Source: explicit user request/design brief (max 4-star merge, same-hero
  matching, both tap-tap and drag-and-drop input, no adjacency
  requirement, no-op at max star) — clarified via Q&A before
  implementation, building directly on the prior Hero Character Data
  Architecture task's `HeroDefinition.heroId`/`HeroInstance.starLevel`.
- What's left: no visual/audio feedback on a successful merge (FEEL/
  DoTween still unintegrated project-wide). No combat/damage system
  consumes the resulting `HeroStats` yet. `MergeSystem.CanMerge` is
  public but currently unused by any preview/highlight UI.
- TODO(design) markers left in code: `HeroMergeInput.dragThresholdPixels`
  (10) is an ASSUMED PLACEHOLDER — no spec for tap-vs-drag sensitivity;
  the max-star constant (4) is hardcoded in `MergeSystem` rather than
  sourced from a shared config, since none exists yet.

### Doc sync pass (reconstructed from docs and code)
- Status: complete
- Note: entries in this section were reconstructed by reading the current
  docs and C# source, not from observing the work as it happened. No dates
  or details beyond what the sources show are asserted.
- What was done: Reconciled docs with code. Updated SYS_HeroDefinition.md
  (current HeroStats fields, the five attack/targeting enums,
  spawnHeightOffset, star-scaling caveat), SYS_Generator.md (removed retired
  HeroClassConfig/Ground-Air-Vehicles and ManaEconomy-stub text; documented
  GeneratorConfig fields, spawnHeightOffset use, GeneratorSpawner details),
  SYS_ManaEconomy.md (HeroClassConfig -> GeneratorConfig reference),
  SYS_GridManager.md (fixed truncated line and encoding artifacts;
  documented anchor-as-center, independent cell size, merge-area overflow
  warning, OnValidate, gizmos, CellSizeX/Z), SYS_MergeSystem.md (virtual
  drag plane, SmoothDamp, trigger-collider toggle, RaycastAll ignoring self,
  Camera.main/legacy Input, reposition behavior), SYSTEM_MAP.md (missing
  edges), PROJECT_INDEX.md (last-updated line, stale mana-cost unknown,
  new unknowns), CURRENT_TASK.md (duplicate title line only).
- What's left: GeneratorSetupTool.cs and HeroDefinitionValidator.cs were not
  re-read. Design docs (GAME_DESIGN_GENERATOR_VARIANT.md, GAMEPLAY_LOOPS.md,
  DECISIONS_LOG.md, IMPLEMENTATION_PLAN.md) were not provided. Nothing was
  compiled or run in Unity.
- TODO(design) markers: none added to code (docs only).

### GeneratorSpawner / GeneratorLoadout + GridData multi-grid (reconstructed)
- Status: complete (per docs/code)
- What was done: GridManager slot logic extracted into plain C# GridData;
  GridManager is a per-instance MonoBehaviour wrapper with SetDimensions().
  GeneratorLoadout (ScriptableObject) and GeneratorSpawner place Generator
  prefabs into a dedicated Generator grid at Start(), wiring scene
  dependencies via Generator.Initialize(...).
- What's left: layout beyond one row and player-selection mapping onto
  GeneratorLoadout are unresolved.
- TODO(design): single-row layout in GeneratorSpawner is an ASSUMED
  PLACEHOLDER.

### Strategic hero repositioning (reconstructed)
- Status: complete (per docs/code)
- What was done: HeroMergeInput drag release now (1) merges if over another
  hero, (2) else moves to the nearest open slot via
  GridManager.TryGetNearestOpenSlotIndex, (3) else snaps back. Tap-tap-select
  remains merge-only.
- What's left: repositioned heroes do not apply spawnHeightOffset; a failed
  merge drop can fall through to repositioning - both unconfirmed as
  intended.

### Drag-feel changes in HeroMergeInput (reconstructed)
- Status: complete (per code)
- What was done: dragging uses a fixed-height virtual plane
  (dragHeightOffset), SmoothDamp movement (dragSmoothTime), a temporary
  trigger collider, and RaycastAll ignoring the hero's own collider.
- TODO(design): dragHeightOffset (0.5) and dragSmoothTime (0.06) are
  ASSUMED PLACEHOLDERS.

### HeroStats expansion and attack/targeting enums (reconstructed)
- Status: complete (per code)
- What was done: HeroStats now carries hitPoints, targets, hitSpeed, range,
  damage, damagePerSec, count, speed, attackType, projectileSpeed,
  attackArea plus targetTeam/targetType/targetRestriction/targetPreference.
  Enums (AttackType, TargetTeam, TargetType, TargetRestriction,
  TargetPreference) are declared in HeroStats.cs. HeroDefinition gained
  spawnHeightOffset.
- What's left: intended per-field star scaling, overlap between the
  targeting enums, and GeneratorType vs HeroRole are unresolved.
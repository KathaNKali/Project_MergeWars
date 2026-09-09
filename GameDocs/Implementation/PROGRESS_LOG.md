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

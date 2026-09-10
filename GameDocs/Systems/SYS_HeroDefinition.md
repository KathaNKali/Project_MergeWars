# SYS_HeroDefinition.md — Hero Character Data Architecture (Generator Variant)

## Responsibility
Owns the per-hero data model: identity, combat role, and stat computation.
This is the foundation the future MergeSystem builds on top of (matching
and star-level mutation), but MergeSystem itself is a separate, not-yet-built
task.

The character architecture is broken into seven layers:

| Layer | Design Function | Status this pass |
|---|---|---|
| Class | Core gameplay | **Real** — `HeroRole` enum (Tank, MeleeDps, RangedDps, Support, Controller) |
| Subclass | Specialization | Scaffolded placeholder only (`HeroSubclass`) |
| Species | Identity + unique traits | Scaffolded placeholder only (`HeroSpecies`) |
| Background | Starting specialization | Scaffolded placeholder only (`HeroBackground`) |
| Stats | Mathematical foundation | **Real** — `HeroStats` struct + `HeroDefinition.GetStatsForStar` |
| Feats | Build customization | Scaffolded placeholder only (`HeroFeat`) |
| Equipment / Spells | Moment-to-moment loadout | Scaffolded placeholder only (`HeroEquipmentSpell`) |

## Does NOT know about
- Combat/damage resolution — no combat system exists yet. `HeroStats` is
  pure data; nothing consumes it for actual damage calculations this pass.
- Merging — `MergeSystem` does not exist yet. This system only defines the
  data (`heroId`, `starLevel`, `HeroDefinition.GetStatsForStar`) that a
  future MergeSystem will read and mutate. No merge validation, matching,
  or grid interaction happens here.
- Grid/slot placement — `HeroInstance` is attached to a spawned hero
  GameObject by `Generator`; it has no knowledge of `GridManager` or slot
  indices.
- Synergies between heroes — explicitly out of scope this pass.

## Key types
- **HeroRole** (`Assets/Scripts/Heroes/HeroRole.cs`) — enum, replaces the
  old `HeroClassId` (Ground/Air/Vehicles). CONFIRMED: fixed for a hero's
  lifetime, does not change on merge/star-up.
- **HeroStats** (`Assets/Scripts/Heroes/HeroStats.cs`) — serializable
  struct (Health, Attack, AttackSpeed, Range, Armor, CritChance,
  CritDamage). All field values on any `HeroDefinition` are ASSUMED
  PLACEHOLDER — no balance doc specifies real numbers yet. Includes a
  static `Multiply(stats, multiplier)` helper used for star scaling.
- **HeroDefinition** (`Assets/Scripts/Heroes/HeroDefinition.cs`) —
  ScriptableObject, **one asset per individual hero** (CONFIRMED — not per
  class/role; this is the easiest authoring model for adding new heroes
  one at a time). Fields:
  - `heroId` (string) — CONFIRMED as the unique key the future MergeSystem
	will use to decide mergeable matches: two heroes merge if `heroId`
	matches AND `starLevel` matches. This is NOT prefab/reference identity
	— supersedes an earlier draft note that assumed prefab-identity
	matching before this was clarified.
  - `displayName`, `role` (HeroRole), `baseStats` (HeroStats, at star 1),
	`perStarMultiplier` (float, default 1.5 — ASSUMED PLACEHOLDER, flat
	multiplier per star is the CONFIRMED mechanic but the value itself is
	a guess pending balance), `prefab` (GameObject, pooled/visual prefab).
  - `GetStatsForStar(int starLevel)` — compounds `perStarMultiplier` per
	star above 1 (star 1 = unscaled `baseStats`). Single source of truth
	for star-scaled stats.
  - Placeholder references to the five scaffolded layers (subclass,
	species, background, feats, equipmentSpells) — all optional/empty
	this pass.
- **HeroInstance** (`Assets/Scripts/Heroes/HeroInstance.cs`) — MonoBehaviour
  attached to a spawned hero GameObject by `Generator` at spawn time.
  Holds `definition` (HeroDefinition reference) and `starLevel` (int,
  default 1). `CurrentStats` is computed on demand via
  `definition.GetStatsForStar(starLevel)` — never cached, so it is always
  correct immediately after a star-level change. This is the runtime
  occupant identity component the future MergeSystem will read
  (`definition.heroId` + `starLevel`) and mutate (`starLevel`) on a
  successful merge.
- **HeroSubclass / HeroSpecies / HeroBackground / HeroFeat /
  HeroEquipmentSpell** (`Assets/Scripts/Heroes/`) — scaffolded-only
  placeholder classes (a single `id` string field each), explicitly marked
  `// TODO(design): scaffolded only, not implemented this pass`. No real
  content, rules, or gameplay behavior — exist purely so `HeroDefinition`
  has fields to reference once each layer is actually designed.

## Uniqueness validation
`Assets/Editor/HeroDefinitionValidator.cs` — editor menu item
`MergeWars/Validate/Check Duplicate Hero IDs`. Scans all `HeroDefinition`
assets via `AssetDatabase.FindAssets("t:HeroDefinition")`, groups by
`heroId`, and logs a `Debug.LogWarning` per duplicate group listing the
conflicting asset paths. CONFIRMED as non-blocking/manual-run — no
automatic enforcement on save or build.

## Generator integration
`GeneratorConfig` (`Assets/Scripts/Generator/GeneratorConfig.cs`) replaces
the old `HeroClassConfig`: holds `manaCost` (CONFIRMED to stay on this
generator-side wrapper, not per-hero) and `heroPool` (`HeroDefinition[]`).
`GetRandomHeroDefinition()` picks one at random — same random-selection
behavior as before, now at the individual-hero level rather than the
per-class-prefab-array level.

`Generator.TryTap()` picks a random `HeroDefinition` from
`config.heroPool`, spends `config.manaCost`, pools the hero via
`PoolManager.Get(definition.prefab)`, then gets-or-adds a `HeroInstance`
component on the spawned object and sets `definition` +
`starLevel = 1` before placing it into the grid. See SYS_Generator.md for
the full flow.

## Open Unknowns
- Whether cap-upgrade-style stat growth beyond star 4 exists is
  unspecified (max star is 4, confirmed elsewhere — this doc only covers
  the stat-computation mechanism, not the merge cap itself).
- Real values for every `HeroStats` field and `perStarMultiplier` are
  ASSUMED PLACEHOLDERS pending a balance pass.
- Content/rules for Subclass, Species, Background, Feats, and
  Equipment/Spells are entirely undesigned — scaffolds only.
- Whether `HeroDefinitionValidator` should also run automatically (e.g.
  via `AssetPostprocessor` on import) is unconfirmed — currently manual-run
  only, per explicit request.

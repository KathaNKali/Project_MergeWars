# SYS_HeroDefinition.md - Hero Character Data Architecture (Generator Variant)

_Synced with code: HeroStats.cs, HeroDefinition.cs, HeroInstance.cs, HeroRole.cs, GeneratorConfig.cs, MergeSystem.cs. Editor tools (HeroDefinitionValidator, GeneratorSetupTool) were not re-read this pass - see "Not verified"._

## Responsibility
Owns the per-hero data model: identity, combat role, and stat computation.
MergeSystem (built, see SYS_MergeSystem.md) builds on top of this layer -
it reads `definition.heroId` + `starLevel` and mutates `starLevel`.

The character architecture is broken into seven layers:

| Layer | Design Function | Status |
|---|---|---|
| Class | Core gameplay | **Real** - `HeroRole` enum (Tank, MeleeDps, RangedDps, Support, Controller) |
| Subclass | Specialization | Scaffolded placeholder only (`HeroSubclass`) |
| Species | Identity + unique traits | Scaffolded placeholder only (`HeroSpecies`) |
| Background | Starting specialization | Scaffolded placeholder only (`HeroBackground`) |
| Stats | Mathematical foundation | **Real** - `HeroStats` struct + attack/targeting enums + `HeroDefinition.GetStatsForStar` |
| Feats | Build customization | Scaffolded placeholder only (`HeroFeat`) |
| Equipment / Spells | Moment-to-moment loadout | Scaffolded placeholder only (`HeroEquipmentSpell`) |

## Does NOT know about
- Combat/damage resolution - no combat system exists yet. `HeroStats` is
  pure data; nothing consumes it for actual damage calculations, and
  nothing reads the targeting enums yet.
- Merging rules - `MergeSystem` owns matching/validation. This system only
  defines the data (`heroId`, `starLevel`, `GetStatsForStar`) MergeSystem
  reads and mutates.
- Grid/slot placement - `HeroInstance` has no knowledge of `GridManager`
  or slot indices.
- Synergies between heroes - out of scope.

## Key types
- **HeroRole** (`Assets/Scripts/Heroes/HeroRole.cs`) - enum (Tank,
  MeleeDps, RangedDps, Support, Controller). Replaced the retired
  `HeroClassId`. CONFIRMED: fixed for a hero's lifetime, does not change
  on merge/star-up.
- **HeroStats** (`Assets/Scripts/Heroes/HeroStats.cs`, namespace
  `MergeWars.Heroes`) - serializable struct. Fields:
  - Core combat (float): `hitPoints`, `targets`, `hitSpeed`, `range`,
    `damage`, `damagePerSec`, `count`, `speed`.
  - Attack: `attackType` (`AttackType`), `projectileSpeed` (float),
    `attackArea` (float).
  - Targeting (enums): `targetTeam`, `targetType`, `targetRestriction`,
    `targetPreference`.
  - Static `Multiply(stats, multiplier)` scales ALL numeric fields
    (including `targets`, `hitSpeed`, `range`, `count`, `speed`,
    `projectileSpeed`, `attackArea`) and copies the enum fields unchanged.
  - All field values on any `HeroDefinition` are ASSUMED PLACEHOLDER - no
    balance doc specifies real numbers.
- **Attack/targeting enums** - declared in `HeroStats.cs` alongside the
  struct (same namespace, so file location does not affect usage). Nothing
  outside `HeroStats` references them yet.
  - `AttackType`: Melee, Ranged, Area
  - `TargetTeam`: Enemy, Ally, Self, AllyOrSelf
  - `TargetType`: Ground, Air, GroundAndAir, Buildings, GroundAndBuildings,
    AirAndBuildings, All
  - `TargetRestriction`: None, GroundOnly, AirOnly, BuildingsOnly,
    TroopsOnly, HeroesOnly, DefensiveBuildingsOnly, SupportUnitsOnly,
    BossOnly
  - `TargetPreference`: None, Nearest, LowestHealth, HighestHealth,
    LowestDamage, HighestDamage, Buildings, DefensiveBuildings,
    SupportUnits, Tanks, Backline, AirUnits, GroundUnits, Boss
  - Unity serializes enums as integers: append new values at the end (or
    give explicit numbers). Inserting/reordering/deleting values silently
    changes already-authored `HeroDefinition` assets.
- **HeroDefinition** (`Assets/Scripts/Heroes/HeroDefinition.cs`) -
  ScriptableObject, **one asset per individual hero** (CONFIRMED - not per
  class/role). Fields:
  - `heroId` (string) - CONFIRMED as the unique key MergeSystem uses to
    decide mergeable matches: two heroes merge if `heroId` matches AND
    `starLevel` matches. NOT prefab/reference identity.
  - `displayName`, `role` (HeroRole), `baseStats` (HeroStats, at star 1),
    `perStarMultiplier` (float, default 1.5 - ASSUMED PLACEHOLDER; flat
    multiplier per star is the CONFIRMED mechanic, the value is a guess),
    `prefab` (GameObject, pooled/visual prefab).
  - `spawnHeightOffset` (float, default 0.5 - ASSUMED PLACEHOLDER) -
    vertical offset added on top of the grid slot's world position at
    spawn (Generator) so a centered-pivot model does not sit half-buried.
    Tune per hero. Note: HeroMergeInput's reposition path snaps to the
    slot's world position WITHOUT this offset (see SYS_MergeSystem.md).
  - `GetStatsForStar(int starLevel)` - compounds `perStarMultiplier` per
    star above 1 (star 1 = unscaled `baseStats`). Single source of truth
    for star-scaled stats.
  - Placeholder references to the five scaffolded layers (subclass,
    species, background, feats, equipmentSpells) - all optional/empty.
- **HeroInstance** (`Assets/Scripts/Heroes/HeroInstance.cs`) -
  MonoBehaviour attached to a spawned hero by `Generator`. Holds
  `definition` and `starLevel` (default 1; MergeSystem is the only
  system that mutates it after spawn). `CurrentStats` is computed on
  demand via `definition.GetStatsForStar(starLevel)` - never cached.
- **HeroSubclass / HeroSpecies / HeroBackground / HeroFeat /
  HeroEquipmentSpell** - scaffolded-only placeholder classes (a single
  `id` string each), marked `// TODO(design)`. No content or rules.

## Uniqueness validation
`Assets/Editor/HeroDefinitionValidator.cs` - menu item
`MergeWars/Validate/Check Duplicate Hero IDs`. Groups all
`HeroDefinition` assets by `heroId` and logs a `Debug.LogWarning` per
duplicate group. CONFIRMED as non-blocking/manual-run. (Source file not
re-read this pass.)

## Generator integration
`GeneratorConfig` (`Assets/Scripts/Generator/GeneratorConfig.cs`) holds
`manaCost` (CONFIRMED to stay on the generator-side wrapper, not per-hero)
and `heroPool` (`HeroDefinition[]`); `GetRandomHeroDefinition()` picks one
at random. `Generator.TryTap()` spends `config.manaCost`, pools the hero
via `PoolManager.Get(definition.prefab)`, positions it at slot position +
`spawnHeightOffset`, then gets-or-adds a `HeroInstance` and sets
`definition` + `starLevel = 1`. See SYS_Generator.md.

## Open Unknowns
- Real values for every `HeroStats` field, `perStarMultiplier`, and
  `spawnHeightOffset` are ASSUMED PLACEHOLDERS pending a balance pass.
- **Star scaling of non-power stats (CONFLICT, not resolved):**
  `Multiply` scales every numeric field. If `hitSpeed` means seconds
  between attacks, higher stars would attack slower; `targets`, `count`,
  `range`, `speed`, `projectileSpeed`, `attackArea` scaling may also be
  unintended. Confirm intended per-field scaling before combat consumes
  these.
- **Overlapping targeting enums (not resolved):** `TargetType`,
  `TargetRestriction` and `TargetPreference` all express ground/air/
  building concepts. How they combine (and whether they are separate
  concepts) is undocumented. Settle before a combat/targeting task.
- Enum file location: currently in `HeroStats.cs`. Splitting into their
  own file(s) is optional and deferred until another system consumes them.
- **`GeneratorConfig.GeneratorType` vs `HeroRole` (not resolved):**
  `GeneratorConfig` has its own `GeneratorType` enum (Tank, Assassin,
  Support, Controller, DamageDealer, Marksman, Artillery) alongside
  `barracksID`, `barracksName`, `classType`. How this relates to
  `HeroRole` is undocumented.
- Whether stat growth beyond star 4 exists: max star is 4 (enforced in
  MergeSystem); this doc only covers the stat mechanism.
- Content/rules for Subclass, Species, Background, Feats, and
  Equipment/Spells are undesigned - scaffolds only.
- Whether `HeroDefinitionValidator` should run automatically is
  unconfirmed - manual-run only.

## Not verified
- `HeroDefinitionValidator.cs` and `GeneratorSetupTool.cs` were not shared
  this pass. If the setup tool still assigns the old stat fields (Health,
  Attack, AttackSpeed, Armor, CritChance, CritDamage), it will no longer
  compile against the current `HeroStats`. Nothing here was compiled or
  run in Unity.
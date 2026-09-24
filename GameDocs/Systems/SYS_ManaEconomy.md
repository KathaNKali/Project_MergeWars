# SYS_ManaEconomy.md — Generator Variant

## Status: Real implementation (built on shared CurrencyEconomy base)
Mana is now a real currency pool, no longer the earlier bare stub. It
shares its implementation with Coins via a common base class,
`CurrencyEconomy` (see `Assets/Scripts/Economy/CurrencyEconomy.cs`), since
both currencies follow the same pattern (current amount, max cap,
afford/spend, cap-upgrade tiers).

## Confirmed this pass
- CONFIRMED: mana (and coins) are earned by destroying enemy bases in
  combat.
- CONFIRMED: mana has a max cap that the player upgrades over time,
  similar to coins — the upgrade cost/tier logic lives inside the economy
  system itself (not a separate "Upgrades" system).

## Responsibility
Tracks a current mana amount and a max cap, answers whether a cost is
affordable, deducts it on spend, and lets the cap be raised by spending
toward the next upgrade tier.

## Depends on
- (none — foundational economy value other systems read from)

## Does NOT know about
- Grid, generators, combat, unlock state — purely a currency pool.
- How mana is earned (see "Deferred" below) — it does not listen for
  combat/base-destruction events itself.

## Implementation
- Location: `Assets/Scripts/Economy/ManaEconomy.cs` (namespace
  `MergeWars.Economy`), thin subclass of
  `Assets/Scripts/Economy/CurrencyEconomy.cs`.
- API (inherited from `CurrencyEconomy`): `int CurrentAmount`,
  `int MaxCap`, `bool CanAfford(int cost)`, `bool Spend(int cost)`,
  `bool HasNextUpgradeTier()`, `bool TryUpgradeCap()`. `ManaEconomy` adds
  `int CurrentMana => CurrentAmount` as a naming-convenience alias so
  existing callers (e.g. `Generator`) keep reading "mana" terminology.
- Upgrade tiers are a serialized `List<CurrencyUpgradeTier>`
  (`{cost, newMaxCap}`) configured per-instance in the Inspector.
  `TryUpgradeCap()` spends the next tier's cost (in mana itself) and
  raises `maxCap` to that tier's value, advancing to the next tier.

## Deferred (intentionally not built yet)
- No `AddMana(amount)` or similar reward-hook API exists yet. Wiring mana
  gains from destroying a base is explicitly deferred until the
  combat/base-destruction system is actually built — adding it now would
  be building ahead of an unbuilt system.

## Open Unknowns (do not resolve silently)
- Whether mana should be a single global pool or per-player/per-base is
  UNRESOLVED. Current implementation is a single global pool (one
  `ManaEconomy` component, matching the prior stub's behavior) — this is
  NOT a final design decision, just what's implemented for now.
- Whether a cap-upgrade's cost is paid in mana itself (assumed here, see
  `CurrencyUpgradeTier` in `CurrencyEconomy.cs`) or a different currency
  (e.g. coins upgrading mana's cap) is unspecified.
- Starting `currentAmount`, starting `maxCap`, and all upgrade tier
  `cost`/`newMaxCap` values are ASSUMED PLACEHOLDERS — no balance doc
  specifies real numbers.
- Real per-generator hero mana costs (`GeneratorConfig.manaCost`, default 10) are still
  placeholders (see SYS_Generator.md) - unrelated to this file's own
  Unknowns but worth resolving together during a real balance pass.
  (Previously referred to the retired `HeroClassConfig.manaCost`.)
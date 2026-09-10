# SYS_CoinEconomy.md — Generator Variant

## Status: Real implementation (built on shared CurrencyEconomy base)
Coins share their implementation with Mana via a common base class,
`CurrencyEconomy` (see `Assets/Scripts/Economy/CurrencyEconomy.cs`), since
both currencies follow the same pattern (current amount, max cap,
afford/spend, cap-upgrade tiers). See `SYS_ManaEconomy.md` for the
sibling system.

## Confirmed this pass
- CONFIRMED: coins (and mana) are earned by destroying enemy bases in
  combat.
- CONFIRMED: coins have a max cap that the player upgrades over time —
  the upgrade cost/tier logic lives inside the economy system itself (not
  a separate "Upgrades" system).

## Responsibility
Tracks a current coin amount and a max cap, answers whether a cost is
affordable, deducts it on spend, and lets the cap be raised by spending
toward the next upgrade tier.

## Depends on
- (none — foundational economy value other systems read from)

## Does NOT know about
- Grid, generators, combat, unlock state — purely a currency pool.
- How coins are earned (see "Deferred" below) — it does not listen for
  combat/base-destruction events itself.
- What coins are spent on — no shop/purchase system exists yet, so
  CoinEconomy currently has no consumer/spender at all. That's expected,
  not a gap to fix here.

## Implementation
- Location: `Assets/Scripts/Economy/CoinEconomy.cs` (namespace
  `MergeWars.Economy`), thin subclass of
  `Assets/Scripts/Economy/CurrencyEconomy.cs`.
- API (inherited from `CurrencyEconomy`): `int CurrentAmount`,
  `int MaxCap`, `bool CanAfford(int cost)`, `bool Spend(int cost)`,
  `bool HasNextUpgradeTier()`, `bool TryUpgradeCap()`. `CoinEconomy` adds
  `int CurrentCoins => CurrentAmount` as a naming-convenience alias.
- Upgrade tiers are a serialized `List<CurrencyUpgradeTier>`
  (`{cost, newMaxCap}`) configured per-instance in the Inspector.

## Deferred (intentionally not built yet)
- No `AddCoins(amount)` or similar reward-hook API exists yet. Wiring
  coin gains from destroying a base is explicitly deferred until the
  combat/base-destruction system is actually built.
- No shop/purchase system consumes coins yet — CoinEconomy exists purely
  as a currency pool with no spender for now.

## Open Unknowns (do not resolve silently)
- Whether coins should be a single global pool or per-player/per-base is
  UNRESOLVED, same as ManaEconomy.
- Whether a cap-upgrade's cost is paid in coins itself (assumed here) or
  a different currency is unspecified.
- Starting `currentAmount`, starting `maxCap`, and all upgrade tier
  `cost`/`newMaxCap` values are ASSUMED PLACEHOLDERS — no balance doc
  specifies real numbers.

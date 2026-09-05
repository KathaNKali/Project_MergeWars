# SYS_ManaEconomy.md — Generator Variant

## Status: STUB ONLY (not a real design pass)
This file was created only because Generator needed something to depend
on for mana checks/spend, and no ManaEconomy design existed anywhere in
`/GameDocs/`. Nothing below is a confirmed design decision — treat all of
it as ⚠️ ASSUMED PLACEHOLDER until a real design pass replaces this file.

## Responsibility (assumed)
Tracks a single current-mana value and answers whether a cost is
affordable, and deducts it on spend.

## Depends on
- (assumed none — foundational economy value other systems read from)

## Does NOT know about
- Grid, generators, combat, unlock state — purely a number + two methods

## Implementation
- Location: `Assets/Scripts/Economy/ManaEconomy.cs` (namespace
  `MergeWars.Economy`), MonoBehaviour.
- API: `bool CanAfford(int cost)`, `bool Spend(int cost)` (returns false
  and does nothing if unaffordable), `int CurrentMana` (read-only).
- `// TODO(design)`: starting mana value (currently 100), whether mana
  regenerates over time, whether there's a cap, and all real hero mana
  costs are unspecified anywhere in `/GameDocs/`. Replace this whole file
  and the script once real design values exist.

namespace MergeWars.Economy
{
    /// <summary>
    /// Coin currency pool. Same shape as ManaEconomy (current amount, max
    /// cap, afford/spend, cap-upgrade tiers) via the shared
    /// CurrencyEconomy base. Coins are earned by destroying enemy bases
    /// in combat, same as Mana — that reward hook is intentionally not
    /// implemented here yet, deferred until the combat/base-destruction
    /// system exists. No spender/consumer of coins exists yet either
    /// (e.g. a shop/upgrade-purchase system) — that's expected, not a gap
    /// to fix here.
    /// See /GameDocs/Systems/SYS_CoinEconomy.md.
    /// </summary>
    public class CoinEconomy : CurrencyEconomy
    {
        public int CurrentCoins => CurrentAmount;
    }
}

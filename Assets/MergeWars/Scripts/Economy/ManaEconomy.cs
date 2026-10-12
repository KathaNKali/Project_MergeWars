namespace MergeWars.Economy
{
    /// <summary>
    /// Mana currency pool. Real implementation built on the shared
    /// CurrencyEconomy base (current amount, max cap, afford/spend,
    /// cap-upgrade tiers). Mana is earned by destroying enemy bases in
    /// combat — that reward hook is intentionally not implemented here
    /// yet, deferred until the combat/base-destruction system exists.
    /// See /GameDocs/Systems/SYS_ManaEconomy.md.
    /// </summary>
    public class ManaEconomy : CurrencyEconomy
    {
        // Backward-compatible alias so existing callers (e.g. Generator)
        // can keep reading "mana" terminology instead of the generic
        // CurrentAmount name.
        public int CurrentMana => CurrentAmount;
    }
}

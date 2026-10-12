using System.Collections.Generic;
using UnityEngine;

namespace MergeWars.Economy
{
    /// <summary>
    /// One tier in a currency's max-cap upgrade path: spend `cost` of the
    /// currency itself to raise its cap to `newMaxCap`.
    /// </summary>
    [System.Serializable]
    public class CurrencyUpgradeTier
    {
        // TODO(design): whether an upgrade's cost is paid in the same
        // currency it upgrades (assumed here) or a different one (e.g.
        // coins upgrading mana's cap) is not specified anywhere in
        // /GameDocs/. Revisit if that turns out to be wrong.
        public int cost;
        public int newMaxCap;
    }

    /// <summary>
    /// Shared base for a single currency pool (Mana, Coins, ...): tracks a
    /// current amount and a max cap, supports afford/spend, and lets the
    /// cap itself be upgraded through a serialized list of tiers.
    ///
    /// Does NOT know how the currency is earned (e.g. destroying a base)
    /// — that reward hook is intentionally not implemented here yet,
    /// deferred until the combat/base-destruction system exists (per
    /// explicit decision — see SYS_ManaEconomy.md / SYS_CoinEconomy.md).
    ///
    /// ⚠️ OPEN QUESTION (unresolved, not silently decided): whether this
    /// should be a single global pool or per-player/per-base. Currently
    /// implemented as a single global pool, matching prior stub behavior.
    /// </summary>
    public abstract class CurrencyEconomy : MonoBehaviour
    {
        [Tooltip("ASSUMED PLACEHOLDER starting amount — not confirmed anywhere in /GameDocs/.")]
        [SerializeField] private int currentAmount = 0;

        [Tooltip("ASSUMED PLACEHOLDER starting max cap — not confirmed anywhere in /GameDocs/.")]
        [SerializeField] private int maxCap = 100;

        [Tooltip("ASSUMED PLACEHOLDER upgrade tiers (cost paid in this same currency -> new max cap), in order.")]
        [SerializeField] private List<CurrencyUpgradeTier> upgradeTiers = new List<CurrencyUpgradeTier>();

        [SerializeField] private int currentTierIndex = 0;

        public int CurrentAmount => currentAmount;
        public int MaxCap => maxCap;

        public bool CanAfford(int cost)
        {
            return currentAmount >= cost;
        }

        public bool Spend(int cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            currentAmount -= cost;
            return true;
        }

        /// <summary>
        /// Returns true if there is another upgrade tier beyond the
        /// current cap.
        /// </summary>
        public bool HasNextUpgradeTier()
        {
            return currentTierIndex < upgradeTiers.Count;
        }

        /// <summary>
        /// Attempts to spend toward the next cap-upgrade tier. Returns
        /// false (no-op, nothing spent) if there is no next tier or the
        /// tier's cost can't be afforded.
        /// </summary>
        public bool TryUpgradeCap()
        {
            if (!HasNextUpgradeTier())
            {
                return false;
            }

            CurrencyUpgradeTier tier = upgradeTiers[currentTierIndex];

            if (!Spend(tier.cost))
            {
                return false;
            }

            maxCap = tier.newMaxCap;
            currentTierIndex++;
            return true;
        }
    }
}

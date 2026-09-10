using UnityEngine;

namespace MergeWars.Heroes
{
    /// <summary>
    /// Per-hero data asset — the single source of truth for one individual
    /// hero's identity and stats. One asset per hero (not per class/role),
    /// so adding a new hero means creating a new HeroDefinition asset. See
    /// /GameDocs/Systems/SYS_HeroDefinition.md for the full layered
    /// architecture (Class/Subclass/Species/Background/Stats/Feats/
    /// Equipment-Spells). Only Class (HeroRole) and Stats are real this
    /// pass; the remaining layers are scaffolded placeholders.
    ///
    /// heroId is the unique identifier the future MergeSystem will use to
    /// decide whether two heroes can merge (same heroId + same star level).
    /// It is NOT the same as prefab/reference identity. Uniqueness is
    /// validated via the editor tool in Assets/Editor/HeroDefinitionValidator.cs
    /// (warns on duplicates, does not block).
    ///
    /// Class/Subclass/Species/Background are fixed for a hero's lifetime —
    /// only star level (and therefore derived Stats) change, via merging.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroDefinition", menuName = "MergeWars/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        [Tooltip("Unique, human-readable identifier for this hero (e.g. \"tank_bruteog_01\"). Used by the future MergeSystem to determine mergeable matches. Must be unique across all HeroDefinition assets — see HeroDefinitionValidator.")]
        public string heroId;

        public string displayName;

        [Tooltip("Core combat role — fixed for this hero's lifetime, does not change on merge/star-up.")]
        public HeroRole role;

        [Tooltip("Base stats at star level 1. Star-scaled stats are computed via GetStatsForStar, never mutated directly.")]
        public HeroStats baseStats;

        // TODO(design): ASSUMED PLACEHOLDER — flat multiplier per star is
        // the confirmed mechanic, but 1.5 is a guessed value pending a
        // real balance pass.
        [Tooltip("Flat multiplier applied per star above 1. ASSUMED PLACEHOLDER value — no balance doc specifies this yet.")]
        public float perStarMultiplier = 1.5f;

        [Tooltip("Pooled/visual prefab for this hero, used by Generator via PoolManager.Get().")]
        public GameObject prefab;

        [Header("Scaffolded layers (TODO(design) — not implemented this pass)")]
        public HeroSubclass subclass;
        public HeroSpecies species;
        public HeroBackground background;
        public HeroFeat[] feats;
        public HeroEquipmentSpell[] equipmentSpells;

        /// <summary>
        /// Computes stats for a given star level by compounding
        /// perStarMultiplier per star above 1 (star 1 = baseStats
        /// unscaled). This is the single source of truth for star-scaled
        /// stats — HeroInstance.CurrentStats calls this rather than
        /// caching/mutating a stored value.
        /// </summary>
        public HeroStats GetStatsForStar(int starLevel)
        {
            int starsAboveBase = Mathf.Max(0, starLevel - 1);
            float totalMultiplier = Mathf.Pow(perStarMultiplier, starsAboveBase);
            return HeroStats.Multiply(baseStats, totalMultiplier);
        }
    }
}

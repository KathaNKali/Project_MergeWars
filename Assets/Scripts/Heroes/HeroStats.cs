namespace MergeWars.Heroes
{
    /// <summary>
    /// Mathematical foundation layer of the character architecture (see
    /// /GameDocs/Systems/SYS_HeroDefinition.md). Pure data, no behavior —
    /// combat/damage resolution does not exist yet and is out of scope
    /// here. All field values assigned on a HeroDefinition are
    /// ASSUMED PLACEHOLDER; no balance doc specifies real numbers yet.
    /// </summary>
    [System.Serializable]
    public struct HeroStats
    {
        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float health;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float attack;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float attackSpeed;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float range;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float armor;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float critChance;

        // TODO(design): ASSUMED PLACEHOLDER — no balance doc exists yet.
        public float critDamage;

        /// <summary>
        /// Scales every field by a flat multiplier. Used by
        /// HeroDefinition.GetStatsForStar to apply the confirmed
        /// flat-per-star-multiplier scaling rule.
        /// </summary>
        public static HeroStats Multiply(HeroStats stats, float multiplier)
        {
            return new HeroStats
            {
                health = stats.health * multiplier,
                attack = stats.attack * multiplier,
                attackSpeed = stats.attackSpeed * multiplier,
                range = stats.range * multiplier,
                armor = stats.armor * multiplier,
                critChance = stats.critChance * multiplier,
                critDamage = stats.critDamage * multiplier
            };
        }
    }
}

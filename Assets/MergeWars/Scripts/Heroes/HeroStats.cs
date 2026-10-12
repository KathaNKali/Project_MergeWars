namespace MergeWars.Heroes
{
    public enum AttackType
    {
        Melee,
        Ranged,
        Area
    }

    public enum TargetTeam
    {
        Enemy,
        Ally,
        Self,
        AllyOrSelf
    }

    public enum TargetType
    {
        Ground,
        Air,
        GroundAndAir,
        Buildings,
        GroundAndBuildings,
        AirAndBuildings,
        All
    }

    public enum TargetRestriction
    {
        None,
        GroundOnly,
        AirOnly,
        BuildingsOnly,
        TroopsOnly,
        HeroesOnly,
        DefensiveBuildingsOnly,
        SupportUnitsOnly,
        BossOnly
    }

    public enum TargetPreference
    {
        None,
        Nearest,
        LowestHealth,
        HighestHealth,
        LowestDamage,
        HighestDamage,
        Buildings,
        DefensiveBuildings,
        SupportUnits,
        Tanks,
        Backline,
        AirUnits,
        GroundUnits,
        Boss
    }

    [System.Serializable]
    public struct HeroStats
    {
        // Core combat stats
        public float hitPoints;
        public float targets;
        public float hitSpeed;
        public float range;
        public float damage;
        public float damagePerSec;
        public float count;
        public float speed;

        // Attack
        public AttackType attackType;
        public float projectileSpeed;
        public float attackArea;

        // Targeting
        public TargetTeam targetTeam;
        public TargetType targetType;
        public TargetRestriction targetRestriction;
        public TargetPreference targetPreference;

        /// <summary>
        /// Scales numeric stats by a flat multiplier.
        /// Targeting and attack-type properties remain unchanged.
        /// </summary>
        public static HeroStats Multiply(HeroStats stats, float multiplier)
        {
            return new HeroStats
            {
                // Core combat
                hitPoints = stats.hitPoints * multiplier,
                targets = stats.targets * multiplier,
                hitSpeed = stats.hitSpeed * multiplier,
                range = stats.range * multiplier,
                damage = stats.damage * multiplier,
                damagePerSec = stats.damagePerSec * multiplier,
                count = stats.count * multiplier,
                speed = stats.speed * multiplier,

                // Attack
                attackType = stats.attackType,
                projectileSpeed = stats.projectileSpeed * multiplier,
                attackArea = stats.attackArea * multiplier,

                // Targeting
                targetTeam = stats.targetTeam,
                targetType = stats.targetType,
                targetRestriction = stats.targetRestriction,
                targetPreference = stats.targetPreference
            };
        }
    }
}
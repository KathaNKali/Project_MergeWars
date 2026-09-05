using UnityEngine;

namespace MergeWars.Generators
{
    public enum HeroClassId
    {
        Ground,
        Air,
        Vehicles
    }

    /// <summary>
    /// Config for a generator's hero class: which class it represents,
    /// its mana cost to spawn, and the prefab to spawn (pooled via
    /// PoolManager, never Instantiate()'d directly).
    /// </summary>
    [CreateAssetMenu(fileName = "HeroClassConfig", menuName = "MergeWars/Hero Class Config")]
    public class HeroClassConfig : ScriptableObject
    {
        public HeroClassId classId;

        // TODO(design): mana costs are ASSUMED PLACEHOLDER values — no
        // SYS_ManaEconomy.md or balance doc specifies real numbers yet.
        public int manaCost = 10;

        public GameObject heroPrefab;
    }
}

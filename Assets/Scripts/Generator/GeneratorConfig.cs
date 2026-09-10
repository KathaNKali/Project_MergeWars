using UnityEngine;
using MergeWars.Heroes;

namespace MergeWars.Generators
{
    /// <summary>
    /// Config for a single Generator: mana cost to spawn (shared across
    /// whichever hero spawns — confirmed, not per-hero) and the pool of
    /// HeroDefinitions this generator can spawn. Replaces the old
    /// HeroClassConfig (per-class prefab array) now that heroes are
    /// authored individually via HeroDefinition assets. A generator can
    /// hold multiple different heroes; one is chosen at random each time
    /// it spawns (same random-selection behavior as before).
    /// </summary>
    [CreateAssetMenu(fileName = "GeneratorConfig", menuName = "MergeWars/Generator Config")]
    public class GeneratorConfig : ScriptableObject
    {
        // TODO(design): mana costs are ASSUMED PLACEHOLDER values — no
        // SYS_ManaEconomy.md or balance doc specifies real numbers yet.
        public int manaCost = 10;

        [Tooltip("Pool of heroes this generator can spawn. One is chosen at random each time the generator spawns.")]
        public HeroDefinition[] heroPool;

        /// <summary>
        /// Picks one HeroDefinition at random from heroPool. Returns null
        /// if the pool is empty/null.
        /// </summary>
        public HeroDefinition GetRandomHeroDefinition()
        {
            if (heroPool == null || heroPool.Length == 0)
            {
                return null;
            }

            int index = Random.Range(0, heroPool.Length);
            return heroPool[index];
        }
    }
}

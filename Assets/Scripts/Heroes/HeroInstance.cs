using UnityEngine;

namespace MergeWars.Heroes
{
    /// <summary>
    /// Runtime occupant identity component attached to a spawned hero
    /// GameObject. Bridges the static per-hero data (HeroDefinition) with
    /// per-instance runtime state (starLevel). This is what the future
    /// MergeSystem will read (definition.heroId + starLevel) and mutate
    /// (starLevel) on a successful merge. See
    /// /GameDocs/Systems/SYS_HeroDefinition.md.
    ///
    /// Does NOT know about combat, merging, or the grid — Generator sets
    /// definition/starLevel at spawn time; MergeSystem (not built yet)
    /// will be the only other system that mutates starLevel.
    /// </summary>
    public class HeroInstance : MonoBehaviour
    {
        [Tooltip("The hero this instance was spawned from. Set by Generator at spawn time.")]
        public HeroDefinition definition;

        [Tooltip("Current star level (1-4). Only mutated by MergeSystem on a successful merge.")]
        public int starLevel = 1;

        /// <summary>
        /// Stats for this instance's current star level, computed on
        /// demand via HeroDefinition.GetStatsForStar — never cached, so
        /// it is always correct immediately after a star-level change.
        /// </summary>
        public HeroStats CurrentStats => definition != null
            ? definition.GetStatsForStar(starLevel)
            : default;
    }
}

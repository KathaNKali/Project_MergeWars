using UnityEngine;
using MergeWars.Grid;
using MergeWars.Pooling;
using MergeWars.Heroes;

namespace MergeWars.Merge
{
    /// <summary>
    /// Central merge validation/execution logic, decoupled from input.
    /// Input triggers (drag, tap-tap-select) call TryMerge with two slot
    /// indices; this system does not know how the merge was triggered.
    ///
    /// Match rule (CONFIRMED): two heroes merge if their
    /// HeroInstance.definition.heroId matches exactly AND their starLevel
    /// matches — NOT prefab/reference identity. No adjacency requirement.
    /// Max star level is 4 — merging two max-star heroes is a no-op.
    ///
    /// Does NOT know about: economy/mana, combat, Generator, or how the
    /// player selected the two heroes. See
    /// /GameDocs/Systems/SYS_MergeSystem.md.
    /// </summary>
    public class MergeSystem : MonoBehaviour
    {
        // TODO(design): max star level is CONFIRMED as 4, but this is
        // hardcoded here rather than sourced from a shared constant/config
        // since no such config exists yet.
        private const int MaxStarLevel = 4;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private PoolManager poolManager;

        /// <summary>
        /// Attempts to merge the hero at slotIndexA into the hero at
        /// slotIndexB (destination). Returns false (no-op) if either slot
        /// is empty, either occupant lacks a HeroInstance/definition,
        /// heroId or starLevel don't match, or the destination is already
        /// at max star level.
        /// </summary>
        public bool TryMerge(int slotIndexA, int slotIndexB)
        {
            if (gridManager == null || poolManager == null || slotIndexA == slotIndexB)
            {
                return false;
            }

            GameObject occupantA = gridManager.GetOccupant(slotIndexA);
            GameObject occupantB = gridManager.GetOccupant(slotIndexB);

            if (occupantA == null || occupantB == null)
            {
                return false;
            }

            HeroInstance instanceA = occupantA.GetComponent<HeroInstance>();
            HeroInstance instanceB = occupantB.GetComponent<HeroInstance>();

            if (!CanMerge(instanceA, instanceB))
            {
                return false;
            }

            // Destination is slotIndexB (the target of the drag/second tap).
            instanceB.starLevel += 1;

            gridManager.RemoveOccupant(slotIndexA);
            poolManager.Release(occupantA);

            return true;
        }

        /// <summary>
        /// Validates whether two hero instances are mergeable: same
        /// heroId, same starLevel, and destination not already at max
        /// star. Exposed separately so input systems (e.g. drag hover
        /// highlighting) can preview validity without executing a merge.
        /// </summary>
        public bool CanMerge(HeroInstance instanceA, HeroInstance instanceB)
        {
            if (instanceA == null || instanceB == null)
            {
                return false;
            }

            if (instanceA.definition == null || instanceB.definition == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(instanceA.definition.heroId) ||
                instanceA.definition.heroId != instanceB.definition.heroId)
            {
                return false;
            }

            if (instanceA.starLevel != instanceB.starLevel)
            {
                return false;
            }

            if (instanceB.starLevel >= MaxStarLevel)
            {
                return false;
            }

            return true;
        }
    }
}

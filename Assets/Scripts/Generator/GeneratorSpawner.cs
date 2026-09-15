using UnityEngine;
using MergeWars.Grid;
using MergeWars.Economy;
using MergeWars.Pooling;
using MergeWars.Merge;

namespace MergeWars.Generators
{
    /// <summary>
    /// One-time, game-start spawner: places every Generator prefab listed
    /// in a GeneratorLoadout into the Generator grid (a dedicated
    /// GridManager instance, separate from the Merge grid). Sizes the
    /// Generator grid to fit the loadout (rows = 1, columns = generator
    /// count for this prototype pass) via GridManager.SetDimensions, then
    /// instantiates each generator directly (no PoolManager — generators
    /// are placed once at game start, not repeatedly created/destroyed),
    /// wires its Merge-side scene dependencies via Generator.Initialize()
    /// (a Generator prefab asset can't itself reference scene objects
    /// like the Merge grid's GridManager/ManaEconomy/PoolManager), and
    /// places it into the next open slot.
    ///
    /// Does not decide which generators the player has chosen — it only
    /// spawns whatever GeneratorLoadout currently lists. Player-selection
    /// logic can later swap the assigned loadout or supply a filtered
    /// list without changing this component. See
    /// /GameDocs/Systems/SYS_Generator.md.
    /// </summary>
    public class GeneratorSpawner : MonoBehaviour
    {
        [SerializeField] private GridManager generatorGridManager;
        [SerializeField] private GeneratorLoadout loadout;

        [Tooltip("Spawned generators are parented here, never to this spawner's own transform.")]
        [SerializeField] private Transform generatorsContainer;

        [Header("Wired into each spawned Generator via Initialize() \u2014 these are scene objects a prefab asset can't reference directly.")]
        [Tooltip("The Merge grid \u2014 where TryTap() places spawned heroes. NOT the Generator grid.")]
        [SerializeField] private GridManager mergeGridManager;
        [SerializeField] private ManaEconomy manaEconomy;
        [SerializeField] private PoolManager poolManager;
        [Tooltip("Spawned heroes (from tapping a generator) are parented here, never to the Generator itself.")]
        [SerializeField] private Transform championsContainer;
        [Tooltip("Optional. If assigned, spawned heroes get merge input wired up, same as Generator's own optional field.")]
        [SerializeField] private MergeSystem mergeSystem;

        private void Start()
        {
            SpawnLoadout();
        }

        /// <summary>
        /// Resizes the Generator grid to fit the loadout and spawns every
        /// generator in it into a slot. No-op (logs a warning) if the
        /// GridManager or loadout isn't assigned, or the loadout is empty.
        /// </summary>
        public void SpawnLoadout()
        {
            if (generatorGridManager == null || loadout == null || loadout.generatorPrefabs == null || loadout.generatorPrefabs.Length == 0)
            {
                Debug.LogWarning("[GeneratorSpawner] Missing GridManager/GeneratorLoadout, or loadout is empty — no generators spawned.", this);
                return;
            }

            int count = loadout.generatorPrefabs.Length;

            // TODO(design): ASSUMED PLACEHOLDER layout — single row, one
            // column per generator. No spec exists yet for wrapping to
            // multiple rows once generator count grows; revisit once the
            // real loadout size/selection system is designed.
            generatorGridManager.SetDimensions(1, count);

            for (int i = 0; i < count; i++)
            {
                Generator prefab = loadout.generatorPrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                if (!generatorGridManager.TryGetOpenSlot(out int slotIndex))
                {
                    Debug.LogWarning($"[GeneratorSpawner] No open slot for generator index {i} ('{prefab.name}') — grid may be undersized.", this);
                    break;
                }

                Generator instance = Instantiate(prefab, generatorsContainer, worldPositionStays: false);
                instance.transform.position = generatorGridManager.GetWorldPosition(slotIndex);
                instance.Initialize(mergeGridManager, manaEconomy, poolManager, championsContainer, mergeSystem);

                generatorGridManager.PlaceOccupant(slotIndex, instance.gameObject);
            }
        }
    }
}

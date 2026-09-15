using UnityEngine;
using MergeWars.Grid;
using MergeWars.Pooling;
using MergeWars.Economy;
using MergeWars.Heroes;
using MergeWars.Merge;

namespace MergeWars.Generators
{
    /// <summary>
    /// 3D world object on the battlefield, tapped via Physics.Raycast
    /// (requires a Collider). Requests a pooled hero from PoolManager,
    /// spends mana via ManaEconomy, and places the hero into
    /// GridManager's next open slot. Generator's involvement ends at
    /// spawn — it does not own or track the hero afterward, and has no
    /// unlock check (every generator is tappable from the start; that is
    /// intentionally deferred, not stubbed).
    /// See /GameDocs/Systems/SYS_Generator.md.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Generator : MonoBehaviour
    {
        [SerializeField] private GeneratorConfig config;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ManaEconomy manaEconomy;
        [SerializeField] private PoolManager poolManager;

        [Tooltip("Spawned heroes are parented here, never to the Generator itself.")]
        [SerializeField] private Transform championsContainer;

        [Tooltip("Optional. If assigned, spawned heroes get a HeroMergeInput component wired to this MergeSystem/GridManager, enabling tap-tap and drag-to-merge input.")]
        [SerializeField] private MergeSystem mergeSystem;

        /// <summary>
        /// Wires this Generator's scene-object dependencies (the Merge
        /// grid's GridManager, ManaEconomy, PoolManager, the champions
        /// container, and optionally MergeSystem) at runtime. Required
        /// when Generator instances are instantiated from a prefab (e.g.
        /// by GeneratorSpawner) rather than hand-placed in the scene,
        /// since a prefab asset cannot itself hold references to scene
        /// objects — those fields must be assigned after instantiation.
        /// Hand-placed/scene-authored Generator instances can instead
        /// assign these directly in the Inspector (as a per-instance
        /// prefab override) and skip calling this.
        /// </summary>
        public void Initialize(GridManager gridManager, ManaEconomy manaEconomy, PoolManager poolManager, Transform championsContainer, MergeSystem mergeSystem = null)
        {
            this.gridManager = gridManager;
            this.manaEconomy = manaEconomy;
            this.poolManager = poolManager;
            this.championsContainer = championsContainer;
            this.mergeSystem = mergeSystem;
        }

        /// <summary>
        /// Attempts to spend mana and spawn this generator's hero into the
        /// next open grid slot. Returns false (no-op, no mana spent) if
        /// mana can't be afforded or the grid has no open slot.
        /// </summary>
        public bool TryTap()
        {
            if (config == null || gridManager == null || manaEconomy == null || poolManager == null)
            {
                return false;
            }

            if (!manaEconomy.CanAfford(config.manaCost))
            {
                return false;
            }

            if (!gridManager.TryGetOpenSlot(out int slotIndex))
            {
                return false;
            }

            HeroDefinition heroDefinition = config.GetRandomHeroDefinition();
            if (heroDefinition == null || heroDefinition.prefab == null)
            {
                return false;
            }

            manaEconomy.Spend(config.manaCost);

            GameObject hero = poolManager.Get(heroDefinition.prefab);
            if (hero == null)
            {
                return false;
            }

            hero.transform.SetParent(championsContainer, worldPositionStays: false);
            Vector3 slotPosition = gridManager.GetWorldPosition(slotIndex);
            hero.transform.position = slotPosition + Vector3.up * heroDefinition.spawnHeightOffset;

            HeroInstance heroInstance = hero.GetComponent<HeroInstance>();
            if (heroInstance == null)
            {
                heroInstance = hero.AddComponent<HeroInstance>();
            }
            heroInstance.definition = heroDefinition;
            heroInstance.starLevel = 1;

            if (mergeSystem != null)
            {
                HeroMergeInput mergeInput = hero.GetComponent<HeroMergeInput>();
                if (mergeInput == null)
                {
                    mergeInput = hero.AddComponent<HeroMergeInput>();
                }
                mergeInput.Initialize(gridManager, mergeSystem);
            }

            gridManager.PlaceOccupant(slotIndex, hero);

            return true;
        }

        // Simple direct entry point for raycast-based tapping (relies on
        // Physics.Raycast against this object's Collider via Unity's
        // built-in mouse-event dispatch). A dedicated input dispatcher
        // system may replace this later; not built here since none was
        // requested.
        private void OnMouseDown()
        {
            TryTap();
        }
    }
}

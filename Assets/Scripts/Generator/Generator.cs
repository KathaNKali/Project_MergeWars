using UnityEngine;
using UnityEngine.UI;
using MergeWars.Grid;
using MergeWars.Pooling;
using MergeWars.Economy;
using MergeWars.Heroes;
using MergeWars.Merge;
using MergeWars.Lanes;

namespace MergeWars.Generators
{
    /// <summary>
    /// UI Button-tapped generator (not a 3D raycast-tapped world object).
    /// Requests a pooled hero from PoolManager, spends mana via
    /// ManaEconomy, and places the hero into GridManager's next open
    /// slot. Generator's involvement ends at spawn — it does not own or
    /// track the hero afterward, and has no unlock check (every generator
    /// is tappable from the start; that is intentionally deferred, not
    /// stubbed).
    /// See /GameDocs/Systems/SYS_Generator.md.
    /// </summary>
    [RequireComponent(typeof(Button))]
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

        [Tooltip("Optional. If assigned, spawned heroes get their HeroInstance.laneIndex set from this hero's spawn position.")]
        [SerializeField] private LaneBounds laneBounds;

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
        public void Initialize(GridManager gridManager, ManaEconomy manaEconomy, PoolManager poolManager, Transform championsContainer, MergeSystem mergeSystem = null, LaneBounds laneBounds = null)
        {
            this.gridManager = gridManager;
            this.manaEconomy = manaEconomy;
            this.poolManager = poolManager;
            this.championsContainer = championsContainer;
            this.mergeSystem = mergeSystem;
            this.laneBounds = laneBounds;
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
                mergeInput.Initialize(gridManager, mergeSystem, laneBounds) ;
            }

            gridManager.PlaceOccupant(slotIndex, hero);

            if (laneBounds != null)
            {
                heroInstance.laneIndex = laneBounds.GetLaneIndex(hero.transform.position);
            }

            return true;
        }

        private Button button;
        private UnityEngine.Events.UnityAction tapListener;

        private void Awake()
        {
            button = GetComponent<Button>();
            tapListener = () => TryTap();
            button.onClick.AddListener(tapListener);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(tapListener);
            }
        }
    }
}

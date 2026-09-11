using UnityEngine;
using MergeWars.Grid;
using MergeWars.Heroes;

namespace MergeWars.Merge
{
    /// <summary>
    /// Single input component handling both confirmed merge triggers:
    /// (1) tap-tap-select — tap a hero to select it, tap a second
    /// different hero to attempt a merge, tap the same hero again to
    /// deselect; (2) drag-and-drop — press and drag a hero, release over
    /// another hero to attempt a merge, release over nothing to snap back.
    /// A single component (not two) is required because Unity's
    /// OnMouseDown/OnMouseDrag/OnMouseUp dispatch is per-GameObject; tap
    /// vs. drag is disambiguated here by total mouse movement distance
    /// during the press.
    ///
    /// Resolves this GameObject's (and the target's) slot index via
    /// GridManager and calls into MergeSystem — this component does not
    /// implement merge validity itself. See
    /// /GameDocs/Systems/SYS_MergeSystem.md.
    ///
    /// Requires a Collider for Physics.Raycast-based mouse dispatch (same
    /// input model as Generator).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(HeroInstance))]
    public class HeroMergeInput : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private MergeSystem mergeSystem;

        /// <summary>
        /// Wires this component's dependencies at runtime — used by
        /// Generator when adding this component to a freshly spawned hero
        /// (placeholder prefabs won't have it pre-authored with Inspector
        /// references). Prefab-authored instances can instead assign
        /// gridManager/mergeSystem directly in the Inspector and skip this.
        /// </summary>
        public void Initialize(GridManager gridManager, MergeSystem mergeSystem)
        {
            this.gridManager = gridManager;
            this.mergeSystem = mergeSystem;
        }

        [Tooltip("Layer mask used when raycasting for a hero drop target at release/tap time. Should include only the layer heroes are on (e.g. 'Heroes'), not the ground.")]
        [SerializeField] private LayerMask dropTargetMask = ~0;

        // TODO(design): ASSUMED PLACEHOLDER — no spec exists for exactly
        // how far above the ground/grid a hero should visually lift while
        // being dragged/selected. Purely cosmetic — GridManager still
        // controls the final settled world position once released. This is
        // now also the fixed height of the virtual drag plane, so the hero
        // no longer bobs up/down with real ground geometry while dragging.
        [Tooltip("Height (relative to the hero's position when picked up) the hero is held at while being dragged. Drag movement happens on a flat virtual plane at this height, so it never bobs with ground geometry.")]
        [SerializeField] private float dragHeightOffset = 0.5f;

        // TODO(design): ASSUMED PLACEHOLDER — no spec exists for how
        // "snappy" vs "floaty" the card-like drag glide should feel.
        [Tooltip("Smoothing time for SmoothDamp while dragging — lower is snappier, higher is floatier/more card-like glide.")]
        [SerializeField] private float dragSmoothTime = 0.06f;

        // TODO(design): ASSUMED PLACEHOLDER — no spec exists for how far
        // the mouse must move (in screen pixels) before a press counts as
        // a drag instead of a tap.
        [SerializeField] private float dragThresholdPixels = 10f;

        private Collider ownCollider;

        // Static so tap-select state is shared across all heroes — only
        // one hero can be "pending" at a time.
        private static HeroMergeInput selectedForTap;

        private Vector3 originalWorldPosition;
        private Vector3 mouseDownScreenPosition;
        private bool isPressed;
        private bool isDragging;

        // Virtual drag plane state — movement while dragging is resolved
        // against a flat plane at a fixed height instead of the real
        // ground collider, so height never varies with ground geometry.
        private Plane dragPlane;
        private Vector3 dragTargetPosition;
        private Vector3 dragVelocity;
        private bool ownColliderWasTrigger;

        private void Awake()
        {
            ownCollider = GetComponent<Collider>();
        }

        private void OnMouseDown()
        {
            originalWorldPosition = transform.position;
            mouseDownScreenPosition = Input.mousePosition;
            isPressed = true;
            isDragging = false;
        }

        private void OnMouseDrag()
        {
            if (!isPressed)
            {
                return;
            }

            if (!isDragging)
            {
                float movedPixels = Vector3.Distance(Input.mousePosition, mouseDownScreenPosition);
                if (movedPixels < dragThresholdPixels)
                {
                    return;
                }
                StartDrag();
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                dragTargetPosition = ray.GetPoint(enter);
            }
        }

        /// <summary>
        /// Begins the drag: sets up a fixed-height virtual plane (so the
        /// hero holds a constant height regardless of ground geometry) and
        /// switches the collider to a trigger so it can pass freely over
        /// other heroes without physical collision/jostling while dragged.
        /// </summary>
        private void StartDrag()
        {
            isDragging = true;
            dragVelocity = Vector3.zero;
            dragTargetPosition = transform.position;

            float planeHeight = originalWorldPosition.y + dragHeightOffset;
            dragPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));

            if (ownCollider != null)
            {
                ownColliderWasTrigger = ownCollider.isTrigger;
                ownCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!isDragging)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, dragTargetPosition, ref dragVelocity, dragSmoothTime);
        }

        /// <summary>
        /// Raycasts against the given layer mask while excluding this
        /// hero's own collider — without this, dragging a hero can hit
        /// its own collider first and snap the hero toward the camera
        /// instead of onto the intended ground/slot position.
        /// </summary>
        private bool TryRaycastIgnoringSelf(Ray ray, LayerMask mask, out RaycastHit hit)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, mask);
            RaycastHit closest = default;
            float closestDistance = Mathf.Infinity;
            bool found = false;

            foreach (RaycastHit candidate in hits)
            {
                if (ownCollider != null && candidate.collider == ownCollider)
                {
                    continue;
                }

                if (candidate.distance < closestDistance)
                {
                    closestDistance = candidate.distance;
                    closest = candidate;
                    found = true;
                }
            }

            hit = closest;
            return found;
        }

        private void OnMouseUp()
        {
            if (!isPressed)
            {
                return;
            }
            isPressed = false;

            if (isDragging)
            {
                isDragging = false;
                if (ownCollider != null)
                {
                    ownCollider.isTrigger = ownColliderWasTrigger;
                }
                HandleDragRelease();
            }
            else
            {
                HandleTap();
            }
        }

        private void HandleDragRelease()
        {
            GameObject dropTarget = ResolveHeroUnderCursor();

            if (TryMergeWith(dropTarget))
            {
                return;
            }

            // No valid merge — snap back to original position (no tween;
            // DoTween is not yet integrated per PROJECT_INDEX.md).
            transform.position = originalWorldPosition;
        }

        private void HandleTap()
        {
            if (selectedForTap == this)
            {
                selectedForTap = null;
                return;
            }

            if (selectedForTap == null)
            {
                selectedForTap = this;
                return;
            }

            HeroMergeInput previouslySelected = selectedForTap;
            selectedForTap = null;

            if (gridManager == null || mergeSystem == null)
            {
                return;
            }

            if (gridManager.TryGetSlotIndexForOccupant(previouslySelected.gameObject, out int slotIndexA) &&
                gridManager.TryGetSlotIndexForOccupant(gameObject, out int slotIndexB))
            {
                mergeSystem.TryMerge(slotIndexA, slotIndexB);
            }
        }

        private bool TryMergeWith(GameObject dropTarget)
        {
            if (dropTarget == null || dropTarget == gameObject || gridManager == null || mergeSystem == null)
            {
                return false;
            }

            if (!gridManager.TryGetSlotIndexForOccupant(gameObject, out int slotIndexA) ||
                !gridManager.TryGetSlotIndexForOccupant(dropTarget, out int slotIndexB))
            {
                return false;
            }

            return mergeSystem.TryMerge(slotIndexA, slotIndexB);
        }

        private GameObject ResolveHeroUnderCursor()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (TryRaycastIgnoringSelf(ray, dropTargetMask, out RaycastHit hit))
            {
                HeroInstance heroInstance = hit.collider.GetComponent<HeroInstance>();
                if (heroInstance != null)
                {
                    return heroInstance.gameObject;
                }
            }

            return null;
        }

        private void OnDisable()
        {
            // Guard against a pooled/deactivated hero leaving stale
            // selection or press state behind.
            if (selectedForTap == this)
            {
                selectedForTap = null;
            }
            if (isDragging && ownCollider != null)
            {
                ownCollider.isTrigger = ownColliderWasTrigger;
            }
            isPressed = false;
            isDragging = false;
        }
    }
}
 
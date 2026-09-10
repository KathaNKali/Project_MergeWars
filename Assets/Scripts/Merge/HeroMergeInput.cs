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

        [Tooltip("Layer mask used when raycasting for a drop target while dragging. Defaults to Everything.")]
        [SerializeField] private LayerMask dropTargetMask = ~0;

        // TODO(design): ASSUMED PLACEHOLDER — no spec exists for how far
        // the mouse must move (in screen pixels) before a press counts as
        // a drag instead of a tap.
        [SerializeField] private float dragThresholdPixels = 10f;

        // Static so tap-select state is shared across all heroes — only
        // one hero can be "pending" at a time.
        private static HeroMergeInput selectedForTap;

        private Vector3 originalWorldPosition;
        private Vector3 mouseDownScreenPosition;
        private bool isPressed;
        private bool isDragging;

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
                isDragging = true;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropTargetMask))
            {
                transform.position = hit.point;
            }
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
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropTargetMask))
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
            isPressed = false;
            isDragging = false;
        }
    }
}

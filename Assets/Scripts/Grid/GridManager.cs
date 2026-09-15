using UnityEngine;

namespace MergeWars.Grid
{
    /// <summary>
    /// Per-instance owner of one grid: a MonoBehaviour wrapper around a
    /// GridData instance, holding Inspector-configured dimensions,
    /// placement, and cell size, and exposing the same slot API by
    /// delegating to GridData. Each GridManager component in the scene is
    /// an independent grid — there is no singleton/shared-instance
    /// assumption, so multiple grids (e.g. the Merge grid and the
    /// Generator grid) can coexist, each with its own size/anchor,
    /// wired to whichever consumer(s) need that specific grid.
    ///
    /// Responsibility is limited to storing slot data and mapping slot
    /// index ? world position ? occupant reference. It has no knowledge of
    /// merge/swap validity, mana cost, combat, base layout, or "unlock"
    /// state — see /GameDocs/Systems/SYS_GridManager.md.
    ///
    /// Cell generation order: slot index 0 is the top-right cell. Indexing
    /// fills right-to-left across the top row first, then proceeds
    /// downward row by row. This is purely an indexing convention for how
    /// the data array is populated — it must not be treated as a
    /// slot-selection priority rule by callers.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [Tooltip("Number of rows in the grid. Default usage in this project is 4, but this class must not assume a fixed size.")]
        [SerializeField] private int rows = 4;

        [Tooltip("Number of columns in the grid. Default usage in this project is 4, but this class must not assume a fixed size.")]
        [SerializeField] private int columns = 4;

        [Header("Placement")]
        [Tooltip("Author-assigned world anchor — represents the CENTER of the merge area (not a corner). The grid is always built centered on this transform, regardless of row/column count.")]
        [SerializeField] private Transform anchor;

        [Header("Cell Size (independent of merge area)")]
        [Tooltip("World-space size of a single cell along the column axis. Independent of merge area size — does not auto-shrink/grow to fit.")]
        [SerializeField] private float cellSizeX = 1f;

        [Tooltip("World-space size of a single cell along the row axis. Independent of merge area size — does not auto-shrink/grow to fit.")]
        [SerializeField] private float cellSizeZ = 1f;

        [Header("Merge Area (reference bounds — centering guide, not an auto-fit target)")]
        [Tooltip("Reference world-space width (along the column axis) of the merge area you define. The grid is always centered on the anchor within this reference, but cell size is independent — the grid's actual footprint (columns * cellSizeX) can be smaller OR larger than this. If it exceeds this width, a warning is logged (see CheckMergeAreaOverflow).")]
        [SerializeField] private float mergeAreaWidth = 4f;

        [Tooltip("Reference world-space depth (along the row axis) of the merge area you define. The grid is always centered on the anchor within this reference, but cell size is independent — the grid's actual footprint (rows * cellSizeZ) can be smaller OR larger than this. If it exceeds this depth, a warning is logged (see CheckMergeAreaOverflow).")]
        [SerializeField] private float mergeAreaDepth = 4f;

        // TODO(design): Which local direction counts as "column increases
        // to the right" and "row increases downward" relative to the
        // anchor transform is not specified in SYS_GridManager.md or
        // CURRENT_TASK.md. Defaulting to anchor.right for columns and
        // anchor -forward ("back") for rows as an ASSUMED PLACEHOLDER.
        [Header("Axis Convention (ASSUMED PLACEHOLDER — see TODO(design) above)")]
        [SerializeField] private Vector3 columnAxis = Vector3.right;
        [SerializeField] private Vector3 rowAxis = Vector3.back;

        private GridData grid = new GridData();

        public int Rows => rows;
        public int Columns => columns;
        public int TotalSlots => rows * columns;

        /// <summary>Author-set cell width (along column axis), independent of merge area size.</summary>
        public float CellSizeX => cellSizeX;

        /// <summary>Author-set cell depth (along row axis), independent of merge area size.</summary>
        public float CellSizeZ => cellSizeZ;

        private void Awake()
        {
            BuildGrid();
        }

        /// <summary>
        /// Resizes this grid to the given rows/columns and rebuilds it
        /// (still centered on the anchor, same as any Inspector-driven
        /// dimension change). Lets an external owner that decides grid
        /// size from its own data (e.g. a GeneratorSpawner sizing the
        /// Generator grid from how many generators it's placing) resize
        /// this grid without GridManager needing to know why — sizing
        /// policy stays outside GridManager, per SYS_GridManager.md.
        /// </summary>
        public void SetDimensions(int newRows, int newColumns)
        {
            rows = newRows;
            columns = newColumns;
            BuildGrid();
        }

        private void OnValidate()
        {
            // Rebuild in editor so moving the anchor / changing dimensions
            // is reflected immediately (e.g. for gizmo/editor visualization).
            if (rows > 0 && columns > 0)
            {
                BuildGrid();
            }
        }

        /// <summary>
        /// (Re)builds the slot data array in generation order: top-right
        /// first, right-to-left across each row, then downward row by row.
        /// Delegates the actual slot-array construction to GridData.
        /// </summary>
        public void BuildGrid()
        {            if (anchor == null)
            {
                anchor = transform;
            }

            CheckMergeAreaOverflow();

            grid.Build(rows, columns, anchor, columnAxis, rowAxis, cellSizeX, cellSizeZ);
        }

        /// <summary>
        /// Cell size is independent of the merge area (option 3b, confirmed
        /// design) — the grid is not auto-fitted/clamped to stay inside it.
        /// This only logs a warning if the grid's actual footprint
        /// (columns * cellSizeX, rows * cellSizeZ) exceeds the reference
        /// merge area bounds, so the overflow is visible without silently
        /// resizing anything.
        /// </summary>
        private void CheckMergeAreaOverflow()
        {
            float gridWidth = columns * cellSizeX;
            float gridDepth = rows * cellSizeZ;

            if (gridWidth > mergeAreaWidth || gridDepth > mergeAreaDepth)
            {
                Debug.LogWarning($"[GridManager] Grid footprint ({gridWidth:F2} x {gridDepth:F2}) exceeds the defined merge area bounds ({mergeAreaWidth:F2} x {mergeAreaDepth:F2}) on '{name}'. Cell size is independent of the merge area by design — reduce rows/columns/cellSize or increase the merge area if this is unintended.", this);
            }
        }

        /// <summary>
        /// Finds the first open slot in generation order. Returns false if
        /// the grid is full or not yet built.
        /// </summary>
        public bool TryGetOpenSlot(out int slotIndex)
        {
            return grid.TryGetOpenSlot(out slotIndex);
        }

        public bool PlaceOccupant(int slotIndex, GameObject occupant)
        {
            return grid.PlaceOccupant(slotIndex, occupant);
        }

        public bool RemoveOccupant(int slotIndex)
        {
            return grid.RemoveOccupant(slotIndex);
        }

        public GameObject GetOccupant(int slotIndex)
        {
            return grid.GetOccupant(slotIndex);
        }

        /// <summary>
        /// Reverse lookup: finds the slot index currently holding the given
        /// occupant. Returns false if the occupant isn't placed in any
        /// slot. Used by merge-input systems that resolve a tapped/dragged
        /// hero GameObject back to its slot.
        /// </summary>
        public bool TryGetSlotIndexForOccupant(GameObject occupant, out int slotIndex)
        {
            return grid.TryGetSlotIndexForOccupant(occupant, out slotIndex);
        }

        public Vector3 GetWorldPosition(int slotIndex)
        {
            return grid.GetWorldPosition(slotIndex, anchor != null ? anchor.position : transform.position);
        }

        /// <summary>
        /// Finds the nearest unoccupied slot to a world position, within
        /// maxDistance (see GridData.TryGetNearestOpenSlotIndex). Used by
        /// drag-and-drop input to reposition an occupant into an open
        /// slot instead of only merging or snapping back.
        /// </summary>
        public bool TryGetNearestOpenSlotIndex(Vector3 worldPosition, float maxDistance, out int slotIndex)
        {
            return grid.TryGetNearestOpenSlotIndex(worldPosition, maxDistance, out slotIndex);
        }

        [Header("Debug Gizmo")]
        [Tooltip("Draw a wire cube per slot in the Scene view (edit mode and play mode).")]
        [SerializeField] private bool drawDebugGizmo = true;

        [SerializeField] private Color emptySlotColor = new Color(0f, 1f, 1f, 0.5f);
        [SerializeField] private Color occupiedSlotColor = new Color(1f, 0.5f, 0f, 0.75f);

        [Tooltip("Also draw a wireframe of the reference merge area bounds (mergeAreaWidth × mergeAreaDepth), centered on the anchor. This is a reference/warning guide only — the grid does not auto-fit to it (cell size is independent).")]
        [SerializeField] private bool drawMergeAreaBounds = true;
        [SerializeField] private Color mergeAreaBoundsColor = Color.yellow;
        [Tooltip("Color used for the merge area wireframe when the grid's actual footprint exceeds the reference bounds.")]
        [SerializeField] private Color mergeAreaOverflowColor = Color.red;

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmo || rows <= 0 || columns <= 0)
            {
                return;
            }

            // In edit mode, always rebuild so the gizmo stays in sync with
            // any change to this GameObject or its anchor (moving/rotating
            // the transform in the Scene view doesn't raise OnValidate,
            // only Inspector field edits do). In play mode, only rebuild
            // when the slot count is out of date, so we don't clobber live
            // occupant state on every gizmo redraw.
            if (!Application.isPlaying || grid.Slots == null || grid.Slots.Length != rows * columns)
            {
                BuildGrid();
            }

            if (drawMergeAreaBounds)
            {
                Transform origin = anchor != null ? anchor : transform;
                Vector3 boundsSize = new Vector3(mergeAreaWidth, Mathf.Min(mergeAreaWidth, mergeAreaDepth) * 0.05f, mergeAreaDepth);

                bool overflowing = (columns * cellSizeX) > mergeAreaWidth || (rows * cellSizeZ) > mergeAreaDepth;
                Gizmos.color = overflowing ? mergeAreaOverflowColor : mergeAreaBoundsColor;
                Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, boundsSize);
                Gizmos.matrix = Matrix4x4.identity;
            }

            Vector3 cellSize = new Vector3(cellSizeX, Mathf.Min(cellSizeX, cellSizeZ) * 0.1f, cellSizeZ);

            for (int i = 0; i < grid.Slots.Length; i++)
            {
                GridSlotData slot = grid.Slots[i];
                Gizmos.color = slot.isOccupied ? occupiedSlotColor : emptySlotColor;
                Gizmos.DrawWireCube(slot.worldPosition, cellSize);
            }
        }
    }
}

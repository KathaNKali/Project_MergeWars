using UnityEngine;

namespace MergeWars.Grid
{
    /// <summary>
    /// Owns the M×N grid data structure and slot?world-position mapping
    /// for the merge/deployment zone (generator variant).
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
        [Tooltip("Author-assigned world anchor/origin. The grid is built relative to this transform; moving it in the editor relocates the whole grid.")]
        [SerializeField] private Transform anchor;

        [Tooltip("World-space size of a single cell along the column axis.")]
        [SerializeField] private float cellSizeX = 1f;

        [Tooltip("World-space size of a single cell along the row axis.")]
        [SerializeField] private float cellSizeZ = 1f;

        // TODO(design): Which local direction counts as "column increases
        // to the right" and "row increases downward" relative to the
        // anchor transform is not specified in SYS_GridManager.md or
        // CURRENT_TASK.md. Defaulting to anchor.right for columns and
        // anchor -forward ("back") for rows as an ASSUMED PLACEHOLDER.
        [Header("Axis Convention (ASSUMED PLACEHOLDER — see TODO(design) above)")]
        [SerializeField] private Vector3 columnAxis = Vector3.right;
        [SerializeField] private Vector3 rowAxis = Vector3.back;

        private GridSlotData[] slots;

        public int Rows => rows;
        public int Columns => columns;
        public int TotalSlots => rows * columns;

        private void Awake()
        {
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
        /// </summary>
        public void BuildGrid()
        {
            if (anchor == null)
            {
                anchor = transform;
            }

            slots = new GridSlotData[rows * columns];

            for (int row = 0; row < rows; row++)
            {
                for (int colFromRight = 0; colFromRight < columns; colFromRight++)
                {
                    int col = columns - 1 - colFromRight;
                    int slotIndex = row * columns + colFromRight;

                    Vector3 worldPosition = ComputeWorldPosition(row, col);
                    slots[slotIndex] = new GridSlotData(slotIndex, row, col, worldPosition);
                }
            }
        }

        private Vector3 ComputeWorldPosition(int row, int col)
        {
            Transform origin = anchor != null ? anchor : transform;
            Vector3 columnOffset = origin.TransformDirection(columnAxis.normalized) * (col * cellSizeX);
            Vector3 rowOffset = origin.TransformDirection(rowAxis.normalized) * (row * cellSizeZ);
            return origin.position + columnOffset + rowOffset;
        }

        private bool IsValidIndex(int slotIndex)
        {
            return slots != null && slotIndex >= 0 && slotIndex < slots.Length;
        }

        /// <summary>
        /// Finds the first open slot in generation order. Returns false if
        /// the grid is full or not yet built.
        /// </summary>
        public bool TryGetOpenSlot(out int slotIndex)
        {
            if (slots != null)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (!slots[i].isOccupied)
                    {
                        slotIndex = i;
                        return true;
                    }
                }
            }

            slotIndex = -1;
            return false;
        }

        public bool PlaceOccupant(int slotIndex, GameObject occupant)
        {
            if (!IsValidIndex(slotIndex) || slots[slotIndex].isOccupied)
            {
                return false;
            }

            slots[slotIndex].occupant = occupant;
            slots[slotIndex].isOccupied = true;
            return true;
        }

        public bool RemoveOccupant(int slotIndex)
        {
            if (!IsValidIndex(slotIndex) || !slots[slotIndex].isOccupied)
            {
                return false;
            }

            slots[slotIndex].occupant = null;
            slots[slotIndex].isOccupied = false;
            return true;
        }

        public GameObject GetOccupant(int slotIndex)
        {
            return IsValidIndex(slotIndex) ? slots[slotIndex].occupant : null;
        }

        public Vector3 GetWorldPosition(int slotIndex)
        {
            return IsValidIndex(slotIndex) ? slots[slotIndex].worldPosition : (anchor != null ? anchor.position : transform.position);
        }

        [Header("Debug Gizmo")]
        [Tooltip("Draw a wire cube per slot in the Scene view (edit mode and play mode).")]
        [SerializeField] private bool drawDebugGizmo = true;

        [SerializeField] private Color emptySlotColor = new Color(0f, 1f, 1f, 0.5f);
        [SerializeField] private Color occupiedSlotColor = new Color(1f, 0.5f, 0f, 0.75f);

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmo || rows <= 0 || columns <= 0)
            {
                return;
            }

            // Slots may not be built yet (e.g. component just added, not
            // touched via Inspector). Rebuild so the gizmo reflects current
            // settings without mutating play-mode occupant state.
            if (slots == null || slots.Length != rows * columns)
            {
                BuildGrid();
            }

            Vector3 cellSize = new Vector3(cellSizeX, Mathf.Min(cellSizeX, cellSizeZ) * 0.1f, cellSizeZ);

            for (int i = 0; i < slots.Length; i++)
            {
                GridSlotData slot = slots[i];
                Gizmos.color = slot.isOccupied ? occupiedSlotColor : emptySlotColor;
                Gizmos.DrawWireCube(slot.worldPosition, cellSize);
            }
        }
    }
}

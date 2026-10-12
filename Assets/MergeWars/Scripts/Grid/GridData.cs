using UnityEngine;

namespace MergeWars.Grid
{
    /// <summary>
    /// Plain C# grid data structure: owns the slot array and slot?world
    /// position mapping/logic, decoupled from GridManager's MonoBehaviour
    /// and Inspector concerns. Extracted so multiple independent grid
    /// instances (e.g. the Merge grid and the Generator grid) can each be
    /// owned by their own GridManager component \u2014 with different
    /// dimensions, anchors, and cell sizes \u2014 without duplicating this
    /// logic per grid type.
    ///
    /// Not a MonoBehaviour, not a ScriptableObject \u2014 purely runtime data
    /// + slot lookup/mutation, same as GridSlotData. See
    /// /GameDocs/Systems/SYS_GridManager.md.
    /// </summary>
    public class GridData
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int TotalSlots => Rows * Columns;

        /// <summary>Raw slot array, exposed read-only (e.g. for gizmo drawing by an owner).</summary>
        public GridSlotData[] Slots { get; private set; }

        /// <summary>
        /// (Re)builds the slot array in generation order: top-right first,
        /// right-to-left across each row, then downward row by row. Cell
        /// world positions are centered on the anchor.
        /// </summary>
        public void Build(int rows, int columns, Transform anchor, Vector3 columnAxis, Vector3 rowAxis, float cellSizeX, float cellSizeZ)
        {
            Rows = rows;
            Columns = columns;
            Slots = new GridSlotData[rows * columns];

            for (int row = 0; row < rows; row++)
            {
                for (int colFromRight = 0; colFromRight < columns; colFromRight++)
                {
                    int col = columns - 1 - colFromRight;
                    int slotIndex = row * columns + colFromRight;

                    Vector3 worldPosition = ComputeWorldPosition(row, col, rows, columns, anchor, columnAxis, rowAxis, cellSizeX, cellSizeZ);
                    Slots[slotIndex] = new GridSlotData(slotIndex, row, col, worldPosition);
                }
            }
        }

        /// <summary>
        /// Computes a cell's world position centered within the merge area
        /// reference: row/col are offset from the center index
        /// ((count-1)/2f) rather than from a corner, so the whole grid is
        /// centered on the anchor regardless of row/column count.
        /// </summary>
        private static Vector3 ComputeWorldPosition(int row, int col, int rows, int columns, Transform anchor, Vector3 columnAxis, Vector3 rowAxis, float cellSizeX, float cellSizeZ)
        {
            float centeredCol = col - (columns - 1) / 2f;
            float centeredRow = row - (rows - 1) / 2f;

            Vector3 columnOffset = anchor.TransformDirection(columnAxis.normalized) * (centeredCol * cellSizeX);
            Vector3 rowOffset = anchor.TransformDirection(rowAxis.normalized) * (centeredRow * cellSizeZ);
            return anchor.position + columnOffset + rowOffset;
        }

        public bool IsValidIndex(int slotIndex)
        {
            return Slots != null && slotIndex >= 0 && slotIndex < Slots.Length;
        }

        /// <summary>
        /// Finds the first open slot in generation order. Returns false if
        /// the grid is full or not yet built.
        /// </summary>
        public bool TryGetOpenSlot(out int slotIndex)
        {
            if (Slots != null)
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    if (!Slots[i].isOccupied)
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
            if (!IsValidIndex(slotIndex) || Slots[slotIndex].isOccupied)
            {
                return false;
            }

            Slots[slotIndex].occupant = occupant;
            Slots[slotIndex].isOccupied = true;
            return true;
        }

        public bool RemoveOccupant(int slotIndex)
        {
            if (!IsValidIndex(slotIndex) || !Slots[slotIndex].isOccupied)
            {
                return false;
            }

            Slots[slotIndex].occupant = null;
            Slots[slotIndex].isOccupied = false;
            return true;
        }

        public GameObject GetOccupant(int slotIndex)
        {
            return IsValidIndex(slotIndex) ? Slots[slotIndex].occupant : null;
        }

        /// <summary>
        /// Reverse lookup: finds the slot index currently holding the given
        /// occupant. Returns false if the occupant isn't placed in any
        /// slot. Used by merge-input systems that resolve a tapped/dragged
        /// hero GameObject back to its slot.
        /// </summary>
        public bool TryGetSlotIndexForOccupant(GameObject occupant, out int slotIndex)
        {
            if (occupant != null && Slots != null)
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    if (Slots[i].isOccupied && Slots[i].occupant == occupant)
                    {
                        slotIndex = i;
                        return true;
                    }
                }
            }

            slotIndex = -1;
            return false;
        }

        public Vector3 GetWorldPosition(int slotIndex, Vector3 fallback)
        {
            return IsValidIndex(slotIndex) ? Slots[slotIndex].worldPosition : fallback;
        }

        /// <summary>
        /// Finds the nearest unoccupied slot to a world position, within
        /// maxDistance, comparing on the grid's horizontal (X/Z) plane —
        /// consistent with the flat drag-plane assumption already used by
        /// HeroMergeInput. Used to let a dragged occupant be dropped into
        /// an open slot (repositioning) rather than only merging or
        /// snapping back. Returns false if no open slot is within range.
        /// </summary>
        public bool TryGetNearestOpenSlotIndex(Vector3 worldPosition, float maxDistance, out int slotIndex)
        {
            slotIndex = -1;

            if (Slots == null)
            {
                return false;
            }

            float bestDistanceSquared = maxDistance * maxDistance;
            bool found = false;

            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].isOccupied)
                {
                    continue;
                }

                float dx = Slots[i].worldPosition.x - worldPosition.x;
                float dz = Slots[i].worldPosition.z - worldPosition.z;
                float distanceSquared = dx * dx + dz * dz;

                if (distanceSquared <= bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    slotIndex = i;
                    found = true;
                }
            }

            return found;
        }
    }
}

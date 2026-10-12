using UnityEngine;

namespace MergeWars.Grid
{
    /// <summary>
    /// Plain serializable data for a single grid slot. No MonoBehaviour,
    /// no per-cell GameObject — kept separate from any visual layer so it
    /// can be persisted (save/resume) without refactoring.
    /// </summary>
    [System.Serializable]
    public class GridSlotData
    {
        public int slotIndex;
        public int row;
        public int col;
        public Vector3 worldPosition;
        public bool isOccupied;

        // TODO(design): Occupant is represented as a bare GameObject
        // reference because no MergeSystem/Generator occupant data
        // contract has been defined yet (SYS_GridManager.md marks this as
        // outside GridManager's responsibility). Revisit this type once
        // that system exists.
        public GameObject occupant;

        public GridSlotData(int slotIndex, int row, int col, Vector3 worldPosition)
        {
            this.slotIndex = slotIndex;
            this.row = row;
            this.col = col;
            this.worldPosition = worldPosition;
            this.isOccupied = false;
            this.occupant = null;
        }
    }
}

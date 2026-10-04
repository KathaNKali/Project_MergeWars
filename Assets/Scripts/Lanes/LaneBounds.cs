using UnityEngine;

namespace MergeWars.Lanes
{
    /// <summary>
    /// Standalone world-position-based lane query. Answers "which lane is
    /// this position in?" on demand — it does not cache or store any
    /// per-hero state itself; callers read the result and store it on
    /// their own occupant (e.g. HeroInstance.laneIndex).
    ///
    /// Deliberately decoupled from GridManager/GridData: a hero's lane is
    /// derived from its world position, not from which grid slot/column it
    /// occupies, so lane assignment survives grid resizes (GridManager's
    /// BuildGrid/SetDimensions discard and rebuild slot data, which would
    /// make a column-derived lane fragile). Lane boundaries are modeled as
    /// plain world-space corridors (no colliders/triggers) spanning the
    /// full depth from the hero side to the enemy side, so the same
    /// boundary data can later double as a march corridor for a future
    /// attack/movement system.
    ///
    /// Does NOT know about: GridManager/GridData, grid occupancy, merge
    /// rules, combat, or movement — it only answers lane-membership
    /// queries for a given world position. See
    /// /GameDocs/Systems/SYS_LaneSystem.md.
    /// </summary>
    public class LaneBounds : MonoBehaviour
    {
        [Header("Lane Configuration")]
        [Tooltip("Number of lanes. Current design targets 1-3, but nothing here hardcodes that range.")]
        [SerializeField] private int laneCount = 2;

        // TODO(design): ASSUMED PLACEHOLDER — no balance/layout doc
        // specifies a real per-lane width yet.
        [Tooltip("World-space width of a single lane along laneAxis. Lanes are evenly spaced and centered on the anchor — no per-lane uneven-width authoring in this pass.")]
        [SerializeField] private float laneWidth = 2f;

        [Header("Placement")]
        [Tooltip("Author-assigned world anchor — represents the CENTER of the whole lane span. Falls back to this GameObject's own transform if unassigned.")]
        [SerializeField] private Transform anchor;

        // TODO(design): ASSUMED PLACEHOLDER — mirrors GridManager's
        // columnAxis convention/caveat; which local direction counts as
        // "lane index increases to the right" is not specified elsewhere.
        [Header("Axis Convention (ASSUMED PLACEHOLDER — see TODO(design) above)")]
        [SerializeField] private Vector3 laneAxis = Vector3.right;

        public int LaneCount => laneCount;

        private void Awake()
        {
            if (anchor == null)
            {
                anchor = transform;
            }
        }

        /// <summary>
        /// Returns which lane the given world position falls in, as a live
        /// query (never cached) — callers should re-call this whenever a
        /// position changes (placement, drag, mid-battle movement) rather
        /// than trusting a previously stored value to stay current.
        /// Positions outside every lane's range are clamped to the nearest
        /// lane; this always returns a valid index in [0, laneCount - 1]
        /// and never "no lane".
        /// </summary>
        public int GetLaneIndex(Vector3 worldPosition)
        {
            if (laneCount <= 0)
            {
                return 0;
            }

            Transform origin = anchor != null ? anchor : transform;
            Vector3 worldAxis = origin.TransformDirection(laneAxis).normalized;

            float signedDistance = Vector3.Dot(worldPosition - origin.position, worldAxis);
            float totalWidth = laneWidth * laneCount;
            float distanceFromLeftEdge = signedDistance + totalWidth * 0.5f;

            int laneIndex = Mathf.FloorToInt(distanceFromLeftEdge / laneWidth);
            return Mathf.Clamp(laneIndex, 0, laneCount - 1);
        }

        [Header("Debug Gizmo")]
        [Tooltip("Draw a wire corridor per lane in the Scene view (edit mode and play mode).")]
        [SerializeField] private bool drawDebugGizmo = true;

        [Tooltip("Cosmetic-only depth of the visualized corridor along the anchor's forward axis — does not affect GetLaneIndex, which has no depth/Z bound.")]
        [SerializeField] private float gizmoCorridorDepth = 20f;

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmo || laneCount <= 0 || laneWidth <= 0f)
            {
                return;
            }

            Transform origin = anchor != null ? anchor : transform;
            Vector3 worldAxis = origin.TransformDirection(laneAxis).normalized;
            float totalWidth = laneWidth * laneCount;

            for (int i = 0; i < laneCount; i++)
            {
                float laneCenterOffset = -totalWidth * 0.5f + laneWidth * (i + 0.5f);
                Vector3 laneCenter = origin.position + worldAxis * laneCenterOffset;

                Gizmos.color = Color.HSVToRGB((float)i / laneCount, 0.7f, 1f);
                Gizmos.matrix = Matrix4x4.TRS(laneCenter, origin.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(laneWidth, laneWidth * 0.1f, gizmoCorridorDepth));
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}

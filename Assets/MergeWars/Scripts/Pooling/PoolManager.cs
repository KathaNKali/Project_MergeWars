using System.Collections.Generic;
using UnityEngine;

namespace MergeWars.Pooling
{
    [System.Serializable]
    public class PoolPrewarmEntry
    {
        public GameObject prefab;
        public int initialPoolSize = 0;
    }

    /// <summary>
    /// Owns pre-warmed pools of inactive GameObjects, keyed by prefab.
    /// Foundational utility — has no knowledge of what a champion or
    /// building *is* gameplay-wise, and depends on nothing else.
    /// See /GameDocs/Systems/SYS_PoolManager.md.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        [Tooltip("Optional pre-warm list: prefab + how many inactive instances to create up front.")]
        [SerializeField] private List<PoolPrewarmEntry> prewarmEntries = new List<PoolPrewarmEntry>();

        [SerializeField] private Transform poolContainer;

        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

        // Maps a spawned instance back to the prefab key it came from, so
        // Release() knows which pool to return it to.
        private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

        private void Awake()
        {
            if (poolContainer == null)
            {
                poolContainer = transform;
            }

            foreach (PoolPrewarmEntry entry in prewarmEntries)
            {
                if (entry.prefab == null)
                {
                    continue;
                }

                Queue<GameObject> queue = GetOrCreateQueue(entry.prefab);
                for (int i = 0; i < entry.initialPoolSize; i++)
                {
                    GameObject instance = CreateInstance(entry.prefab);
                    instance.SetActive(false);
                    queue.Enqueue(instance);
                }
            }
        }

        private Queue<GameObject> GetOrCreateQueue(GameObject prefab)
        {
            if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools[prefab] = queue;
            }

            return queue;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, poolContainer);
            instanceToPrefab[instance] = prefab;
            return instance;
        }

        /// <summary>
        /// Reactivates a pooled instance for the given prefab key, or
        /// instantiates a new one if the pool is empty.
        /// </summary>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            Queue<GameObject> queue = GetOrCreateQueue(prefab);

            GameObject instance = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Deactivates the instance and returns it to the pool matching
        /// the prefab it was created from.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(poolContainer);

            if (instanceToPrefab.TryGetValue(instance, out GameObject prefab))
            {
                GetOrCreateQueue(prefab).Enqueue(instance);
            }
            else
            {
                // Instance wasn't created by this PoolManager (e.g. raw
                // Instantiate elsewhere) — nothing to return it to.
                Destroy(instance);
            }
        }
    }
}

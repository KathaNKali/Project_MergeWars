# SYS_PoolManager.md

## Responsibility
Owns pre-warmed pools of inactive GameObjects, keyed by prefab type
(champion tiers/classes now; building types later once EnemyBaseManager
needs it). Provides Get(prefabKey) → reactivates or instantiates, and
Release(instance) → deactivates and returns to pool, instead of
Instantiate()/Destroy().
Source: Research — Cross-Cutting: Object Pooling (§6). Recommended shape,
now promoted to CONFIRMED since we're building it this pass.

## Depends on
- (none — foundational utility, other systems depend on it, not vice versa)

## Does NOT know about
- What a champion or building *is* gameplay-wise — just manages
  instantiate/reactivate/deactivate lifecycle for whatever prefab key it's
  given
- Grid, mana, combat — purely a lifecycle utility

## Confirmed this pass
- CONFIRMED: separate pool per prefab type (not one generic pool) — Research
  file's explicit recommendation given differing object shapes between
  champion tiers and (later) building types
- CONFIRMED: MergeSystem, Generator, and later EnemyBaseManager depend on
  this; it depends on none of them (Research file's ownership direction)
- SCOPE NOTE: this pass only needs champion-tier pools (for Generator).
  Building-type pools are out of scope until EnemyBaseManager needs them —
  don't build that pool category speculatively.

## Implementation notes (added after PoolManager + Generator build)
- Location: `Assets/Scripts/Pooling/PoolManager.cs` (namespace
  `MergeWars.Pooling`), MonoBehaviour.
- Internally: `Dictionary<GameObject, Queue<GameObject>>` keyed by prefab
  reference (the prefab asset itself is the pool key), plus a
  `Dictionary<GameObject, GameObject>` mapping live instances back to
  their originating prefab so `Release()` knows which queue to return to.
- API implemented exactly as specified: `Get(GameObject prefab) →
  GameObject` (reactivates a queued instance or instantiates a new one if
  the pool is empty) and `Release(GameObject instance)` (deactivates,
  reparents under an internal pool container, returns to its queue).
- Optional `List<PoolPrewarmEntry>` (`{prefab, initialPoolSize}`) serialized
  field pre-warms pools in `Awake()`.
- If `Release()` is called with an instance the pool didn't create (no
  entry in the instance→prefab map), it is destroyed rather than pooled,
  since there's no queue to return it to.

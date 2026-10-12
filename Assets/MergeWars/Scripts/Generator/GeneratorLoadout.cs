using UnityEngine;

namespace MergeWars.Generators
{
    /// <summary>
    /// Authored config data: an ordered list of Generator prefabs to spawn
    /// into the Generator grid at game start. Purely authored/config data
    /// (no runtime state) — matches the pattern used by GeneratorConfig
    /// and HeroDefinition. GeneratorSpawner reads this at Start() and does
    /// not write back to it, so this asset can safely be shared.
    ///
    /// For now this simply lists every generator to spawn (prototype:
    /// 2 generators). A future player-selection/loadout system can either
    /// swap which GeneratorLoadout asset is assigned, or build a runtime
    /// list elsewhere and hand it to GeneratorSpawner instead — this asset
    /// is not expected to hold per-session/mutable selection state itself.
    /// See /GameDocs/Systems/SYS_Generator.md.
    /// </summary>
    [CreateAssetMenu(fileName = "GeneratorLoadout", menuName = "MergeWars/Generator Loadout")]
    public class GeneratorLoadout : ScriptableObject
    {
        [Tooltip("Ordered list of Generator prefabs to spawn into the Generator grid at game start.")]
        public Generator[] generatorPrefabs;
    }
}

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MergeWars.Heroes;

namespace MergeWars.EditorTools
{
    /// <summary>
    /// Editor-only validation for HeroDefinition.heroId uniqueness. Manual,
    /// non-blocking — warns via the console rather than enforcing at
    /// asset-save/build time. See /GameDocs/Systems/SYS_HeroDefinition.md.
    /// </summary>
    public static class HeroDefinitionValidator
    {
        [MenuItem("MergeWars/Validate/Check Duplicate Hero IDs")]
        public static void CheckDuplicateHeroIds()
        {
            string[] guids = AssetDatabase.FindAssets("t:HeroDefinition");
            var byId = new Dictionary<string, List<string>>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                HeroDefinition definition = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
                if (definition == null)
                {
                    continue;
                }

                string heroId = string.IsNullOrEmpty(definition.heroId) ? "<empty>" : definition.heroId;

                if (!byId.TryGetValue(heroId, out List<string> paths))
                {
                    paths = new List<string>();
                    byId[heroId] = paths;
                }
                paths.Add(path);
            }

            var duplicates = byId.Where(kvp => kvp.Value.Count > 1).ToList();

            if (duplicates.Count == 0)
            {
                Debug.Log("MergeWars: HeroDefinition validation passed — no duplicate heroId values found across " + guids.Length + " asset(s).");
                return;
            }

            foreach (var kvp in duplicates)
            {
                Debug.LogWarning($"MergeWars: duplicate heroId \"{kvp.Key}\" found on {kvp.Value.Count} HeroDefinition assets:\n  - {string.Join("\n  - ", kvp.Value)}");
            }
        }
    }
}
#endif

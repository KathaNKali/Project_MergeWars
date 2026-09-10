#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using MergeWars.Generators;
using MergeWars.Heroes;

namespace MergeWars.EditorTools
{
    /// <summary>
    /// One-shot editor utility to create placeholder hero prefabs,
    /// HeroDefinition assets, and a sample GeneratorConfig referencing
    /// them, since final art/content doesn't exist yet. Editor-only — not
    /// included in player builds.
    /// </summary>
    public static class GeneratorSetupTool
    {
        private const string PrefabFolder = "Assets/Prefabs/Heroes";
        private const string ConfigFolder = "Assets/Configs/Heroes";
        private const string GeneratorConfigPath = "Assets/Configs/Generators/GeneratorConfig_Placeholder.asset";

        private struct PlaceholderHero
        {
            public string heroId;
            public HeroRole role;
            public PrimitiveType shape;

            public PlaceholderHero(string heroId, HeroRole role, PrimitiveType shape)
            {
                this.heroId = heroId;
                this.role = role;
                this.shape = shape;
            }
        }

        [MenuItem("MergeWars/Setup/Create Placeholder Hero Assets")]
        public static void CreatePlaceholderHeroAssets()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ConfigFolder);
            EnsureFolder("Assets/Configs/Generators");

            var placeholders = new[]
            {
                new PlaceholderHero("tank_placeholder_01", HeroRole.Tank, PrimitiveType.Cube),
                new PlaceholderHero("meleedps_placeholder_01", HeroRole.MeleeDps, PrimitiveType.Capsule),
                new PlaceholderHero("rangeddps_placeholder_01", HeroRole.RangedDps, PrimitiveType.Sphere),
            };

            var definitions = new HeroDefinition[placeholders.Length];
            for (int i = 0; i < placeholders.Length; i++)
            {
                definitions[i] = CreateHeroDefinition(placeholders[i]);
            }

            CreateGeneratorConfig(definitions, manaCost: 10);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MergeWars: created placeholder hero prefabs + HeroDefinition assets in " + PrefabFolder + " and " + ConfigFolder + ", plus a sample GeneratorConfig at " + GeneratorConfigPath);
        }

        private static HeroDefinition CreateHeroDefinition(PlaceholderHero placeholder)
        {
            string prefabPath = $"{PrefabFolder}/Hero_{placeholder.heroId}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                GameObject temp = GameObject.CreatePrimitive(placeholder.shape);
                temp.name = $"Hero_{placeholder.heroId}";
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
                Object.DestroyImmediate(temp);
            }

            string configPath = $"{ConfigFolder}/HeroDefinition_{placeholder.heroId}.asset";
            HeroDefinition definition = AssetDatabase.LoadAssetAtPath<HeroDefinition>(configPath);

            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<HeroDefinition>();
                AssetDatabase.CreateAsset(definition, configPath);
            }

            definition.heroId = placeholder.heroId;
            definition.displayName = placeholder.heroId;
            definition.role = placeholder.role;
            definition.prefab = prefab;
            EditorUtility.SetDirty(definition);

            return definition;
        }

        private static void CreateGeneratorConfig(HeroDefinition[] heroPool, int manaCost)
        {
            GeneratorConfig config = AssetDatabase.LoadAssetAtPath<GeneratorConfig>(GeneratorConfigPath);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GeneratorConfig>();
                AssetDatabase.CreateAsset(config, GeneratorConfigPath);
            }

            config.manaCost = manaCost;
            config.heroPool = heroPool;
            EditorUtility.SetDirty(config);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using MergeWars.Generators;

namespace MergeWars.EditorTools
{
    /// <summary>
    /// One-shot editor utility to create the placeholder hero prefabs and
    /// HeroClassConfig assets (Ground/Air/Vehicles) referenced by
    /// Generator, since final art doesn't exist yet. Editor-only — not
    /// included in player builds.
    /// </summary>
    public static class GeneratorSetupTool
    {
        private const string PrefabFolder = "Assets/Prefabs/Heroes";
        private const string ConfigFolder = "Assets/Configs/HeroClasses";

        [MenuItem("MergeWars/Setup/Create Placeholder Hero Assets")]
        public static void CreatePlaceholderHeroAssets()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ConfigFolder);

            CreateHeroClass(HeroClassId.Ground, PrimitiveType.Capsule, manaCost: 10);
            CreateHeroClass(HeroClassId.Air, PrimitiveType.Sphere, manaCost: 15);
            CreateHeroClass(HeroClassId.Vehicles, PrimitiveType.Cube, manaCost: 20);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MergeWars: created placeholder hero prefabs + HeroClassConfig assets in " + PrefabFolder + " and " + ConfigFolder);
        }

        private static void CreateHeroClass(HeroClassId classId, PrimitiveType shape, int manaCost)
        {
            string prefabPath = $"{PrefabFolder}/Hero_{classId}_Placeholder.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                GameObject temp = GameObject.CreatePrimitive(shape);
                temp.name = $"Hero_{classId}_Placeholder";
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
                Object.DestroyImmediate(temp);
            }

            string configPath = $"{ConfigFolder}/HeroClassConfig_{classId}.asset";
            HeroClassConfig config = AssetDatabase.LoadAssetAtPath<HeroClassConfig>(configPath);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<HeroClassConfig>();
                AssetDatabase.CreateAsset(config, configPath);
            }

            config.classId = classId;
            config.manaCost = manaCost;
            config.heroPrefabs = new[] { prefab };
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

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// SoulvanScriptableObjectImporter: Unity Editor tool to import ScriptableObjects
/// from JSON seed data. Automates asset creation for mission modules, datacore tiers,
/// heat modifiers, and vehicle blueprints.
/// </summary>
public class SoulvanScriptableObjectImporter : EditorWindow
{
    private string sourceFolder = "Assets/Scripts/ScriptableObjects/seed-json";
    private string targetFolder = "Assets/ScriptableObjects";
    private bool overwriteExisting = false;
    private Vector2 scrollPosition;
    private string importLog = "";

    [MenuItem("Soulvan/Import ScriptableObjects from JSON")]
    public static void ShowWindow()
    {
        GetWindow<SoulvanScriptableObjectImporter>("SO Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Soulvan ScriptableObject Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Source Folder:");
        sourceFolder = EditorGUILayout.TextField(sourceFolder);

        EditorGUILayout.LabelField("Target Folder:");
        targetFolder = EditorGUILayout.TextField(targetFolder);

        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import All", GUILayout.Height(30)))
        {
            ImportAll();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Import Log:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        EditorGUILayout.TextArea(importLog, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Import all ScriptableObjects from JSON files
    /// </summary>
    void ImportAll()
    {
        importLog = "Starting import...\n";

        // Find all JSON files in source folder
        string[] jsonFiles = Directory.GetFiles(sourceFolder, "*.json", SearchOption.AllDirectories);

        if (jsonFiles.Length == 0)
        {
            importLog += "No JSON files found in source folder.\n";
            return;
        }

        int successCount = 0;
        int errorCount = 0;

        foreach (string jsonFile in jsonFiles)
        {
            importLog += $"\nProcessing: {Path.GetFileName(jsonFile)}\n";

            try
            {
                string json = File.ReadAllText(jsonFile);

                // Determine type from filename
                string filename = Path.GetFileNameWithoutExtension(jsonFile);

                if (filename.Contains("mission-module"))
                {
                    int count = ImportMissionModules(json);
                    successCount += count;
                    importLog += $"  Imported {count} mission modules\n";
                }
                else if (filename.Contains("datacore-tier"))
                {
                    int count = ImportDatacoreTiers(json);
                    successCount += count;
                    importLog += $"  Imported {count} datacore tiers\n";
                }
                else if (filename.Contains("heat-modifier"))
                {
                    int count = ImportHeatModifiers(json);
                    successCount += count;
                    importLog += $"  Imported {count} heat modifier sets\n";
                }
                else if (filename.Contains("vehicle-blueprint"))
                {
                    int count = ImportVehicleBlueprints(json);
                    successCount += count;
                    importLog += $"  Imported {count} vehicle blueprints\n";
                }
                else
                {
                    importLog += $"  Skipped: Unknown file type\n";
                }
            }
            catch (System.Exception e)
            {
                errorCount++;
                importLog += $"  ERROR: {e.Message}\n";
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        importLog += $"\n=== Import Complete ===\n";
        importLog += $"Success: {successCount} assets\n";
        importLog += $"Errors: {errorCount}\n";
    }

    /// <summary>
    /// Import mission modules
    /// </summary>
    int ImportMissionModules(string json)
    {
        var modules = JsonHelper.FromJson<MissionModuleJson>(json);
        int count = 0;

        string folder = $"{targetFolder}/MissionModules";
        CreateFolderIfNotExists(folder);

        foreach (var data in modules)
        {
            SO_MissionModule module = ScriptableObject.CreateInstance<SO_MissionModule>();
            module.moduleId = data.moduleId;
            module.displayName = data.displayName;
            module.description = data.description;
            module.weight = data.weight;
            module.entryTags = data.entryTags;
            module.exitTags = data.exitTags;
            module.heatModifier = data.heatModifier;
            module.heatMultiplier = data.heatMultiplier;
            module.creditBonus = data.creditBonus;
            module.experienceReward = data.experienceReward;
            module.notes = data.notes;

            string assetPath = $"{folder}/{data.moduleId.Replace(":", "_")}.asset";
            CreateAsset(module, assetPath);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Import datacore tiers
    /// </summary>
    int ImportDatacoreTiers(string json)
    {
        var tiers = JsonHelper.FromJson<DatacoreTierJson>(json);
        int count = 0;

        string folder = $"{targetFolder}/Datacores";
        CreateFolderIfNotExists(folder);

        foreach (var data in tiers)
        {
            SO_DatacoreTier tier = ScriptableObject.CreateInstance<SO_DatacoreTier>();
            tier.tierId = data.tierId;
            tier.displayName = data.displayName;
            tier.rarityScore = data.rarityScore;
            tier.dropChance = data.dropChance;
            tier.uiColor = ParseColor(data.uiColor);
            tier.glowIntensity = data.glowIntensity;
            tier.creditReward = data.creditReward;
            tier.experienceReward = data.experienceReward;
            tier.daoInfluenceBonus = data.daoInfluenceBonus;
            tier.minPerformanceScore = data.minPerformanceScore;
            tier.requiredRole = data.requiredRole;

            string assetPath = $"{folder}/{data.tierId.Replace(":", "_")}.asset";
            CreateAsset(tier, assetPath);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Import heat modifiers
    /// </summary>
    int ImportHeatModifiers(string json)
    {
        var data = JsonUtility.FromJson<HeatModifiersJson>(json);

        string folder = $"{targetFolder}/Heat";
        CreateFolderIfNotExists(folder);

        SO_HeatModifiers heatModifiers = ScriptableObject.CreateInstance<SO_HeatModifiers>();
        heatModifiers.modifiers = new List<SO_HeatModifiers.HeatModifier>();

        foreach (var mod in data.modifiers)
        {
            SO_HeatModifiers.HeatModifier modifier = new SO_HeatModifiers.HeatModifier
            {
                action = mod.action,
                heatDelta = mod.heatDelta,
                description = mod.description
            };
            heatModifiers.modifiers.Add(modifier);
        }

        string assetPath = $"{folder}/HeatModifiers.asset";
        CreateAsset(heatModifiers, assetPath);

        return 1;
    }

    /// <summary>
    /// Import vehicle blueprints
    /// </summary>
    int ImportVehicleBlueprints(string json)
    {
        var blueprints = JsonHelper.FromJson<VehicleBlueprintJson>(json);
        int count = 0;

        string folder = $"{targetFolder}/Vehicles";
        CreateFolderIfNotExists(folder);

        foreach (var data in blueprints)
        {
            SO_VehicleBlueprint blueprint = ScriptableObject.CreateInstance<SO_VehicleBlueprint>();
            blueprint.blueprintId = data.blueprintId;
            blueprint.displayName = data.displayName;
            blueprint.description = data.description;
            blueprint.maxSpeed = data.maxSpeed;
            blueprint.acceleration = data.acceleration;
            blueprint.handling = data.handling;
            blueprint.brakeForce = data.brakeForce;
            blueprint.boostMultiplier = data.boostMultiplier;
            blueprint.boostDuration = data.boostDuration;
            blueprint.boostCooldown = data.boostCooldown;
            blueprint.signatureBase = data.signatureBase;
            blueprint.signatureBoost = data.signatureBoost;
            blueprint.signatureDamaged = data.signatureDamaged;
            blueprint.maxHealth = data.maxHealth;
            blueprint.armorRating = data.armorRating;
            blueprint.upgradeSlots = data.upgradeSlots;

            string assetPath = $"{folder}/{data.blueprintId.Replace(":", "_")}.asset";
            CreateAsset(blueprint, assetPath);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Create asset at path
    /// </summary>
    void CreateAsset(ScriptableObject asset, string path)
    {
        if (!overwriteExisting && AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
        {
            importLog += $"    Skipped (already exists): {Path.GetFileName(path)}\n";
            return;
        }

        AssetDatabase.CreateAsset(asset, path);
        importLog += $"    Created: {Path.GetFileName(path)}\n";
    }

    /// <summary>
    /// Create folder if it doesn't exist
    /// </summary>
    void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }

    /// <summary>
    /// Parse hex color string
    /// </summary>
    Color ParseColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white;
    }

    // JSON data classes
    [System.Serializable]
    class MissionModuleJson
    {
        public string type;
        public string moduleId;
        public string displayName;
        public string description;
        public float weight;
        public string[] entryTags;
        public string[] exitTags;
        public float heatModifier;
        public float heatMultiplier;
        public int creditBonus;
        public int experienceReward;
        public string notes;
    }

    [System.Serializable]
    class DatacoreTierJson
    {
        public string type;
        public string tierId;
        public string displayName;
        public int rarityScore;
        public float dropChance;
        public string uiColor;
        public float glowIntensity;
        public int creditReward;
        public int experienceReward;
        public float daoInfluenceBonus;
        public float minPerformanceScore;
        public string requiredRole;
    }

    [System.Serializable]
    class HeatModifiersJson
    {
        public string type;
        public List<HeatModifierData> modifiers;

        [System.Serializable]
        public class HeatModifierData
        {
            public string action;
            public float heatDelta;
            public string description;
        }
    }

    [System.Serializable]
    class VehicleBlueprintJson
    {
        public string type;
        public string blueprintId;
        public string displayName;
        public string description;
        public float maxSpeed;
        public float acceleration;
        public float handling;
        public float brakeForce;
        public float boostMultiplier;
        public float boostDuration;
        public float boostCooldown;
        public float signatureBase;
        public float signatureBoost;
        public float signatureDamaged;
        public float maxHealth;
        public float armorRating;
        public string[] upgradeSlots;
    }
}

/// <summary>
/// JSON array helper
/// </summary>
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrapper = $"{{\"items\":{json}}}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapper);
        return w.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

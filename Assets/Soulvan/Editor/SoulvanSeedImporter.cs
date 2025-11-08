using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// Lightweight JSON importer that expects MiniJSON (or similar) to parse arbitrary JSON into Dictionary<string, object>.
// Drop JSON seed files under Assets/Soulvan/ScriptableObjects/seed-json
// Usage: Window -> Soulvan -> Import Seed JSON

public class SoulvanSeedImporter : EditorWindow {
  string seedJsonFolder = "Assets/Soulvan/ScriptableObjects/seed-json";

  [MenuItem("Soulvan/Import Seed JSON")]
  public static void ShowWindow() {
    GetWindow<SoulvanSeedImporter>("Soulvan Seed Importer");
  }

  void OnGUI() {
    GUILayout.Label("Seed JSON Importer", EditorStyles.boldLabel);
    seedJsonFolder = EditorGUILayout.TextField("Seed JSON Folder", seedJsonFolder);

    if (GUILayout.Button("Import JSON -> ScriptableObjects")) {
      ImportAll(seedJsonFolder);
    }
  }

  static void ImportAll(string folder) {
    if (!Directory.Exists(folder)) {
      EditorUtility.DisplayDialog("Error", $"Folder not found: {folder}", "OK");
      return;
    }

    var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
    int created = 0;
    foreach (var f in files) {
      try {
        var text = File.ReadAllText(f);
        var j = MiniJSON.Json.Deserialize(text) as Dictionary<string, object>;
        if (j == null || !j.ContainsKey("type")) continue;
        string type = j["type"] as string;
        if (type == "SO_MissionModule") {
          CreateMissionModuleAsset(j, f);
          created++;
        } else if (type == "SO_DatacoreTier") {
          CreateDatacoreAsset(j, f);
          created++;
        } else if (type == "SO_HeatModifiers") {
          CreateHeatModifiersAsset(j, f);
          created++;
        } else if (type == "SO_VehicleBlueprint") {
          CreateVehicleBlueprintAsset(j, f);
          created++;
        } else {
          Debug.LogWarning($"Unknown type in {f}: {type}");
        }
      } catch (System.Exception ex) {
        Debug.LogError($"Failed import {f}: {ex.Message}");
      }
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    EditorUtility.DisplayDialog("Import Complete", $"Created {created} assets from JSON.", "OK");
  }

  static void CreateMissionModuleAsset(Dictionary<string, object> j, string srcPath) {
    var obj = ScriptableObject.CreateInstance<SO_MissionModule>();
    obj.moduleId = FieldString(j, "moduleId");
    obj.displayName = FieldString(j, "displayName");
    obj.weight = FieldFloat(j, "weight", 1f);
    obj.entryTags = FieldStringArray(j, "entryTags");
    obj.exitTags = FieldStringArray(j, "exitTags");
    var name = string.IsNullOrEmpty(obj.moduleId) ? Path.GetFileNameWithoutExtension(srcPath) : obj.moduleId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/MissionModules/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateDatacoreAsset(Dictionary<string, object> j, string srcPath) {
    var obj = ScriptableObject.CreateInstance<SO_DatacoreTier>();
    obj.tierId = FieldString(j, "tierId");
    obj.displayName = FieldString(j, "displayName");
    obj.rarityScore = (int)FieldFloat(j, "rarityScore", 1);
    obj.uiColor = ParseColor(FieldString(j, "uiColor", "#FFFFFF"));
    var name = string.IsNullOrEmpty(obj.tierId) ? Path.GetFileNameWithoutExtension(srcPath) : obj.tierId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Datacores/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateHeatModifiersAsset(Dictionary<string, object> j, string srcPath) {
    var obj = ScriptableObject.CreateInstance<SO_HeatModifiers>();
    var mods = new List<SO_HeatModifiers.HeatEntry>();
    if (j.ContainsKey("modifiers")) {
      var arr = j["modifiers"] as List<object>;
      if (arr != null) {
        foreach (var item in arr) {
          var entry = item as Dictionary<string, object>;
          if (entry == null) continue;
          var he = new SO_HeatModifiers.HeatEntry();
          he.action = FieldString(entry, "action");
          he.heatDelta = (int)FieldFloat(entry, "heatDelta", 0);
          mods.Add(he);
        }
      }
    }
    obj.entries = mods.ToArray();
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Heat/SO_HeatModifiers.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateVehicleBlueprintAsset(Dictionary<string, object> j, string srcPath) {
    var obj = ScriptableObject.CreateInstance<SO_VehicleBlueprint>();
    obj.blueprintId = FieldString(j, "blueprintId");
    obj.displayName = FieldString(j, "displayName");
    obj.maxSpeed = FieldFloat(j, "maxSpeed", 200f);
    obj.acceleration = FieldFloat(j, "acceleration", 30f);
    obj.handling = FieldFloat(j, "handling", 0.5f);
    obj.signatureBase = (int)FieldFloat(j, "signatureBase", 10);
    obj.upgradeSlots = FieldStringArray(j, "upgradeSlots");
    var name = string.IsNullOrEmpty(obj.blueprintId) ? Path.GetFileNameWithoutExtension(srcPath) : obj.blueprintId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Vehicles/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  // helpers
  static string FieldString(Dictionary<string, object> j, string key, string fallback = "") {
    if (!j.ContainsKey(key)) return fallback;
    return j[key]?.ToString() ?? fallback;
  }
  static float FieldFloat(Dictionary<string, object> j, string key, float fallback = 0f) {
    if (!j.ContainsKey(key)) return fallback;
    double d;
    if (double.TryParse(j[key].ToString(), out d)) return (float)d;
    return fallback;
  }
  static string[] FieldStringArray(Dictionary<string, object> j, string key) {
    if (!j.ContainsKey(key)) return new string[0];
    var arr = j[key] as List<object>;
    if (arr == null) return new string[0];
    var outArr = new string[arr.Count];
    for (int i = 0; i < arr.Count; i++) outArr[i] = arr[i].ToString();
    return outArr;
  }

  static Color ParseColor(string hex) {
    Color c = Color.white;
    if (ColorUtility.TryParseHtmlString(hex, out c)) return c;
    return Color.white;
  }
}

// NOTE: This script requires the lightweight MiniJSON used widely in Unity editor snippets.
// If you don't have MiniJSON, create a small Json utility or use Unity's JsonUtility with adjusted JSON shape.
// This script assumes these ScriptableObject classes exist: SO_MissionModule, SO_DatacoreTier, SO_HeatModifiers, SO_VehicleBlueprint and that Heat/Vehicle SO types have public fields as shown earlier.

// SoulvanJsonUtilityImporter.cs
// Place under Assets/Soulvan/Editor/
// Usage: Window -> Soulvan -> Import Strongly Typed JSON -> select folder (Assets/Soulvan/ScriptableObjects/seed-json)

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SoulvanJsonUtilityImporter : EditorWindow {
  string seedJsonFolder = "Assets/Soulvan/ScriptableObjects/seed-json";

  [MenuItem("Soulvan/Import Strongly Typed JSON")]
  public static void ShowWindow() => GetWindow<SoulvanJsonUtilityImporter>("Soulvan JSON Importer");

  void OnGUI() {
    GUILayout.Label("Strongly Typed JSON Importer", EditorStyles.boldLabel);
    seedJsonFolder = EditorGUILayout.TextField("Seed JSON Folder", seedJsonFolder);
    if (GUILayout.Button("Import JSON -> ScriptableObjects")) ImportAll(seedJsonFolder);
  }

  public static void ImportAll(string folder) {
    if (!Directory.Exists(folder)) {
      EditorUtility.DisplayDialog("Error", $"Folder not found: {folder}", "OK");
      return;
    }

    var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
    int created = 0;
    foreach (var f in files) {
      try {
        var text = File.ReadAllText(f);
        var header = JsonUtility.FromJson<JsonTypeHeader>(text);
        if (header == null || string.IsNullOrEmpty(header.type)) continue;
        if (header.type == "SO_MissionModule") {
          var payload = JsonUtility.FromJson<SO_MissionModuleDto>(text);
          CreateMissionModuleAsset(payload);
          created++;
        } else if (header.type == "SO_DatacoreTier") {
          var payload = JsonUtility.FromJson<SO_DatacoreTierDto>(text);
          CreateDatacoreAsset(payload);
          created++;
        } else if (header.type == "SO_HeatModifiers") {
          var payload = JsonUtility.FromJson<SO_HeatModifiersDto>(text);
          CreateHeatModifiersAsset(payload);
          created++;
        } else if (header.type == "SO_VehicleBlueprint") {
          var payload = JsonUtility.FromJson<SO_VehicleBlueprintDto>(text);
          CreateVehicleBlueprintAsset(payload);
          created++;
        } else {
          Debug.LogWarning($"Unknown type in {f}: {header.type}");
        }
      } catch (System.Exception ex) {
        Debug.LogError($"Failed import {f}: {ex.Message}");
      }
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    EditorUtility.DisplayDialog("Import Complete", $"Created {created} assets from JSON.", "OK");
  }

  // DTOs used by JsonUtility (must match JSON exactly)
  [System.Serializable] class JsonTypeHeader { public string type; }

  [System.Serializable]
  class SO_MissionModuleDto {
    public string type;
    public string moduleId;
    public string displayName;
    public float weight;
    public string[] entryTags;
    public string[] exitTags;
    public string notes;
  }

  [System.Serializable]
  class SO_DatacoreTierDto {
    public string type;
    public string tierId;
    public string displayName;
    public int rarityScore;
    public string uiColor;
  }

  [System.Serializable]
  class SO_HeatModifiersDto {
    public string type;
    public HeatEntryDto[] modifiers;
    [System.Serializable] public class HeatEntryDto { public string action; public int heatDelta; }
  }

  [System.Serializable]
  class SO_VehicleBlueprintDto {
    public string type;
    public string blueprintId;
    public string displayName;
    public float maxSpeed;
    public float acceleration;
    public float handling;
    public int signatureBase;
    public string[] upgradeSlots;
  }

  // Create asset helpers (assumes SO classes exist in project)
  static void CreateMissionModuleAsset(SO_MissionModuleDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_MissionModule>();
    obj.moduleId = dto.moduleId;
    obj.displayName = dto.displayName;
    obj.weight = dto.weight;
    obj.entryTags = dto.entryTags ?? new string[0];
    obj.exitTags = dto.exitTags ?? new string[0];
    var name = obj.moduleId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/MissionModules/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateDatacoreAsset(SO_DatacoreTierDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_DatacoreTier>();
    obj.tierId = dto.tierId;
    obj.displayName = dto.displayName;
    obj.rarityScore = dto.rarityScore;
    obj.uiColor = ParseColor(dto.uiColor);
    var name = obj.tierId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Datacores/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateHeatModifiersAsset(SO_HeatModifiersDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_HeatModifiers>();
    var list = new List<SO_HeatModifiers.HeatEntry>();
    if (dto.modifiers != null) {
      foreach (var m in dto.modifiers) {
        var he = new SO_HeatModifiers.HeatEntry { action = m.action, heatDelta = m.heatDelta };
        list.Add(he);
      }
    }
    obj.entries = list.ToArray();
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Heat/SO_HeatModifiers.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static void CreateVehicleBlueprintAsset(SO_VehicleBlueprintDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_VehicleBlueprint>();
    obj.blueprintId = dto.blueprintId;
    obj.displayName = dto.displayName;
    obj.maxSpeed = dto.maxSpeed;
    obj.acceleration = dto.acceleration;
    obj.handling = dto.handling;
    obj.signatureBase = dto.signatureBase;
    obj.upgradeSlots = dto.upgradeSlots ?? new string[0];
    var name = obj.blueprintId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Vehicles/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
  }

  static Color ParseColor(string hex) {
    Color c = Color.white;
    if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out c)) return c;
    return Color.white;
  }
}

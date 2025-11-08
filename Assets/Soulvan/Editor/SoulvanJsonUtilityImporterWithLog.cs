// SoulvanJsonUtilityImporterWithLog.cs
// Place under Assets/Soulvan/Editor/
// Usage: Window -> Soulvan -> Import Typed JSON with CSV Log
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class SoulvanJsonUtilityImporterWithLog : EditorWindow {
  string seedJsonFolder = "Assets/Soulvan/ScriptableObjects/seed-json";
  string csvOutputPath = "Assets/Soulvan/Docs/import-log.csv";

  [MenuItem("Soulvan/Import Typed JSON with CSV Log")]
  public static void ShowWindow() => GetWindow<SoulvanJsonUtilityImporterWithLog>("Soulvan Importer CSV");

  void OnGUI() {
    GUILayout.Label("Import JSON -> ScriptableObjects + CSV Log", EditorStyles.boldLabel);
    seedJsonFolder = EditorGUILayout.TextField("Seed JSON Folder", seedJsonFolder);
    csvOutputPath = EditorGUILayout.TextField("CSV Output Path", csvOutputPath);
    if (GUILayout.Button("Import and Write CSV")) ImportAll(seedJsonFolder, csvOutputPath);
  }

  public static void ImportAll(string folder, string csvPath) {
    if (!Directory.Exists(folder)) {
      EditorUtility.DisplayDialog("Error", $"Folder not found: {folder}", "OK");
      return;
    }

    var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
    var csv = new StringBuilder();
    csv.AppendLine("sourceFile,type,createdAssetPath,status,message");
    int created = 0;

    foreach (var f in files) {
      string status = "skipped";
      string message = "";
      string createdPath = "";
      try {
        var text = File.ReadAllText(f);
        var header = JsonUtility.FromJson<JsonTypeHeader>(text);
        if (header == null || string.IsNullOrEmpty(header.type)) {
          message = "missing type header";
        } else {
          if (header.type == "SO_MissionModule") {
            var dto = JsonUtility.FromJson<SO_MissionModuleDto>(text);
            createdPath = CreateMissionModuleAsset(dto);
            status = "created"; created++;
          } else if (header.type == "SO_DatacoreTier") {
            var dto = JsonUtility.FromJson<SO_DatacoreTierDto>(text);
            createdPath = CreateDatacoreAsset(dto);
            status = "created"; created++;
          } else if (header.type == "SO_HeatModifiers") {
            var dto = JsonUtility.FromJson<SO_HeatModifiersDto>(text);
            createdPath = CreateHeatModifiersAsset(dto);
            status = "created"; created++;
          } else if (header.type == "SO_VehicleBlueprint") {
            var dto = JsonUtility.FromJson<SO_VehicleBlueprintDto>(text);
            createdPath = CreateVehicleBlueprintAsset(dto);
            status = "created"; created++;
          } else {
            message = $"unknown type: {header.type}";
          }
        }
      } catch (System.Exception ex) {
        status = "error";
        message = ex.Message.Replace("\n"," ").Replace("\r"," ");
      }

      csv.AppendLine($"{Path.GetFileName(f)},{(JsonUtility.FromJson<JsonTypeHeader>(File.ReadAllText(f))?.type ?? "")},\"{createdPath}\",{status},\"{message}\"");
    }

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();

    try {
      Directory.CreateDirectory(Path.GetDirectoryName(csvPath));
      File.WriteAllText(csvPath, csv.ToString(), Encoding.UTF8);
      AssetDatabase.ImportAsset(csvPath);
    } catch (System.Exception ex) {
      Debug.LogError($"Failed to write CSV: {ex.Message}");
    }

    EditorUtility.DisplayDialog("Import Complete", $"Created {created} assets. CSV written to {csvPath}", "OK");
  }

  [System.Serializable] class JsonTypeHeader { public string type; }
  [System.Serializable] class SO_MissionModuleDto { public string type; public string moduleId; public string displayName; public float weight; public string[] entryTags; public string[] exitTags; public string notes; }
  [System.Serializable] class SO_DatacoreTierDto { public string type; public string tierId; public string displayName; public int rarityScore; public string uiColor; }
  [System.Serializable] class SO_HeatModifiersDto { public string type; public HeatEntryDto[] modifiers; [System.Serializable] public class HeatEntryDto { public string action; public int heatDelta; } }
  [System.Serializable] class SO_VehicleBlueprintDto { public string type; public string blueprintId; public string displayName; public float maxSpeed; public float acceleration; public float handling; public int signatureBase; public string[] upgradeSlots; }

  // asset creators return created asset path (or empty)
  static string CreateMissionModuleAsset(SO_MissionModuleDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_MissionModule>();
    obj.moduleId = dto.moduleId; obj.displayName = dto.displayName; obj.weight = dto.weight;
    obj.entryTags = dto.entryTags ?? new string[0]; obj.exitTags = dto.exitTags ?? new string[0];
    var name = obj.moduleId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/MissionModules/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
    return assetPath;
  }

  static string CreateDatacoreAsset(SO_DatacoreTierDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_DatacoreTier>();
    obj.tierId = dto.tierId; obj.displayName = dto.displayName; obj.rarityScore = dto.rarityScore; obj.uiColor = ParseColor(dto.uiColor);
    var name = obj.tierId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Datacores/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
    return assetPath;
  }

  static string CreateHeatModifiersAsset(SO_HeatModifiersDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_HeatModifiers>();
    var list = new List<SO_HeatModifiers.HeatEntry>();
    if (dto.modifiers != null) foreach (var m in dto.modifiers) list.Add(new SO_HeatModifiers.HeatEntry { action = m.action, heatDelta = m.heatDelta });
    obj.entries = list.ToArray();
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Heat/SO_HeatModifiers.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
    return assetPath;
  }

  static string CreateVehicleBlueprintAsset(SO_VehicleBlueprintDto dto) {
    var obj = ScriptableObject.CreateInstance<SO_VehicleBlueprint>();
    obj.blueprintId = dto.blueprintId; obj.displayName = dto.displayName; obj.maxSpeed = dto.maxSpeed; obj.acceleration = dto.acceleration; obj.handling = dto.handling; obj.signatureBase = dto.signatureBase; obj.upgradeSlots = dto.upgradeSlots ?? new string[0];
    var name = obj.blueprintId.Replace(":", "_");
    var assetPath = $"Assets/Soulvan/ScriptableObjects/Vehicles/{name}.asset";
    Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
    AssetDatabase.CreateAsset(obj, assetPath);
    return assetPath;
  }

  static Color ParseColor(string hex) { Color c = Color.white; if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out c)) return c; return Color.white; }
}

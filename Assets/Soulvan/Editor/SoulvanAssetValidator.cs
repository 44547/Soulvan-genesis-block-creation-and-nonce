// SoulvanAssetValidator.cs
// Place under Assets/Soulvan/Editor/
// Usage: Window -> Soulvan -> Validate Soulvan Assets
// Scans key ScriptableObject folders and checks for expected fields, emitting warnings in the Console.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class SoulvanAssetValidator : EditorWindow {
  [MenuItem("Soulvan/Validate Soulvan Assets")]
  public static void ShowWindow() => GetWindow<SoulvanAssetValidator>("Soulvan Validator");

  void OnGUI() {
    GUILayout.Label("Soulvan Asset Validator", EditorStyles.boldLabel);
    if (GUILayout.Button("Validate ScriptableObjects")) ValidateAll();
  }

  static void ValidateAll() {
    var issues = new List<string>();
    ValidateMissionModules(issues);
    ValidateDatacores(issues);
    ValidateHeatModifiers(issues);
    ValidateVehicleBlueprints(issues);

    if (issues.Count == 0) {
      EditorUtility.DisplayDialog("Validation", "No issues found. All Soulvan assets look good.", "OK");
    } else {
      var report = string.Join("\n", issues);
      Debug.LogWarning($"Soulvan Asset Validator found {issues.Count} issues:\n{report}");
      EditorUtility.DisplayDialog("Validation Complete", $"Found {issues.Count} issues. Check Console for details.", "OK");
    }
  }

  static void ValidateMissionModules(List<string> issues) {
    var guids = AssetDatabase.FindAssets("t:SO_MissionModule", new[] { "Assets/Soulvan/ScriptableObjects/MissionModules" });
    if (guids.Length == 0) { issues.Add("No SO_MissionModule assets found in ScriptableObjects/MissionModules."); return; }
    foreach (var g in guids) {
      var path = AssetDatabase.GUIDToAssetPath(g);
      var so = AssetDatabase.LoadAssetAtPath<SO_MissionModule>(path);
      if (so == null) { issues.Add($"Failed to load SO_MissionModule at {path}."); continue; }
      if (string.IsNullOrEmpty(so.moduleId)) issues.Add($"SO_MissionModule missing moduleId: {path}");
      if (string.IsNullOrEmpty(so.displayName)) issues.Add($"SO_MissionModule missing displayName: {path}");
      if (so.entryTags == null) issues.Add($"SO_MissionModule entryTags null: {path}");
      if (so.exitTags == null) issues.Add($"SO_MissionModule exitTags null: {path}");
      if (so.weight <= 0f) issues.Add($"SO_MissionModule weight <= 0: {path}");
    }
  }

  static void ValidateDatacores(List<string> issues) {
    var guids = AssetDatabase.FindAssets("t:SO_DatacoreTier", new[] { "Assets/Soulvan/ScriptableObjects/Datacores" });
    if (guids.Length == 0) { issues.Add("No SO_DatacoreTier assets found in ScriptableObjects/Datacores."); return; }
    foreach (var g in guids) {
      var path = AssetDatabase.GUIDToAssetPath(g);
      var so = AssetDatabase.LoadAssetAtPath<SO_DatacoreTier>(path);
      if (so == null) { issues.Add($"Failed to load SO_DatacoreTier at {path}."); continue; }
      if (string.IsNullOrEmpty(so.tierId)) issues.Add($"SO_DatacoreTier missing tierId: {path}");
      if (so.rarityScore <= 0) issues.Add($"SO_DatacoreTier rarityScore <= 0: {path}");
    }
  }

  static void ValidateHeatModifiers(List<string> issues) {
    var guids = AssetDatabase.FindAssets("t:SO_HeatModifiers", new[] { "Assets/Soulvan/ScriptableObjects/Heat" });
    if (guids.Length == 0) { issues.Add("No SO_HeatModifiers asset found in ScriptableObjects/Heat."); return; }
    foreach (var g in guids) {
      var path = AssetDatabase.GUIDToAssetPath(g);
      var so = AssetDatabase.LoadAssetAtPath<SO_HeatModifiers>(path);
      if (so == null) { issues.Add($"Failed to load SO_HeatModifiers at {path}."); continue; }
      if (so.entries == null || so.entries.Length == 0) issues.Add($"SO_HeatModifiers has no entries: {path}");
      else {
        for (int i = 0; i < so.entries.Length; i++) {
          var e = so.entries[i];
          if (string.IsNullOrEmpty(e.action)) issues.Add($"Heat entry #{i} missing action: {path}");
        }
      }
    }
  }

  static void ValidateVehicleBlueprints(List<string> issues) {
    var guids = AssetDatabase.FindAssets("t:SO_VehicleBlueprint", new[] { "Assets/Soulvan/ScriptableObjects/Vehicles" });
    if (guids.Length == 0) { issues.Add("No SO_VehicleBlueprint assets found in ScriptableObjects/Vehicles."); return; }
    foreach (var g in guids) {
      var path = AssetDatabase.GUIDToAssetPath(g);
      var so = AssetDatabase.LoadAssetAtPath<SO_VehicleBlueprint>(path);
      if (so == null) { issues.Add($"Failed to load SO_VehicleBlueprint at {path}."); continue; }
      if (string.IsNullOrEmpty(so.blueprintId)) issues.Add($"SO_VehicleBlueprint missing blueprintId: {path}");
      if (so.maxSpeed <= 0f) issues.Add($"SO_VehicleBlueprint maxSpeed <= 0: {path}");
      if (so.acceleration <= 0f) issues.Add($"SO_VehicleBlueprint acceleration <= 0: {path}");
    }
  }
}

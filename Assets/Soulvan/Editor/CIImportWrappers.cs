// CIImportWrappers.cs
// Place under Assets/Soulvan/Editor/
// Provides stable static methods you can call via -executeMethod in CI.
// Methods:
//  - Soulvan.CI.CIImportWrappers.SetEditorPrefsFromEnv
//  - Soulvan.CI.CIImportWrappers.RunStrongJsonImport
//  - Soulvan.CI.CIImportWrappers.RunCsvJsonImportWithLog

using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Soulvan.CI {
  public static class CIImportWrappers {
    // Sets EditorPrefs from environment variables (safe for CI ephemeral use)
    // Expects env vars: SOULVAN_API_BASE_URL, SOULVAN_REPLAY_ENDPOINT, SOULVAN_DAO_ENDPOINT, SOULVAN_DEFAULT_CONTRIBUTOR, SOULVAN_IMPORTER_CSV_PATH (optional)
    public static void SetEditorPrefsFromEnv() {
      try {
        var apiBase = Environment.GetEnvironmentVariable("SOULVAN_API_BASE_URL") ?? "https://api.dev.soulvan";
        var replayEndpoint = Environment.GetEnvironmentVariable("SOULVAN_REPLAY_ENDPOINT") ?? $"{apiBase}/replay/log";
        var daoEndpoint = Environment.GetEnvironmentVariable("SOULVAN_DAO_ENDPOINT") ?? $"{apiBase}/dao/impact";
        var defaultContributor = Environment.GetEnvironmentVariable("SOULVAN_DEFAULT_CONTRIBUTOR") ?? "C_CI";
        var csvPath = Environment.GetEnvironmentVariable("SOULVAN_IMPORTER_CSV_PATH") ?? "Assets/Soulvan/Docs/import-log.csv";

        EditorPrefs.SetString("Soulvan:ApiBaseUrl", apiBase);
        EditorPrefs.SetString("Soulvan:ReplayEndpoint", replayEndpoint);
        EditorPrefs.SetString("Soulvan:DaoEndpoint", daoEndpoint);
        EditorPrefs.SetString("Soulvan:DefaultContributor", defaultContributor);
        EditorPrefs.SetString("Soulvan:ImporterCsvPath", csvPath);

        Debug.Log($"[CI] Soulvan EditorPrefs set (ApiBase, Replay, Dao, Contributor, CSV) -> {apiBase}, {replayEndpoint}, {daoEndpoint}, {defaultContributor}, {csvPath}");
      } catch (Exception ex) {
        Debug.LogError("[CI] SetEditorPrefsFromEnv failed: " + ex.Message);
        throw;
      }
    }

    // Runs the strongly-typed JSON importer window method (calls its internal import entry if present)
    // This wrapper attempts to call static ImportAll on SoulvanJsonUtilityImporter, otherwise it calls the menu item.
    public static void RunStrongJsonImport() {
      try {
        var importerType = Type.GetType("SoulvanJsonUtilityImporter");
        if (importerType != null) {
          var mi = importerType.GetMethod("ImportAll", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
          if (mi != null) {
            var seedFolder = Environment.GetEnvironmentVariable("SOULVAN_SEED_JSON_FOLDER") ?? "Assets/Soulvan/ScriptableObjects/seed-json";
            mi.Invoke(null, new object[] { seedFolder });
            Debug.Log("[CI] Invoked SoulvanJsonUtilityImporter.ImportAll");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
          }
        }

        // Fallback: execute menu item
        var menuStr = "Soulvan/Import Strongly Typed JSON";
        EditorApplication.ExecuteMenuItem(menuStr);
        Debug.Log($"[CI] Executed menu item: {menuStr}");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } catch (Exception ex) {
        Debug.LogError("[CI] RunStrongJsonImport failed: " + ex.Message);
        throw;
      }
    }

    // Runs the CSV-generating importer variant if present
    public static void RunCsvJsonImportWithLog() {
      try {
        var importerType = Type.GetType("SoulvanJsonUtilityImporterWithLog");
        if (importerType != null) {
          var mi = importerType.GetMethod("ImportAll", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
          if (mi != null) {
            var seedFolder = Environment.GetEnvironmentVariable("SOULVAN_SEED_JSON_FOLDER") ?? "Assets/Soulvan/ScriptableObjects/seed-json";
            var csvPath = Environment.GetEnvironmentVariable("SOULVAN_IMPORTER_CSV_PATH") ?? "Assets/Soulvan/Docs/import-log.csv";
            mi.Invoke(null, new object[] { seedFolder, csvPath });
            Debug.Log("[CI] Invoked SoulvanJsonUtilityImporterWithLog.ImportAll");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
          }
        }

        // Fallback to menu item if available
        var menuStr = "Soulvan/Import Typed JSON with CSV Log";
        EditorApplication.ExecuteMenuItem(menuStr);
        Debug.Log($"[CI] Executed menu item: {menuStr}");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } catch (Exception ex) {
        Debug.LogError("[CI] RunCsvJsonImportWithLog failed: " + ex.Message);
        throw;
      }
    }
  }
}

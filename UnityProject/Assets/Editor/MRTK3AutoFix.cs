using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.IO;
using MixedReality.Toolkit;

public static class MRTK3AutoFix
{
    [MenuItem("MRTK3/Auto-Fix Project Configuration")]
    public static void AutoFixAll()
    {
        FixSyntheticHandsConfig();
        EnableXRSimulationLoader();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MRTK3 Auto-Fix] Done. Now run MRTK > Setup MRTK3 in Scene.");
        EditorUtility.DisplayDialog("MRTK3 Auto-Fix",
            "Fixed!\n\nNext:\n1. MRTK > Setup MRTK3 in Scene\n2. Press Play", "OK");
    }

    static void FixSyntheticHandsConfig()
    {
        string configPath = "Assets/MRTK.Generated/MRTKSyntheticHandsConfig.asset";

        // Find the default config in package cache
        string defaultConfigPath = null;
        string[] guids = AssetDatabase.FindAssets("MRTKSyntheticHandsConfig t:ScriptableObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Default Configs") && path.Contains("MRTKSyntheticHandsConfig"))
            {
                defaultConfigPath = path;
                break;
            }
        }

        if (defaultConfigPath == null)
        {
            Debug.LogError("[MRTK3 Auto-Fix] Default MRTKSyntheticHandsConfig not found in package cache.");
            return;
        }

        // Copy default config to project (preserves existing GUID if file exists)
        bool success = AssetDatabase.CopyAsset(defaultConfigPath, configPath);
        if (!success)
        {
            // CopyAsset fails if dest exists; delete and retry
            AssetDatabase.DeleteAsset(configPath);
            success = AssetDatabase.CopyAsset(defaultConfigPath, configPath);
        }
        if (success)
        {
            SyncMRTKProfileReference(configPath);
            Debug.Log("[MRTK3 Auto-Fix] SyntheticHandsConfig replaced with default (all bindings restored).");
        }
        else
            Debug.LogError("[MRTK3 Auto-Fix] Failed to copy default SyntheticHandsConfig.");
    }

    static void SyncMRTKProfileReference(string configPath)
    {
        // Get the GUID of the newly copied config
        string configMetaPath = configPath + ".meta";
        if (!File.Exists(configMetaPath)) return;

        string configGuid = null;
        foreach (string line in File.ReadAllLines(configMetaPath))
        {
            if (line.TrimStart().StartsWith("guid:"))
            {
                configGuid = line.Split(':')[1].Trim();
                break;
            }
        }
        if (configGuid == null) return;

        // Find MRTKProfile and update its reference
        string profilePath = "Assets/MRTK.Generated/MRTKProfile.asset";
        var profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(profilePath);
        if (profile == null) return;

        var so = new SerializedObject(profile);
        var configs = so.FindProperty("subsystemConfigs.entries");
        for (int i = 0; i < configs.arraySize; i++)
        {
            var valueProp = configs.GetArrayElementAtIndex(i).FindPropertyRelative("value");
            var guidProp = valueProp.FindPropertyRelative("guid");
            if (guidProp != null && guidProp.stringValue != configGuid)
            {
                // Check if this entry is for SyntheticHandsSubsystem
                var keyRef = configs.GetArrayElementAtIndex(i).FindPropertyRelative("key.reference");
                if (keyRef != null && keyRef.stringValue.Contains("SyntheticHandsSubsystem"))
                {
                    guidProp.stringValue = configGuid;
                    Debug.Log($"[MRTK3 Auto-Fix] MRTKProfile SyntheticHandsConfig reference updated to {configGuid}");
                }
            }
        }
        so.ApplyModifiedProperties();

        // Ensure SyntheticHandsSubsystem is in loadedSubsystems
        so = new SerializedObject(profile);
        var loadedSubs = so.FindProperty("loadedSubsystems");
        bool found = false;
        for (int i = 0; i < loadedSubs.arraySize; i++)
        {
            var refProp = loadedSubs.GetArrayElementAtIndex(i).FindPropertyRelative("reference");
            if (refProp != null && refProp.stringValue.Contains("SyntheticHandsSubsystem"))
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            loadedSubs.InsertArrayElementAtIndex(loadedSubs.arraySize);
            var newEntry = loadedSubs.GetArrayElementAtIndex(loadedSubs.arraySize - 1);
            newEntry.FindPropertyRelative("reference").stringValue =
                "MixedReality.Toolkit.Input.SyntheticHandsSubsystem, MixedReality.Toolkit.Input";
            Debug.Log("[MRTK3 Auto-Fix] Added SyntheticHandsSubsystem to loadedSubsystems.");
        }
        so.ApplyModifiedProperties();
    }

    static void EnableXRSimulationLoader()
    {
        var buildTargetGroup = BuildTargetGroup.Standalone;

        EditorBuildSettings.TryGetConfigObject(
            XRGeneralSettings.k_SettingsKey,
            out XRGeneralSettingsPerBuildTarget buildTargetSettings);

        if (buildTargetSettings == null)
        {
            Debug.LogWarning("[MRTK3 Auto-Fix] XR General Settings not found. Enable XR Plug-in Management manually.");
            return;
        }

        var xrSettings = buildTargetSettings.SettingsForBuildTarget(buildTargetGroup);
        if (xrSettings == null)
        {
            buildTargetSettings.CreateDefaultSettingsForBuildTarget(buildTargetGroup);
            xrSettings = buildTargetSettings.SettingsForBuildTarget(buildTargetGroup);
        }

        if (xrSettings.AssignedSettings == null)
        {
            buildTargetSettings.CreateDefaultManagerSettingsForBuildTarget(buildTargetGroup);
            xrSettings = buildTargetSettings.SettingsForBuildTarget(buildTargetGroup);
        }

        bool ok = XRPackageMetadataStore.AssignLoader(
            xrSettings.AssignedSettings,
            "UnityEngine.XR.Simulation.SimulationLoader",
            buildTargetGroup);

        if (ok)
            Debug.Log("[MRTK3 Auto-Fix] XR Simulation Loader enabled.");
        else
            Debug.LogWarning("[MRTK3 Auto-Fix] Could not enable XR Simulation Loader. Enable manually in Project Settings > XR Plug-in Management.");
    }
}

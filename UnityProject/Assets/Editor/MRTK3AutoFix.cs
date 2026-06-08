using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using System.IO;

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

        // Delete broken config
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
            File.Delete(configPath + ".meta");
        }

        // Copy default config to project
        bool success = AssetDatabase.CopyAsset(defaultConfigPath, configPath);
        if (success)
            Debug.Log("[MRTK3 Auto-Fix] SyntheticHandsConfig replaced with default (all bindings restored).");
        else
            Debug.LogError("[MRTK3 Auto-Fix] Failed to copy default SyntheticHandsConfig.");
    }

    static void EnableXRSimulationLoader()
    {
        var buildTargetGroup = BuildTargetGroup.Standalone;
        var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);

        if (settings == null)
            settings = XRGeneralSettingsPerBuildTarget.GetOrCreate().SettingsForBuildTarget(buildTargetGroup);

        if (settings == null)
        {
            Debug.LogWarning("[MRTK3 Auto-Fix] Could not get XR General Settings.");
            return;
        }

        if (settings.AssignedSettings == null)
        {
            settings.AssignedSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
        }

        string simulationLoader = "UnityEngine.XR.Simulation.SimulationLoader";
        bool ok = XRPackageMetadataStore.AssignLoader(settings.AssignedSettings, simulationLoader, buildTargetGroup);

        if (ok)
            Debug.Log("[MRTK3 Auto-Fix] XR Simulation Loader enabled.");
        else
            Debug.LogWarning("[MRTK3 Auto-Fix] Could not enable XR Simulation Loader. Enable manually in Project Settings > XR Plug-in Management.");
    }
}

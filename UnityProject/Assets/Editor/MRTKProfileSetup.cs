using UnityEditor;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System;
using System.Collections;
using System.Reflection;

[InitializeOnLoad]
public static class MRTKProfileSetup
{
    static MRTKProfileSetup()
    {
        EditorApplication.delayCall += EnsureMRTKProfile;
    }

    static void EnsureMRTKProfile()
    {
        string profilePath = "Assets/MRTK.Generated/MRTKProfile.asset";
        string configPath = "Assets/MRTK.Generated/MRTKSyntheticHandsConfig.asset";
        string settingsPath = "Assets/MRTK.Generated/MRTKSettings.asset";
        string folder = "Assets/MRTK.Generated";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "MRTK.Generated");
        }

        // 1. Create SyntheticHandsConfig if missing
        SyntheticHandsConfig synthConfig = AssetDatabase.LoadAssetAtPath<SyntheticHandsConfig>(configPath);
        if (synthConfig == null)
        {
            synthConfig = ScriptableObject.CreateInstance<SyntheticHandsConfig>();
            AssetDatabase.CreateAsset(synthConfig, configPath);
            Debug.Log("[MRTKProfileSetup] SyntheticHandsConfig created.");
        }

        // 2. Create MRTKProfile if missing
        MRTKProfile profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<MRTKProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        // 3. Register SyntheticHandsConfig in profile via reflection
        var field = typeof(MRTKProfile).GetField("subsystemConfigs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var dict = field.GetValue(profile) as IDictionary;
            if (dict != null)
            {
                var subsystemType = new SystemType(typeof(SyntheticHandsSubsystem));
                if (!dict.Contains(subsystemType))
                {
                    dict[subsystemType] = synthConfig;
                    EditorUtility.SetDirty(profile);
                    Debug.Log("[MRTKProfileSetup] SyntheticHandsConfig registered to MRTKProfile.");
                }
            }
        }

        AssetDatabase.SaveAssets();

        // 4. Register profile in MRTKSettings
        MRTKSettings settings = AssetDatabase.LoadAssetAtPath<MRTKSettings>(settingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MRTKSettings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
        }

        settings.SetProfileForBuildTarget(BuildTargetGroup.Standalone, profile);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        MRTKProfile.Instance = profile;
        Debug.Log("[MRTKProfileSetup] MRTK Profile fully configured.");
    }
}

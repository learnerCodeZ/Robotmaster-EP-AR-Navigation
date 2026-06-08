using UnityEditor;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System;
using System.Collections;
using System.IO;
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

        // 1. Load or create SyntheticHandsConfig (don't overwrite existing)
        SyntheticHandsConfig synthConfig = AssetDatabase.LoadAssetAtPath<SyntheticHandsConfig>(configPath);
        if (synthConfig == null)
        {
            // Try to copy from package cache default first
            string defaultPath = null;
            string[] guids = AssetDatabase.FindAssets("MRTKSyntheticHandsConfig t:ScriptableObject");
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.Contains("Default Configs") && p.Contains("MRTKSyntheticHandsConfig"))
                {
                    defaultPath = p;
                    break;
                }
            }
            if (defaultPath != null)
            {
                AssetDatabase.CopyAsset(defaultPath, configPath);
                synthConfig = AssetDatabase.LoadAssetAtPath<SyntheticHandsConfig>(configPath);
                Debug.Log("[MRTKProfileSetup] SyntheticHandsConfig copied from default.");
            }
            else
            {
                synthConfig = ScriptableObject.CreateInstance<SyntheticHandsConfig>();
                AssetDatabase.CreateAsset(synthConfig, configPath);
                Debug.Log("[MRTKProfileSetup] SyntheticHandsConfig created (no default found).");
            }
        }

        // 2. Create MRTKProfile if missing
        MRTKProfile profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<MRTKProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        // 3. Register SyntheticHandsConfig in profile via reflection
        var subsystemType = new SystemType(typeof(SyntheticHandsSubsystem));
        var field = typeof(MRTKProfile).GetField("subsystemConfigs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var dict = field.GetValue(profile) as IDictionary;
            if (dict != null)
            {
                if (!dict.Contains(subsystemType))
                {
                    dict[subsystemType] = synthConfig;
                    EditorUtility.SetDirty(profile);
                    Debug.Log("[MRTKProfileSetup] SyntheticHandsConfig registered to MRTKProfile.");
                }
            }
        }

        // 3b. Add SyntheticHandsSubsystem to loadedSubsystems
        var loadedField = typeof(MRTKProfile).GetField("loadedSubsystems", BindingFlags.NonPublic | BindingFlags.Instance);
        if (loadedField != null)
        {
            var loadedList = loadedField.GetValue(profile) as IList;
            if (loadedList != null)
            {
                bool found = false;
                foreach (var entry in loadedList)
                {
                    if (entry is SystemType st && st.Equals(subsystemType))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    loadedList.Add(subsystemType);
                    EditorUtility.SetDirty(profile);
                    Debug.Log("[MRTKProfileSetup] SyntheticHandsSubsystem added to loadedSubsystems.");
                }
            }
        }

        AssetDatabase.SaveAssets();

        // 3c. Sync GUID reference in MRTKProfile
        string configMetaPath = configPath + ".meta";
        if (File.Exists(configMetaPath))
        {
            string configGuid = null;
            foreach (string line in File.ReadAllLines(configMetaPath))
            {
                if (line.TrimStart().StartsWith("guid:"))
                {
                    configGuid = line.Split(':')[1].Trim();
                    break;
                }
            }
            if (configGuid != null)
            {
                var so = new SerializedObject(profile);
                var configs = so.FindProperty("subsystemConfigs.entries");
                for (int i = 0; i < configs.arraySize; i++)
                {
                    var keyRef = configs.GetArrayElementAtIndex(i).FindPropertyRelative("key.reference");
                    if (keyRef != null && keyRef.stringValue.Contains("SyntheticHandsSubsystem"))
                    {
                        var guidProp = configs.GetArrayElementAtIndex(i).FindPropertyRelative("value.guid");
                        if (guidProp != null && guidProp.stringValue != configGuid)
                        {
                            guidProp.stringValue = configGuid;
                            Debug.Log($"[MRTKProfileSetup] Synced SyntheticHandsConfig GUID to {configGuid}");
                        }
                    }
                }
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

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

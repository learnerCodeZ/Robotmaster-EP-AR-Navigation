using UnityEditor;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System.Collections;
using System.Reflection;

public static class MRTKProfileDebugger
{
    [MenuItem("MRTK3/Debug Profile Config")]
    public static void DebugProfile()
    {
        string profilePath = "Assets/MRTK.Generated/MRTKProfile.asset";
        var profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(profilePath);
        if (profile == null)
        {
            Debug.LogError("[MRTK Debug] MRTKProfile not found at " + profilePath);
            return;
        }

        Debug.Log("[MRTK Debug] MRTKProfile found: " + profile.name);
        Debug.Log("[MRTK Debug] Instance is null: " + (MRTKProfile.Instance == null));
        Debug.Log("[MRTK Debug] Instance == profile: " + (MRTKProfile.Instance == profile));

        // Check subsystemConfigs via reflection
        var field = typeof(MRTKProfile).GetField("subsystemConfigs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var dict = field.GetValue(profile) as IDictionary;
            if (dict != null)
            {
                Debug.Log("[MRTK Debug] subsystemConfigs count: " + dict.Count);
                foreach (DictionaryEntry entry in dict)
                {
                    var key = entry.Key;
                    var val = entry.Value;
                    Debug.Log($"[MRTK Debug]   Key: {key} (type={key.GetType().Name}), Value: {val} (type={val?.GetType().Name})");
                    if (key is SystemType st)
                    {
                        Debug.Log($"[MRTK Debug]     SystemType.ToString() = \"{st.ToString()}\"");
                        Debug.Log($"[MRTK Debug]     SystemType.GetHashCode() = {st.GetHashCode()}");
                    }
                }
            }
            else
            {
                Debug.LogError("[MRTK Debug] subsystemConfigs is null or not IDictionary");
            }
        }

        // Check loadedSubsystems
        var loadedField = typeof(MRTKProfile).GetField("loadedSubsystems", BindingFlags.NonPublic | BindingFlags.Instance);
        if (loadedField != null)
        {
            var loadedList = loadedField.GetValue(profile) as IList;
            if (loadedList != null)
            {
                Debug.Log("[MRTK Debug] loadedSubsystems count: " + loadedList.Count);
                foreach (var entry in loadedList)
                {
                    Debug.Log($"[MRTK Debug]   Loaded: {entry}");
                }
            }
        }

        // Test TryGetConfigForSubsystem
        var testType = new SystemType(typeof(SyntheticHandsSubsystem));
        Debug.Log("[MRTK Debug] Looking up SyntheticHandsSubsystem...");
        Debug.Log($"[MRTK Debug]   Search key toString: \"{testType.ToString()}\"");
        Debug.Log($"[MRTK Debug]   Search key hashCode: {testType.GetHashCode()}");

        bool found = profile.TryGetConfigForSubsystem(typeof(SyntheticHandsSubsystem), out var config);
        Debug.Log($"[MRTK Debug]   TryGetConfigForSubsystem result: {found}, config: {config}");
    }
}

using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System.Collections;
using System.Reflection;

public static class MRTKInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        var profile = MRTKProfile.Instance;

        // Load profile from Resources if not set
        if (profile == null)
        {
            var profiles = Resources.LoadAll<MRTKProfile>("");
            if (profiles.Length > 0)
            {
                profile = profiles[0];
                MRTKProfile.Instance = profile;
                Debug.Log("[MRTKInitializer] MRTKProfile loaded from Resources.");
            }
        }

        if (profile == null)
        {
            Debug.LogError("[MRTKInitializer] MRTKProfile not found!");
            return;
        }

        // Load SyntheticHandsConfig from Resources
        SyntheticHandsConfig config = null;
        var configs = Resources.LoadAll<SyntheticHandsConfig>("");
        if (configs.Length > 0) config = configs[0];

        if (config == null)
        {
            Debug.LogError("[MRTKInitializer] SyntheticHandsConfig not found in Resources!");
            return;
        }

        // Inject config into profile dictionary via reflection
        var field = typeof(MRTKProfile).GetField("subsystemConfigs",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) return;

        var dict = field.GetValue(profile) as IDictionary;
        if (dict == null) return;

        var key = new SystemType(typeof(SyntheticHandsSubsystem));
        if (!dict.Contains(key))
        {
            dict[key] = config;
            Debug.Log("[MRTKInitializer] SyntheticHandsConfig injected into MRTKProfile.");
        }
        else
        {
            var existing = dict[key];
            if (existing == null)
            {
                dict[key] = config;
                Debug.Log("[MRTKInitializer] SyntheticHandsConfig value was null - fixed.");
            }
        }
    }
}

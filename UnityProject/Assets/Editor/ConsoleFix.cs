using UnityEditor;
using MixedReality.Toolkit;

[InitializeOnLoad]
public static class ConsoleFix
{
    static ConsoleFix()
    {
        // Set BEFORE RuntimeInitializeOnLoad runs (prevents error pause on SyntheticHands)
        EditorPrefs.SetBool("DeveloperConsoleErrorPause", false);

        // Ensure MRTKProfile.Instance is set
        var profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(
            "Assets/MRTK.Generated/MRTKProfile.asset");
        if (profile != null)
        {
            MRTKProfile.Instance = profile;
        }
    }
}

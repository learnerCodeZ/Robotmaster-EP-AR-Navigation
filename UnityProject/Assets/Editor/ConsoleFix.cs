using UnityEditor;

[InitializeOnLoad]
public static class ConsoleFix
{
    static ConsoleFix()
    {
        EditorApplication.delayCall += () =>
        {
            EditorPrefs.SetBool("DeveloperConsoleErrorPause", false);
        };
    }
}

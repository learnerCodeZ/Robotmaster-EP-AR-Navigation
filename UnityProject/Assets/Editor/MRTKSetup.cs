using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class MRTKSetup
{
    private const string XR_RIG_PATH = "Library/PackageCache/org.mixedrealitytoolkit.input@719fc273754c/Assets/Prefabs/MRTK XR Rig.prefab";
    private const string INPUT_SIM_PATH = "Library/PackageCache/org.mixedrealitytoolkit.input@719fc273754c/Simulation/Prefabs/MRTKInputSimulator.prefab";

    [MenuItem("MRTK/Setup MRTK3 in Scene")]
    static void SetupMRTK()
    {
        string root = Path.GetDirectoryName(Application.dataPath).Replace('\\', '/');

        // --- MRTK XR Rig ---
        string xrRigAssetPath = Path.Combine(root, XR_RIG_PATH).Replace('\\', '/');
        GameObject xrRigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(xrRigAssetPath);
        if (xrRigPrefab == null)
        {
            EditorUtility.DisplayDialog("MRTK Setup",
                "MRTK XR Rig prefab not found at:\n" + xrRigAssetPath, "OK");
            return;
        }

        GameObject existingRig = GameObject.Find("MRTK XR Rig");
        if (existingRig != null)
        {
            if (!EditorUtility.DisplayDialog("MRTK Setup",
                "MRTK XR Rig already exists in scene.\nReplace it?", "Yes", "No"))
                return;
            Undo.DestroyObjectImmediate(existingRig);
        }

        GameObject xrRig = (GameObject)PrefabUtility.InstantiatePrefab(xrRigPrefab);
        Undo.RegisterCreatedObjectUndo(xrRig, "Setup MRTK3");

        // --- MRTKInputSimulator ---
        string simAssetPath = Path.Combine(root, INPUT_SIM_PATH).Replace('\\', '/');
        GameObject simPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(simAssetPath);
        if (simPrefab != null)
        {
            GameObject existingSim = GameObject.Find("MRTKInputSimulator");
            if (existingSim != null)
                Undo.DestroyObjectImmediate(existingSim);

            GameObject sim = (GameObject)PrefabUtility.InstantiatePrefab(simPrefab);
            Undo.RegisterCreatedObjectUndo(sim, "Setup MRTK3");
        }

        // --- Replace Main Camera ---
        GameObject oldCam = GameObject.FindWithTag("MainCamera");
        Transform newCam = xrRig.transform.Find("Camera Offset/Main Camera");
        if (oldCam != null && newCam != null && oldCam != newCam.gameObject)
        {
            Undo.DestroyObjectImmediate(oldCam);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[MRTK Setup] MRTK3 rig and input simulator added to scene.");
        EditorUtility.DisplayDialog("MRTK Setup", "MRTK3 setup complete!\n\nPress Play to test hand simulation.", "OK");
    }
}

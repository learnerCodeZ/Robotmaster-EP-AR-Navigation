using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MRTKSetup
{
    [MenuItem("MRTK/Setup MRTK3 in Scene")]
    static void SetupMRTK()
    {
        GameObject xrRigPrefab = FindPrefab("MRTK XR Rig");
        if (xrRigPrefab == null)
        {
            EditorUtility.DisplayDialog("MRTK Setup",
                "MRTK XR Rig prefab not found.\n\nMake sure MRTK3 Input package is installed.", "OK");
            return;
        }

        GameObject simPrefab = FindPrefab("MRTKInputSimulator");

        // --- MRTK XR Rig ---
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

    static GameObject FindPrefab(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("mixedrealitytoolkit") && path.Contains(name))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        return null;
    }
}

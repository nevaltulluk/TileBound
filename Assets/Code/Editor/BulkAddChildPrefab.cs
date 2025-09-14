// Assets/Editor/BulkAddChildInPlace.cs
// Opens each .prefab in a folder, adds a child prefab (once), and saves IN PLACE.
// No deletes, no re-creates, no variants. Behaves.

using UnityEditor;
using UnityEngine;
using System.IO;

public class BulkAddChildInPlace : EditorWindow
{
    private DefaultAsset sourceFolder;
    private GameObject childPrefab;

    private bool includeSubfolders = true;
    private string parentPath = "";            // e.g. "Armature/Hips" or "" for root
    private string childNameOverride = "";     // optional; else uses child prefab's name
    private bool skipIfSameNameExists = true;

    private Vector3 localPosition = Vector3.zero;
    private Vector3 localRotation = Vector3.zero;
    private Vector3 localScale    = Vector3.one;

    private bool dryRun = false;

    [MenuItem("Tools/Prefabs/Bulk: Add Child (Save In Place)")]
    private static void Open() => GetWindow<BulkAddChildInPlace>("Bulk Add Child (In-Place)");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Adds a child prefab into every .prefab under a folder, saving in place.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Source Folder", sourceFolder, typeof(DefaultAsset), false);
        childPrefab  = (GameObject)EditorGUILayout.ObjectField("Child Prefab", childPrefab, typeof(GameObject), false);

        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
        parentPath = EditorGUILayout.TextField(new GUIContent("Parent Path (optional)", "e.g. Root/Bones/Hips; blank = root"), parentPath);
        childNameOverride = EditorGUILayout.TextField(new GUIContent("Child Name Override (optional)"), childNameOverride);
        skipIfSameNameExists = EditorGUILayout.Toggle("Skip if a child with same name exists", skipIfSameNameExists);

        localPosition = EditorGUILayout.Vector3Field("Local Position", localPosition);
        localRotation = EditorGUILayout.Vector3Field("Local Rotation", localRotation);
        localScale    = EditorGUILayout.Vector3Field("Local Scale", localScale);

        EditorGUILayout.Space();
        dryRun = EditorGUILayout.Toggle("Dry Run (log only)", dryRun);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!CanRun()))
        {
            if (GUILayout.Button("Run"))
                Run();
        }
    }

    private bool CanRun() => sourceFolder && childPrefab;

    private void Run()
    {
        string srcPath = AssetDatabase.GetAssetPath(sourceFolder);
        if (!AssetDatabase.IsValidFolder(srcPath)) { Debug.LogError("Pick a valid Source Folder."); return; }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { srcPath });

        int touched = 0, modified = 0, skipped = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!includeSubfolders)
                {
                    string parent = Path.GetDirectoryName(path)?.Replace("\\","/");
                    if (parent != srcPath) continue;
                }

                touched++;

                if (dryRun)
                {
                    Debug.Log($"[DryRun] Would process: {path}");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);
                if (!root) { skipped++; continue; }

                bool changed = InjectChild(root);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path); // save same file
                    modified++;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (dryRun)
            Debug.Log($"Dry run done. Would touch {touched} prefabs.");
        else
            Debug.Log($"Done. Touched {touched} prefabs. Modified: {modified}, Skipped: {skipped}.");
    }

    // returns true if we actually added something
    private bool InjectChild(GameObject root)
    {
        // find/ensure parent
        Transform parentTr = string.IsNullOrEmpty(parentPath) ? root.transform : root.transform.Find(parentPath);
        if (!parentTr && !string.IsNullOrEmpty(parentPath) && parentPath.Contains("/"))
            parentTr = EnsureParentPath(root.transform, parentPath);
        if (!parentTr) parentTr = root.transform;

        string desiredName = string.IsNullOrEmpty(childNameOverride) ? childPrefab.name : childNameOverride;

        if (skipIfSameNameExists)
        {
            var exists = parentTr.Find(desiredName);
            if (exists != null) return false;
        }

        // instantiate into prefab contents context; fall back to Instantiate() for older versions
#if UNITY_2021_2_OR_NEWER
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(childPrefab, parentTr);
        instance.name = desiredName;
#else
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(childPrefab);
        instance.name = desiredName;
        instance.transform.SetParent(parentTr, false);
#endif
        instance.transform.localPosition = localPosition;
        instance.transform.localEulerAngles = localRotation;
        instance.transform.localScale = localScale;

        EditorUtility.SetDirty(instance);
        EditorUtility.SetDirty(root);

        return true;
    }

    private static Transform EnsureParentPath(Transform root, string path)
    {
        var parts = path.Split('/');
        Transform cur = root;
        foreach (var p in parts)
        {
            if (string.IsNullOrEmpty(p)) continue;
            var next = cur.Find(p);
            if (!next)
            {
                var go = new GameObject(p);
                go.transform.SetParent(cur, false);
                next = go.transform;
            }
            cur = next;
        }
        return cur;
    }
}

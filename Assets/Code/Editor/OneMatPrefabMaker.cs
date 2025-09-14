// File: Assets/Editor/OneMatPrefabMaker.cs
using System.IO;
using UnityEditor;
using UnityEngine;

public class OneMatPrefabMaker : EditorWindow
{
    [Header("Inputs")]
    private DefaultAsset sourceFolder;
    private DefaultAsset outputFolder;
    private Material targetMat;
    private bool includeSubfolders = true;
    private bool overwriteExisting = false;
    private bool dryRun = false;

    [MenuItem("Tools/Materials/Make Prefabs w/ One Material")]
    private static void Open() => GetWindow<OneMatPrefabMaker>("FBX → Prefab (One Mat)");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create prefab variants from FBXs in a folder and assign a single material to all renderer slots.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Source Folder (FBX)", sourceFolder, typeof(DefaultAsset), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder (Prefabs)", outputFolder, typeof(DefaultAsset), false);
        targetMat = (Material)EditorGUILayout.ObjectField("Material", targetMat, typeof(Material), false);
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Prefabs", overwriteExisting);
        dryRun = EditorGUILayout.Toggle("Dry Run (log only)", dryRun);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(sourceFolder == null || outputFolder == null || targetMat == null))
        {
            if (GUILayout.Button("Make Prefabs"))
                Run();
        }
    }

    private void Run()
    {
        string srcPath = AssetDatabase.GetAssetPath(sourceFolder);
        string outPath = AssetDatabase.GetAssetPath(outputFolder);

        if (!AssetDatabase.IsValidFolder(srcPath) || !AssetDatabase.IsValidFolder(outPath))
        {
            Debug.LogError("Pick valid project folders for Source and Output.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { srcPath });

        int made = 0, skipped = 0, slots = 0, renderers = 0;

        // Helpful for speed; we’re mass-importing/saving stuff.
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string modelPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!modelPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!includeSubfolders)
                {
                    string parent = Path.GetDirectoryName(modelPath).Replace("\\", "/");
                    if (parent != srcPath) continue;
                }

                var modelRoot = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (modelRoot == null) continue;

                // Build an output path like: OutputFolder/SubTree/Name_mat.prefab
                string relativeSub = GetRelativeSubFolder(srcPath, modelPath); // may be ""
                string outFolderForThis = CombineProjectFolders(outputFolder, relativeSub);
                EnsureFolderExists(outFolderForThis);

                string baseName = Path.GetFileNameWithoutExtension(modelPath);
                string prefabName = $"{baseName}_mat.prefab";
                string prefabPath = $"{outFolderForThis}/{prefabName}";

                if (!overwriteExisting && File.Exists(prefabPath))
                {
                    skipped++;
                    continue;
                }

                if (dryRun)
                {
                    // Count what we would touch
                    var tmp = PrefabUtility.InstantiatePrefab(modelRoot) as GameObject;
                    var (rCount, sCount) = CountAndAssign(tmp, targetMat, assign:false);
                    DestroyImmediate(tmp);
                    Debug.Log($"[DryRun] Would create {prefabPath} → renderers:{rCount}, slots:{sCount}");
                    renderers += rCount; slots += sCount; made++;
                    continue;
                }

                // Instantiate the model, assign materials, save variant as prefab.
                var instance = PrefabUtility.InstantiatePrefab(modelRoot) as GameObject;
                var (r, s) = CountAndAssign(instance, targetMat, assign:true);
                renderers += r; slots += s;

                // Save as a prefab (variant over the model prefab)
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);

                made++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (dryRun)
            Debug.Log($"Dry run done. Would create {made} prefabs, skip {skipped}, touching {renderers} renderers and {slots} slots in total.");
        else
            Debug.Log($"Done. Created/overwrote {made} prefabs, skipped {skipped}. Touched {renderers} renderers, {slots} slots. Material: {targetMat.name}");
    }

    private static (int renderers, int slots) CountAndAssign(GameObject root, Material mat, bool assign)
    {
        int r = 0, s = 0;
        foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
        {
            var mats = rend.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            r++;
            s += mats.Length;

            if (assign)
            {
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                rend.sharedMaterials = mats;
            }
        }
        return (r, s);
    }

    // e.g. src="/Assets/Models", model="/Assets/Models/Enemies/Goblin.fbx" → "Enemies"
    private static string GetRelativeSubFolder(string srcFolder, string assetPath)
    {
        var parent = Path.GetDirectoryName(assetPath).Replace("\\","/");
        if (!parent.StartsWith(srcFolder)) return "";
        string rel = parent.Substring(srcFolder.Length).TrimStart('/');
        return rel;
    }

    // Combines an Object folder and "Enemies" → "Assets/…/Output/Enemies"
    private static string CombineProjectFolders(DefaultAsset rootFolder, string sub)
    {
        string root = AssetDatabase.GetAssetPath(rootFolder).TrimEnd('/');
        if (string.IsNullOrEmpty(sub)) return root;
        return $"{root}/{sub}";
    }

    private static void EnsureFolderExists(string projectFolderPath)
    {
        // projectFolderPath like "Assets/Prefabs/Enemies"
        var parts = projectFolderPath.Split('/');
        string cur = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}

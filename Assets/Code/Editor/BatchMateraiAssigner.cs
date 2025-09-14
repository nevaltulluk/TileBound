using UnityEditor;
using UnityEngine;

public class OneMaterialForAllWindow : EditorWindow
{
    private DefaultAsset folder;
    private Material targetMat;
    private bool includeSubfolders = true;
    private bool dryRun = false;

    [MenuItem("Tools/Materials/Assign One Material To All (FBX)")]
    private static void Open()
    {
        GetWindow<OneMaterialForAllWindow>("One Material For All");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Assign one material to all renderer slots in every FBX under a folder.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);
        targetMat = (Material)EditorGUILayout.ObjectField("Material", targetMat, typeof(Material), false);
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        dryRun = EditorGUILayout.Toggle("Dry Run (log only)", dryRun);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(folder == null || targetMat == null))
        {
            if (GUILayout.Button("Assign"))
            {
                Assign();
            }
        }
    }

    private void Assign()
    {
        string folderPath = AssetDatabase.GetAssetPath(folder);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Pick a valid project folder.");
            return;
        }

        string[] searchIn = new[] { folderPath };
        string[] guids = AssetDatabase.FindAssets("t:Model", searchIn);

        int editedPrefabs = 0;
        int editedRenderers = 0;
        int editedSlots = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Limit to FBX files inside the chosen folder hierarchy unless user unchecks subfolders
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!includeSubfolders)
                {
                    string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                    if (parent != folderPath) continue;
                }

                if (dryRun)
                {
                    // just report what we'd touch
                    var temp = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!temp) continue;
                    var count = CountAffected(temp);
                    if (count.renderers > 0)
                        Debug.Log($"[DryRun] {path} -> renderers:{count.renderers}, slots:{count.slots}");
                    continue;
                }

                // Open prefab contents for editing safely
                var root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                int localRenderers = 0;
                int localSlots = 0;

                foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) continue;

                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = targetMat;

                    r.sharedMaterials = mats;

                    localRenderers++;
                    localSlots += mats.Length;
                }

                // Save back if something actually changed
                if (localRenderers > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    editedPrefabs++;
                    editedRenderers += localRenderers;
                    editedSlots += localSlots;
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
            Debug.Log("Dry run complete. Nothing was changed.");
        else
            Debug.Log($"Done. Edited {editedPrefabs} FBX prefabs, {editedRenderers} renderers, {editedSlots} material slots. Material: {targetMat.name}");
    }

    private (int renderers, int slots) CountAffected(GameObject go)
    {
        int r = 0, s = 0;
        foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
            {
                r++;
                s += renderer.sharedMaterials.Length;
            }
        }
        return (r, s);
    }
}

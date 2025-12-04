using System.Collections.Generic;
using Code;
using UnityEditor;
using UnityEngine;

public class DarkModeCleanupTool : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> cleanedObjects = new List<string>();
    private int objectsScanned;
    private int duplicatesFound;
    private int prefabsScanned;
    private int prefabsCleaned;

    [MenuItem("Tools/Dark Mode/Cleanup Duplicates")]
    private static void Open()
    {
        GetWindow<DarkModeCleanupTool>("Dark Mode Cleanup Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Dark Mode Cleanup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scans scene and prefabs for GameObjects with duplicate DarkModeImage components and removes duplicates, keeping only one.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clean Current Scene"))
        {
            CleanScene();
        }
        
        if (GUILayout.Button("Clean All Prefabs"))
        {
            CleanAllPrefabs();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (cleanedObjects.Count > 0)
        {
            EditorGUILayout.LabelField("Cleaned Objects:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var info in cleanedObjects)
            {
                EditorGUILayout.LabelField(info, EditorStyles.helpBox);
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Results: {duplicatesFound} duplicates found and cleaned out of {objectsScanned} objects scanned.", EditorStyles.helpBox);
            
            if (prefabsScanned > 0)
            {
                EditorGUILayout.LabelField($"Prefabs: {prefabsCleaned} cleaned out of {prefabsScanned} scanned.", EditorStyles.helpBox);
            }
        }
    }

    private void CleanScene()
    {
        cleanedObjects.Clear();
        objectsScanned = 0;
        duplicatesFound = 0;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("No active scene found.");
            return;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            ProcessGameObject(rootObject, "Scene");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Scene cleanup complete. Found and cleaned {duplicatesFound} duplicates out of {objectsScanned} objects scanned.");
    }

    private void CleanAllPrefabs()
    {
        cleanedObjects.Clear();
        objectsScanned = 0;
        duplicatesFound = 0;
        prefabsScanned = 0;
        prefabsCleaned = 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                prefabsScanned++;

                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabAsset == null) continue;

                GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
                if (prefabContents == null) continue;

                bool modified = false;
                foreach (Transform child in prefabContents.GetComponentsInChildren<Transform>(true))
                {
                    if (CleanDarkModeImageDuplicates(child.gameObject, path))
                    {
                        modified = true;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                    prefabsCleaned++;
                }

                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"Prefab cleanup complete. Cleaned {prefabsCleaned} prefabs out of {prefabsScanned} scanned. Found and cleaned {duplicatesFound} duplicates out of {objectsScanned} objects scanned.");
    }

    private void ProcessGameObject(GameObject go, string source)
    {
        CleanDarkModeImageDuplicates(go, source);
        
        foreach (Transform child in go.transform)
        {
            ProcessGameObject(child.gameObject, source);
        }
    }

    private bool CleanDarkModeImageDuplicates(GameObject go, string source)
    {
        objectsScanned++;

        DarkModeImage[] darkModeImages = go.GetComponents<DarkModeImage>();
        
        if (darkModeImages.Length <= 1)
        {
            return false; // No duplicates
        }

        // Found duplicates - find the component with the best data to keep
        DarkModeImage bestComponent = null;
        Sprite bestOriginalSprite = null;
        Sprite bestDarkSprite = null;

        foreach (var component in darkModeImages)
        {
            SerializedObject so = new SerializedObject(component);
            var originalSprite = so.FindProperty("originalSprite").objectReferenceValue as Sprite;
            var darkSprite = so.FindProperty("darkSprite").objectReferenceValue as Sprite;
            
            // Prefer component with data, especially dark sprite
            if (bestComponent == null || 
                (darkSprite != null && bestDarkSprite == null) ||
                (originalSprite != null && bestOriginalSprite == null))
            {
                bestComponent = component;
                bestOriginalSprite = originalSprite;
                bestDarkSprite = darkSprite;
            }
        }

        // Remove all components
        foreach (var component in darkModeImages)
        {
            DestroyImmediate(component);
        }

        // Add back one component with the best data
        DarkModeImage newComponent = go.AddComponent<DarkModeImage>();
        SerializedObject newSo = new SerializedObject(newComponent);
        newSo.FindProperty("originalSprite").objectReferenceValue = bestOriginalSprite;
        newSo.FindProperty("darkSprite").objectReferenceValue = bestDarkSprite;
        newSo.ApplyModifiedProperties();

        int duplicatesRemoved = darkModeImages.Length - 1;
        duplicatesFound += duplicatesRemoved;
        cleanedObjects.Add($"[{source}] {go.name}: Removed {duplicatesRemoved} duplicate DarkModeImage component(s), kept 1");

        // Mark object as dirty
        if (!EditorUtility.IsPersistent(go))
        {
            EditorUtility.SetDirty(go);
        }

        return true;
    }
}


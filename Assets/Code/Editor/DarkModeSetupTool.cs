using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DarkModeSetupTool : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> processedImages = new List<string>();
    private int imagesScanned;
    private int imagesProcessed;
    private int prefabsScanned;
    private int prefabsProcessed;

    [MenuItem("Tools/Dark Mode/Setup Tool")]
    private static void Open()
    {
        GetWindow<DarkModeSetupTool>("Dark Mode Setup Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Dark Mode Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scans scene and prefabs for Image components and automatically adds DarkModeImage components when dark variants are found.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan Current Scene"))
        {
            ScanScene();
        }
        
        if (GUILayout.Button("Scan All Prefabs"))
        {
            ScanAllPrefabs();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (processedImages.Count > 0)
        {
            EditorGUILayout.LabelField("Processed Images:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (var info in processedImages)
            {
                EditorGUILayout.LabelField(info, EditorStyles.helpBox);
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Results: {imagesProcessed} images processed out of {imagesScanned} scanned.", EditorStyles.helpBox);
            
            if (prefabsScanned > 0)
            {
                EditorGUILayout.LabelField($"Prefabs: {prefabsProcessed} processed out of {prefabsScanned} scanned.", EditorStyles.helpBox);
            }
        }
    }

    private void ScanScene()
    {
        processedImages.Clear();
        imagesScanned = 0;
        imagesProcessed = 0;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("No active scene found.");
            return;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (var rootObject in rootObjects)
        {
            ProcessGameObject(rootObject);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Scene scan complete. Processed {imagesProcessed} images out of {imagesScanned} scanned.");
    }

    private void ScanAllPrefabs()
    {
        processedImages.Clear();
        imagesScanned = 0;
        imagesProcessed = 0;
        prefabsScanned = 0;
        prefabsProcessed = 0;

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
                    if (ProcessImageComponent(child.gameObject, path))
                    {
                        modified = true;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                    prefabsProcessed++;
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

        Debug.Log($"Prefab scan complete. Processed {prefabsProcessed} prefabs out of {prefabsScanned} scanned. {imagesProcessed} images processed out of {imagesScanned} scanned.");
    }

    private void ProcessGameObject(GameObject go)
    {
        ProcessImageComponent(go, "Scene");
        
        foreach (Transform child in go.transform)
        {
            ProcessGameObject(child.gameObject);
        }
    }

    private bool ProcessImageComponent(GameObject go, string source)
    {
        Image image = go.GetComponent<Image>();
        if (image == null || image.sprite == null) return false;

        imagesScanned++;

        // Skip if already has DarkModeImage component
        if (go.GetComponent<DarkModeImage>() != null)
        {
            return false;
        }

        string spriteName = image.sprite.name;
        Sprite darkSprite = FindDarkSprite(spriteName);

        if (darkSprite != null)
        {
            DarkModeImage darkModeImage = go.GetComponent<DarkModeImage>();
            if (darkModeImage == null)
            {
                darkModeImage = go.AddComponent<DarkModeImage>();
            }

            // Use SerializedObject to set private serialized fields
            SerializedObject so = new SerializedObject(darkModeImage);
            so.FindProperty("originalSprite").objectReferenceValue = image.sprite;
            so.FindProperty("darkSprite").objectReferenceValue = darkSprite;
            so.ApplyModifiedProperties();

            processedImages.Add($"[{source}] {go.name}: {spriteName} → {darkSprite.name}");
            imagesProcessed++;
            
            // Mark scene object as dirty
            if (!EditorUtility.IsPersistent(go))
            {
                EditorUtility.SetDirty(go);
            }

            return true;
        }

        return false;
    }

    private Sprite FindDarkSprite(string spriteName)
    {
        // Try different naming patterns: {name}dark, {name}Dark, {name}_dark, {name}_Dark
        string[] patterns = new[]
        {
            spriteName + "dark",
            spriteName + "Dark",
            spriteName + "_dark",
            spriteName + "_Dark"
        };

        // Search in Assets folder
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            foreach (string pattern in patterns)
            {
                if (fileName.Equals(pattern, System.StringComparison.OrdinalIgnoreCase))
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        return sprite;
                    }
                }
            }
        }

        return null;
    }
}


using System.IO;
using UnityEditor;
using UnityEngine;

public class FBXFileMover : EditorWindow
{
    private string rootFolderPath = string.Empty;

    [MenuItem("Tools/FBX Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<FBXFileMover>("FBX Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Prefabs from FBX and Materials", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Root Folder"))
        {
            rootFolderPath = EditorUtility.OpenFolderPanel("Select root directory", "", "");
            if (!string.IsNullOrEmpty(rootFolderPath))
            {
                ProcessFolder(rootFolderPath);
            }
        }

        if (!string.IsNullOrEmpty(rootFolderPath))
        {
            GUILayout.Label("Selected Folder: " + rootFolderPath);
        }
    }

    private void ProcessFolder(string folderPath)
    {
        CreatePrefabs(folderPath);  // First, try to create prefabs for the current directory.

        // Then, process subdirectories recursively.
        foreach (var directory in Directory.GetDirectories(folderPath))
        {
            ProcessFolder(directory);
        }
    }

    private void CreatePrefabs(string directory)
    {
        var materials = Directory.GetFiles(directory, "*.mat");
        if (materials.Length == 0) return;

        var parentDirInfo = Directory.GetParent(directory);
        var grandParentDirInfo = parentDirInfo?.Parent;
        if (grandParentDirInfo == null)
        {
            Debug.LogError($"Could not find a grandparent directory for {directory}");
            return;
        }

        var fbxDirectory = Path.Combine(grandParentDirInfo.FullName, "3D Models");
        if (!Directory.Exists(fbxDirectory)) return;

        GameObject combinedInstance = new GameObject(grandParentDirInfo.Name);
        bool createdPrefab = false;

        foreach (var matPath in materials)
        {
            var matSuffix = GetSuffix(matPath);
            if (string.IsNullOrEmpty(matSuffix)) continue;

            var material = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPathFromAbsolutePath(matPath));

            foreach (var fbxPath in Directory.GetFiles(fbxDirectory, "*" + matSuffix + ".fbx"))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPathFromAbsolutePath(fbxPath));

                var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                instance.transform.SetParent(combinedInstance.transform, false);

                var renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer)
                {
                    renderer.material = material;
                }

                createdPrefab = true;
            }
        }

        if (createdPrefab)
        {
            var prefabPath = Path.Combine(directory, "SkinPrefab" + ".prefab");
            prefabPath = GetAssetPathFromAbsolutePath(prefabPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(combinedInstance, prefabPath, InteractionMode.AutomatedAction);
            DestroyImmediate(combinedInstance);
        }
    }

    private string GetSuffix(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (fileName.EndsWith("_A")) return "_A";
        if (fileName.EndsWith("_B")) return "_B";
        if (fileName.EndsWith("_C")) return "_C";
        return null;
    }

    private string GetAssetPathFromAbsolutePath(string absolutePath)
    {
        return "Assets" + absolutePath.Substring(Application.dataPath.Length);
    }
}
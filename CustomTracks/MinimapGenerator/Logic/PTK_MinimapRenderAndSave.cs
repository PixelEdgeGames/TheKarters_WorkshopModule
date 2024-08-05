using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_MinimapRenderAndSave : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Logic Setup")]
    public PTK_MinimapRender_CornerPosCalc minimapCornerCalc;
    public Camera renderTexCamera;
    public RenderTexture colorRenderTexture;
    public RenderTexture edgeRenderTexture;
    public UnityEngine.UI.Image generatedPreviewImage;

    public Shader renderEdgesShader;
    public GameObject models3DParent;

    [Header("Finish line")]
    public Transform finishLineTransform;
    public int iFinishLineCellSizeInPixels = 16;
    public int iFinishLineWidthCellCount = 4;

    [Header("Edit Camera Pos & Size")]
    [Range(-2,2)]
    public float fEnviroSizeInTexture = 1.0f;
    public Vector2 cameraOffsetForRender;
    [Header("Last Generated Minimap")]
    public string strLastMinimapGeneratedAssetsPath;
    public Sprite lastGeneratedMinimapSprite;
    public PTK_MinimapSO lastCreatedMinimapInfoSO;



#endif

    private void Awake()
    {
        if(Application.isPlaying)
            this.gameObject.SetActive(false);
    }


#if UNITY_EDITOR

    int iMinimapLayer = 31;
    private void OnEnable()
    {
        SetLayerRecursively(models3DParent.transform, iMinimapLayer);
    }

    private void Update()
    {
        if(Application.isPlaying == false)
        {

            // Get the current stage. Prefab Mode is a different stage from the main scene stage.
            var currentStage = UnityEditor.SceneManagement.StageUtility.GetCurrentStage();

            // Check if the current stage is a Prefab Stage, which indicates Prefab Editing Mode.
            bool isInPrefabMode = currentStage is UnityEditor.Experimental.SceneManagement.PrefabStage;

            if (isInPrefabMode == false && UnityEditor.Selection.activeGameObject != null && (UnityEditor.Selection.activeGameObject == this.gameObject || UnityEditor.Selection.activeGameObject.GetComponentInParent<PTK_MinimapRenderAndSave>() == this))
            {
                UnityEditor.Tools.visibleLayers = (1 << iMinimapLayer);
                models3DParent.gameObject.SetActive(true);
            }
            else
            {
                if (UnityEditor.Tools.visibleLayers == (1 << iMinimapLayer)) // were only minimap layer presented?
                {
                    UnityEditor.Tools.visibleLayers = -1;

                    models3DParent.gameObject.SetActive(false);
                }
            }
        }

        models3DParent.transform.localPosition = Vector3.zero;

        SetLayerRecursively(models3DParent.transform, iMinimapLayer);
        renderTexCamera.orthographicSize =800.0f + 800.0f * (1.0f-fEnviroSizeInTexture);
        renderTexCamera.transform.position = new Vector3(-cameraOffsetForRender.x, 1000,-cameraOffsetForRender.y);
    }
    void SetLayerRecursively(Transform parent, int layer)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.layer = layer;
            SetLayerRecursively(child, layer);
        }
    }

    string directoryPath = "";



    [EasyButtons.Button]
    public bool Editor_RenderMinimapAndSaveFileInChoosenDirectory()
    {
        if(finishLineTransform == null)
        {
            Debug.LogError("Please assign finish line transform!");
            return false;
        }

        renderTexCamera.targetTexture = colorRenderTexture;
        renderTexCamera.Render();

        renderTexCamera.targetTexture = edgeRenderTexture;
        renderTexCamera.RenderWithShader(renderEdgesShader,"");

        renderTexCamera.targetTexture = colorRenderTexture;


        string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        string initialDirectory =Path.GetDirectoryName(  scenePath);//strLastMinimapGeneratedAssetsPath;
        string strSelectedDirPath = UnityEditor.EditorUtility.OpenFolderPanel("Save texture as PNG", initialDirectory, "");
        if (strSelectedDirPath == "")
            return false;

        if (strSelectedDirPath.StartsWith(Application.dataPath))
        {
            // Save the path relative to Application.dataPath, excluding the Application.dataPath part itself
            strLastMinimapGeneratedAssetsPath = strSelectedDirPath.Substring(Application.dataPath.Length);

            // Trim leading path separators to ensure the path is relative
            strLastMinimapGeneratedAssetsPath = strLastMinimapGeneratedAssetsPath.TrimStart('\\', '/');
        }

        directoryPath = strSelectedDirPath;

        if (!string.IsNullOrEmpty(directoryPath))
        {
            bool bCanCreateNewMinimap = DeleteExistingMinimapFiles(directoryPath);

            if(bCanCreateNewMinimap == true)
                SaveRenderTextureToPNG( directoryPath);


            bool userClickedOK = UnityEditor.EditorUtility.DisplayDialog(
                "Minimap Rendering Complete",
                "The minimap was rendered successfully!",
                "OK"
            );

            return true;
        }


         UnityEditor.EditorUtility.DisplayDialog(
            "Minimap Rendering ERROR",
            "PATH EMPTY.",
            "OK"

        );
        return false;
    }

    void SaveRenderTextureToPNG( string directoryPath)
    {
        minimapCornerCalc.RefreshCornerPos();

        // color tex
        RenderTexture.active = colorRenderTexture;
        Texture2D texture2D = new Texture2D(colorRenderTexture.width, colorRenderTexture.height, TextureFormat.RGBA32, false,false);
        texture2D.ReadPixels(new Rect(0, 0, colorRenderTexture.width, colorRenderTexture.height), 0, 0);
        texture2D.Apply();

        // edge tex
        RenderTexture.active = edgeRenderTexture;
        Texture2D edgeTexture2D = new Texture2D(edgeRenderTexture.width, edgeRenderTexture.height, TextureFormat.RGBAFloat, false, false);
        edgeTexture2D.ReadPixels(new Rect(0, 0, edgeRenderTexture.width, edgeRenderTexture.height), 0, 0);
        edgeTexture2D.Apply();


        Color[] pixels = texture2D.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i].gamma; // Convert from Gamma to Linear
        }
        texture2D.SetPixels(pixels);
        texture2D.Apply();

        Color32 edgeColor = new Color32(176, 110, 73, 255);
        PTK_MinimapImageProcessing.DetectAndDrawEdges(texture2D, edgeTexture2D, 20, edgeColor, fHeightDiffToDetectEdge);
        PTK_MinimapImageProcessing.SmoothTransition(texture2D, edgeColor);

        PTK_MinimapImageProcessing.DrawFinishLine(texture2D, finishLineTransform, iFinishLineCellSizeInPixels, iFinishLineCellSizeInPixels * iFinishLineWidthCellCount, 2,minimapCornerCalc);

        string filename = $"minimap_blX_{minimapCornerCalc.bl_CornerWorldPos.x}_blY_{minimapCornerCalc.bl_CornerWorldPos.y}_trX_{minimapCornerCalc.tr_CornerWorldPos.x}_trY_{minimapCornerCalc.tr_CornerWorldPos.y}.png";

        // inside assets directory, lets save just as MInimap without bl, tr info
        if (directoryPath.Contains("Assets") == true)
            filename = "Minimap.png";

        string fullPathMinimap = Path.Combine(directoryPath, filename);
        byte[] pngData = texture2D.EncodeToPNG();
        File.WriteAllBytes(fullPathMinimap, pngData);
        DestroyImmediate(texture2D);

        if(fullPathMinimap.Contains("Assets")) // project dir
        {
            string strTexPathFromAssets = fullPathMinimap.Substring(fullPathMinimap.IndexOf("Assets"), fullPathMinimap.Length - fullPathMinimap.IndexOf("Assets") );
            // Refresh the AssetDatabase to include the newly saved texture
            UnityEditor.AssetDatabase.Refresh();

            // Wait for the import to finish
            UnityEditor.AssetDatabase.ImportAsset(strTexPathFromAssets, UnityEditor.ImportAssetOptions.ForceUpdate);

            // Access the texture importer of the saved texture
            UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(strTexPathFromAssets) as UnityEditor.TextureImporter;
            if (importer != null)
            {
                importer.textureType = UnityEditor.TextureImporterType.Sprite; // Set the texture to be used as a Sprite
                importer.mipmapEnabled = true; // Enable mipmaps

                // Apply the changes to the importer
                importer.SaveAndReimport();
            }

            lastGeneratedMinimapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(strTexPathFromAssets);
            generatedPreviewImage.sprite = lastGeneratedMinimapSprite;

            EditOrCreateMinimapSO(directoryPath, lastGeneratedMinimapSprite, minimapCornerCalc.bl_CornerWorldPos, minimapCornerCalc.tr_CornerWorldPos);
        }

        Debug.Log($"Minimap saved to: {fullPathMinimap}");
    }



    public float fHeightDiffToDetectEdge = 5.0f;
   

    bool DeleteExistingMinimapFiles(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath, "minimap.png");
        if (files.Length > 0)
        {
            bool deleteFiles = UnityEditor.EditorUtility.DisplayDialog(
                "Delete Existing Minimap Files",
                "Do you want to delete the current minimap files in the selected directory?",
                "Yes",
                "No"
            );

            if (deleteFiles)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                    Debug.Log($"Deleted {file}");
                }
                UnityEditor.AssetDatabase.Refresh(); // Refresh the AssetDatabase after deleting the files.
            }else
            {
                return false;
            }
        }

        return true;
    }

    public void EditOrCreateMinimapSO(string directoryPath,Sprite minimapSprite,Vector2 bottomLeft, Vector2 topRight)
    {
        // Ensure the directory path is relative to the Assets folder
        directoryPath = UnityEditor.FileUtil.GetProjectRelativePath(directoryPath);

        // Construct the path to the ScriptableObject asset
        string assetPath = Path.Combine(directoryPath, "MinimapInfoSO.asset");

        // Try to load an existing ScriptableObject from the specified path
        lastCreatedMinimapInfoSO = UnityEditor.AssetDatabase.LoadAssetAtPath<PTK_MinimapSO>(assetPath);

        if (lastCreatedMinimapInfoSO == null)
        {
            // If not found, create a new instance of the ScriptableObject
            lastCreatedMinimapInfoSO = ScriptableObject.CreateInstance<PTK_MinimapSO>();

            // Make sure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the asset
            UnityEditor.AssetDatabase.CreateAsset(lastCreatedMinimapInfoSO, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("Created new MinimapInfoSO instance.");
        }
        else
        {
            Debug.Log("MinimapInfoSO instance found and loaded for editing.");
        }

        // Example modifications (customize this part as needed)
        lastCreatedMinimapInfoSO.minimapSprite = minimapSprite; // Assign your sprite here
        lastCreatedMinimapInfoSO.bottomLeftCorner = bottomLeft; // Example vector
        lastCreatedMinimapInfoSO.topRightCorner = topRight; // Example vector

        var mapModTrack = this.GetComponentInParent<PTK_ModTrack>();
        if (mapModTrack != null)
        {
            mapModTrack.minimapInfo = lastCreatedMinimapInfoSO;
            UnityEditor.EditorUtility.SetDirty(mapModTrack);
        }

        // Save modifications
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(lastCreatedMinimapInfoSO);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

    }

#endif

    // (Vector2 bl, Vector2 tr) = ExtractVectorsFromFilename(filename);
    public static (Vector2, Vector2) ExtractVectorsFromFilename(string minimapFileName)
    {
        var match = System.Text.RegularExpressions.Regex.Match(minimapFileName, @"minimap_blX_(-?\d+\.?\d*)_blY_(-?\d+\.?\d*)_trX_(-?\d+\.?\d*)_trY_(-?\d+\.?\d*)\.png");
        if (match.Success)
        {
            float blX = float.Parse(match.Groups[1].Value);
            float blY = float.Parse(match.Groups[2].Value);
            float trX = float.Parse(match.Groups[3].Value);
            float trY = float.Parse(match.Groups[4].Value);
            return (new Vector2(blX, blY), new Vector2(trX, trY));
        }
        else
        {
            Debug.LogError("Filename format does not match the expected pattern.");
            return (Vector2.zero,Vector2.zero);
        }
    }
}

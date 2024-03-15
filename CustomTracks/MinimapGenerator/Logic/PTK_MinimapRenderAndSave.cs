using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_MinimapRenderAndSave : MonoBehaviour
{
    [Header("Logic Setup")]
    public PTK_MinimapRender_CornerPosCalc minimapCornerCalc;
    public Camera renderTexCamera;
    public GameObject models3DParent;

    [Header("Edit Camera Pos & Size")]
    [Range(0,2)]
    public float fEnviroSizeInTexture = 1.0f;
    public Vector2 cameraOffsetForRender;
    [Header("Last Generated Minimap")]
    public Sprite lastGeneratedMinimapSprite;
    private void Awake()
    {
        if(Application.isPlaying)
            this.gameObject.SetActive(false);
    }


#if UNITY_EDITOR
    private void OnEnable()
    {
        SetLayerRecursively(models3DParent.transform, 31);
    }

    private void Update()
    {
        SetLayerRecursively(models3DParent.transform, 31);
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

    [EasyButtons.Button]
    void RenderMinimapAndSaveFileInChoosenDirectory()
    {
        renderTexCamera.Render();
        string directoryPath = UnityEditor.EditorPrefs.GetString("MinimapLastSaveDir");
        directoryPath = ( UnityEditor.EditorUtility.OpenFolderPanel("Save texture as PNG", directoryPath,""));
        UnityEditor.EditorPrefs.SetString("MinimapLastSaveDir", directoryPath);


        if (!string.IsNullOrEmpty(directoryPath))
        {
            bool bCanCreateNewMinimap = DeleteExistingMinimapFiles(directoryPath);

            if(bCanCreateNewMinimap == true)
                SaveRenderTextureToPNG(renderTexCamera.targetTexture, directoryPath);
        }
    }

    void SaveRenderTextureToPNG(RenderTexture renderTexture, string directoryPath)
    {
        minimapCornerCalc.RefreshCornerPos();

        RenderTexture.active = renderTexture;
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false,false);
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        Color[] pixels = texture2D.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = pixels[i].gamma; // Convert from Gamma to Linear
        }
        texture2D.SetPixels(pixels);
        texture2D.Apply();

        Color32 edgeColor = new Color32(176, 110, 73, 255);
        DetectAndDrawEdges(texture2D, 16, edgeColor);
        SmoothTransition(texture2D, edgeColor);

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

            EditOrCreateMinimapSO(directoryPath, lastGeneratedMinimapSprite, minimapCornerCalc.bl_CornerWorldPos, minimapCornerCalc.tr_CornerWorldPos);
        }

        Debug.Log($"Minimap saved to: {fullPathMinimap}");
    }

    public static void DetectAndDrawEdges(Texture2D texture, int edgeLineWidth, Color edgeColor)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        Color[] newPixels = new Color[pixels.Length];

        // Copy original pixels to newPixels to start modifications
        System.Array.Copy(pixels, newPixels, pixels.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Find edges based on alpha value
                if (IsEdge(x, y, width, height, pixels))
                {
                    // Draw edge with specified line width and smooth transition
                    for (int dy = -edgeLineWidth; dy <= edgeLineWidth; dy++)
                    {
                        for (int dx = -edgeLineWidth; dx <= edgeLineWidth; dx++)
                        {
                            int newX = x + dx;
                            int newY = y + dy;
                            if (newX >= 0 && newY >= 0 && newX < width && newY < height)
                            {
                                // Calculate distance from the edge to determine color intensity
                                float distance = Mathf.Sqrt(dx * dx + dy * dy) / edgeLineWidth;
                                distance = Mathf.Clamp01(distance);

                                // Only blend colors where the original pixel is transparent to preserve image content
                                if (pixels[newY * width + newX].a < 1)
                                {
                                    Color originalColor = newPixels[newY * width + newX];
                                    Color blendedColor = Color.Lerp(edgeColor, originalColor, distance);

                                    newPixels[newY * width + newX] = blendedColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        texture.SetPixels(newPixels);
        texture.Apply();
    }

    private static bool IsEdge(int x, int y, int width, int height, Color[] pixels)
    {
        bool isCurrentPixelTransparent = pixels[y * width + x].a < 1;
        // Check surrounding pixels to determine if this is an edge
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            if (newX >= 0 && newY >= 0 && newX < width && newY < height)
            {
                bool isNeighborTransparent = pixels[newY * width + newX].a < 1;
                if (isCurrentPixelTransparent != isNeighborTransparent)
                {
                    return true; // Edge detected
                }
            }
        }

        return false;
    }

    public static void SmoothTransition(Texture2D texture, Color edgeColor, float tolerance = 0.1f)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        Color[] newPixels = new Color[pixels.Length];

        // Copy original pixels to start modifications
        System.Array.Copy(pixels, newPixels, pixels.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if the 4x4 block around the current pixel contains the edge color
                if (ContainsEdgeColor(x, y, width, height, pixels, edgeColor, tolerance))
                {
                    // Process a 4x4 block centered at (x, y)
                    for (int dy = -2; dy <= 1; dy++)
                    {
                        for (int dx = -2; dx <= 1; dx++)
                        {
                            int newX = x + dx;
                            int newY = y + dy;
                            if (newX >= 0 && newY >= 0 && newX < width && newY < height)
                            {
                                // Blend current pixel with its neighbors within the 4x4 block
                                Color blendedColor = BlendWithNeighbors(newX, newY, width, height, pixels, edgeColor, tolerance);
                                newPixels[newY * width + newX] = blendedColor;
                            }
                        }
                    }
                }
            }
        }

        // Apply the smoothed transitions
        texture.SetPixels(newPixels);
        texture.Apply();
    }

    private static bool ContainsEdgeColor(int x, int y, int width, int height, Color[] pixels, Color edgeColor, float tolerance)
    {
        for (int dy = -2; dy <= 1; dy++)
        {
            for (int dx = -2; dx <= 1; dx++)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX >= 0 && newY >= 0 && newX < width && newY < height)
                {
                    if (IsSimilarColor(pixels[newY * width + newX], edgeColor, tolerance))
                    {
                        return true; // Found a pixel with the edge color
                    }
                }
            }
        }
        return false;
    }

    private static bool IsSimilarColor(Color color1, Color color2, float tolerance)
    {
        return Math.Abs(color1.r - color2.r) < tolerance && Math.Abs(color1.g - color2.g) < tolerance &&
               Math.Abs(color1.b - color2.b) < tolerance && Math.Abs(color1.a - color2.a) < tolerance;
    }

    private static Color BlendWithNeighbors(int x, int y, int width, int height, Color[] pixels, Color edgeColor, float tolerance)
    {
        Color blendedColor = pixels[y * width + x];
        int count = 1; // Start with 1 to include the original pixel

        // Sum colors of the pixel and its immediate neighbors
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX >= 0 && newY >= 0 && newX < width && newY < height)
                {
                    if (IsSimilarColor(pixels[newY * width + newX], edgeColor, tolerance))
                    {
                        blendedColor += pixels[newY * width + newX];
                        count++;
                    }
                }
            }
        }

        // Average the colors if there was at least one edge color in the neighbors
        if (count > 1)
        {
            return blendedColor / count;
        }
        else
        {
            return pixels[y * width + x]; // Return the original color if no edge colors were found
        }
    }

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

    public static void EditOrCreateMinimapSO(string directoryPath,Sprite minimapSprite,Vector2 bottomLeft, Vector2 topRight)
    {
        // Ensure the directory path is relative to the Assets folder
        directoryPath = UnityEditor.FileUtil.GetProjectRelativePath(directoryPath);

        // Construct the path to the ScriptableObject asset
        string assetPath = Path.Combine(directoryPath, "MinimapInfoSO.asset");

        // Try to load an existing ScriptableObject from the specified path
        PTK_MinimapSO minimapSO = UnityEditor.AssetDatabase.LoadAssetAtPath<PTK_MinimapSO>(assetPath);

        if (minimapSO == null)
        {
            // If not found, create a new instance of the ScriptableObject
            minimapSO = ScriptableObject.CreateInstance<PTK_MinimapSO>();

            // Make sure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create the asset
            UnityEditor.AssetDatabase.CreateAsset(minimapSO, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("Created new MinimapInfoSO instance.");
        }
        else
        {
            Debug.Log("MinimapInfoSO instance found and loaded for editing.");
        }

        // Example modifications (customize this part as needed)
        minimapSO.minimapSprite = minimapSprite; // Assign your sprite here
        minimapSO.bottomLeftCorner = bottomLeft; // Example vector
        minimapSO.topRightCorner = topRight; // Example vector

        // Save modifications
        UnityEditor.EditorUtility.SetDirty(minimapSO);
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

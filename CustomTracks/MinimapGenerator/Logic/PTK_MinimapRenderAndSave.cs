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
    [Range(0,2)]
    public float fEnviroSizeInTexture = 1.0f;
    public Vector2 cameraOffsetForRender;
    [Header("Last Generated Minimap")]
    public string strLastMinimapGeneratedDirPath;
    public Sprite lastGeneratedMinimapSprite;



#endif

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

    string directoryPath = "";

    [EasyButtons.Button]
    void RenderMinimapAndSaveFileInChoosenDirectory()
    {
        renderTexCamera.targetTexture = colorRenderTexture;
        renderTexCamera.Render();

        renderTexCamera.targetTexture = edgeRenderTexture;
        renderTexCamera.RenderWithShader(renderEdgesShader,"");

        renderTexCamera.targetTexture = colorRenderTexture;

        strLastMinimapGeneratedDirPath = ( UnityEditor.EditorUtility.OpenFolderPanel("Save texture as PNG", strLastMinimapGeneratedDirPath, ""));
        directoryPath = strLastMinimapGeneratedDirPath;

        if (!string.IsNullOrEmpty(directoryPath))
        {
            bool bCanCreateNewMinimap = DeleteExistingMinimapFiles(directoryPath);

            if(bCanCreateNewMinimap == true)
                SaveRenderTextureToPNG( directoryPath);
        }
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
        DetectAndDrawEdges(texture2D, edgeTexture2D, 20, edgeColor);
        SmoothTransition(texture2D, edgeColor);

        DrawFinishLine(texture2D, finishLineTransform, iFinishLineCellSizeInPixels, iFinishLineCellSizeInPixels * iFinishLineWidthCellCount, 2);

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

    public  void DetectAndDrawEdges(Texture2D texture, Texture2D edgeTexture, int edgeLineWidth, Color edgeColor)
    {
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        Color[] newPixels = new Color[pixels.Length];

        var rawData = edgeTexture.GetRawTextureData<float>();
        float[] heightAtPixelColor = new float[edgeTexture.width * edgeTexture.height];
        int iHeightIndex = 0;

        for (int i = 0; i < rawData.Length; i += 4)
        {
            heightAtPixelColor[iHeightIndex] = rawData[i + 1];
            iHeightIndex++;
        }

        // Copy original pixels to newPixels to start modifications
        System.Array.Copy(pixels, newPixels, pixels.Length);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
               
                // Find edges based on alpha value
                //if (IsEdge(x, y, width, height, pixels))
                if (IsEdgeHeightBased(x, y, width, height, heightAtPixelColor))
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

                                float fCurPixelHeight = heightAtPixelColor[newY * width + newX];
                                float fEdgePixelHeight = heightAtPixelColor[y * width + x];
                                // Only blend colors where the original pixel is transparent to preserve image content
                                bool bCanDrawEdgeOnPixel =  fCurPixelHeight == 0; // no track data here
                                bCanDrawEdgeOnPixel = (fEdgePixelHeight - fCurPixelHeight) > fHeightDiffToDetectEdge; // edge heigth is higher then neightbour (if edge is above the track that is under then we want to draw it)
                                if (bCanDrawEdgeOnPixel == true)
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

    public void DrawFinishLine(Texture2D texture, Transform finishLineTransform, int cellSize, int finishLineWidthPixels, int iBorderLineWidth)
    {
        int finishLineHeightPixels = cellSize * 2; // Assuming 2 rows of checkers
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        Color[] newPixels = new Color[pixels.Length];

        // Copy original pixels to newPixels to start modifications
        System.Array.Copy(pixels, newPixels, pixels.Length);

        // Center of the finish line in texture space
        Vector2 center = WorldToTexture(finishLineTransform.position, texture);
        // Direction of the finish line in texture space
        Vector3 direction = finishLineTransform.forward; direction.y = 0.0f; direction.Normalize();

        // Calculate the rotation angle from the direction
        float angleRad = Vector3.Angle(direction, Vector3.forward) * Mathf.Deg2Rad;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Convert current pixel position to texture space relative to center
                Vector2 pos = new Vector2(x - center.x, y - center.y);
                // Rotate the position to align with the finish line rotation
                Vector2 rotatedPos = RotatePoint(pos, angleRad);

                // Check if the pixel falls within the finish line width and height
                if (Math.Abs(rotatedPos.x) <= finishLineWidthPixels / 2.0f && Math.Abs(rotatedPos.y) <= finishLineHeightPixels / 2.0f)
                {
                    if(rotatedPos.y  < 2 && rotatedPos.y > -2)
                    {
                        newPixels[y * width + x] = Color.black; // middle line
                        continue;
                    }

                    // Check for border
                    bool isBorder = Math.Abs(rotatedPos.x) >= (finishLineWidthPixels / 2.0f - iBorderLineWidth) || Math.Abs(rotatedPos.y) >= (finishLineHeightPixels / 2.0f - iBorderLineWidth);

                    if (isBorder)
                    {
                        newPixels[y * width + x] = Color.black; // Border color
                    }
                    else
                    {
                        // Determine the cell coordinates within the checker pattern
                        int cellX = (int)((rotatedPos.x + finishLineWidthPixels / 2.0f) / cellSize) % 2;
                        int cellY = (int)((rotatedPos.y + finishLineHeightPixels / 2.0f) / cellSize) % 2;

                        // Alternate colors based on cell coordinates
                        if ((cellX + cellY) % 2 == 0)
                        {
                            newPixels[y * width + x] = Color.white;
                        }
                        else
                        {
                            newPixels[y * width + x] = Color.black;
                        }
                    }
                }
            }
        }

        texture.SetPixels(newPixels);
        texture.Apply();
    }
    // Convert world position to texture space (Example implementation)
    private Vector2 WorldToTexture(Vector3 worldPos, Texture2D texture)
    {
        // Normalize the world position to a value between 0 and 1
        float normalizedX = (worldPos.x - minimapCornerCalc.bl_CornerWorldPos.x) / (minimapCornerCalc.tr_CornerWorldPos.x - minimapCornerCalc.bl_CornerWorldPos.x);
        float normalizedY = (worldPos.y - minimapCornerCalc.bl_CornerWorldPos.y) / (minimapCornerCalc.tr_CornerWorldPos.y - minimapCornerCalc.bl_CornerWorldPos.y);

        // Convert the normalized position to texture space
        int textureX = Mathf.FloorToInt(normalizedX * texture.width);
        int textureY = Mathf.FloorToInt(normalizedY * texture.height);

        return new Vector2(textureX, textureY);
    }


    // Rotate a point by an angle in radians
    private Vector2 RotatePoint(Vector2 point, float angle)
    {
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        return new Vector2(
            cosAngle * point.x - sinAngle * point.y,
            sinAngle * point.x + cosAngle * point.y
        );
    }

    /// <summary>
    ///  finish line end
    /// </summary>

    private  bool IsEdge(int x, int y, int width, int height, Color[] pixels)
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

    public float fHeightDiffToDetectEdge = 5.0f;
    private bool IsEdgeHeightBased(int x, int y, int width, int height, float[] heights)
    {
        float currentPixelHeight = heights[y * width + x];

        // Check surrounding pixels to determine if this is an edge
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int newX = x + dx[i];
            int newY = y + dy[i];
            if (newX >= 0 && newY >= 0 && newX < width && newY < height)
            {
                float fNeightbourHeight = heights[newY * width + newX];
                if (currentPixelHeight < 1000 && fNeightbourHeight > 0) // this pixel is outside track
                {
                    return true; // Edge detected
                }

                if ((currentPixelHeight - fNeightbourHeight) > fHeightDiffToDetectEdge) // this pixel is outside track
                {
                    return true; // Edge detected
                }
            }
        }

        return false;
    }

    public  void SmoothTransition(Texture2D texture, Color edgeColor, float tolerance = 0.1f)
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

    private  bool ContainsEdgeColor(int x, int y, int width, int height, Color[] pixels, Color edgeColor, float tolerance)
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

    private  bool IsSimilarColor(Color color1, Color color2, float tolerance)
    {
        return Math.Abs(color1.r - color2.r) < tolerance && Math.Abs(color1.g - color2.g) < tolerance &&
               Math.Abs(color1.b - color2.b) < tolerance && Math.Abs(color1.a - color2.a) < tolerance;
    }

    private  Color BlendWithNeighbors(int x, int y, int width, int height, Color[] pixels, Color edgeColor, float tolerance)
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

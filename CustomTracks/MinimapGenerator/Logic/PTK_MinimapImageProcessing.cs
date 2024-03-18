using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_MinimapImageProcessing
{
    public static void DetectAndDrawEdges(Texture2D texture, Texture2D edgeTexture, int edgeLineWidth, Color edgeColor,float fHeightDiffToDetectEdge)
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
                if (IsEdgeHeightBased(x, y, width, height, heightAtPixelColor, fHeightDiffToDetectEdge))
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
                                bool bCanDrawEdgeOnPixel = fCurPixelHeight == 0; // no track data here
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

    public static void DrawFinishLine(Texture2D texture, Transform finishLineTransform, int cellSize, int finishLineWidthPixels, int iBorderLineWidth, PTK_MinimapRender_CornerPosCalc minimapCornerCalc)
    {
        int finishLineHeightPixels = cellSize * 2; // Assuming 2 rows of checkers
        Color[] pixels = texture.GetPixels();
        int width = texture.width;
        int height = texture.height;
        Color[] newPixels = new Color[pixels.Length];

        // Copy original pixels to newPixels to start modifications
        System.Array.Copy(pixels, newPixels, pixels.Length);

        // Center of the finish line in texture space
        Vector2 center = WorldToTexture(finishLineTransform.position, texture, minimapCornerCalc);
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
                    if (rotatedPos.y < 2 && rotatedPos.y > -2)
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
    private static Vector2 WorldToTexture(Vector3 worldPos, Texture2D texture, PTK_MinimapRender_CornerPosCalc minimapCornerCalc)
    {
        // Normalize the world position to a value between 0 and 1
        float normalizedX = (worldPos.x - minimapCornerCalc.bl_CornerWorldPos.x) / (minimapCornerCalc.tr_CornerWorldPos.x - minimapCornerCalc.bl_CornerWorldPos.x);
        float normalizedY = (worldPos.z - minimapCornerCalc.bl_CornerWorldPos.y) / (minimapCornerCalc.tr_CornerWorldPos.y - minimapCornerCalc.bl_CornerWorldPos.y);

        // Convert the normalized position to texture space
        int textureX = Mathf.FloorToInt(normalizedX * texture.width);
        int textureY = Mathf.FloorToInt(normalizedY * texture.height);

        return new Vector2(textureX, textureY);
    }


    // Rotate a point by an angle in radians
    private static Vector2 RotatePoint(Vector2 point, float angle)
    {
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        return new Vector2(
            cosAngle * point.x - sinAngle * point.y,
            sinAngle * point.x + cosAngle * point.y
        );
    }

    private static bool IsEdgeHeightBased(int x, int y, int width, int height, float[] heights,float fHeightDiffToDetectEdge)
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
}

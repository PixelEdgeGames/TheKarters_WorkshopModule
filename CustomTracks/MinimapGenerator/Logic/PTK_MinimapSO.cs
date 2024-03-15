using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapInfoSO", menuName = "PixelTools/MinimapInfoSO", order = 1)]
public class PTK_MinimapSO : ScriptableObject
{
    public Sprite minimapSprite;
    public Vector2 bottomLeftCorner;
    public Vector2 topRightCorner;
   
}

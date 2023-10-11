using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModInfo", menuName = "PixelTools/ModInfo", order = 0)]
public class PTK_ModInfo : ScriptableObject
{
    public string ModName;
    public string ModAuthor;
    public List<string> SelectedPaths = new List<string>();
    public DateTime LastBuildDate;
    public int UserModVersion;
    public float GameModVersion = 1.0f;

}
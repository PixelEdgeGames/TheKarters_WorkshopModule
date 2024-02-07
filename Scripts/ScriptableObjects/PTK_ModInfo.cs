using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModInfo", menuName = "PixelTools/ModInfo", order = 0)]
public class PTK_ModInfo : ScriptableObject
{
    public string LastBuildDateTime = "";
    public string ModName;
    public string ModAuthor;
    public List<string> SelectedPaths = new List<string>();
    public int UserModVersion;
    [SerializeField]
    public const float GameModPluginVersion = 1.0f;

    public CPTK_ModContentInfoFile modContentInfo = new CPTK_ModContentInfoFile();

    [System.Serializable]
    public class CThumbForObject
    {
        public string strObjDirName = "";
        public Sprite spriteThumbnail;
    }
    public List<CThumbForObject> thumbnailsForObjects = new List<CThumbForObject>();
    public Sprite GetThumbnailForObject(string strObjDirName)
    {
        for (int i = 0; i < thumbnailsForObjects.Count; i++)
        {
            if (thumbnailsForObjects[i].strObjDirName == strObjDirName)
                return thumbnailsForObjects[i].spriteThumbnail;
        }

        return null;
    }

}
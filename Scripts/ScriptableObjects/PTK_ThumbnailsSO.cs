using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ThumbnailsSO : ScriptableObject
{
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

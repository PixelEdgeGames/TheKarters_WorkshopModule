using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PTK_CharactersInfo 
{
    [System.Serializable]
    public class CharInfo
    {
        public string strCharacterDirectoryName;
        public string strCharacterName;
        public float fCharacterHeight;
    }

    public CharInfo GetInfoForCharacterDirName(string strCharDirName)
    {
        for(int i=0;i<charInfos.Count;i++)
        {
            if (charInfos[i].strCharacterDirectoryName == strCharDirName)
                return charInfos[i];
        }

        return null;
    }
    public List<CharInfo> charInfos = new List<CharInfo>();
}

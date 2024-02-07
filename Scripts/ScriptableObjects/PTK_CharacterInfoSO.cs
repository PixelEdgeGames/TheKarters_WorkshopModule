using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModInfo", menuName = "PixelTools/CharInfoSO", order = 0)]
public class PTK_CharacterInfoSO : ScriptableObject
{
    [System.Serializable]
    public class CCharInfo
    {
        [Header("Directory Name is used for grouping characters in the game. This is only for UI!")]
        public string strCharNameForUI = "Default Name";
        public float fCharacterHeight = 1.5f;
    }

    public CCharInfo charInfo = new CCharInfo();

    public void CopyInfoTo(CPTK_ModContentInfoFile modInfo,string strCharacterDirectoryName)
    {
       var character =  modInfo.GetCharacterFromDirectoryName(strCharacterDirectoryName,true);
        character.inGameCharacterInfo.strCharNameForUI = this.charInfo.strCharNameForUI;
        character.inGameCharacterInfo.fCharacterHeight = this.charInfo.fCharacterHeight;
    }
}

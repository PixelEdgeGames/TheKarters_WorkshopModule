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

        [Header("Leave Default")]
        public Vector3 vCharOffsetPosInVehicle = Vector3.zero;
        public string strAttachItemHandBoneName_L = "Hand L";
        public string strAttachItemHandBoneName_R = "Hand R";
    }

    public CCharInfo charInfo = new CCharInfo();

    public void CopyInfoTo(CPTK_ModContentInfoFile modInfo,string strCharacterDirectoryName)
    {
       var character =  modInfo.GetCharacterFromDirectoryName(strCharacterDirectoryName,true);
        CopyInfoTo(character.inGameCharacterInfo);
      
    }

    public void CopyInfoTo(CCharInfo copyTo)
    {
        copyTo.strCharNameForUI = this.charInfo.strCharNameForUI;
        copyTo.fCharacterHeight = this.charInfo.fCharacterHeight;

        copyTo.vCharOffsetPosInVehicle = this.charInfo.vCharOffsetPosInVehicle;
        copyTo.strAttachItemHandBoneName_L = this.charInfo.strAttachItemHandBoneName_L;
        copyTo.strAttachItemHandBoneName_R = this.charInfo.strAttachItemHandBoneName_R;
    }
}

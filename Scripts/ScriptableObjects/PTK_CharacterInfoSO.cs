using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModInfo", menuName = "PixelTools/CharInfoSO", order = 0)]
public class PTK_CharacterInfoSO : ScriptableObject
{
    [Header("Directory Name is used for grouping characters in the game. This is only for UI!")]
    public string strCharNameForUI = "Default Name";
    public float fCharacterHeight = 1.5f;

    public void CopyInfoTo(PTK_CharactersInfo.CharInfo charInfo)
    {
        charInfo.strCharacterName = strCharNameForUI;
        charInfo.fCharacterHeight = fCharacterHeight;
    }
}

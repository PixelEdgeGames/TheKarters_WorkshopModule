using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CPTK_ModContentInfoFile 
{
    public List<CCharacter> characters = new List<CCharacter>();

    [System.Serializable]
    public class CCharacter
    {
        public string strCharacterDirName = "TemplateName_Name";
        public string strCharacterAnimConfigFileName = "";

        public PTK_CharacterInfoSO inGameCharacterInfo ;


        public List<CCharacterOutfit> outfits = new List<CCharacterOutfit>();

        public CCharacterOutfit GetOutfitFromName(string strName, bool bCreateIfNotFound)
        {
            foreach (var outfit in outfits)
            {
                if (outfit.strOutfitDirName == strName)
                    return outfit;
            }


            var created = new CCharacterOutfit() { strOutfitDirName = strName };
            outfits.Add(created);
            return created;
        }

        [System.Serializable]
        public class CCharacterOutfit
        {
            public string strOutfitDirName = "TemplateName_Outfit";
            public List<CCharacterOutfit_Material> materialVariants = new List<CCharacterOutfit_Material>();

            [System.Serializable]
            public class CCharacterOutfit_Material
            {
                public string strOutfitMaterialDirName = "TemplateName_Outfitmat";

                public string strPrefabFileName = "EmptyPrefabName";
            }


            public CCharacterOutfit_Material GetMatVariantFromName(string strName,bool bCreateIfNotFound)
            {
                foreach (var matVariant in materialVariants)
                {
                    if (matVariant.strOutfitMaterialDirName == strName)
                        return matVariant;
                }


                var created = new CCharacterOutfit_Material() { strOutfitMaterialDirName = strName };
                materialVariants.Add(created);
                return created;
                
            }

        }
    }

    public CCharacter GetCharacterFromDirectoryName(string strName, bool bCreateIfNotFound)
    {
        foreach(var character in characters)
        {
            if (character.strCharacterDirName == strName)
                return character;
        }

        var created = new CCharacter() { strCharacterDirName = strName };
        characters.Add(created);
        return created;
    }


    public void SaveToFile(string path)
    {
        string jsonString = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, jsonString);
    }

    public static CPTK_ModContentInfoFile LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"No file found at {path}");
        }

        string jsonString = File.ReadAllText(path);
        CPTK_ModContentInfoFile infoFile = JsonUtility.FromJson<CPTK_ModContentInfoFile>(jsonString);

        return infoFile;
    }
}

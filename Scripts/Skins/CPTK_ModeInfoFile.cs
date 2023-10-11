using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CPTK_ModeInfoFile 
{
    public string strModName;
    public string strModAuthor;
    public DateTime modLastUpdateDate;

    public List<CCharacter> characters = new List<CCharacter>();

    [System.Serializable]
    public class CCharacter
    {
        public string strCharacterName = "TemplateName_Name";
        public string strCharacterAnimConfigFileName = "";

        public List<CCharacterOutfit> outfits = new List<CCharacterOutfit>();

        public CCharacterOutfit GetOutfitFromName(string strName, bool bCreateIfNotFound)
        {
            foreach (var outfit in outfits)
            {
                if (outfit.strOutfitName == strName)
                    return outfit;
            }


            var created = new CCharacterOutfit() { strOutfitName = strName };
            outfits.Add(created);
            return created;
        }

        [System.Serializable]
        public class CCharacterOutfit
        {
            public string strOutfitName = "TemplateName_Outfit";
            public List<CCharacterOutfit_Material> materialVariants = new List<CCharacterOutfit_Material>();

            [System.Serializable]
            public class CCharacterOutfit_Material
            {
                public string strOutfitMaterialName = "TemplateName_Outfitmat";

                public string strPrefabFileName = "EmptyPrefabName";
            }


            public CCharacterOutfit_Material GetMatVariantFromName(string strName,bool bCreateIfNotFound)
            {
                foreach (var matVariant in materialVariants)
                {
                    if (matVariant.strOutfitMaterialName == strName)
                        return matVariant;
                }


                var created = new CCharacterOutfit_Material() { strOutfitMaterialName = strName };
                materialVariants.Add(created);
                return created;
                
            }

        }
    }

    public CCharacter GetCharacterFromName(string strName, bool bCreateIfNotFound)
    {
        foreach(var character in characters)
        {
            if (character.strCharacterName == strName)
                return character;
        }

        var created = new CCharacter() { strCharacterName = strName };
        characters.Add(created);
        return created;
    }


    public void SaveToFile(string path)
    {
        string jsonString = JsonUtility.ToJson(this, true);
        File.WriteAllText(path, jsonString);
    }

    public static CPTK_ModeInfoFile LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"No file found at {path}");
        }

        string jsonString = File.ReadAllText(path);
        CPTK_ModeInfoFile infoFile = JsonUtility.FromJson<CPTK_ModeInfoFile>(jsonString);

        return infoFile;
    }
}

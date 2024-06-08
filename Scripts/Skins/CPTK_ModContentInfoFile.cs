using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CPTK_ModContentInfoFile 
{
    public List<CCharacter> characters = new List<CCharacter>();
    public List<CItemWithColorVariant> vehicles = new List<CItemWithColorVariant>();
    public List<CItemWithColorVariant> wheels = new List<CItemWithColorVariant>();
    public List<CItemWithColorVariant> stickers = new List<CItemWithColorVariant>();
    public List<CTrackInfo> tracks = new List<CTrackInfo>();


    [System.Serializable]
    public class CTrackInfo
    {
        public string strTrackSceneName_AddressableKey = "";
        public string strTrackDirName = "";
        public int iLeaderboardVersion = 0;
        public int iGeneratedTargetUniqueConfigID = -1;
    }

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

                public string strPrefabFileName_AddressableKey = "EmptyPrefabName";
                public int iGeneratedTargetUniqueConfigID = -1;
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


    [System.Serializable]
    public class CItemWithColorVariant
    {
        public enum EType
        {
            E_VEHICLE,
            E_WHEEL,
            E_STICKER,

            __COUNT
        }

        public EType eItemType = EType.__COUNT;
        public string strItemDirName = "TemplateName_Name";


        public List<CItemColorVariant> colorVariants = new List<CItemColorVariant>();

        public CItemColorVariant GetColorVariantFromName(string strDirName, bool bCreateIfNotFound)
        {
            foreach (var colorVar in colorVariants)
            {
                if (colorVar.strVariantDirName == strDirName)
                    return colorVar;
            }


            var created = new CItemColorVariant() { strVariantDirName = strDirName, eItemType = eItemType };
            colorVariants.Add(created);
            return created;
        }

        [System.Serializable]
        public class CItemColorVariant
        {
            public EType eItemType = EType.__COUNT;
            public string strVariantDirName = "TemplateName_Outfitmat";
            public string strPrefabFileName_AddressableKey = "EmptyPrefabName";
            public int iGeneratedTargetUniqueConfigID = -1;
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

    public CItemWithColorVariant GetVehicleFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.strItemDirName == strDirName)
                return vehicle;
        }

        var created = new CItemWithColorVariant() { strItemDirName = strDirName ,eItemType = CItemWithColorVariant.EType.E_VEHICLE};
        vehicles.Add(created);
        return created;
    }

    public CItemWithColorVariant GetWheelFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        foreach (var wheel in wheels)
        {
            if (wheel.strItemDirName == strDirName)
                return wheel;
        }

        var created = new CItemWithColorVariant() { strItemDirName = strDirName, eItemType = CItemWithColorVariant.EType.E_WHEEL };
        wheels.Add(created);
        return created;
    }


    public CItemWithColorVariant GetStickerFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        foreach (var sticker in stickers)
        {
            if (sticker.strItemDirName == strDirName)
                return sticker;
        }

        var created = new CItemWithColorVariant() { strItemDirName = strDirName, eItemType = CItemWithColorVariant.EType.E_STICKER };
        stickers.Add(created);
        return created;
    }

    public CTrackInfo GetTrackFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        foreach (var track in tracks)
        {
            if (track.strTrackDirName == strDirName)
                return track;
        }

        var created = new CTrackInfo() { strTrackDirName = strDirName };
        tracks.Add(created);
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

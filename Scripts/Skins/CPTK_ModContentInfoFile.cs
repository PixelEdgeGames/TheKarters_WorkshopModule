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

    // Dictionary caches for O(1) lookups - not serialized
    [NonSerialized] private Dictionary<string, CCharacter> _characterCache;
    [NonSerialized] private Dictionary<string, CItemWithColorVariant> _vehicleCache;
    [NonSerialized] private Dictionary<string, CItemWithColorVariant> _wheelCache;
    [NonSerialized] private Dictionary<string, CItemWithColorVariant> _stickerCache;
    [NonSerialized] private Dictionary<string, CTrackInfo> _trackCache;

    /// <summary>
    /// Builds all lookup caches from the lists. Call this after loading/deserializing.
    /// </summary>
    public void BuildLookupCaches()
    {
        _characterCache = new Dictionary<string, CCharacter>(characters.Count);
        foreach (var character in characters)
        {
            if (!string.IsNullOrEmpty(character.strCharacterDirName))
                _characterCache[character.strCharacterDirName] = character;
        }

        _vehicleCache = new Dictionary<string, CItemWithColorVariant>(vehicles.Count);
        foreach (var vehicle in vehicles)
        {
            if (!string.IsNullOrEmpty(vehicle.strItemDirName))
                _vehicleCache[vehicle.strItemDirName] = vehicle;
        }

        _wheelCache = new Dictionary<string, CItemWithColorVariant>(wheels.Count);
        foreach (var wheel in wheels)
        {
            if (!string.IsNullOrEmpty(wheel.strItemDirName))
                _wheelCache[wheel.strItemDirName] = wheel;
        }

        _stickerCache = new Dictionary<string, CItemWithColorVariant>(stickers.Count);
        foreach (var sticker in stickers)
        {
            if (!string.IsNullOrEmpty(sticker.strItemDirName))
                _stickerCache[sticker.strItemDirName] = sticker;
        }

        _trackCache = new Dictionary<string, CTrackInfo>(tracks.Count);
        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.strTrackDirName))
                _trackCache[track.strTrackDirName] = track;
        }
    }


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
        // Use cache for O(1) lookup if available
        if (_characterCache != null && _characterCache.TryGetValue(strName, out var cached))
            return cached;

        // Fallback to linear search (for when cache not built)
        foreach (var character in characters)
        {
            if (character.strCharacterDirName == strName)
                return character;
        }

        if (!bCreateIfNotFound)
            return null;

        var created = new CCharacter() { strCharacterDirName = strName };
        characters.Add(created);
        if (_characterCache != null)
            _characterCache[strName] = created;
        return created;
    }

    public CItemWithColorVariant GetVehicleFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        // Use cache for O(1) lookup if available
        if (_vehicleCache != null && _vehicleCache.TryGetValue(strDirName, out var cached))
            return cached;

        // Fallback to linear search
        foreach (var vehicle in vehicles)
        {
            if (vehicle.strItemDirName == strDirName)
                return vehicle;
        }

        if (!bCreateIfNotFound)
            return null;

        var created = new CItemWithColorVariant() { strItemDirName = strDirName, eItemType = CItemWithColorVariant.EType.E_VEHICLE };
        vehicles.Add(created);
        if (_vehicleCache != null)
            _vehicleCache[strDirName] = created;
        return created;
    }

    public CItemWithColorVariant GetWheelFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        // Use cache for O(1) lookup if available
        if (_wheelCache != null && _wheelCache.TryGetValue(strDirName, out var cached))
            return cached;

        // Fallback to linear search
        foreach (var wheel in wheels)
        {
            if (wheel.strItemDirName == strDirName)
                return wheel;
        }

        if (!bCreateIfNotFound)
            return null;

        var created = new CItemWithColorVariant() { strItemDirName = strDirName, eItemType = CItemWithColorVariant.EType.E_WHEEL };
        wheels.Add(created);
        if (_wheelCache != null)
            _wheelCache[strDirName] = created;
        return created;
    }

    public CItemWithColorVariant GetStickerFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        // Use cache for O(1) lookup if available
        if (_stickerCache != null && _stickerCache.TryGetValue(strDirName, out var cached))
            return cached;

        // Fallback to linear search
        foreach (var sticker in stickers)
        {
            if (sticker.strItemDirName == strDirName)
                return sticker;
        }

        if (!bCreateIfNotFound)
            return null;

        var created = new CItemWithColorVariant() { strItemDirName = strDirName, eItemType = CItemWithColorVariant.EType.E_STICKER };
        stickers.Add(created);
        if (_stickerCache != null)
            _stickerCache[strDirName] = created;
        return created;
    }

    public CTrackInfo GetTrackFromDirectoryName(string strDirName, bool bCreateIfNotFound)
    {
        // Use cache for O(1) lookup if available
        if (_trackCache != null && _trackCache.TryGetValue(strDirName, out var cached))
            return cached;

        // Fallback to linear search
        foreach (var track in tracks)
        {
            if (track.strTrackDirName == strDirName)
                return track;
        }

        if (!bCreateIfNotFound)
            return null;

        var created = new CTrackInfo() { strTrackDirName = strDirName };
        tracks.Add(created);
        if (_trackCache != null)
            _trackCache[strDirName] = created;
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

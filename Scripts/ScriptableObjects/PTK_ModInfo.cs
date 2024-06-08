using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModInfo", menuName = "PixelTools/ModInfo", order = 0)]
public class PTK_ModInfo : ScriptableObject
{
    public static string strThumbScreenImageExt = ".jpg";
    public string LastBuildDateTime = "";
    public string ModName;
    [Header("Used to generate constant uqnique IDs for your items! Important to avoid conflicts with other mods.")]
    public string UniqueModNameHashToGenerateItemsKeys = "";
    public string ModAuthor = "";
    public int  iTrackLeaderboardVersion = 0;
    public bool bUploadModDescriptionToServer = true;
    public bool bUploadAndReplaceScreenshootsOnServer = true;
    public string strModDescription = "";
    public string strModTag = ""; // update!
    public string strVisibility = ""; //  "Public", "FriendsOnly", "Unlisted", "Private"
    public string strUniqueModServerUpdateKEY = ""; // update! // used to detect if we need to upload it as new mod or update our current mod
    public string strUploadHashedKey = "";

    public List<string> SelectedPaths = new List<string>();
    public float UserModVersion;
    public string strModChangelog = "";

    [SerializeField]
    public const float GameModPluginVersion = 0.1f;

    public CPTK_ModContentInfoFile modContentInfo = new CPTK_ModContentInfoFile();

    // we need to load elements from mods Addressables.LoadAssetAsync<PTK_ModInfo>(strModAddressableName + objectToLoad);
    [HideInInspector]
    public string strModAddressableName = "";

    [System.Serializable]
    public class CThumbForObject
    {
        public string strObjDirName_AddressableKey = "";
        public Sprite spriteThumbnail;
    }
    public List<CThumbForObject> thumbnailsForObjects = new List<CThumbForObject>();
    public Sprite GetThumbnailForObject(string strObjDirName)
    {
        for (int i = 0; i < thumbnailsForObjects.Count; i++)
        {
            if (thumbnailsForObjects[i].strObjDirName_AddressableKey == strObjDirName)
                return thumbnailsForObjects[i].spriteThumbnail;
        }

        return null;
    }


    public const string strUploadKey_FileName = "UploadPasswordNotShared.txt"; // do not change name! we are looking for it in the game
    public const string strNameToDecrypt_UploadPassword = "PixelReUploadProtector";// do not change name! we are looking for it in the game

}
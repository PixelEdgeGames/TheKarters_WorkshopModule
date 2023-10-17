using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Linq;
using System.Text.RegularExpressions;

public class PTK_PackageExporter : EditorWindow
{
    private SkinTreeView treeView;
    private TreeViewState treeViewState;

    private List<PTK_ModInfo> allMods = new List<PTK_ModInfo>();
    private PTK_ModInfo currentMod;
    private string newModName = "";
    private int selectedIndex = -1;
    private int iLastSelectedIndex = -1;
    private float gameModVersion = 1.0f; // Your constant game mod version.


    [MenuItem("PixelTools/PTK Package Exporter")]
    public static void ShowWindow()
    {
        GetWindow<PTK_PackageExporter>("PTK Package Exporter");
    }

    string strModSO_Path = "Assets/Workshop_Mods/LocalUserModsGenerationConfigs";

    private void OnEnable()
    {

        if (treeViewState == null)
            treeViewState = new TreeViewState();

        treeView = new SkinTreeView(treeViewState);
        treeView.LoadStateFromPrefs();  // Load state from EditorPrefs
        treeView.Reload();
        bRefreshDirectories = true;

        treeView.OnCheckedItemsChanged += HandleCheckedItemsChanged;


        RefreshMods();
    }

    void RefreshMods()
    {

        AssetDatabase.Refresh();

        allMods.Clear();

        string[] guids = AssetDatabase.FindAssets("t:PTK_ModInfo", new[] { strModSO_Path });
        foreach (var guid in guids)
        {
            PTK_ModInfo mod = AssetDatabase.LoadAssetAtPath<PTK_ModInfo>(AssetDatabase.GUIDToAssetPath(guid));

            if (mod != null)
                allMods.Add(mod);
        }
        allMods.RemoveAll(item => item == null);

        if (currentMod != null)
        {
            treeView.SetCheckedPaths(new HashSet<string>(currentMod.SelectedPaths));

            if (allMods.Contains(currentMod) == false)
            {
                if (allMods.Count > 0)
                {
                    currentMod = allMods[0];
                }
                else
                {
                    currentMod = null;
                }
                selectedIndex = 0;
            }
        }
        else if (allMods.Count > 0)
        {
            currentMod = allMods[0];
            selectedIndex = 0;
        }
        else
        {
            selectedIndex = 0;
        }
    }

    private void OnDisable()
    {
        if (treeView != null)
            treeView.SaveStateToPrefs();  // Save state when window is disabled

        treeView.OnCheckedItemsChanged -= HandleCheckedItemsChanged;
    }
    private void HandleCheckedItemsChanged(HashSet<string> checkedItems)
    {
        if (currentMod != null)
        {
            currentMod.SelectedPaths = checkedItems.ToList();
            EditorUtility.SetDirty(currentMod); // Mark the ScriptableObject as "dirty" so that changes are saved.
        }
    }

    private Vector2 scrollPosition;
    private Vector2 scrollPositionNames;

    bool bRefreshDirectories = false;

    private string GenerateUniqueModName(string baseName)
    {
        int count = 0;
        string potentialName = baseName + " " + UnityEngine.Random.Range(3445, 9999);
        while (AssetExists(potentialName))
        {
            count++;
            potentialName = baseName + " " + UnityEngine.Random.Range(3445, 9999) + "_" + count;
        }
        return potentialName;
    }

    private bool AssetExists(string name)
    {
        string assetPath = System.IO.Path.Combine(strModSO_Path, name + ".asset");
        return AssetDatabase.LoadAssetAtPath(assetPath, typeof(PTK_ModInfo)) != null;
    }

    void ModConfigGUI()
    {
        GUILayout.BeginHorizontal();
        // Dropdown for selecting a mod
        if (allMods.Count > 0)
        {
            string[] modNames = allMods.Select(mod => mod.ModName).ToArray();
            selectedIndex = EditorGUILayout.Popup("Select Mod", selectedIndex, modNames);

            if (selectedIndex >= 0)
            {
                currentMod = allMods[selectedIndex];
            }
        }else
        {
            EditorGUILayout.Popup("Select Mod", 0, new string[] { },GUILayout.Width(200));
        }

        if(iLastSelectedIndex != selectedIndex)
        {
            RefreshMods();
        }

        iLastSelectedIndex = selectedIndex;

        GUI.color = currentMod != null ? Color.red : Color.gray;
        // Delete mod button with confirmation
        if (GUILayout.Button("Delete Mod", GUILayout.Width(100)) && currentMod != null)
        {
            if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete this mod?", "Yes", "No"))
            {
                allMods.Remove(currentMod);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentMod));
                AssetDatabase.Refresh();
                currentMod = null;
                selectedIndex = -1;

                if (allMods.Count > 0)
                {
                    currentMod = allMods[0];
                    selectedIndex = 0;
                }
            }
        }
        GUI.color = Color.white;

        GUI.color = Color.yellow;
        GUILayout.Space(50);
        // Button to create a new mod
        if (GUILayout.Button("Create New Mod",GUILayout.Width(160)))
        {
            PTK_ModInfo newMod = CreateInstance<PTK_ModInfo>();
            string uniqueModName = GenerateUniqueModName("Mod ");
            newMod.ModName ="NEW " +  uniqueModName;
            AssetDatabase.CreateAsset(newMod,System.IO.Path.Combine( strModSO_Path , uniqueModName + ".asset"));
            AssetDatabase.Refresh();

            allMods.Add(newMod);
            currentMod = newMod;
            selectedIndex = allMods.Count - 1;
        }

       

        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(30);

        // Only show these fields if a mod is selected
        if (currentMod != null)
        {

            // Mod name editing
            if(currentMod != null)
            {
                currentMod.ModName = EditorGUILayout.TextField("Mod Name", currentMod.ModName);
                currentMod.ModAuthor = EditorGUILayout.TextField("Mod Author", currentMod.ModAuthor);


                // Display mod information
                EditorGUILayout.LabelField("Last Build Date:", currentMod.LastBuildDate.ToString());
                EditorGUILayout.LabelField("Paths Included:", currentMod.SelectedPaths.Count.ToString());
                currentMod.UserModVersion = EditorGUILayout.IntField("Mod Version (User)", currentMod.UserModVersion);
                EditorGUILayout.FloatField("Game Mod Version", gameModVersion);
            }
        }

        GUILayout.Space(30);
    }
    private void OnGUI()
    {
        ModConfigGUI();


        string ignorePhrases = "Ctrl+D,Outfits,Blender";
         string noCheckboxPhrases = " Color Variations";



        scrollPosition = GUILayout.BeginScrollView(scrollPosition);




        GUILayout.Space(10);
        GUILayout.Label("Selected for Export: " + "(" + treeView.GetCheckedItems().Count + ")", EditorStyles.boldLabel);
        scrollPositionNames = GUILayout.BeginScrollView(scrollPositionNames,GUILayout.Height(100));
        GUILayout.BeginVertical("box");

        int iIndex = 1;
        foreach (var dirName in treeView.GetCheckedItems())
        {
            GUILayout.Label(iIndex.ToString() +": " +  dirName); iIndex++;
        }

        for (int i = 0; i < 10 - treeView.GetCheckedItems().Count; i++)
        {
            GUILayout.Label(iIndex.ToString() + ": "); iIndex++;
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.Space(30);

        GUI.enabled = currentMod != null;
        if (GUILayout.Button("Export"))
        {
            foreach (var dirName in treeView.GetCheckedItems())
            {
                OptimizeTextureSizesInDirectory(dirName);
            }

            ExportToAddressables();
        }
        GUI.enabled = true;

        GUILayout.Space(20);

        if (GUILayout.Button("Refresh Directories") || bRefreshDirectories)
        {
            bRefreshDirectories = false;
            treeView.SetIgnorePhrases(ignorePhrases.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            treeView.SetNoCheckboxPhrases(noCheckboxPhrases.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            treeView.Reload();
        }

        GUILayout.Space(30);
        // Draw tree view inside a flexible space so it takes up the rest of the scroll view
        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        Rect treeViewRect = GUILayoutUtility.GetRect(0, position.height, GUILayout.ExpandHeight(true));
        treeView.OnGUI(treeViewRect);
        GUILayout.EndVertical();

        GUILayout.EndScrollView();
    }

    public static void OptimizeTextureSizesInDirectory(string directoryPath)
    {
        // Ensure the directory path starts with "Assets"
        if (!directoryPath.StartsWith("Assets"))
        {
            Debug.LogError("The directory path should start with 'Assets'.");
            return;
        }

        // Get all the material paths in the directory
        string[] materialPaths = AssetDatabase.FindAssets("t:Material", new string[] { directoryPath });

        for (int i = 0; i < materialPaths.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialPaths[i]);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.HasProperty("_MainTex")) // Assuming "_MainTex" is the property name for diffuse textures
            {
                Texture2D diffuseTexture = mat.GetTexture("_MainTex") as Texture2D;
                if (diffuseTexture)
                {
                    SetTextureMaxSize(AssetDatabase.GetAssetPath(diffuseTexture), 2048);
                }
            }

            // Iterate through other textures in the material
            foreach (string texturePropertyName in mat.GetTexturePropertyNames())
            {
                if (texturePropertyName != "_MainTex")
                {
                    Texture2D texture = mat.GetTexture(texturePropertyName) as Texture2D;
                    if (texture)
                    {
                        SetTextureMaxSize(AssetDatabase.GetAssetPath(texture), 1024);
                    }
                }
            }

            // Display progress bar
            float progress = (float)i / materialPaths.Length;
            if (EditorUtility.DisplayCancelableProgressBar("Optimizing Textures", path, progress))
            {
                break; // Stop the operation if the user cancels the progress
            }
        }

        EditorUtility.ClearProgressBar();
    }

    private static void SetTextureMaxSize(string texturePath, int maxSize)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter && textureImporter.maxTextureSize != maxSize)
        {
            textureImporter.maxTextureSize = maxSize;
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        }
    }

    ////
    /// Addressables
    ///

    CPTK_ModInfoFile modInfoFile = null;
    private void ExportToAddressables()
    {
        if(currentMod == null)
        {
            return;
        }

        // Reference to the AddressableAssetSettings
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogError("Failed to access AddressableAssetSettings.");
            return;
        }

        // Clear all groups
        var allGroups = new List<AddressableAssetGroup>(settings.groups);
        foreach (var group in allGroups)
        {
            settings.RemoveGroup(group);
        }

        Dictionary<AddressableAssetEntry, AddressableAssetGroup> entries = new Dictionary<AddressableAssetEntry, AddressableAssetGroup>();

        modInfoFile = new CPTK_ModInfoFile();

        modInfoFile.strModName = currentMod.ModName;
        modInfoFile.strModAuthor = currentMod.ModAuthor;
        modInfoFile.modLastUpdateDate = DateTime.Now;

        string strUserCOnfigured_ModName = currentMod.ModName;
        string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
        string buildPathModDir = System.IO.Path.Combine(projectPath, "Mods", strUserCOnfigured_ModName);
        string buildPath = System.IO.Path.Combine(buildPathModDir, "[BuildTarget]");
        foreach (var dirName in treeView.GetCheckedItems())
        {
            // Create a group for the directory
            // Create a group for the directory
            var groupName = Path.GetFileNameWithoutExtension(dirName);  // Assuming you want the directory name as the group name
            var newGroup = settings.FindGroup(groupName);
            if (newGroup == null)
            {
                newGroup = settings.CreateGroup(groupName, false, false, true, new List<AddressableAssetGroupSchema>());

                // Attach necessary schemas
                var bundleSchema = newGroup.AddSchema<BundledAssetGroupSchema>();
                var contentUpdateSchema = newGroup.AddSchema<ContentUpdateGroupSchema>();

                // Set some default values for the schemas if required
                //  bundleSchema.BuildPath.SetVariableByName(settings, "LocalBuildPath");
                //    bundleSchema.LoadPath.SetVariableByName(settings, "LocalLoadPath");
                string buildPathVariableName = "CustomBuildPath";
                string loadPathVariableName = "CustomLoadPath";

               

                string strDirectoryOfMod = System.IO.Path.Combine(Application.dataPath, "..", "Mods", strUserCOnfigured_ModName, EditorUserBuildSettings.selectedStandaloneTarget.ToString());
                if(System.IO.Directory.Exists(strDirectoryOfMod))
                    System.IO.Directory.Delete(strDirectoryOfMod ,true);

                settings.profileSettings.CreateValue(buildPathVariableName, buildPath);
                settings.profileSettings.SetValue(settings.activeProfileId, buildPathVariableName, buildPath);

                // string loadPath = "file://./Mods/{LOCAL_FILE_NAME}/[BuildTarget]";
                //string loadPath = "file://./Mods/"+ strUserCOnfigured_ModName  + "/[BuildTarget]";
                //string loadPath = "../Mods/" + strUserCOnfigured_ModName + "/[BuildTarget]";
                //string loadPath = "file://{DATA_PATH}/Mods/" + strUserCOnfigured_ModName + "/[BuildTarget]";

                string strUserConfiguredModName = strUserCOnfigured_ModName;  // Replace with your actual mod name
                                                                              //   string loadPath = $"file://{{DATA_PATH}}\\Mods\\{strUserConfiguredModName}\\[BuildTarget]";
                string loadPath = "file://{DATA_PATH}\\Mods\\"+ "{MOD_NAME}" +"\\[BuildTarget]";

                settings.profileSettings.CreateValue(loadPathVariableName, loadPath);
                settings.profileSettings.SetValue(settings.activeProfileId, loadPathVariableName, loadPath);

                settings.RemoteCatalogBuildPath.SetVariableByName(settings, buildPathVariableName);
                settings.RemoteCatalogLoadPath.SetVariableByName(settings, loadPathVariableName);


                bundleSchema.BuildPath.SetVariableByName(settings, buildPathVariableName);
                bundleSchema.LoadPath.SetVariableByName(settings, loadPathVariableName);

                bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                bundleSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
                // Add more default settings if needed
            }


            // Fetch all asset paths from the directory
            string[] assetPathsInDir = GetAssetsInDirectoryNonRecursive(dirName);

            foreach (string assetPath in assetPathsInDir)
            {
                var fullPath = assetPath;

                // If it's a directory, skip this iteration.
                if (AssetDatabase.IsValidFolder(fullPath))
                {
                  //  continue;
                }

                // Check if the asset is a .fbx and resides inside 'Color Variants' directory or its subdirectories
                if (fullPath.EndsWith(".fbx") && IsInsideColorVariantsDirectory(fullPath))
                {
                    continue; // Skip this asset and move to the next one
                }

                var guid = AssetDatabase.AssetPathToGUID(fullPath);
                var entry = settings.CreateOrMoveEntry(guid, newGroup);

                string strAddressFileKey = ConstructName(fullPath, groupName);
                entry.SetAddress(strAddressFileKey, false);

                UpdateModFileInfo(fullPath, groupName, strAddressFileKey);

                entries[entry] = newGroup;
            }
        }

        SimplifyAddresses(entries);

        modInfoFile.SaveToFile(buildPathModDir + "modInfoFile.json");

        EditorUtility.SetDirty(settings); 
        AssetDatabase.SaveAssets();
        treeView.SaveStateToPrefs();  // Save state after changes
    }

    private  Regex Pattern_Character = new Regex(@"Skins_Workshop/Characters/(?<characterName>[^/]+)/Outfits/(?<outfit>[^/]+)/Color Variations/(?<materialVar>[^/]+)");
    private static readonly Regex Pattern_AnimConfig = new Regex(@"Skins_Workshop/Characters/(?<characterName>[^/]+)");

    void UpdateModFileInfo(string strFullPath, string strGroupName, string strFileKey)
    {
        if(strFullPath.Contains("PTK_Workshop_Char Anim Config") == true)
        {
            var match = Pattern_AnimConfig.Match(strFullPath);

            if (match.Success)
            {
                string strCharacterName = match.Groups["characterName"].Value;
                modInfoFile.GetCharacterFromName(strCharacterName, true).strCharacterAnimConfigFileName = strFileKey;
            }
            else
            {
                Debug.LogError("Cant match PTK_Workshop_Char Anim Config file name!");
            }
        }
        else if (strFullPath.Contains("Skins_Workshop/Characters"))
        {
            var match = Pattern_Character.Match(strFullPath);

            if(match.Success == true)
            {
                string strCharacterName = match.Groups["characterName"].Value;
                string strCharacterOutfit = match.Groups["outfit"].Value;
                string strMaterialVar = match.Groups["materialVar"].Value;

                modInfoFile.GetCharacterFromName(strCharacterName, true).GetOutfitFromName(strCharacterOutfit, true).GetMatVariantFromName(strMaterialVar, true).strPrefabFileName = strFileKey;
            }
        }
    }
    public static string ConstructName(string assetPath,string strGroupName)
    {

        string[] forbiddenPhrases = { "Color Variations", "Assets","Workshop" };

        // Split the path into parts.
        string[] parts = assetPath.Split(new char[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Filter out parts that contain any forbidden phrases.
        parts = parts.Where(part => !forbiddenPhrases.Any(phrase => part.Contains(phrase))).ToArray();
       
        return string.Join("_", parts).Replace(" ", "");
    }

   

    private string[] GetAssetsInDirectoryNonRecursive(string directory)
    {
        // 1. Get all asset paths in the directory (this includes subdirectories)
        string[] allAssetPaths = AssetDatabase.FindAssets("", new[] { directory })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        string strTargetDirPath = (Application.dataPath + directory.Substring("Assets".Length, directory.Length - "Assets".Length));
        strTargetDirPath = strTargetDirPath.Replace("\\", "/");
        // 2. Filter out the directories and assets in subdirectories
        List<string> filteredAssets = new List<string>();
        foreach (string assetPath in allAssetPaths)
        {
            FileInfo info = new FileInfo(Application.dataPath + assetPath.Substring("Assets".Length, assetPath.Length - "Assets".Length));

            string strDirFullName = info.Directory.FullName.Replace('\\', '/');
            bool bOnlyThisDirectory = false;
            if (info.Exists  )
            {
                if (bOnlyThisDirectory == true && strDirFullName != strTargetDirPath)
                    continue;

                if (assetPath.ToLower().Contains("ctrl+d"))
                    continue;

                if(assetPath.Contains(".prefab") || assetPath.Contains("PTK_Workshop_Char") || (assetPath.Contains("Blender") == false && assetPath.Contains(".fbx") == true))
                    filteredAssets.Add(assetPath);
            }
            
        }

        return filteredAssets.ToArray();
    }

    public static void SimplifyAddresses(Dictionary<AddressableAssetEntry, AddressableAssetGroup> entries)
    {
        foreach (var group in entries)
            group.Value.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, group.Key, false, true);
        AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, true, false);
    }



    private bool IsInsideColorVariantsDirectory(string assetPath)
    {
        var directories = assetPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < directories.Length; i++)
        {
            if (directories[i] == "Color Variations")
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ///// Tree View
    /// </summary>
    class SkinTreeView : TreeView
    {
        public event Action<HashSet<string>> OnCheckedItemsChanged;
        private HashSet<string> ignoreSet = new HashSet<string>();

        public void SetIgnorePhrases(string[] phrases)
        {
            ignoreSet.Clear();
            foreach (var phrase in phrases)
            {
                ignoreSet.Add(phrase.Trim());
            }
        }
        private HashSet<string> noCheckboxSet = new HashSet<string>();

        public void SetNoCheckboxPhrases(string[] phrases)
        {
            noCheckboxSet.Clear();
            foreach (var phrase in phrases)
            {
                noCheckboxSet.Add(phrase.Trim());
            }
        }

        private const string rootPath = "Assets/Workshop_Mods/Skins_Workshop";

        public SkinTreeView(TreeViewState state) : base(state)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            int id = 1;
            var items = CreateChildrenForDirectory(rootPath, ref id);
            SetupParentsAndChildrenFromDepths(root, items);
            return root;
        }
        private List<TreeViewItem> CreateChildrenForDirectory(string path, ref int id, int depth = 0)
        {
            var items = new List<TreeViewItem>();

            // Skip directories that match ignore phrases
            if (ShouldIgnore(path))
                return items;

            // Add current directory (skip for the root)
            if (depth > 0)
                items.Add(new MyTreeViewItem { id = id++, depth = depth, displayName = Path.GetFileName(path), pathSegment = Path.GetFileName(path), fullPath = path });

            var subDirs = Directory.GetDirectories(path);
            foreach (var dir in subDirs)
            {
                items.AddRange(CreateChildrenForDirectory(dir, ref id, depth + 1));
            }

            return items;
        }

        private bool ShouldIgnore(string path)
        {
            foreach (var ignore in ignoreSet)
            {
                if (path.Contains(ignore))
                    return true;
            }
            return false;
        }
        HashSet<string> checkedItems_ = new HashSet<string>();
        public HashSet<string> GetCheckedItems()
        {
            return checkedItems_;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            MyTreeViewItem myItem = args.item as MyTreeViewItem;
            if (!ShouldNotRenderCheckbox(myItem.displayName) && ShouldRenderCheckbox_IfParent(myItem))
            {
                EditorGUI.BeginChangeCheck();
                bool wasChecked = checkedItems_.Contains(myItem.fullPath);
                bool isChecked = EditorGUI.Toggle(new Rect(args.rowRect.x + 2, args.rowRect.y, 16, args.rowRect.height), wasChecked);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isChecked && !wasChecked)
                    {
                        checkedItems_.Add(myItem.fullPath);
                    }
                    else if (!isChecked && wasChecked)
                    {
                        checkedItems_.Remove(myItem.fullPath);
                    }

                    OnCheckedItemsChanged?.Invoke(checkedItems_);
                }
            }

            base.RowGUI(args);


        }
        private bool ShouldRenderCheckbox_IfParent(MyTreeViewItem item)
        {
            if (item == null)
                return false;

           // if ((item as MyTreeViewItem).pathSegment == "Characters")
            //    return true;

            if (item.parent as MyTreeViewItem == null)
                return false;

          //  if ((item.parent as MyTreeViewItem).pathSegment == "Outfits")
        //        return true;

            if ((item.parent as MyTreeViewItem).pathSegment == "Characters")
                return true;


            return false;
        }

        public HashSet<string> GetCheckedPaths()
        {
            return checkedItems_;
        }

        public void SetCheckedPaths(HashSet<string> paths)
        {
            checkedItems_.Clear();
            foreach (var path in paths)
            {
                checkedItems_.Add(path);
            }
        }

        public void SaveStateToPrefs()
        {
            string checkedPaths = string.Join(";", checkedItems_);
            EditorPrefs.SetString("SkinTreeView_Checked_Items", checkedPaths);
        }

        public void LoadStateFromPrefs()
        {
            if (EditorPrefs.HasKey("SkinTreeView_Checked_Items"))
            {
                var savedPaths = EditorPrefs.GetString("SkinTreeView_Checked_Items").Split(';');
                checkedItems_ = new HashSet<string>(savedPaths);
            }
        }

        private class MyTreeViewItem : TreeViewItem
        {
            public string pathSegment;
            public string fullPath; // New field
        }

        private bool ShouldNotRenderCheckbox(string path)
        {
            foreach (var noCheckbox in noCheckboxSet)
            {
                if (path.Contains(noCheckbox))
                    return true;
            }
            return false;
        }

    }
 
}




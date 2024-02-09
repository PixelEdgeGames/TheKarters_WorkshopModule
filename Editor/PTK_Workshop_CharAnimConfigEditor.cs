#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_Workshop_CharAnimConfig))]
public class PTK_Workshop_CharAnimConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PTK_Workshop_CharAnimConfig config = (PTK_Workshop_CharAnimConfig)target;

        if(config.strLastInitializationTime != "")
        {
            GUILayout.Label("lastInitTime: " + config.strLastInitializationTime + " items count: " + config.iLastInitializationElementsCount + "");
        }
        if (GUILayout.Button("Initialize From Directory " ))
        {
            InitializeFromDirectory(config);
        }

        DrawDefaultInspector();

    }


    static void MakeAnimationLooping(AnimationClip clip)
    {
        if (clip != null)
        {
            string strClipPath = AssetDatabase.GetAssetPath(clip); ;

            // Get the current animation clip settings
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);

            // already set
            if (settings.loopTime == true)
                return;

            // Set the loop time to true
            settings.loopTime = true;

            // Apply the modified settings to the clip
            ModelImporter importer = AssetImporter.GetAtPath(strClipPath) as ModelImporter;
            if (importer != null)
            {
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }
            else // animation clip was extracted
            {
                // Retrieve the current settings of the clip

                // Modify settings
                settings.loopTime = true;

                // Apply modified settings back to the clip
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                // Mark clip as dirty to ensure changes are saved
                EditorUtility.SetDirty(clip);

            }
        }
        else
        {
        }
    }
    public static void InitializeFromDirectory(PTK_Workshop_CharAnimConfig config)
    {
        config.iLastInitializationElementsCount = 0;
        config.strLastInitializationTime = System.DateTime.Now.ToLongTimeString();

        string path = AssetDatabase.GetAssetPath(config);
        string directory = System.IO.Path.GetDirectoryName(path);

        string[] assetGUIDs = AssetDatabase.FindAssets("t:AnimationClip", new string[] { directory });

        config.CharacterA = new PTK_Workshop_CharAnimConfig.AnimationCategory();
        config.CharacterB = new PTK_Workshop_CharAnimConfig.AnimationCategory();
        config.CharacterC = new PTK_Workshop_CharAnimConfig.AnimationCategory();

        foreach (string guid in assetGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip obj = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

            if (obj)
            {
                AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                    .Where(asset => asset is AnimationClip)
                    .Cast<AnimationClip>().ToArray();


                string animCategory =new System.IO.DirectoryInfo(Application.dataPath + System.IO.Path.DirectorySeparatorChar +  System.IO.Path.GetDirectoryName(assetPath)).Name;

               string strFileName = System.IO.Path.GetFileName(assetPath);

                PTK_Workshop_CharAnimConfig.AnimationCategory taregtChar = config.CharacterA;

                if(strFileName.EndsWith("_B.fbx"))
                    taregtChar = config.CharacterB;
                else if (strFileName.EndsWith("_C.fbx"))
                    taregtChar = config.CharacterC;

                foreach (AnimationClip clip in clips)
                {
                    if (clip.name.Contains("_preview") == true)
                        continue;

                    if (clip.name.ToLower().Contains("idle") == true || clip.name.ToLower().Contains("loop") == true)
                        MakeAnimationLooping(clip);

                    if (animCategory == "Driving")
                        taregtChar.Driving.Add(clip);
                    else if (animCategory == "Events")
                        taregtChar.Events.Add(clip);
                    else if (animCategory == "Menu")
                        taregtChar.Menu.Add(clip);
                    else if (animCategory == "ItemsModelAnim")
                        taregtChar.ItemsModelAnim.Add(clip);
                    else if (animCategory == "JumpTricks")
                    {
                        if(clip.name.ToLower().Contains("super_long"))
                            taregtChar.JumpTricks_SuperLong.Add(clip);
                        else
                            taregtChar.JumpTricks_NormalShort.Add(clip);
                    }
                    else if (animCategory == "ItemUsage")
                        taregtChar.ItemUsage.Add(clip);
                    else if (animCategory == "WeaponTargetting")
                        taregtChar.WeaponTargeting.Add(clip);
                    else
                        Debug.LogError("Unknown Category " + animCategory);

                    config.iLastInitializationElementsCount++;
                }
            }
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
}
#endif
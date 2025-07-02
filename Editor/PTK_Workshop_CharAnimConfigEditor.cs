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
        if (clip == null)
            return;

        string clipPath = AssetDatabase.GetAssetPath(clip);

        // Check if this is part of a model import
        ModelImporter importer = AssetImporter.GetAtPath(clipPath) as ModelImporter;
        if (importer != null)
        {
            // Get the name of this specific clip
            string clipName = clip.name;

            // Get all clip animations from the importer
            ModelImporterClipAnimation[] clipAnimations = importer.clipAnimations;
            if (clipAnimations == null || clipAnimations.Length == 0)
            {
                // If no clipAnimations defined yet, get from default clips
                clipAnimations = importer.defaultClipAnimations;
            }

            bool foundClip = false;
            bool alreadyLooping = false;

            // Find our specific clip and check if it's already looping
            for (int i = 0; i < clipAnimations.Length; i++)
            {
                if (clipAnimations[i].name == clipName)
                {
                    foundClip = true;
                    alreadyLooping = clipAnimations[i].loopTime;

                    // Skip if already looping
                    if (alreadyLooping)
                        return;

                    clipAnimations[i].loopTime = true;
                    break;
                }
            }

            if (foundClip && !alreadyLooping)
            {
                // Apply changes back to the importer
                importer.clipAnimations = clipAnimations;
                importer.SaveAndReimport();
            }
        }
        else
        {
            // This is a directly editable animation asset
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);

            // Skip if already looping
            if (settings.loopTime)
                return;

            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
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
                    else if (animCategory == "ItemsCommonAnims")
                        taregtChar.ItemsModelAnim_Common.Add(clip);
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
                    else if (animCategory == "WeaponTargetting" || animCategory == "WeaponTargeting")
                        taregtChar.WeaponTargeting.Add(clip);
                    else
                        Debug.LogError("Unknown Category " + animCategory);

                    config.iLastInitializationElementsCount++;
                }
            }
        }


        if (config.CharacterA.ItemsModelAnim_Common.Count == 0)
        {
            Debug.LogError("Item Common animations are empty! __PUT_Blender_ANIM_Export_here/ItemsCommonAnims directory should exist by defeault. If this directory does not exist please copy it from other character and do not remove it in the future.");
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }
}
#endif
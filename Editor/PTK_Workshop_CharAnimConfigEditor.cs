#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_Workshop_CharAnimConfig))]
public class PTK_Workshop_CharAnimConfigEditor : Editor
{
    string strLastInitializationTime = "";
    int iLastInitializationElementsCount = 0;
    public override void OnInspectorGUI()
    {
        PTK_Workshop_CharAnimConfig config = (PTK_Workshop_CharAnimConfig)target;

        if(strLastInitializationTime != "")
        {
            GUILayout.Label("lastInitTime: " + strLastInitializationTime + " items count: " + iLastInitializationElementsCount + "");
        }
        if (GUILayout.Button("Initialize From Directory " ))
        {
            iLastInitializationElementsCount = 0;
            strLastInitializationTime = System.DateTime.Now.ToLongTimeString();
            InitializeFromDirectory(config);
        }

        DrawDefaultInspector();

    }

    private void InitializeFromDirectory(PTK_Workshop_CharAnimConfig config)
    {
        string path = AssetDatabase.GetAssetPath(config);
        string directory = System.IO.Path.GetDirectoryName(path);

        string[] assetGUIDs = AssetDatabase.FindAssets("t:Model", new string[] { directory });

        config.CharacterA = new PTK_Workshop_CharAnimConfig.AnimationCategory();
        config.CharacterB = new PTK_Workshop_CharAnimConfig.AnimationCategory();
        config.CharacterC = new PTK_Workshop_CharAnimConfig.AnimationCategory();

        foreach (string guid in assetGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

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

                    if (animCategory == "Driving")
                        taregtChar.Driving.Add(clip);
                    else if (animCategory == "Events")
                        taregtChar.Events.Add(clip);
                    else if (animCategory == "Menu")
                        taregtChar.Menu.Add(clip);
                    else if (animCategory == "ItemsModelAnim")
                        taregtChar.ItemsModelAnim.Add(clip);
                    else if (animCategory == "JumpTricks")
                        taregtChar.JumpTricks.Add(clip);
                    else if (animCategory == "ItemUsage")
                        taregtChar.ItemUsage.Add(clip);
                    else if (animCategory == "WeaponTargetting")
                        taregtChar.WeaponTargeting.Add(clip);
                    else
                        Debug.LogError("Unknown Category " + animCategory);

                    iLastInitializationElementsCount++;
                }
            }
        }

        EditorUtility.SetDirty(config);
    }
}
#endif
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_ModCommandsEditorHelper))]
public class PTK_ModCommandsEditorHelperEditor : Editor
{
    // A list of available command types, populate these with the actual command script types.
    private Type[] commandTypes = new Type[]
    {
        typeof(PTK_Command_00_TriggerEvents_EnableDisable),
        typeof(PTK_Command_01_GameObjects_EnableDisable),
        typeof(PTK_Command_02_ModTriggerCommandExecutor_ManualReset),
        typeof(PTK_Command_03_ModTriggerCommandExecutor_EnableDisable),
        typeof(PTK_Command_04_AnimationClip_PlayPauseStop),
        typeof(PTK_Command_05_PlayerLogicEffects),
        typeof(PTK_Command_06_CustomCommands),
        // Add more command types here as needed
    };
    
    public override void OnInspectorGUI()
    {
        // Reference to the target object (the script being inspected)
        PTK_ModCommandsEditorHelper helper = (PTK_ModCommandsEditorHelper)target;

        EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

        foreach (Type commandType in commandTypes)
        {
            if (commandType == null) continue;

            // Check if a component of this type is already attached
            Component existingComponent = helper.GetComponent(commandType);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(commandType.Name);

            if (existingComponent != null)
            {
                // Show "Remove" button if the component exists
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove"))
                {
                    DestroyImmediate(existingComponent);
                    EditorUtility.SetDirty(helper.gameObject);
                }
            }
            else
            {
                // Show "Add" button if the component does not exist
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add"))
                {
                    helper.gameObject.AddComponent(commandType);
                    EditorUtility.SetDirty(helper.gameObject);
                }
            }



            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

     

        // Apply any modifications made to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}

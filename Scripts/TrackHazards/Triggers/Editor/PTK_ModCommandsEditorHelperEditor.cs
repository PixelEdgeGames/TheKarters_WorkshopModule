using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_ModCommandsEditorHelper))]
public class PTK_ModCommandsEditorHelperEditor : Editor
{
    // A list of available command types, populate these with the actual command script types.
    // Dictionary to map command types to custom display names
    private Dictionary<Type, string> commandTypesWithNames = new Dictionary<Type, string>
    {
        { typeof(PTK_Command_01_GameObjects_EnableDisable), "Game Objects: Enable/Disable" },
        { typeof(PTK_Command_00_TriggerEvents_EnableDisable), "Trigger Events: Enable/Disable" },
        { typeof(PTK_Command_02_ModTriggerCommandExecutor_ManualReset), "Trigger Command Executor: Manual Reset" },
        { typeof(PTK_Command_03_ModTriggerCommandExecutor_EnableDisable), "Trigger Command Executor: Enable/Disable" },
        { typeof(PTK_Command_07_AnimatorCommands), "Animator Commands" },
        { typeof(PTK_Command_05_PlayerLogicEffects), "Player Logic Effects" },
        { typeof(PTK_Command_06_CustomCommands), "Custom Commands" }
    };
    public override void OnInspectorGUI()
    {
        // Reference to the target object (the script being inspected)
        PTK_ModCommandsEditorHelper helper = (PTK_ModCommandsEditorHelper)target;

        EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

        foreach (KeyValuePair<Type, string> kvp in commandTypesWithNames)
        {
            Type commandType = kvp.Key;
            string displayName = kvp.Value;

            if (commandType == null) continue;

            // Check if a component of this type is already attached
            Component existingComponent = helper.GetComponent(commandType);

            EditorGUILayout.BeginHorizontal();

            // Use the custom display name
            EditorGUILayout.LabelField("Command: " + displayName,EditorStyles.boldLabel);

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

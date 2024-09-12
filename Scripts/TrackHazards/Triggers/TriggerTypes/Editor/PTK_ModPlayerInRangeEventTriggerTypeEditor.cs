using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_ModPlayerInRangeEventTriggerType), true)]
public class PTK_ModPlayerInRangeEventTriggerTypeEditor : Editor
{
    SerializedProperty checkPlayersInVolumes;
    SerializedProperty eventTypesConditionsToCheck;
    SerializedProperty bAreGlobalPlayersWithinVolumesRange;

    private void OnEnable()
    {
        // Cache serialized properties
        checkPlayersInVolumes = serializedObject.FindProperty("checkPlayersInVolumes");
        eventTypesConditionsToCheck = serializedObject.FindProperty("eventTypesConditionsToCheck");
        bAreGlobalPlayersWithinVolumesRange = serializedObject.FindProperty("bAreGlobalPlayersWithinVolumesRange");
    }

    public override void OnInspectorGUI()
    {
        // Start tracking property changes
        serializedObject.Update();

        // Draw all other fields except the ones we will customize
        DrawPropertiesExcluding(serializedObject, "checkPlayersInVolumes", "eventTypesConditionsToCheck", "bAreGlobalPlayersWithinVolumesRange");

        GUILayout.Space(10);

        // Section 1: Volumes
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Volumes", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(checkPlayersInVolumes, new GUIContent("Check Players In Volumes"), true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 2: Conditions
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(eventTypesConditionsToCheck, new GUIContent("Event Types to Check"), true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        /*
        // Section 3: Players In Range (Global)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Global Players In Range", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(bAreGlobalPlayersWithinVolumesRange, new GUIContent("Players In Range"), true);
        EditorGUILayout.EndVertical();
        */

        GUILayout.Space(10);

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}

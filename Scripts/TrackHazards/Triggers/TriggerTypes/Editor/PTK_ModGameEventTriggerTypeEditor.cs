using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_ModGameEventTriggerType), true)]
public class PTK_ModGameEventTriggerTypeEditor : Editor
{
    SerializedProperty eventTypesConditionsToCheck;
    SerializedProperty bTriggerWithPlayerEvents;
    SerializedProperty triggerTargetPlayersSettings;

    private void OnEnable()
    {
        // Cache serialized properties
        eventTypesConditionsToCheck = serializedObject.FindProperty("eventTypesConditionsToCheck");
        bTriggerWithPlayerEvents = serializedObject.FindProperty("bTriggerWithPlayerEvents");
        triggerTargetPlayersSettings = serializedObject.FindProperty("triggerTargetPlayersSettings");
    }

    public override void OnInspectorGUI()
    {
        // Start tracking property changes
        serializedObject.Update();

        // Draw all other fields except the ones we will customize
        DrawPropertiesExcluding(serializedObject, "eventTypesConditionsToCheck", "bTriggerWithPlayerEvents",
    "triggerTargetPlayersSettings");

        GUILayout.Space(10);

        // Section 1: Game Event Type Conditions
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Game Event Conditions", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(eventTypesConditionsToCheck, new GUIContent("Event Types to Check"), true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 7: Player Event Settings (conditionally shown)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Player Event Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(bTriggerWithPlayerEvents, new GUIContent("Trigger With Player Events"));
        if (bTriggerWithPlayerEvents.boolValue)
        {
            EditorGUILayout.PropertyField(triggerTargetPlayersSettings, new GUIContent("Trigger Player Settings"), true);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}

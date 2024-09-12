using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_ModGameVariableConditionsTriggerType), true)]
public class PTK_ModGameVariableConditionsTriggerTypeEditor : Editor
{
    SerializedProperty eTriggerActivationType;
    SerializedProperty variableTypeConditions;

    SerializedProperty bTriggerWithPlayerEvents;
    SerializedProperty triggerTargetPlayersSettings;
    private void OnEnable()
    {
        // Cache serialized properties
        eTriggerActivationType = serializedObject.FindProperty("eTriggerActivationType");
        variableTypeConditions = serializedObject.FindProperty("variableTypeConditions");

        bTriggerWithPlayerEvents = serializedObject.FindProperty("bTriggerWithPlayerEvents");
        triggerTargetPlayersSettings = serializedObject.FindProperty("triggerTargetPlayersSettings");
    }

    public override void OnInspectorGUI()
    {
        // Start tracking property changes
        serializedObject.Update();

        // Draw all other fields except the ones we will customize
        DrawPropertiesExcluding(serializedObject, "eTriggerActivationType", "variableTypeConditions", "bTriggerWithPlayerEvents", "triggerTargetPlayersSettings");

        GUILayout.Space(10);

        // Section 1: Trigger Activation Type
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Trigger Activation Type", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(eTriggerActivationType, new GUIContent("Activation Type"));
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 2: Variable Conditions
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Variable Conditions", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(variableTypeConditions, new GUIContent("Conditions"), true);
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

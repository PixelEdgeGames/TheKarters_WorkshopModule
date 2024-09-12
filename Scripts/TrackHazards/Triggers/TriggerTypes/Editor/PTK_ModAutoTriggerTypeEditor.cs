using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PTK_ModAutoTriggerType),true)]
public class PTK_ModAutoTriggerTypeEditor : Editor
{
    SerializedProperty fMinimumRaceTimeToTrigger;
    SerializedProperty fDelayAfterGameObjectEnabled;
    SerializedProperty bResetTriggerOnEnabledEvent;
    SerializedProperty iRepeatCount;
    SerializedProperty fRepeatWaitTime;
    SerializedProperty bUseRandomRepeatWaitTime;
    SerializedProperty v2RandomWaitRange;
    SerializedProperty bStopTriggerAtRaceEnd;
    SerializedProperty bStopOnFirstPlayerRaceEnd;
    SerializedProperty bOnlyInRaceGameMode;
    SerializedProperty bOnlyInTimeTrialGameMode;
    SerializedProperty eLapCondition;
    SerializedProperty iLapConditionNr;
    SerializedProperty iOnlyIfGlobalLapNrIsEqual;
    SerializedProperty bTriggerWithPlayerEvents;
    SerializedProperty triggerTargetPlayersSettings;

    private void OnEnable()
    {
        // Cache properties
        fMinimumRaceTimeToTrigger = serializedObject.FindProperty("fMinimumRaceTimeToTrigger");
        fDelayAfterGameObjectEnabled = serializedObject.FindProperty("fDelayAfterGameObjectEnabled");
        bResetTriggerOnEnabledEvent = serializedObject.FindProperty("eRepeatCounterResetMode");
        iRepeatCount = serializedObject.FindProperty("iRepeatCount");
        fRepeatWaitTime = serializedObject.FindProperty("fRepeatWaitTime");
        bUseRandomRepeatWaitTime = serializedObject.FindProperty("bUseRandomRepeatWaitTime");
        v2RandomWaitRange = serializedObject.FindProperty("v2RandomWaitRange");
        bStopTriggerAtRaceEnd = serializedObject.FindProperty("bStopTriggerAtRaceEnd");
        bStopOnFirstPlayerRaceEnd = serializedObject.FindProperty("bStopOnFirstPlayerRaceEnd");
        bOnlyInRaceGameMode = serializedObject.FindProperty("bOnlyInRaceGameMode");
        bOnlyInTimeTrialGameMode = serializedObject.FindProperty("bOnlyInTimeTrialGameMode");
        eLapCondition = serializedObject.FindProperty("eLapCondition");
        iLapConditionNr = serializedObject.FindProperty("iLapConditionNr");
        bTriggerWithPlayerEvents = serializedObject.FindProperty("bTriggerWithPlayerEvents");
        triggerTargetPlayersSettings = serializedObject.FindProperty("triggerTargetPlayersSettings");
    }

    public override void OnInspectorGUI()
    {

        PTK_ModAutoTriggerType targetScript = (PTK_ModAutoTriggerType)target;

        // Start tracking property changes
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "fMinimumRaceTimeToTrigger", "fDelayAfterGameObjectEnabled",
    "eRepeatCounterResetMode", "iRepeatCount", "fRepeatWaitTime", "bUseRandomRepeatWaitTime",
    "v2RandomWaitRange", "bStopTriggerAtRaceEnd", "bStopOnFirstPlayerRaceEnd", "bOnlyInRaceGameMode",
    "bOnlyInTimeTrialGameMode", "eLapCondition", "iLapConditionNr", "bTriggerWithPlayerEvents",
    "triggerTargetPlayersSettings", "iRepeatCount");

        GUILayout.Space(10);

        // Section 1: Race Timing Settings (grouped in a box with horizontal alignment)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Race Timing Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(fMinimumRaceTimeToTrigger, new GUIContent("Min Race Time"));
        EditorGUILayout.PropertyField(fDelayAfterGameObjectEnabled, new GUIContent("Delay After Enabled"));
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

     

        GUILayout.Space(10);

        // Section 3: Repeat Settings (group with horizontal alignment)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Repeat Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;

        // Arrange repeat count and wait time side by side
        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.white*0.8f + Color.green * 0.2f;
        EditorGUILayout.PropertyField(iRepeatCount, new GUIContent("Repeat Count"));
        GUI.color = Color.white;

        EditorGUILayout.EndHorizontal();

        // Section 2: Trigger Settings (grouped)

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(bUseRandomRepeatWaitTime, new GUIContent("Use Random Wait Time"));

        EditorGUILayout.EndHorizontal();

        if (bUseRandomRepeatWaitTime.boolValue == false)
            EditorGUILayout.PropertyField(fRepeatWaitTime, new GUIContent("Wait Time"));

        if (bUseRandomRepeatWaitTime.boolValue)
        {
            // Random wait range presented as min/max in a horizontal layout
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Random Wait Range", GUILayout.Width(150));
            EditorGUILayout.PropertyField(v2RandomWaitRange, GUIContent.none, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Repeat Counter Reset Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(bResetTriggerOnEnabledEvent, new GUIContent("Repeat Counter Reset Mode"));
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 4: Stop Conditions (grouped)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Stop Conditions", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(bStopTriggerAtRaceEnd, new GUIContent("Stop At Race End"));
        EditorGUILayout.PropertyField(bStopOnFirstPlayerRaceEnd, new GUIContent("Stop On First Player Finish"));
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 5: Race Mode Restrictions (grouped in horizontal layout)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow*1.3f;
        EditorGUILayout.LabelField("Race Mode Restrictions", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(bOnlyInRaceGameMode, new GUIContent("Only In Race Mode"));
        EditorGUILayout.PropertyField(bOnlyInTimeTrialGameMode, new GUIContent("Only In Time Trial Mode"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Section 6: Lap Conditions (conditional display in horizontal layout)
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Lap Conditions", EditorStyles.boldLabel);
        GUI.color = Color.white;



        EditorGUILayout.PropertyField(eLapCondition, new GUIContent("Lap Condition "));

        if (eLapCondition.enumValueIndex != 0)
        {
            EditorGUILayout.PropertyField(iLapConditionNr, new GUIContent("Lap Nr"));
        }
       

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

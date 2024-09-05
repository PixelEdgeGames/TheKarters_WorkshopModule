using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PTK_ModTriggerEditorHelper))]
public class PTK_ModTriggerEditorHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Reference to the target object (the script being inspected)
        PTK_ModTriggerEditorHelper helper = (PTK_ModTriggerEditorHelper)target;

        if (helper.instantiateInTriggersParent == null)
        {
            EditorGUILayout.HelpBox("Please assign a parent GameObject.", MessageType.Warning);
            return;
        }
        if (helper.GameEventTrigger == null)
        {
            EditorGUILayout.HelpBox("Please assign triggers.", MessageType.Warning);
            return;
        }
        GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel);
        centeredStyle.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Triggers", centeredStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Define the list of trigger prefabs
        var triggers = new List<GameObject>
        {
            helper.PlayerOrBulletPhysicsCollisionBox,
            helper.PlayerOrBulletPhysicsCollisionSphere,
            helper.PlayerOrBulletPhysicsCollisionMultiple,
            helper.PlayerEventTriggerBox,
            helper.PlayerEventTriggerSphere,
            helper.GameEventTrigger,
            helper.GameConditionTrigger
        };
        var triggersCategory = new List<string>
        {
            "Player or Bullet Collision",
            "",
            "",
            "Player Events",
            "",
            "Game Events",
            "Game Condition"
        };

        int iIndex = 0;
        foreach (var triggerPrefab in triggers)
        {
            if (triggersCategory[iIndex] != "")
            {
                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                GUILayout.Space(20);
                GUIStyle rightAlignedLabel = new GUIStyle(GUI.skin.label);
                rightAlignedLabel.alignment = TextAnchor.MiddleLeft;
                rightAlignedLabel.fontStyle = FontStyle.BoldAndItalic;
                EditorGUILayout.LabelField("Trigger From: " + triggersCategory[iIndex], rightAlignedLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            iIndex++;

            if (triggerPrefab == null)
            {
               continue;
            }


            // Find all instances of the trigger type under the parent
            var existingTriggers = new List<GameObject>();
            foreach (Transform child in helper.instantiateInTriggersParent.transform)
            {
                if (child.name == triggerPrefab.name)
                {
                    existingTriggers.Add(child.gameObject);
                }
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(triggerPrefab.name);

            if (existingTriggers.Count > 0)
            {
                // Show "Show" button
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("View"))
                {
                    // Select and highlight the first instance in the hierarchy
                    Selection.activeObject = existingTriggers[0];
                    EditorGUIUtility.PingObject(existingTriggers[0]);
                }

                // Show "Add" button
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add"))
                {
                    GameObject newTrigger = (GameObject)PrefabUtility.InstantiatePrefab(triggerPrefab, helper.instantiateInTriggersParent.transform);
                    newTrigger.name = triggerPrefab.name;

                    // Automatically select and highlight the new trigger in the hierarchy
                    Selection.activeObject = newTrigger;
                    EditorGUIUtility.PingObject(newTrigger);
                    EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
                }

                // Show "Delete" button only if there is exactly one instance
                if (existingTriggers.Count == 1)
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete"))
                    {
                        DestroyImmediate(existingTriggers[0]);
                        EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
                    }
                }
            }
            else
            {
                // Show "Create" button if no instances exist
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Create"))
                {
                    GameObject newTrigger = (GameObject)PrefabUtility.InstantiatePrefab(triggerPrefab, helper.instantiateInTriggersParent.transform);
                    newTrigger.name = triggerPrefab.name;

                    // Automatically select and highlight the new trigger in the hierarchy
                    Selection.activeObject = newTrigger;
                    EditorGUIUtility.PingObject(newTrigger);
                    EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            if (iIndex == 3)
            {
                GUIStyle rightAlignedLabel = new GUIStyle(GUI.skin.label);
                rightAlignedLabel.alignment = TextAnchor.MiddleRight;
                rightAlignedLabel.fontStyle = FontStyle.Italic;
                GUILayout.Label("(ColliderGroup - Colliders must be created inside)", rightAlignedLabel);
            }
        }

        GUILayout.Space(20);
        EditorGUILayout.Space(); 
        EditorGUILayout.LabelField("Command Behaviours", centeredStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if(helper.triggerArrayCommandsExecutorParent != null)
        {
            EditorGUI.BeginChangeCheck();

            helper.triggerArrayCommandsExecutorParent.eCommandBehavioursExecutionMode = (PTK_TriggerArrayCommandsExecutor.ECommandBehaviourExecutionMode)EditorGUILayout.EnumPopup(
               "Command Behaviours Execution",
               helper.triggerArrayCommandsExecutorParent.eCommandBehavioursExecutionMode
           );
            helper.triggerArrayCommandsExecutorParent.eRunCommandCondition = (PTK_TriggerArrayCommandsExecutor.ECommandsRunCondition)EditorGUILayout.EnumPopup(
               "When to send commands",
               helper.triggerArrayCommandsExecutorParent.eRunCommandCondition
           );
            helper.triggerArrayCommandsExecutorParent.eRunCommandsMode = (PTK_TriggerArrayCommandsExecutor.ECommandsRunMode)EditorGUILayout.EnumPopup(
               "Allow Sending Commands Again?",
               helper.triggerArrayCommandsExecutorParent.eRunCommandsMode
           );
            GUILayout.Space(10);

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
            }
        }

        // Apply any modifications made to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
        }


        if (helper.triggerArrayCommandsExecutorParent != null)
        {
            bool bFoundAnyCommand = false;

            for(int iCommandBehParent = 0; iCommandBehParent < helper.triggerArrayCommandsExecutorParent.commandBehavioursParentsToRun.Count;iCommandBehParent++)
            {
                var commandBehavioursToRun = helper.triggerArrayCommandsExecutorParent.commandBehavioursParentsToRun[iCommandBehParent].GetComponentsInChildren<PTK_TriggerCommandsBehaviour>();
                if (commandBehavioursToRun.Length > 0)
                {
                    for (int i = 0; i < commandBehavioursToRun.Length; i++)
                    {
                        bFoundAnyCommand = true;
                        EditorGUILayout.BeginHorizontal();

                        if (commandBehavioursToRun[i].strBehaviourInfo != "")
                            GUILayout.Label(commandBehavioursToRun[i].strBehaviourInfo + " Behaviour");
                        else
                            GUILayout.Label("Command Behaviour " + (i + 1));

                        commandBehavioursToRun[i].fExecuteDelay = EditorGUILayout.FloatField(commandBehavioursToRun[i].fExecuteDelay);

                        GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button("Edit"))
                        {
                            Selection.activeObject = commandBehavioursToRun[i].gameObject;
                            EditorGUIUtility.PingObject(commandBehavioursToRun[i].gameObject);
                        }

                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("Delete"))
                        {
                            DestroyImmediate(commandBehavioursToRun[i].gameObject);
                            EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
                        }
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                }
            }

            if (bFoundAnyCommand == false)
            {
                GUI.color = Color.red;
                GUILayout.Label("Commands to Run Empty - Please Create New");
                GUI.color = Color.white;
            }


            // Option to add a new CommandsBehaviour
            GUI.backgroundColor = Color.green;
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create New Command Behaviour"))
            {
                if (helper.commandsBehaviourPrefab != null)
                {
                    GameObject newBehaviour = (GameObject)PrefabUtility.InstantiatePrefab(helper.commandsBehaviourPrefab.gameObject, helper.commandsBehaviourParent.transform);
                 
                    // Automatically select and highlight the new commands behaviour in the hierarchy
                    Selection.activeObject = newBehaviour;
                    EditorGUIUtility.PingObject(newBehaviour);

                    EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
                }
                else
                {
                    Debug.LogWarning("CommandsBehaviourPrefab is not assigned.");
                }
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        // Apply any modifications made to the serialized object
        serializedObject.ApplyModifiedProperties();

    }
}

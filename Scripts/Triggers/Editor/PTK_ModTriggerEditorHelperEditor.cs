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
            helper.PlayerEventTriggerBox,
            helper.PlayerEventTriggerSphere,
            helper.GameEventTrigger,
            helper.GameConditionTrigger
        };
        var triggersCategory = new List<string>
        {
            "Player or Bullet Collision",
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
                }

                // Show "Delete" button only if there is exactly one instance
                if (existingTriggers.Count == 1)
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete"))
                    {
                        DestroyImmediate(existingTriggers[0]);
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
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(20);
        EditorGUILayout.Space(); 
        EditorGUILayout.LabelField("Command Behaviours", centeredStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if(helper.triggerArrayCommandsExecutorParent != null)
        {
            helper.triggerArrayCommandsExecutorParent.eCommandBehavioursExecutionMode = (PTK_TriggerArrayCommandsExecutor.ECommandBehaviourExecutionMode)EditorGUILayout.EnumPopup(
               "Execution Mode",
               helper.triggerArrayCommandsExecutorParent.eCommandBehavioursExecutionMode
           );
            GUILayout.Space(10);
        }

        // Apply any modifications made to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(helper.triggerArrayCommandsExecutorParent);
        }

        List<PTK_TriggerCommandsBehaviour> behavioursToRemove = new List<PTK_TriggerCommandsBehaviour>();

        if (helper.triggerArrayCommandsExecutorParent != null)
        {
            if (helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Count > 0)
            {
                for (int i = 0; i < helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Count; i++)
                {
                    if (helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i] == null)
                    {
                        behavioursToRemove.Add(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i]);
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();

                    if(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].strBehaviourInfo != "")
                        GUILayout.Label(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].strBehaviourInfo + " Behaviour");
                    else
                        GUILayout.Label("Command Behaviour " + (i + 1));

                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Edit"))
                    {
                        Selection.activeObject = helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].gameObject;
                        EditorGUIUtility.PingObject(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].gameObject);
                    }

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete"))
                    {
                        DestroyImmediate(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].gameObject);
                        helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.RemoveAt(i);
                        i--; // Adjust index after removal
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("Commands to Run Empty - Please Create New");
                GUI.color = Color.white;
            }

            for(int i=0;i< behavioursToRemove.Count;i++)
            {
                helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Remove(behavioursToRemove[i]);
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
                    helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Add(newBehaviour.GetComponent<PTK_TriggerCommandsBehaviour>());

                    // Automatically select and highlight the new commands behaviour in the hierarchy
                    Selection.activeObject = newBehaviour;
                    EditorGUIUtility.PingObject(newBehaviour);
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

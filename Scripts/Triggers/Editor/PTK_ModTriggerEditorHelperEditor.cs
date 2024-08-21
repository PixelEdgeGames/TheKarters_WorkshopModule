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
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);

        // Define the list of trigger prefabs
        var triggers = new List<GameObject>
        {
            helper.GameEventTrigger,
            helper.PlayerEventTriggerBox,
            helper.PlayerEventTriggerSphere,
            helper.PlayerOrBulletPhysicsCollisionBox,
            helper.PlayerOrBulletPhysicsCollisionSphere,
            helper.ConditionTriggerBox,
            helper.ConditionTriggerSphere
        };
        var triggersCategory = new List<string>
        {
            "Game Events",
            "Player Events",
            "",
            "Player or Bullet Collision",
            "",
            "Variable Conditions",
            ""
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

        if (helper.triggerArrayCommandsExecutorParent != null)
        {
            GUI.backgroundColor = Color.yellow;
            if(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Count > 0)
            {
                for (int i = 0; i < helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun.Count; i++)
                {
                    if (GUILayout.Button("Edit Commands to Run (" + (i + 1) + ")"))
                    {
                        Selection.activeObject = helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].gameObject;
                        EditorGUIUtility.PingObject(helper.triggerArrayCommandsExecutorParent.commandBehavioursToRun[i].gameObject);
                    }
                }
            }else
            {
                if (GUILayout.Button("Commands to Run Empty - Please Assign"))
                {
                }
            }
            GUI.backgroundColor = Color.white;
        }

        // Apply any modifications made to the serialized object
        serializedObject.ApplyModifiedProperties();

    }
}

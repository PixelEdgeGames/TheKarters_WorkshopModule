using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PTK_Command_07_AnimatorCommands))]
public class PTK_Command_07_AnimatorCommandsEditor : Editor
{
    private SerializedProperty animatorCallsToSend;

    private void OnEnable()
    {
        animatorCallsToSend = serializedObject.FindProperty("animatorCallsToSend");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        EditorGUILayout.LabelField("Animator Commands", EditorStyles.boldLabel);

        // Button to add a new animator
        if (GUILayout.Button("Add New Animator"))
        {
            animatorCallsToSend.arraySize++;

            SerializedProperty newAnimatorCall = animatorCallsToSend.GetArrayElementAtIndex(animatorCallsToSend.arraySize - 1);
            newAnimatorCall.FindPropertyRelative("animatorTypeObject").objectReferenceValue = null;

            SerializedProperty animatorCall = animatorCallsToSend.GetArrayElementAtIndex(animatorCallsToSend.arraySize - 1);
            animatorCall.isExpanded = true;
            
            // Clear all arrays
            newAnimatorCall.FindPropertyRelative("animClipToPlay").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("blendTreeToPlay").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("pauseResumeEvent").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("triggers").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("booleans").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("floats").arraySize = 0;
            newAnimatorCall.FindPropertyRelative("integers").arraySize = 0;

            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(20);

        // Iterate through each animator call in the array
        for (int i = 0; i < animatorCallsToSend.arraySize; i++)
        {
            Color backgroundMainCol = ((i % 2) == 0 ? (Color.white + Color.blue * 0.8f) : (Color.white + Color.yellow*0.9f));
            GUI.backgroundColor = backgroundMainCol;

            SerializedProperty animatorCall = animatorCallsToSend.GetArrayElementAtIndex(i);

            // Animator foldout with name, property field and delete button
            EditorGUILayout.BeginVertical(GUI.skin.box);

            SerializedProperty animatorTypeObject = animatorCall.FindPropertyRelative("animatorTypeObject");

            string strAnimatorName = "";
            if (animatorTypeObject.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Animator object is NULL - please assign it!", MessageType.Error);
            }else
            {
                strAnimatorName = animatorTypeObject.objectReferenceValue.name;
            }

            EditorGUILayout.BeginHorizontal();

            animatorCall.isExpanded = EditorGUILayout.Foldout(animatorCall.isExpanded, "Animator ( " + strAnimatorName + " )", true);

            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(animatorTypeObject, GUIContent.none,GUILayout.Width(150));

            GUILayout.FlexibleSpace();

            GUI.backgroundColor =Color.red*1.5f;

            if (GUILayout.Button("DEL", GUILayout.Width(150)))
            {
                animatorCallsToSend.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = backgroundMainCol;


            EditorGUILayout.EndHorizontal();

            if (animatorCall.isExpanded)
            {
                // Add buttons for each type
                DrawAddButtons(animatorCall,i);

                GUI.backgroundColor = backgroundMainCol;
                // Separator line
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // Draw existing items
                DrawList(animatorCall.FindPropertyRelative("animClipToPlay"), "Animator Play Anim Clip", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CClipToPlay));
                DrawList(animatorCall.FindPropertyRelative("blendTreeToPlay"), "Animator Play Blend Tree ", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CBlendTreeToPlay));
                DrawList(animatorCall.FindPropertyRelative("pauseResumeEvent"), "Animator Pause/Resume Event", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CPauseResumeState));
                DrawList(animatorCall.FindPropertyRelative("triggers"), "Animator Trigger to Set", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CTrigger));
                DrawList(animatorCall.FindPropertyRelative("booleans"), "Animator Boolean to Set", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CBool));
                DrawList(animatorCall.FindPropertyRelative("floats"), "Animator Float to Set", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CFloat));
                DrawList(animatorCall.FindPropertyRelative("integers"), "Animator Integer to Set", typeof(PTK_Command_07_AnimatorCommands.CAnimatorCalls.CInt));
            }

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }

    Dictionary<int, int> selectedCategoryIndex = new Dictionary<int, int>();
    private void DrawAddButtons(SerializedProperty animatorCall,int iAnimatorIndex)
    {
        GUILayout.Space(10);

        
        EditorGUILayout.BeginHorizontal();

        // Label for 'Add'
        EditorGUILayout.LabelField("Command to Add",GUILayout.Width(110));

        // Dropdown for selecting the category to add
        string[] categories = new string[] { "Play Anim Clip", "Play Blend Tree", "Pause-Resume Event", "Set Trigger", "Set Boolean", "Set Float", "Set Integer" };

        if (selectedCategoryIndex.ContainsKey(iAnimatorIndex) == false)
            selectedCategoryIndex.Add(iAnimatorIndex, 0);

        int iValIndex = selectedCategoryIndex[iAnimatorIndex];
        iValIndex = EditorGUILayout.Popup(iValIndex, categories, GUILayout.Width(160));
        selectedCategoryIndex[iAnimatorIndex] = iValIndex;

        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        // Add button to add the selected type
        if (GUILayout.Button("Add New", GUILayout.Width(100)))
        {
            // Add item based on selected category
            switch (selectedCategoryIndex[iAnimatorIndex])
            {
                case 0: // Clip To Play
                    animatorCall.FindPropertyRelative("animClipToPlay").arraySize++;
                    SerializedProperty newClip = animatorCall.FindPropertyRelative("animClipToPlay").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("animClipToPlay").arraySize - 1);
                    newClip.FindPropertyRelative("clipToPlay").objectReferenceValue = null; // Default to null
                    newClip.FindPropertyRelative("fTransitionTime").floatValue = 0.2f; // Default transition time
                    break;

                case 1: // Blend Tree To Play
                    animatorCall.FindPropertyRelative("blendTreeToPlay").arraySize++;
                    SerializedProperty newBlendTree = animatorCall.FindPropertyRelative("blendTreeToPlay").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("blendTreeToPlay").arraySize - 1);
                    newBlendTree.FindPropertyRelative("strBlendTreeName").stringValue = "Blend Tree"; // Default name
                    newBlendTree.FindPropertyRelative("fTransitionTime").floatValue = 0.2f; // Default transition time
                    break;

                case 2: // Pause/Resume Event
                    animatorCall.FindPropertyRelative("pauseResumeEvent").arraySize++;
                    SerializedProperty newPauseResume = animatorCall.FindPropertyRelative("pauseResumeEvent").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("pauseResumeEvent").arraySize - 1);
                    newPauseResume.FindPropertyRelative("eStateToSet").enumValueIndex = 0; // Default to first enum value (Pause)
                    break;

                case 3: // Trigger
                    animatorCall.FindPropertyRelative("triggers").arraySize++;
                    SerializedProperty newTrigger = animatorCall.FindPropertyRelative("triggers").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("triggers").arraySize - 1);
                    newTrigger.FindPropertyRelative("strTriggerName").stringValue = ""; // Default to empty string
                    break;

                case 4: // Boolean
                    animatorCall.FindPropertyRelative("booleans").arraySize++;
                    SerializedProperty newBool = animatorCall.FindPropertyRelative("booleans").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("booleans").arraySize - 1);
                    newBool.FindPropertyRelative("strBoolName").stringValue = ""; // Default to empty string
                    newBool.FindPropertyRelative("bValue").boolValue = false; // Default to false
                    break;

                case 5: // Float
                    animatorCall.FindPropertyRelative("floats").arraySize++;
                    SerializedProperty newFloat = animatorCall.FindPropertyRelative("floats").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("floats").arraySize - 1);
                    newFloat.FindPropertyRelative("strFloatName").stringValue = ""; // Default to empty string
                    newFloat.FindPropertyRelative("fValue").floatValue = 0.0f; // Default to 0.0
                    newFloat.FindPropertyRelative("fAchiveValueInTime").floatValue = 0.1f; // Default Lerp duration
                    break;

                case 6: // Integer
                    animatorCall.FindPropertyRelative("integers").arraySize++;
                    SerializedProperty newInt = animatorCall.FindPropertyRelative("integers").GetArrayElementAtIndex(animatorCall.FindPropertyRelative("integers").arraySize - 1);
                    newInt.FindPropertyRelative("strIntName").stringValue = ""; // Default to empty string
                    newInt.FindPropertyRelative("iValue").intValue = 0; // Default to 0
                    break;
            }
            EditorUtility.SetDirty(target);
        }
        GUI.backgroundColor = originalColor;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }


    // Helper function to draw each property list
    private void DrawList(SerializedProperty listProperty, string label, System.Type type)
    {
        if (listProperty.arraySize == 0)
            return;

        GUILayout.Space(10);

        Color originalColor = GUI.backgroundColor;
        for (int j = 0; j < listProperty.arraySize; j++)
        {

            SerializedProperty element = listProperty.GetArrayElementAtIndex(j);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label + " " + (j+1), EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("DEL", GUILayout.Width(50)))
            {
                listProperty.DeleteArrayElementAtIndex(j);
                EditorGUILayout.EndHorizontal();
                break;
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();


            SerializedProperty property = element.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            EditorGUI.indentLevel++;
            while (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            EditorGUI.indentLevel--;
        }
    }
}

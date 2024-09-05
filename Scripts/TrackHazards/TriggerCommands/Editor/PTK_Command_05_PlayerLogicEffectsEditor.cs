using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(PTK_Command_05_PlayerLogicEffects), true)]
public class PTK_Command_05_PlayerLogicEffectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Create a serialized object so we can handle property fields
        SerializedObject serializedObject = new SerializedObject(target);
        PTK_Command_05_PlayerLogicEffects script = (PTK_Command_05_PlayerLogicEffects)target;

        // Start scroll view
        EditorGUILayout.BeginScrollView(Vector2.zero);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Execute Delay");
        script.fExecuteDelay =  EditorGUILayout.FloatField(script.fExecuteDelay, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Get all fields of the target object
        FieldInfo[] fields = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Check if the field is a subclass of CPlayerEffectBase
            if (typeof(PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase).IsAssignableFrom(field.FieldType))
            {
                var effect = (PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase)field.GetValue(script);

                if (effect != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    // Draw enabled button
                    bool isEnabled = effect.bExecute;
                    EditorGUILayout.BeginHorizontal();
                    // Display the effect type name

                    GUI.color = isEnabled ? (Color.green + Color.white * 0.3f + Color.yellow * 0.3f) : Color.white*0.9f;
                    string formattedFieldName = AddSpacesToFieldName(field.Name);
                    EditorGUILayout.LabelField(formattedFieldName, EditorStyles.boldLabel);

                    GUI.color = Color.white;
                    GUI.backgroundColor = isEnabled ? (Color.green + Color.white * 0.3f + Color.yellow * 0.3f) : Color.gray;
                    if (GUILayout.Button(isEnabled ?  "ON" : "OFF"))
                    {
                        effect.bExecute = !effect.bExecute;
                    }

                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();

                    // If the effect is enabled, show its properties
                    if (isEnabled)
                    {
                        EditorGUI.indentLevel++;

                        // Convert effect to SerializedObject to handle properties
                        SerializedObject effectSerializedObject = new SerializedObject(script);
                        SerializedProperty effectProperty = effectSerializedObject.FindProperty(field.Name);

                        SerializedProperty property = effectProperty.Copy();
                        SerializedProperty endProperty = property.GetEndProperty();


                        // Iterate over all properties of the effect
                        if (property.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(property, endProperty))
                                    break;

                                if (property.name == "bExecute") continue;

                                EditorGUILayout.PropertyField(property, true);
                            }
                            while (property.NextVisible(false));
                        }

                        effectSerializedObject.ApplyModifiedProperties();

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();

                    // Space between each effect
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider,GUILayout.Height(8));
                }
            }
        }

        // End the scroll view
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    private string AddSpacesToFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;

        return string.Concat(fieldName.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
    }
}

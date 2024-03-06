using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CurveLib.Curves;

[CustomEditor(typeof(PTK_ModPathsCreator))]
public class PTK_ModPathCreatorEditor : Editor
{
    bool bAlreadyGenerated = false;
    public override void OnInspectorGUI()
    {
        PTK_ModPathsCreator pathHolder = (PTK_ModPathsCreator)target;

        EditorGUI.BeginChangeCheck();

        // Draw the default inspector
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() == true)
        {
            pathHolder.GeneratePathsEditor();
        }

        // Add your custom GUI elements here
        if (pathHolder.bPathGenerationSuccess == false && bAlreadyGenerated == true)
        {
            GUI.color = Color.red;
            EditorGUILayout.HelpBox("Paths Generation Error - check console", MessageType.Info);
            GUI.color = Color.white;
        }
        else
        {
            bAlreadyGenerated = false;
            EditorGUILayout.HelpBox("Actions", MessageType.Info);
        }


        if (GUILayout.Button("Refresh Path Preview"))
        {
            pathHolder.RefreshPathLineRenderer();
        }

        if (GUILayout.Button("Initialize New Points"))
        {
            bAlreadyGenerated = false;
            pathHolder.CreatePathPointsEditor();
        }

        if (GUILayout.Button("Generate Paths"))
        {
            bAlreadyGenerated = true;
            pathHolder.GeneratePathsEditor();
        }

        // If your custom GUI has modified the object, mark it as dirty so Unity knows to save the changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

    }
}

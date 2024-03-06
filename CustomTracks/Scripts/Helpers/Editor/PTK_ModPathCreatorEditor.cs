using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(PTK_ModPathsCreator))]
public class PTK_ModPathCreatorEditor : Editor
{
    private void OnSceneGUI()
    {
        PTK_ModPathsCreator pathHolder = target as PTK_ModPathsCreator;
        if (pathHolder.mainRoadPath.Count < 2)
        {
            return; // Need at least two points to draw a line
        }

        // Set the color and line width for the handles
        Handles.color = Color.red;

        for (int i = 0; i < pathHolder.mainRoadPath.Count - 1; i++)
        {
            Vector3 startPoint = pathHolder.mainRoadPath[i].transform.position;
            Vector3 endPoint = pathHolder.mainRoadPath[i + 1].transform.position;

            // Calculate tangents
            Vector3 startTangent, endTangent;
            if (i == 0)
            {
                // For the first segment, the start tangent goes from the start point
                startTangent = startPoint + (endPoint - startPoint) * 0.5f;
            }
            else
            {
                // Otherwise, it's directed towards the previous point
                startTangent = startPoint + (startPoint - pathHolder.mainRoadPath[i - 1].transform.position) * 0.5f;
            }

            if (i == pathHolder.mainRoadPath.Count - 2)
            {
                // For the last segment, the end tangent goes towards the end point
                endTangent = endPoint - (endPoint - startPoint) * 0.5f;
            }
            else
            {
                // Otherwise, it's directed towards the next point
                endTangent = endPoint + (endPoint - pathHolder.mainRoadPath[i + 2].transform.position) * 0.5f;
            }

            Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.green, null, 5); // Adjust line width here
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        base.OnInspectorGUI();

        // Add your custom GUI elements here
        EditorGUILayout.HelpBox("Actions", MessageType.Info);

        PTK_ModPathsCreator pathHolder = (PTK_ModPathsCreator)target;

        // Example of a custom button
        if (GUILayout.Button("Create Path Points"))
        {
            pathHolder.CreatePathPointsEditor();
        }

        if (GUILayout.Button("Generate Paths"))
        {
            pathHolder.GeneratePathsEditor();
        }

        // If your custom GUI has modified the object, mark it as dirty so Unity knows to save the changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}

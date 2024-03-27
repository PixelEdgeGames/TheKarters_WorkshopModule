using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PTK_RoadPointCreatedPoint))]
public class PTK_RoadPointCreatedPointEditor : Editor
{
    Vector3 vLastPos = Vector3.zero;
    float fLastHeight = -1;
    public override void OnInspectorGUI()
    {
        PTK_RoadPointCreatedPoint point = (PTK_RoadPointCreatedPoint)target;

        if (fLastHeight == -1)
            fLastHeight = point.fMeshSizeInPoint;

        EditorGUI.BeginChangeCheck();

        // Draw the default inspector
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() == true || vLastPos != point.transform.position || fLastHeight != point.fMeshSizeInPoint)
        {
            point.parentRoadPointsCreator.RefreshSplineLineRenderer();
            point.parentRoadPointsCreator.DeleteGeneratedRoadPath(); // path point pos changed, delete generated path
        }

        vLastPos = point.transform.position;
        fLastHeight = point.fMeshSizeInPoint;
    }


    void OnSceneGUI()
    {
        PTK_RoadPointCreatedPoint point = (PTK_RoadPointCreatedPoint)target;

        if (point == null)
            return;

        // Convert the point's world position to a GUI position
        Vector2 guiPosition = HandleUtility.WorldToGUIPoint(point.transform.position);

        // Begin a GUI group at the point's position
        Handles.BeginGUI();

        // Set an offset for better visibility or to avoid overlapping with the point (optional)
        Vector2 guiOffset = new Vector2(20, 20);
        guiPosition += guiOffset;

        // Create a layout based on the calculated screen position
        GUILayout.BeginArea(new Rect(guiPosition.x, guiPosition.y, 200, 100));

        GUI.color = Color.yellow;
        GUILayout.Label("Point Tool");

        if (GUILayout.Button("Select Prev", GUILayout.Width(100)))
        {
            int iPointIndex = point.transform.GetSiblingIndex() - 1;
            if (iPointIndex < 0) iPointIndex = point.transform.parent.childCount - 1;
            if (iPointIndex >= point.transform.parent.childCount) iPointIndex = 0;

            Selection.activeGameObject = point.transform.parent.GetChild(iPointIndex).gameObject;
        }

        if (GUILayout.Button("Select Next", GUILayout.Width(100)))
        {
            int iPointIndex = point.transform.GetSiblingIndex() + 1;
            if (iPointIndex < 0) iPointIndex = point.transform.parent.childCount - 1;
            if (iPointIndex >= point.transform.parent.childCount) iPointIndex = 0;

            Selection.activeGameObject = point.transform.parent.GetChild(iPointIndex).gameObject;
        }
        GUILayout.Space(10);

        GUI.color = Color.white;

        // End the GUI area and GUI handling
        GUILayout.EndArea();
        Handles.EndGUI();
    }

}

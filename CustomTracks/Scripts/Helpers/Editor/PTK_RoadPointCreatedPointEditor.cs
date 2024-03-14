using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PTK_RoadPointCreatedPoint))]
public class PTK_RoadPointCreatedPointEditor : Editor
{
    Vector3 vLastPos = Vector3.zero;
    public override void OnInspectorGUI()
    {
        PTK_RoadPointCreatedPoint point = (PTK_RoadPointCreatedPoint)target;

        EditorGUI.BeginChangeCheck();

        // Draw the default inspector
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() == true || vLastPos != point.transform.position)
        {
            point.parentRoadPointsCreator.RefreshSplineLineRenderer();
            point.parentRoadPointsCreator.DeleteGeneratedRoadPath(); // path point pos changed, delete generated path
        }

        vLastPos = point.transform.position;
    }

}

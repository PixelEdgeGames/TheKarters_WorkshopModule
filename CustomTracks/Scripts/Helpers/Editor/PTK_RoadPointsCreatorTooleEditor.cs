using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CurveLib.Curves;

[CustomEditor(typeof(PTK_RoadPointsCreatorTool))]
public class PTK_RoadPointsCreatorTooleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PTK_RoadPointsCreatorTool pointCreator = (PTK_RoadPointsCreatorTool)target;


        if (pointCreator.bEditor_CreateSourcePoints)
            GUI.color = Color.green;

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
        GUI.color = Color.white;

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Source Points Editor");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (pointCreator.bEditor_CreateSourcePoints == false)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Create/Edit Source Points"))
            {
                pointCreator.bEditor_CreateSourcePoints = true;
                pointCreator.DeleteGeneratedRoadPath();
            }

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Road Path",GUILayout.Width(250)))
            {
                pointCreator.GenerateRoadPath();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {

            GUI.color = Color.green;
            GUILayout.Label("Source points creating\nUse LEFT MOUSE CLICK to create point");
            GUI.color = Color.white;

            GUI.color = Color.green;
            GUILayout.Space(10);
            if (GUILayout.Button("Stop creating points"))
            {
                pointCreator.bEditor_CreateSourcePoints = false;
            }
            GUI.color = Color.white;

            GUILayout.Space(10);
            if (GUILayout.Button("In-Air : Add Point in LAST segment"))
            {
                pointCreator.CreatePointInTheMiddleBeforeLast();
            }
        }


        GUI.color = Color.red + Color.yellow * 0.5f;
        GUILayout.Space(50);
        if (GUILayout.Button("Delete Source Points",GUILayout.Width(150)))
        {
            while (pointCreator.sourcePointsTransformParent.childCount > 0)
            {
                GameObject.DestroyImmediate(pointCreator.sourcePointsTransformParent.GetChild(0).gameObject);
            }

            pointCreator.DeleteGeneratedRoadPath();
            pointCreator.DeleteGeneratedMesh();
            pointCreator.RefreshSplineLineRenderer();
        }
        GUI.color = Color.white;

        GUILayout.Space(30);
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true),GUILayout.Width(400));

        GUILayout.BeginHorizontal();

        GUILayout.Label("Mesh Generation    ");

        if (pointCreator.bEditor_AutoGenerateMesh == true)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.white;
        if (GUILayout.Button("Enabled"))
        {
            pointCreator.bEditor_AutoGenerateMesh = true;
        }

        if (pointCreator.bEditor_AutoGenerateMesh == false)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.white;
        if (GUILayout.Button("Disabled"))
        {
            pointCreator.bEditor_AutoGenerateMesh = false;
        }
        GUILayout.EndHorizontal();

        GUI.color = Color.white;

        if (pointCreator.bEditor_AutoGenerateMesh == true)
        {
            GUI.color = Color.white;
            GUILayout.Space(20);
            pointCreator.eMeshType = (PTK_RoadPointsCreatorTool.EMeshType)EditorGUILayout.EnumPopup("MeshType", pointCreator.eMeshType, GUILayout.Width(300));
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Point Height");
            GUILayout.FlexibleSpace();
            pointCreator.fNewPointTargetHeight = EditorGUILayout.FloatField(pointCreator.fNewPointTargetHeight);
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("Collider Layer");
            GUILayout.FlexibleSpace();
            pointCreator.targetMeshModelLayer = EditorGUILayout.LayerField(pointCreator.targetMeshModelLayer);
            GUILayout.EndHorizontal();


            GUILayout.Space(10);
            pointCreator.bMeshRepeatUvY = GUILayout.Toggle(pointCreator.bMeshRepeatUvY, "Repeat Texture");

        }


        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck() == true)
        {
            pointCreator.RefreshSplineLineRenderer();
        }

        // If your custom GUI has modified the object, mark it as dirty so Unity knows to save the changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        GUILayout.Space(10);
        GUILayout.EndVertical();


        GUILayout.Space(40);
        EditorGUI.BeginChangeCheck();

        // Draw the default inspector
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck() == true)
        {
        }
    }

    // Utility function to get layer names
    string[] GetLayerNames()
    {
        var layerNames = new List<string>();
        for (int i = 0; i < 32; i++) // Unity supports 32 layers
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layerNames.Add(layerName);
            }
        }
        return layerNames.ToArray();
    }
}

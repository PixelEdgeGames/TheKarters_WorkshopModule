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

      

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
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
            }

            GUI.color = Color.red + Color.yellow*0.5f;
            GUILayout.Space(10);
            if (GUILayout.Button("Delete Source Points"))
            {
                while(pointCreator.sourcePointsTransformParent.childCount > 0)
                {
                    GameObject.DestroyImmediate(pointCreator.sourcePointsTransformParent.GetChild(0).gameObject);
                }
            }
            GUI.color = Color.white;
        }
        else
        {
            GUILayout.Label("Source points creating\nUse Left Mouse Click to create point");

            GUI.color = Color.green;
            GUILayout.Space(10);
            if (GUILayout.Button("Stop creating points"))
            {
                pointCreator.bEditor_CreateSourcePoints = false;
            }
            GUI.color = Color.white;
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

}

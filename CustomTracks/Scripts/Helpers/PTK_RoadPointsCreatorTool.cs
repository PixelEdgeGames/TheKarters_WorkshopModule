using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class PTK_RoadPointsCreatorTool : MonoBehaviour
{
    public Transform sourcePointsTransformParent;

    [HideInInspector]
    public bool bEditor_CreateSourcePoints = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }
#if UNITY_EDITOR

    private void OnEnable()
    {
        if (Application.isPlaying == false)
        {
            SceneView.duringSceneGui += DuringSceneGui;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying == false)
        {
            SceneView.duringSceneGui -= DuringSceneGui;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
    }

    private void OnUndoRedoPerformed()
    {
    }

    void DuringSceneGui(SceneView sceneView)
    {
        PTK_RoadPointsCreatorTool pointCreator = this;// (PTK_RoadPointsCreatorTool)target;

        if (pointCreator.bEditor_CreateSourcePoints == true)
        {
            Selection.activeGameObject = this.gameObject;

            // Capture the current event.
            Event e = Event.current;

            // Check if the right mouse button was clicked.
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Prevent Unity from selecting another object in the scene when we right-click.
                e.Use();

                // Convert the mouse position into a world space point.
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // If the raycast hits something in the scene, we use the hit point to place our sphere.
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.parent = pointCreator.sourcePointsTransformParent;
                    sphere.transform.position = hit.point;
                    sphere.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
                    GameObject.DestroyImmediate(sphere.GetComponent<Collider>());
                    Undo.RegisterCreatedObjectUndo(sphere, "Create Sphere");
                }
            }
        }
    }

#endif
}

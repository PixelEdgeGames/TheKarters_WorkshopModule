using CurveLib.Curves;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class PTK_RoadPointsCreatorTool : MonoBehaviour
{
    public Transform sourcePointsTransformParent;
    public Transform generatedRoadPathParent;
    public LineRenderer lineRendererSpline;

    [HideInInspector]
    public bool bEditor_CreateSourcePoints = false;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying == true)
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

#if UNITY_EDITOR

    private void OnEnable()
    {
        if (Application.isPlaying == false)
        {
            RefreshSplineLineRenderer();

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
        if(Selection.activeGameObject == this.gameObject)
            RefreshSplineLineRenderer();
    }

    internal void GenerateRoadPath()
    {
        while(generatedRoadPathParent.childCount > 0)
        {
            GameObject.DestroyImmediate(generatedRoadPathParent.GetChild(0).gameObject);
        }

        if (lineRendererSpline.positionCount < 2)
            return;

        GameObject roadPath = new GameObject("Road Path");
        roadPath.transform.parent = generatedRoadPathParent.transform;

       var roadStartPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject.DestroyImmediate(roadStartPoint.GetComponent<Collider>());
        roadStartPoint.name = "Point 0";
        roadStartPoint.transform.parent = roadPath.transform;
        roadStartPoint.transform.position = lineRendererSpline.GetPosition(0);
        roadStartPoint.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

        Vector3 vFromFirstTo2nd = lineRendererSpline.GetPosition(1) - roadStartPoint.transform.position; vFromFirstTo2nd.y = 0.0F;
        roadStartPoint.transform.forward = vFromFirstTo2nd.normalized;

        float fDistEvery = 20.0f;
        float fDistance = 0.0f;
        for(int i=1;i< lineRendererSpline.positionCount;i++)
        {
            Vector3 vFromPrevToCurrent = (lineRendererSpline.GetPosition(i) - lineRendererSpline.GetPosition(i - 1) );
            float fDistanceFromPrevToCurrent = vFromPrevToCurrent.magnitude;

            fDistance += fDistanceFromPrevToCurrent;

            if(fDistance >= fDistEvery)
            {
                fDistance = fDistance- fDistEvery;

                var roadPathPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere); GameObject.DestroyImmediate(roadPathPoint.GetComponent<Collider>());
                roadPathPoint.transform.parent = roadPath.transform;
                roadPathPoint.transform.position = lineRendererSpline.GetPosition(i);
                roadPathPoint.name = "Point " + (roadPath.transform.childCount-1);

                // calculate forward as dir from last to current
                Vector3 vDir = vFromPrevToCurrent; vDir.y = 0.0f;
                roadPathPoint.transform.forward = vDir;
                roadPathPoint.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
        }
    }

    void DuringSceneGui(SceneView sceneView)
    {
        if (Application.isPlaying == true)
            return;

        if (Selection.activeGameObject == this.gameObject || (Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<PTK_RoadPointsCreatorTool>() != null))
        {
            lineRendererSpline.enabled = true;
            sourcePointsTransformParent.gameObject.SetActive(true);
            generatedRoadPathParent.gameObject.SetActive(true);
        }
        else
        {
            sourcePointsTransformParent.gameObject.SetActive(false);
            lineRendererSpline.enabled = false;
            generatedRoadPathParent.gameObject.SetActive(false);
        }

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
                    CreatePointAtPos( hit.point);
                }
            }
        }
    }

    public void CreatePointInTheMiddleBeforeLast()
    {
        if (sourcePointsTransformParent.childCount < 2)
            return;

        Vector3 vPrevBeforeLast = sourcePointsTransformParent.GetChild(sourcePointsTransformParent.childCount - 2).transform.position;
        Vector3 vLastPointPos = sourcePointsTransformParent.GetChild(sourcePointsTransformParent.childCount - 1).transform.position;

        Vector3 vDirFromBeforeToLast = (vLastPointPos - vPrevBeforeLast).normalized;

        float fStepDistanceEvery = 25.0f;

        float fDistFromPrevToLast = (vPrevBeforeLast - vLastPointPos).magnitude;

        if (fDistFromPrevToLast < fStepDistanceEvery)
            fStepDistanceEvery = fDistFromPrevToLast *= 0.5f;

        Vector3 vPosMovingFromBeforeLast = vPrevBeforeLast + vDirFromBeforeToLast * fStepDistanceEvery; // lets create before last every x meters

        if(Vector3.Dot(vDirFromBeforeToLast,(vLastPointPos-vPosMovingFromBeforeLast).normalized) < 0.0F)
        {
            // point will be created in front of last point, we dont allow that
            return;
        }

       var createdObj = CreatePointAtPos(vPosMovingFromBeforeLast);
        createdObj.name = "Middle";
        createdObj.transform.SetSiblingIndex(sourcePointsTransformParent.childCount - 2);

        RefreshSplineLineRenderer();

    }

    private GameObject CreatePointAtPos(Vector3 pos)
    {
        // If the raycast hits something in the scene, we use the hit point to place our sphere.
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = sourcePointsTransformParent;
        sphere.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
        sphere.transform.position = pos + Vector3.up*2.5f*0.5f;

        sphere.AddComponent<PTK_RoadPointCreatedPoint>().parentRoadPointsCreator = this;
        GameObject.DestroyImmediate(sphere.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(sphere, "Create Sphere");

        RefreshSplineLineRenderer();
        return sphere;
    }

    void CreatePoint()
    {

    }

    public void DeleteGeneratedRoadPath()
    {
        // delete generated path in case we edited any of source points
        while (generatedRoadPathParent.childCount > 0)
        {
            GameObject.DestroyImmediate(generatedRoadPathParent.GetChild(0).gameObject);
        }
    }

   public void RefreshSplineLineRenderer()
    {

        if (sourcePointsTransformParent.childCount < 2)
        {
            lineRendererSpline.positionCount = 0;
            return;
        }

        Vector3[] sourcePoints = new Vector3[sourcePointsTransformParent.childCount ]; // +1 so we can add extra point in the end : it will make curve bezier look nicer
        for (int i=0;i< sourcePointsTransformParent.childCount;i++)
        {
            var curPoint = sourcePointsTransformParent.GetChild(i);
            if (i == 0)
            {
                var nextPoint = sourcePointsTransformParent.GetChild(i+1);

                Vector3 vDir = (nextPoint.transform.position - curPoint.position); vDir.y = 0.0f;
                curPoint.transform.forward = vDir.normalized;
            }else
            {
                var prevPoint = sourcePointsTransformParent.GetChild(i - 1);

                Vector3 vDir = (curPoint.position - prevPoint.position  ); vDir.y = 0.0f;
                curPoint.transform.forward = vDir.normalized;
            }

            sourcePoints[i] = sourcePointsTransformParent.GetChild(i).transform.position;
        }


        Vector3[] splinePoints = new Vector3[0];
        var curve2 = new SplineCurve(sourcePoints, false, SplineType.Chordal, tension: 1.0F);
        var len = curve2.GetLength();
        splinePoints = curve2.GetPoints((int)(len * 5));


        if (lineRendererSpline.positionCount != splinePoints.Length)
        {
            lineRendererSpline.positionCount = splinePoints.Length;
        }

        lineRendererSpline.SetPositions(splinePoints);
    }
#endif
}

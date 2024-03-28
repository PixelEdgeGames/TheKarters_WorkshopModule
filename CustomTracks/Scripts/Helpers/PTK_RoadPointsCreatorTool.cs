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
    public Transform generatedRoadMeshParent;
    public Material generatedMeshDefaultMaterial;
    public Material wallColliderMaterial;
    public Material alwaysRespawnCollider;
    public LineRenderer lineRendererSpline;

    [HideInInspector]
    public bool bEditor_CreateSourcePoints = false;
    [HideInInspector]
    public bool bEditor_AutoGenerateMesh = false;
    [HideInInspector]
    public bool bMeshRepeatUvY = true;
    [HideInInspector]
    public EMeshType eMeshType = EMeshType.E_WALL_LEFT;
    [HideInInspector]
    public LayerMask targetMeshModelLayer = 10;
    [HideInInspector]
    public float fNewPointTargetHeight = 7.0f;
    public enum EMeshType
    {
        E_WALL_LEFT,
        E_WALL_RIGHT
    }

    GameObject generatedMeshParent;
    MeshFilter generatedMeshFilter;
    // Start is called before the first frame update
    void Awake()
    {
        if (Application.isPlaying == true)
        {
            if(generatedMeshParent != null)
            {
              var modRaceTrack = GameObject.FindObjectOfType<PTK_ModTrack>();

                // to ensure mesh won't be deleted
                if (modRaceTrack != null)
                    generatedMeshParent.transform.parent = modRaceTrack.extraCollidersParent.transform;
                else
                    generatedMeshParent.transform.parent = null;
            }

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

    public void GenerateRoadPath()
    {
        while(generatedRoadPathParent.childCount > 0)
        {
            GameObject.DestroyImmediate(generatedRoadPathParent.GetChild(0).gameObject);
        }

        if (lineRendererSpline.positionCount < 2)
            return;

        GameObject roadPath = new GameObject("Road Path " + this.name.Replace("PTK_RoadPointsCreatorTool",""));
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

        if (Selection.activeGameObject == this.gameObject || (Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<PTK_RoadPointsCreatorTool>() == this))
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

        var createdPoint = sphere.AddComponent<PTK_RoadPointCreatedPoint>();
        createdPoint.parentRoadPointsCreator = this;
        createdPoint.fMeshSizeInPoint = fNewPointTargetHeight;

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

    public void DeleteGeneratedMesh()
    {
        if (generatedMeshParent != null)
            GameObject.DestroyImmediate(generatedMeshParent);
    }
   public void RefreshSplineLineRenderer()
    {

        if (sourcePointsTransformParent.childCount < 2)
        {
            lineRendererSpline.positionCount = 0;
            return;
        }

        Vector3[] sourcePoints = new Vector3[sourcePointsTransformParent.childCount ]; // +1 so we can add extra point in the end : it will make curve bezier look nicer
        Vector3[] sourcePointsHeights = new Vector3[sourcePointsTransformParent.childCount]; // +1 so we can add extra point in the end : it will make curve bezier look nicer
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
            sourcePointsHeights[i] = sourcePointsTransformParent.GetChild(i).transform.position;
            sourcePointsHeights[i].x = sourcePointsTransformParent.GetChild(i).GetComponent<PTK_RoadPointCreatedPoint>().fMeshAngleInPoint;
            sourcePointsHeights[i].y = sourcePointsTransformParent.GetChild(i).GetComponent<PTK_RoadPointCreatedPoint>().fMeshSizeInPoint;
        }


        Vector3[] splinePoints = new Vector3[0];
        var curve2 = new SplineCurve(sourcePoints, false, SplineType.Chordal, tension: 1.0F);
        var len = curve2.GetLength();
        splinePoints = curve2.GetPoints((int)(len *1));


        Vector3[] splinePointsMeshHeights = new Vector3[0];
        if(bEditor_AutoGenerateMesh == true)
        {
            var curve2MeshHeight = new SplineCurve(sourcePointsHeights, false, SplineType.Chordal, tension: 1.0F);
            var lenMeshHeight = len;
            splinePointsMeshHeights = curve2MeshHeight.GetPoints((int)(lenMeshHeight * 1));
        }

        if (lineRendererSpline.positionCount != splinePoints.Length)
        {
            lineRendererSpline.positionCount = splinePoints.Length;
        }

        lineRendererSpline.SetPositions(splinePoints);

        if(bEditor_AutoGenerateMesh == true && sourcePoints.Length >=2)
        {
            RefreshAndGenerateMesh( splinePoints, splinePointsMeshHeights);
        }else
        {
            if (generatedMeshParent != null)
                GameObject.DestroyImmediate(generatedMeshParent);
        }
    }

    MeshRenderer meshRenderer;
    void RefreshAndGenerateMesh(Vector3[] splinePoints, Vector3[] splinePointsMeshHeights)
    {
        if (generatedMeshParent == null)
        {
            generatedMeshParent = new GameObject("Generated_WallMesh");
            generatedMeshParent.transform.parent = generatedRoadMeshParent.transform;
            generatedMeshFilter = generatedMeshParent.AddComponent<MeshFilter>();
            meshRenderer =  generatedMeshParent.AddComponent<MeshRenderer>();
        }

        generatedMeshFilter.gameObject.layer = targetMeshModelLayer;


        if(targetMeshModelLayer == 10)
            meshRenderer.material = wallColliderMaterial;
        else if (targetMeshModelLayer == 30)
            meshRenderer.material = alwaysRespawnCollider;
        else
            meshRenderer.material = generatedMeshDefaultMaterial;

        // add wall mesh generation here
        // use splinePoints as mesh bottom, splinePointsMeshHeights Y component as height from botton to up


        // Initialize mesh data
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        int iEveryVert = 5;
        // Calculate world space distances for UV mapping
        float fTile = 0.5f;


        float fQuadWidthSum = 0.0f;

        Vector3 vPrevTopRight = Vector3.zero;
        for (int i = 0; i < splinePoints.Length - iEveryVert; i+= iEveryVert)
        {

            // Define quad corners
            Vector3 bottomLeft = splinePoints[i]- Vector3.up*3.0f;
            Vector3 bottomRight = splinePoints[i + iEveryVert] - Vector3.up * 3.0f;
            Vector3 topLeft = bottomLeft + Vector3.up * splinePointsMeshHeights[i].y;
            Vector3 topRight = bottomRight + Vector3.up * splinePointsMeshHeights[i + iEveryVert].y;


            // pitch angle

            float fAnglePoint1 = splinePointsMeshHeights[i].x;
            float fAnglePoint2 = splinePointsMeshHeights[i + iEveryVert].x;

            if (eMeshType == EMeshType.E_WALL_LEFT)
                fAnglePoint1 = fAnglePoint2 = -5;

            if (eMeshType == EMeshType.E_WALL_RIGHT)
                fAnglePoint1 = fAnglePoint2 = 5;


            // Calculate the rotation axis for each segment (perpendicular to the up direction and segment direction)
            Vector3 segmentDirection = (bottomRight - bottomLeft).normalized;

            // Apply rotation to top vertices
            Quaternion rotationPoint1 = Quaternion.AngleAxis(fAnglePoint1 , segmentDirection);
            Quaternion rotationPoint2 = Quaternion.AngleAxis(fAnglePoint2 , segmentDirection);
            topLeft = bottomLeft + rotationPoint1 * (topLeft - bottomLeft);
            topRight = bottomRight + rotationPoint2 * (topRight - bottomRight);

            // to ensure vertices are connected
            if (vPrevTopRight != Vector3.zero)
                topLeft = vPrevTopRight;

            vPrevTopRight = topRight;

            // Add vertices for the quad
            int startVertexIndex = vertices.Count;


            Vector3 vWidth = bottomLeft - bottomRight; vWidth.y = 0.0f;
            float quadWidth = vWidth.magnitude * fTile;


            float fUvY_1 = bMeshRepeatUvY == true ? splinePointsMeshHeights[i].y * fTile : 1;
            float fUvY_2 = bMeshRepeatUvY == true ? splinePointsMeshHeights[i + iEveryVert].y * fTile : 1;


            if (eMeshType == EMeshType.E_WALL_LEFT)
            {
                vertices.Add(bottomLeft);
                vertices.Add(topLeft);
                vertices.Add(topRight);
                vertices.Add(bottomRight);


                // Add UVs based on world space distances, ensuring texture repeats based on width
                uvs.Add(new Vector2(fQuadWidthSum, 0));
                uvs.Add(new Vector2(fQuadWidthSum, fUvY_1));
                uvs.Add(new Vector2(fQuadWidthSum + quadWidth, fUvY_2)); // Repeat based on the ratio of height to width
                uvs.Add(new Vector2(fQuadWidthSum + quadWidth, 0)); // Repeat once across the width
            }
            else if (eMeshType == EMeshType.E_WALL_RIGHT)
            {
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
                vertices.Add(topRight);
                vertices.Add(topLeft);


                // Add UVs based on world space distances, ensuring texture repeats based on width
                uvs.Add(new Vector2(fQuadWidthSum, 0));
                uvs.Add(new Vector2(fQuadWidthSum + quadWidth, 0)); // Repeat once across the width
                uvs.Add(new Vector2(fQuadWidthSum + quadWidth, fUvY_2)); // Repeat based on the ratio of height to width
                uvs.Add(new Vector2(fQuadWidthSum, fUvY_1));
            }





            fQuadWidthSum += quadWidth;


            // Add triangles
            triangles.Add(startVertexIndex);
            triangles.Add(startVertexIndex + 1);
            triangles.Add(startVertexIndex + 2);
            triangles.Add(startVertexIndex);
            triangles.Add(startVertexIndex + 2);
            triangles.Add(startVertexIndex + 3);
        }

        // Create and assign mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // Recalculate normals for proper lighting
        generatedMeshFilter.mesh = mesh;

        if (meshRenderer.GetComponent<MeshCollider>() == null)
            meshRenderer.gameObject.AddComponent<MeshCollider>();

            meshRenderer.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

    }

    Vector3 RotatePointAroundAxis(Vector3 point, float angle, Vector3 axis)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
        return rotation * point;
    }
#endif
}

using CurveLib.Curves;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_ModPathsCreator : MonoBehaviour
{
    [HideInInspector]
    public bool bPathGenerationSuccess = false;

    [SerializeField]
    Transform mainPath_BeforeFinishLineTransform;
    [HideInInspector]
    public PTK_ModPathPoint mainPath_BeforeFinishLinePathPoint;

    [SerializeField]
    Transform mainPath_AfterFinishLinePointTransform;
    [HideInInspector]
    public PTK_ModPathPoint mainPath_AfterFinishLinePathPoint;

    [Header("Alternative Roads - Enable Preview Inside")]
    public List<CRaceTrackPathAlternative> alternativeRaceTrackPaths = new List<CRaceTrackPathAlternative>();

    [System.Serializable]
    public class CRaceTrackPathAlternative
    {
        [System.Serializable]
        public class CUseSegmentSettings
        {
            public bool bUseBreach = false;
            [Header("Entry")]
            public int iEntryRemoveBackward = 0;
            public int iEntryRemoveForward = 0;
            [Header("Exit")]
            public int iExitRemoveForward = 0;
            public int iExitRemoveBackward = 0;
            [HideInInspector]
            public CMainRoadPathBreach roadPathBreach = null;
        }

        public string strName = "Path";
        public bool bDisplayPathBezierPreview = false;
        [Header("2 - means it will be available from lap 2/3")]
       [HideInInspector]
        public int iEnabledFromLapVisibleNr = 1;

        // cant use array because unity UI system throws errors for some reason
        [Header("Alternative road Used Breach Paths - Enable inside")]
        public CUseSegmentSettings segment1 = new CUseSegmentSettings();
        public CUseSegmentSettings segment2 = new CUseSegmentSettings();
        public CUseSegmentSettings segment3 = new CUseSegmentSettings();
        public CUseSegmentSettings segment4 = new CUseSegmentSettings();
        public CUseSegmentSettings segment5 = new CUseSegmentSettings();
        public CUseSegmentSettings segment6 = new CUseSegmentSettings();
        public CUseSegmentSettings segment7 = new CUseSegmentSettings();
        public CUseSegmentSettings segment8 = new CUseSegmentSettings();
        public CUseSegmentSettings segment9 = new CUseSegmentSettings();
        public CUseSegmentSettings segment10 = new CUseSegmentSettings();

        public CUseSegmentSettings[] bUseSegmentIndex
        {
            get
            {
                return new CUseSegmentSettings[] { segment1 , segment2 , segment3 , segment4 , segment5,
                                                    segment6,segment7,segment8,segment9,segment10};
            }
        }
        [HideInInspector]
        public List<PTK_ModPathPoint> roadPath = new List<PTK_ModPathPoint>();
    }

    [System.Serializable]
    public class CMainRoadPathBreach
    {
        [HideInInspector]
        public int iEnabledFromLapVisibleNr = -1;
        public List<PTK_ModPathPoint> breachPoints = new List<PTK_ModPathPoint>();
        public CMainRoadPathBreachConnection breach_Entry = new CMainRoadPathBreachConnection();
        public CMainRoadPathBreachConnection breach_Exit = new CMainRoadPathBreachConnection();

        [System.Serializable]
        public class CMainRoadPathBreachConnection
        {
            public PTK_ModPathPoint breachRoad_Point;
            public PTK_ModPathPoint connectedToMain_Point;
        }
    }

    [Space(30)]
    [Header("--------- IGNORE -----------")]
    [Space(30)]
    public Transform sourcePointsParent;

    [Header("Generated Paths Parents")]
    public Transform raceCalcPathsParent;
    public Transform aiBezierPathsParent;

    [HideInInspector]
    public List<PTK_ModPathPoint> mainRoadPath = new List<PTK_ModPathPoint>();



    // UNITY EDITOR - USED FOR GENERATE
    public LineRenderer linerRenderer;
    public LineRenderer linerRendererTrackWidth_Right;
    public LineRenderer linerRendererTrackWidth_Left;
    [HideInInspector]
    public List<CMainRoadPathBreach> mainRoadPathBreachList = new List<CMainRoadPathBreach>();

    public PTK_RoadBlockerInfo roadBlockerHiddenOnLap2;
    public PTK_RoadBlockerInfo roadBlockerHiddenOnLap3;
    public PTK_RoadBlockerInfo roadBlockerHiddenOnLap4Plus;

    public Material pointNormalMaterial;
    public Material pointRespawnBlockedMaterial;
    // Start is called before the first frame update

    public int GetEnabledFromHudLapNrBasedOnSegmentUnlockLapNr(int iSegmentIndex)
    {
        if (roadBlockerHiddenOnLap4Plus.roadSegmentsIsAffected[iSegmentIndex] == true)
            return 4;

        if (roadBlockerHiddenOnLap3.roadSegmentsIsAffected[iSegmentIndex] == true)
            return 3;

        if (roadBlockerHiddenOnLap2.roadSegmentsIsAffected[iSegmentIndex] == true)
            return 2;

        return -1;
    }
    void Start()
    {
        if(Application.isPlaying == true)
        {
            SetRoadPreviewVisible(false);
        }
    }

    void SetRoadPreviewVisible(bool bVisible)
    {
        linerRenderer.enabled = bVisible;
        linerRendererTrackWidth_Right.enabled = bVisible;
        linerRendererTrackWidth_Left.enabled = bVisible;
        sourcePointsParent.gameObject.SetActive(bVisible);
    }
#if UNITY_EDITOR

    private void OnEnable()
    {
        if (Application.isPlaying == false)
        {
            UnityEditor.SceneView.duringSceneGui += DuringSceneGui;
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying == false)
        {
            SceneView.duringSceneGui -= DuringSceneGui;
        }
    }

    private void DuringSceneGui(SceneView obj)
    {
        if (Application.isPlaying == true)
            return;

        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<PTK_ModTrack>() != null)
        {
            bool bJustShowed = sourcePointsParent.gameObject.activeInHierarchy == false;
           
            SetRoadPreviewVisible(true);

            if (bJustShowed)
            {
                GeneratePathsEditor();
            }
        }
        else
        {
            SetRoadPreviewVisible(false);
        }
    }


#endif

    Vector3 vLastPointPos = Vector3.zero;
    float fLastWidth = -1.0f;
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if(Application.isPlaying == false)
        {
            GameObject selectedGameObject = UnityEditor.Selection.activeGameObject;

            if (selectedGameObject != null && selectedGameObject.GetComponentInParent<PTK_ModPathsCreator>())
            {
                PTK_ModPathPoint modPathPoint = selectedGameObject.GetComponent<PTK_ModPathPoint>();
                if (modPathPoint != null && modPathPoint.fRoadWidthForAI != fLastWidth)
                {
                    GeneratePathsEditor();
                    fLastWidth = modPathPoint.fRoadWidthForAI;
                }

                if (Vector3.Magnitude( selectedGameObject.transform.position- vLastPointPos) > 0.1F)
                {
                    GeneratePathsEditor();
                    vLastPointPos = selectedGameObject.transform.position;
                }
            }
        }
#endif
    }


    CRaceTrackPathAlternative lastRenderedAlternativePath = null;
    public void RefreshPathLineRenderer()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == true)
        {
            return;
        }
        
        Color colorLine = Color.white;

        List<PTK_ModPathPoint> pointsToPresent = mainRoadPath;

        int iAlternativePathToShowCount = 0;
        CRaceTrackPathAlternative alternativePathToShow = null;
        for (int iAlternativeRoadIndex = 0; iAlternativeRoadIndex < alternativeRaceTrackPaths.Count; iAlternativeRoadIndex++)
        {
            CRaceTrackPathAlternative alternativeRoad = alternativeRaceTrackPaths[iAlternativeRoadIndex];

            if (alternativeRoad.bDisplayPathBezierPreview == true)
            {

                iAlternativePathToShowCount++;

                if (iAlternativePathToShowCount == 1)
                    alternativePathToShow = alternativeRoad;
                else 
                {
                    if (alternativeRoad != lastRenderedAlternativePath)
                        alternativePathToShow = alternativeRoad;
                }
            }
        }


        // user selected more than one path to show - uncheck other paths (that are different from the previous)
        for (int iAlternativeRoadIndex = 0; iAlternativeRoadIndex < alternativeRaceTrackPaths.Count; iAlternativeRoadIndex++)
        {
            CRaceTrackPathAlternative alternativeRoad = alternativeRaceTrackPaths[iAlternativeRoadIndex];

            alternativeRoad.strName = "Path " + iAlternativeRoadIndex;

            if (alternativeRoad != alternativePathToShow)
            {
                alternativeRoad.bDisplayPathBezierPreview = false;
            }
        }

        if(alternativePathToShow != null)
        {
            pointsToPresent = alternativePathToShow.roadPath;
            lastRenderedAlternativePath = alternativePathToShow;
            alternativePathToShow.strName +=  " - Preview ENABLED";
            colorLine = Color.blue;
        }


        Vector3[] points = pointsToPresent.Select(point => point.transform.position).ToArray();
        Vector3[] splinePoints = new Vector3[0];
        
        if (points.Length > 0)
        {
            var curve2 = new SplineCurve(points, false, SplineType.Chordal, tension: 1.0F);
            var len = curve2.GetLength();
            splinePoints = curve2.GetPoints((int)(len * 1));
        }

        linerRenderer.startColor = linerRenderer.endColor = colorLine;
        linerRenderer.sharedMaterial.color = colorLine;


        if (linerRenderer.positionCount != splinePoints.Length)
        {
            linerRenderer.positionCount = splinePoints.Length;
        }

        linerRenderer.SetPositions(splinePoints);


        if (linerRendererTrackWidth_Right.positionCount != points.Length * 1)
            linerRendererTrackWidth_Right.positionCount = points.Length * 1;

        for (int i = 0; i < points.Length; i++)
        {
            linerRendererTrackWidth_Right.SetPosition(i * 1 + 0, pointsToPresent[i].transform.position + pointsToPresent[i].transform.right * pointsToPresent[i].fRoadWidthForAI * 0.5f);
        }


        if (linerRendererTrackWidth_Left.positionCount != points.Length * 1)
            linerRendererTrackWidth_Left.positionCount = points.Length * 1;

        for (int i = 0; i < points.Length; i++)
        {
            linerRendererTrackWidth_Left.SetPosition(i * 1 + 0, pointsToPresent[i].transform.position - pointsToPresent[i].transform.right * pointsToPresent[i].fRoadWidthForAI * 0.5f);
        }
#endif
    }

#if UNITY_EDITOR
     void CreatePathPointsEditor()
    {
        if (Application.isPlaying == true)
        {
            return;
        }


        List<List<PTK_ModPathPoint>> breachPointsSegments = new List<List<PTK_ModPathPoint>>();
        List<int> breachEnabledFromHUDLapNr = new List<int>();

        mainRoadPath = new List<PTK_ModPathPoint>();

        for (int iPath=0;iPath< sourcePointsParent.childCount;iPath++)
        {
            if(iPath > 0)
            {
                breachPointsSegments.Add(new List<PTK_ModPathPoint>());

                // brach is numbered from path 1 (we are not including 0 which is main path
                breachEnabledFromHUDLapNr.Add(GetEnabledFromHudLapNrBasedOnSegmentUnlockLapNr(iPath - 1));
            }

            Transform pointsParent = sourcePointsParent.GetChild(iPath);

            if (pointsParent.childCount == 1)
                pointsParent = pointsParent.GetChild(0); // there is object that have points inside them (points are not in this object but inside this one)

            PTK_ModPathPoint lastPathPoint = null;
            for (int iPathPointNr = 0; iPathPointNr < pointsParent.childCount;iPathPointNr++)
            {
                Transform childPathPoint = pointsParent.GetChild(iPathPointNr);

                childPathPoint.name = "Point " + iPathPointNr;

                PTK_ModPathPoint pathPointCurrent = childPathPoint.GetComponent<PTK_ModPathPoint>();

                if (pathPointCurrent == null)
                    pathPointCurrent = childPathPoint.gameObject.AddComponent<PTK_ModPathPoint>();


                if (pathPointCurrent.fDistanceFromFinishLine != -1)
                    childPathPoint.name += " --- Dist " + ((int)(pathPointCurrent.fDistanceFromFinishLine)).ToString() + "m";

                if(iPathPointNr == (pointsParent.childCount-1))
                {
                    // last point
                    if(pathPointCurrent.fSegmentLengthDiffFromMain != 0)
                    {
                        if(pathPointCurrent.fSegmentLengthDiffFromMain < 0)
                        {
                            pathPointCurrent.name += " ( SHORTER than main: " + (-pathPointCurrent.fSegmentLengthDiffFromMain).ToString() + " )";
                        }else if (pathPointCurrent.fSegmentLengthDiffFromMain > 0)
                        {
                            pathPointCurrent.name += " ( LONGER than main: " + (pathPointCurrent.fSegmentLengthDiffFromMain).ToString() + " )";
                        }
                    }
                }
                // performance reasons in editor - we won't render too many spheres
                LODGroup lodGroup = childPathPoint.gameObject.GetComponent<LODGroup>();
                if (lodGroup  == null)
                {
                    lodGroup = childPathPoint.gameObject.AddComponent<LODGroup>();

                    Renderer[] renderers = childPathPoint.GetComponentsInChildren<Renderer>();
                    LOD[] lods = new LOD[1];
                    lods[0] = new LOD(0.005f, renderers); // Set the LOD's screen relative transition height to a very low value

                    pathPointCurrent.pointRenderers = renderers;

                    lodGroup.SetLODs(lods);

                    lodGroup.size = 10; // Set the LODGroup's size to control visibility distance
                    lodGroup.RecalculateBounds();
                }

                for (int iRenderer = 0; iRenderer < pathPointCurrent.pointRenderers.Length; iRenderer++)
                {
                    pathPointCurrent.pointRenderers[iRenderer].sharedMaterial = pathPointCurrent.bNoGroundBelow_DisableRespawnOnPoint == false ?  pointNormalMaterial : pointRespawnBlockedMaterial;
                }

                pathPointCurrent.prevPoint = lastPathPoint;

                if (lastPathPoint != null)
                    lastPathPoint.nextPoint = pathPointCurrent;

                if (iPath == 0)
                {
                    pathPointCurrent.bIsMainRoadPoint = true;
                    mainRoadPath.Add(pathPointCurrent);
                }
                else
                {
                    breachPointsSegments[iPath - 1].Add(pathPointCurrent);
                }

                lastPathPoint = pathPointCurrent;
            }
        }

        for(int i=0;i< breachPointsSegments.Count;i++)
        {
            if(breachPointsSegments[i].Count == 0)
            {
                breachPointsSegments.RemoveAt(i);
                breachEnabledFromHUDLapNr.RemoveAt(i);
                i--;
            }
        }

        if(mainPath_AfterFinishLinePointTransform != null)
            mainPath_AfterFinishLinePointTransform.name += " AfterFinishLine";

        if(mainPath_BeforeFinishLineTransform != null)
            mainPath_BeforeFinishLineTransform.name += " BeforeFinishLine";


        mainRoadPathBreachList.Clear();

        for(int i=0;i< breachPointsSegments.Count;i++)
        {
            CMainRoadPathBreach breachInfo = new CMainRoadPathBreach();

            breachInfo.iEnabledFromLapVisibleNr = breachEnabledFromHUDLapNr[i];
            breachInfo.breachPoints = breachPointsSegments[i];
            breachInfo.breach_Entry.breachRoad_Point = breachPointsSegments[i][0];
            breachInfo.breach_Exit.breachRoad_Point = breachPointsSegments[i][breachPointsSegments[i].Count-1];

            float fClosestDistanceToEntry = 999999.0f;
            float fClosestDistanceToExit = 999999.0f;

            for(int iMainPoint = 0; iMainPoint < mainRoadPath.Count;iMainPoint++)
            {
                float fDistEntry = Vector3.Magnitude(breachInfo.breach_Entry.breachRoad_Point.transform.position - mainRoadPath[iMainPoint].transform.position);
                if (fDistEntry < fClosestDistanceToEntry)
                {
                    fClosestDistanceToEntry = fDistEntry;
                    breachInfo.breach_Entry.connectedToMain_Point = mainRoadPath[iMainPoint];
                }


                float fDistExit = Vector3.Magnitude(breachInfo.breach_Exit.breachRoad_Point.transform.position - mainRoadPath[iMainPoint].transform.position);
                if (fDistExit < fClosestDistanceToExit)
                {
                    fClosestDistanceToExit = fDistExit;
                    breachInfo.breach_Exit.connectedToMain_Point = mainRoadPath[iMainPoint];
                }
            }
            mainRoadPathBreachList.Add(breachInfo);
        }
    }

    [EasyButtons.Button]
    public void GeneratePathsEditor()
    {
        if (Application.isPlaying == true)
        {
            return;
        }

        bPathGenerationSuccess = false;


        CreatePathPointsEditor();


        if (mainPath_BeforeFinishLineTransform == null || mainPath_AfterFinishLinePointTransform == null)
        {
            // if we already have road path and before and after finish line is not selected - show error
            if(mainRoadPath.Count > 0)
            {
                Debug.LogError("Please assign before finish line and after finish line points from main road");
            }
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return;
        }


        for (int iAlternativeRoadIndex=0;iAlternativeRoadIndex< alternativeRaceTrackPaths.Count;iAlternativeRoadIndex++)
        {
            CRaceTrackPathAlternative alternativeRoad = alternativeRaceTrackPaths[iAlternativeRoadIndex];

            alternativeRoad.iEnabledFromLapVisibleNr = -1;

            alternativeRoad.roadPath.Clear();

            List<CRaceTrackPathAlternative.CUseSegmentSettings> usedBreachPoints = new List<CRaceTrackPathAlternative.CUseSegmentSettings>();

            for(int iBreachIndex = 0; iBreachIndex < mainRoadPathBreachList.Count;iBreachIndex++)
            {
                if(alternativeRoad.bUseSegmentIndex.Length > iBreachIndex &&  alternativeRoad.bUseSegmentIndex[iBreachIndex].bUseBreach == true)
                {
                    int iEnabledInHUDLapNr = GetEnabledFromHudLapNrBasedOnSegmentUnlockLapNr(iBreachIndex);

                    alternativeRoad.iEnabledFromLapVisibleNr = Mathf.Max(alternativeRoad.iEnabledFromLapVisibleNr, iEnabledInHUDLapNr);

                    alternativeRoad.bUseSegmentIndex[iBreachIndex].roadPathBreach = mainRoadPathBreachList[iBreachIndex];
                    usedBreachPoints.Add(alternativeRoad.bUseSegmentIndex[iBreachIndex]);
                }
            }

            int iCurrentRoadBranch = 0;

            PTK_ModPathPoint brachPointExit_SearchForMainPoint = null;
            CRaceTrackPathAlternative.CUseSegmentSettings breachUsedForSearchMainPointExit = null;

            List<PTK_ModPathPoint> pointsMarkedForRemoveBecauseOfOffset = new List<PTK_ModPathPoint>();
            List<int> pointRemoveAmountAndDirection = new List<int>();
            List<int> pointRemoveAmountAndDirectionOpposite = new List<int>();

            for (int iMainRoadPathIndex = 0; iMainRoadPathIndex < mainRoadPath.Count;iMainRoadPathIndex++)
            {
                if(brachPointExit_SearchForMainPoint != null)
                {
                    if(brachPointExit_SearchForMainPoint == mainRoadPath[iMainRoadPathIndex])
                    {
                        // we are on brach exit point - lets continue using main road

                        if (breachUsedForSearchMainPointExit.iExitRemoveForward > 0 || breachUsedForSearchMainPointExit.iExitRemoveBackward > 0)
                        {
                            pointsMarkedForRemoveBecauseOfOffset.Add(mainRoadPath[iMainRoadPathIndex]);
                            pointRemoveAmountAndDirection.Add(breachUsedForSearchMainPointExit.iExitRemoveForward);
                            pointRemoveAmountAndDirectionOpposite.Add(-breachUsedForSearchMainPointExit.iExitRemoveBackward);
                        }

                        brachPointExit_SearchForMainPoint = null;
                        breachUsedForSearchMainPointExit = null;

                        iCurrentRoadBranch++;
                    }else
                    {
                        // we are waiting to reach main point road that brach is ending
                        continue;
                    }
                }

                if(usedBreachPoints.Count > iCurrentRoadBranch  && usedBreachPoints[iCurrentRoadBranch].roadPathBreach.breach_Entry.connectedToMain_Point == mainRoadPath[iMainRoadPathIndex])
                {
                    CRaceTrackPathAlternative.CUseSegmentSettings breachSettings = usedBreachPoints[iCurrentRoadBranch];
                    
                    if (breachSettings.iEntryRemoveBackward > 0 || breachSettings.iEntryRemoveForward > 0)
                    {
                        pointsMarkedForRemoveBecauseOfOffset.Add(alternativeRoad.roadPath[alternativeRoad.roadPath.Count-1]);
                        pointRemoveAmountAndDirection.Add(-breachSettings.iEntryRemoveBackward);
                        pointRemoveAmountAndDirectionOpposite.Add(breachSettings.iEntryRemoveForward);
                    }

                    // we are using this breach, and entry point of this brach is current main point - instead of using main points we need to use breach points up to the exit main point
                    alternativeRoad.roadPath.AddRange(breachSettings.roadPathBreach.breachPoints);
                    brachPointExit_SearchForMainPoint = breachSettings.roadPathBreach.breach_Exit.connectedToMain_Point;
                    breachUsedForSearchMainPointExit = breachSettings;
                }
                else
                {
                    alternativeRoad.roadPath.Add(mainRoadPath[iMainRoadPathIndex]);
                }
            }


            ///////////////
            // removing point
            //////////////
            ///
            // removing points after all points were added, becasue the alternative road can have multiple segments, not only main points - we want to remove points from segment start/end regardless if it is main road or another segment
            List<PTK_ModPathPoint> removePointsBecauseOfOffset = new List<PTK_ModPathPoint>();
            for(int i=0;i< alternativeRoad.roadPath.Count;i++)
            {
                for(int iRemovedPoint = 0; iRemovedPoint < pointsMarkedForRemoveBecauseOfOffset.Count;iRemovedPoint++ )
                {
                    if (alternativeRoad.roadPath[i] == pointsMarkedForRemoveBecauseOfOffset[iRemovedPoint])
                    {
                        int iRemovePoints = pointRemoveAmountAndDirection[iRemovedPoint];

                        int iIndexRemoved = 0;
                        while(iRemovePoints > 0) // next points
                        {
                            if(alternativeRoad.roadPath.Count > (i + iIndexRemoved))
                                 removePointsBecauseOfOffset.Add(alternativeRoad.roadPath[i + iIndexRemoved]);

                            iIndexRemoved++;
                            iRemovePoints--;
                        }

                        iIndexRemoved = 0;
                        while (iRemovePoints < 0) // previous points
                        {
                            if ( (i - iIndexRemoved) > 0)
                                removePointsBecauseOfOffset.Add(alternativeRoad.roadPath[i - iIndexRemoved]);

                            iIndexRemoved++;

                            iRemovePoints++;
                        }

                        // opposite
                        iRemovePoints = pointRemoveAmountAndDirectionOpposite[iRemovedPoint];

                        iIndexRemoved = 0;
                        while (iRemovePoints > 0) // next points
                        {
                            if (alternativeRoad.roadPath.Count > (i + iIndexRemoved+1))
                                removePointsBecauseOfOffset.Add(alternativeRoad.roadPath[i + iIndexRemoved+1]);

                            iIndexRemoved++;
                            iRemovePoints--;
                        }

                        iIndexRemoved = 0;
                        while (iRemovePoints < 0) // previous points
                        {
                            if ((i - iIndexRemoved) > 0)
                                removePointsBecauseOfOffset.Add(alternativeRoad.roadPath[i - iIndexRemoved]);

                            iIndexRemoved++;

                            iRemovePoints++;
                        }
                    }
                }
            }

            for(int i=0;i< removePointsBecauseOfOffset.Count;i++)
            {
                alternativeRoad.roadPath.Remove(removePointsBecauseOfOffset[i]);
            }
        }


        if(mainPath_BeforeFinishLineTransform.GetComponent<PTK_ModPathPoint>() == null || mainPath_AfterFinishLinePointTransform.GetComponent<PTK_ModPathPoint>() == null)
        {
            Debug.LogError("Before and after finish line points should be points from road path!");
            return;
        }

        mainPath_BeforeFinishLinePathPoint = mainPath_BeforeFinishLineTransform.GetComponent<PTK_ModPathPoint>();
        mainPath_AfterFinishLinePathPoint = mainPath_AfterFinishLinePointTransform.GetComponent<PTK_ModPathPoint>();

         bPathGenerationSuccess = true;
        RefreshPathLineRenderer();


        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }



    [EasyButtons.Button]
    public void RefreshPointsDistanceFromFinishLine()
    {
        if(mainPath_BeforeFinishLinePathPoint == null)
        {
            Debug.LogError("Point before finish line is not assigned yet - cant calculate distances");
            return;
        }

        int iStartBeforeFinishLineIndex = 0;
        for (int i = 0; i < mainRoadPath.Count; i++)
        {
            mainRoadPath[i].fDistanceFromFinishLine = -1.0f;

            if (mainRoadPath[i] == mainPath_BeforeFinishLinePathPoint)
                iStartBeforeFinishLineIndex = i + 1;
        }

        for (int iBreach = 0; iBreach < mainRoadPathBreachList.Count; iBreach++)
        {
            for (int iPoint = 0; iPoint < mainRoadPathBreachList[iBreach].breachPoints.Count; iPoint++)
            {
                mainRoadPathBreachList[iBreach].breachPoints[iPoint].fDistanceFromFinishLine = -1;
                mainRoadPathBreachList[iBreach].breachPoints[iPoint].fSegmentLengthDiffFromMain = -1;
            }
        }


        float fDistSum = 0;
        for (int i = 0; i < mainRoadPath.Count; i++)
        {
            int iIndexFromStartPoint = (i + iStartBeforeFinishLineIndex) % mainRoadPath.Count;
            int iNextPoint = (iIndexFromStartPoint + 1) % mainRoadPath.Count; ;

            Vector3 vDistNoY = (mainRoadPath[iIndexFromStartPoint].transform.position - mainRoadPath[iNextPoint].transform.position);
            vDistNoY.y = 0.0f;

            fDistSum += vDistNoY.magnitude;
            mainRoadPath[iIndexFromStartPoint].fDistanceFromFinishLine = fDistSum;
        }

        // breach uses distances from entry/exit (player will just finish it faster)
        for (int iBreach = 0; iBreach < mainRoadPathBreachList.Count; iBreach++)
        {
            float fDistEntry = mainRoadPathBreachList[iBreach].breach_Entry.connectedToMain_Point.fDistanceFromFinishLine;
            float fDistExit = mainRoadPathBreachList[iBreach].breach_Exit.connectedToMain_Point.fDistanceFromFinishLine;


            fDistSum = fDistEntry;

            mainRoadPathBreachList[iBreach].breachPoints[0].fDistanceFromFinishLine = fDistSum;

            for (int iPoint = 0; iPoint < mainRoadPathBreachList[iBreach].breachPoints.Count; iPoint++)
            {
                if(iPoint > 0)
                {
                    Vector3 vDistNoY = (mainRoadPathBreachList[iBreach].breachPoints[iPoint].transform.position - mainRoadPathBreachList[iBreach].breachPoints[iPoint-1].transform.position);
                    vDistNoY.y = 0.0f;
                    fDistSum += vDistNoY.magnitude;
                    mainRoadPathBreachList[iBreach].breachPoints[iPoint].fDistanceFromFinishLine = fDistSum; // real distance
                }

                float fDistanceAlignedToEntryAndExit = Mathf.Lerp(fDistEntry, fDistExit, (iPoint + 1) / (float)mainRoadPathBreachList[iBreach].breachPoints.Count); // distance aligned to entry and exit main road, this way dist won't change a lot during entry, player will just move on it faster
                mainRoadPathBreachList[iBreach].breachPoints[iPoint].fDistanceFromMain_EntryExitMatched = fDistanceAlignedToEntryAndExit;

                mainRoadPathBreachList[iBreach].breachPoints[iPoint].fSegmentLengthDiffFromMain =   mainRoadPathBreachList[iBreach].breachPoints[iPoint].fDistanceFromFinishLine - fDistanceAlignedToEntryAndExit; // lets use this to present how much this segment is shorter from main road
            }
        }

    }
    public bool HavePathPointTransformsForFinishLineAssigned()
    {
        if (mainPath_BeforeFinishLineTransform == null)
            return false;

        if (mainPath_AfterFinishLinePointTransform == null)
            return false;

        return true;

    }

#endif
}

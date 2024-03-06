using CurveLib.Curves;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_ModPathsCreator : MonoBehaviour
{
    [HideInInspector]
    public bool bPathGenerationSuccess = false;
    public PTK_ModPathPoint mainPath_BeforeFinishLinePoint;
    public PTK_ModPathPoint mainPath_AfterFinishLinePoint;

    [Header("Breach Points & Alternative Roads (0 is main)")]
    public List<CRaceTrackPathAlternative> alternativeRaceTrackPaths = new List<CRaceTrackPathAlternative>();

    [System.Serializable]
    public class CRaceTrackPathAlternative
    {
        [System.Serializable]
        public class CUseBreachSettings
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

        public bool bDisplayPathAlternative = false;
        [Header("2 - means it will be available from lap 2/3")]
        public int iEnabledFromLapVisibleNr = 1;

        // cant use array because unity UI system throws errors for some reason
        [Header("Alternative road Used Breach Paths")]
        public CUseBreachSettings breach1 = new CUseBreachSettings();
        public CUseBreachSettings breach2 = new CUseBreachSettings();
        public CUseBreachSettings breach3 = new CUseBreachSettings();
        public CUseBreachSettings breach4 = new CUseBreachSettings();
        public CUseBreachSettings breach5 = new CUseBreachSettings();
        public CUseBreachSettings breach6 = new CUseBreachSettings();
        public CUseBreachSettings breach7 = new CUseBreachSettings();
        public CUseBreachSettings breach8 = new CUseBreachSettings();
        public CUseBreachSettings breach9 = new CUseBreachSettings();
        public CUseBreachSettings breach10 = new CUseBreachSettings();

        public CUseBreachSettings[] bUseBreachIndex
        {
            get
            {
                return new CUseBreachSettings[] { breach1 , breach2 , breach3 , breach4 , breach5,
                                                    breach6,breach7,breach8,breach9,breach10};
            }
        }
        [HideInInspector]
        public List<PTK_ModPathPoint> roadPath = new List<PTK_ModPathPoint>();
    }



    [System.Serializable]
    public class CMainRoadPathBreach
    {
        public PTK_ModPathPoint breachRoad_StartPoint;
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
    [HideInInspector]
    public List<CMainRoadPathBreach> mainRoadPathBreachList = new List<CMainRoadPathBreach>();

    [HideInInspector]
    public List<List<PTK_ModPathPoint>> breachPointsSegments = new List<List<PTK_ModPathPoint>>();
    // Start is called before the first frame update

    void Start()
    {
        linerRenderer.enabled = false;
        sourcePointsParent.gameObject.SetActive(false);
    }

    Vector3 vLastPointPos = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if(Application.isPlaying == false)
        {
            GameObject selectedGameObject = UnityEditor.Selection.activeGameObject;

            if (selectedGameObject != null && selectedGameObject.GetComponentInParent<PTK_ModPathsCreator>())
            {
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
        Color colorLine = Color.white;

        List<PTK_ModPathPoint> pointsToPresent = mainRoadPath;

        int iAlternativePathToShowCount = 0;
        CRaceTrackPathAlternative alternativePathToShow = null;
        for (int iAlternativeRoadIndex = 0; iAlternativeRoadIndex < alternativeRaceTrackPaths.Count; iAlternativeRoadIndex++)
        {
            CRaceTrackPathAlternative alternativeRoad = alternativeRaceTrackPaths[iAlternativeRoadIndex];

            if (alternativeRoad.bDisplayPathAlternative == true)
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

            if(alternativeRoad != alternativePathToShow)
            {
                alternativeRoad.bDisplayPathAlternative = false;
            }
        }

        if(alternativePathToShow != null)
        {
            pointsToPresent = alternativePathToShow.roadPath;
            lastRenderedAlternativePath = alternativePathToShow;
            colorLine = Color.blue;
        }


        Vector3[] points = pointsToPresent.Select(point => point.transform.position).ToArray();
        var curve2 = new SplineCurve(points, false, SplineType.Catmullrom, tension: 0.5F);
        var len = curve2.GetLength();
        var ps = curve2.GetPoints((int)(len * 1));

        linerRenderer.startColor = linerRenderer.endColor = colorLine;
        linerRenderer.sharedMaterial.color = colorLine;

        if (linerRenderer.positionCount != ps.Length)
            linerRenderer.positionCount = ps.Length;
        linerRenderer.SetPositions(ps);
    }

#if UNITY_EDITOR
    public void CreatePathPointsEditor()
    {
        breachPointsSegments.Clear();

        mainRoadPath = new List<PTK_ModPathPoint>();
        mainRoadPath.Add(mainPath_BeforeFinishLinePoint);

        for (int iPath=0;iPath< sourcePointsParent.childCount;iPath++)
        {
            if(iPath > 0)
            {
                breachPointsSegments.Add(new List<PTK_ModPathPoint>());
            }

            Transform pointsParent = sourcePointsParent.GetChild(iPath);

            if (pointsParent.childCount == 1)
                pointsParent = pointsParent.GetChild(0); // there is object that have points inside them (points are not in this object but inside this one)

            PTK_ModPathPoint lastPathPoint = null;
            for (int iPathPointNr = 0; iPathPointNr < pointsParent.childCount;iPathPointNr++)
            {
                Transform childPathPoint = pointsParent.GetChild(iPathPointNr);
                PTK_ModPathPoint pathPointCurrent = childPathPoint.GetComponent<PTK_ModPathPoint>();

                if (pathPointCurrent == null)
                    pathPointCurrent = childPathPoint.gameObject.AddComponent<PTK_ModPathPoint>();


                // performance reasons in editor - we won't render too many spheres
                LODGroup lodGroup = childPathPoint.gameObject.GetComponent<LODGroup>();
                if (lodGroup  == null)
                {
                    lodGroup = childPathPoint.gameObject.AddComponent<LODGroup>();

                    Renderer[] renderers = childPathPoint.GetComponentsInChildren<Renderer>();
                    LOD[] lods = new LOD[1];
                    lods[0] = new LOD(0.005f, renderers); // Set the LOD's screen relative transition height to a very low value

                    lodGroup.SetLODs(lods);

                    lodGroup.size = 10; // Set the LODGroup's size to control visibility distance
                    lodGroup.RecalculateBounds();
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
                i--;
            }
        }

        mainRoadPath.Add(mainPath_AfterFinishLinePoint);

        mainRoadPathBreachList.Clear();

        for(int i=0;i< breachPointsSegments.Count;i++)
        {
            CMainRoadPathBreach breachInfo = new CMainRoadPathBreach();

            breachInfo.breachRoad_StartPoint = breachPointsSegments[i][0];
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


        RefreshPathLineRenderer();
    }

    [EasyButtons.Button]
    public void GeneratePathsEditor()
    {
        bPathGenerationSuccess = false;

        CreatePathPointsEditor();

        for(int iAlternativeRoadIndex=0;iAlternativeRoadIndex< alternativeRaceTrackPaths.Count;iAlternativeRoadIndex++)
        {
            CRaceTrackPathAlternative alternativeRoad = alternativeRaceTrackPaths[iAlternativeRoadIndex];

            alternativeRoad.roadPath.Clear();

            List<CRaceTrackPathAlternative.CUseBreachSettings> usedBreachPoints = new List<CRaceTrackPathAlternative.CUseBreachSettings>();

            for(int iBreachIndex = 0; iBreachIndex < mainRoadPathBreachList.Count;iBreachIndex++)
            {
                if(alternativeRoad.bUseBreachIndex.Length > iBreachIndex &&  alternativeRoad.bUseBreachIndex[iBreachIndex].bUseBreach == true)
                {
                    alternativeRoad.bUseBreachIndex[iBreachIndex].roadPathBreach = mainRoadPathBreachList[iBreachIndex];
                    usedBreachPoints.Add(alternativeRoad.bUseBreachIndex[iBreachIndex]);
                }
            }

            int iCurrentRoadBranch = 0;

            PTK_ModPathPoint brachPointExit_SearchForMainPoint = null;
            CRaceTrackPathAlternative.CUseBreachSettings breachUsedForSearchMainPointExit = null;

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
                    CRaceTrackPathAlternative.CUseBreachSettings breachSettings = usedBreachPoints[iCurrentRoadBranch];
                    
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

        bPathGenerationSuccess = true;
        RefreshPathLineRenderer();
    }

#endif
}

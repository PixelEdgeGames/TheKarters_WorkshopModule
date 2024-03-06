using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModPathsCreator : MonoBehaviour
{
    public PTK_ModPathPoint mainPath_BeforeFinishLinePoint;
    public PTK_ModPathPoint mainPath_AfterFinishLinePoint;

    public List<CRaceTrackPathAlternative> alternativeRaceTrackPaths = new List<CRaceTrackPathAlternative>();

    [System.Serializable]
    public class CRaceTrackPathAlternative
    {
        [Header("2 - means it will be available from lap 2/3")]
        public int iEnabledFromLapVisibleNr = 1;

        public List<CMainRoadPathBreach> mainRoadPathBreachList = new List<CMainRoadPathBreach>();
        public class CMainRoadPathBreach
        {
            public CMainRoadPathBreach breach_Start;
            public CMainRoadPathBreach breach_Exit;

            [System.Serializable]
            public class CMainRoadPathBreachConnection
            {
                public PTK_ModPathPoint mainRoadPathFrom;
                public PTK_ModPathPoint alternativeRoadPathTo;
            }
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR
    [EasyButtons.Button]
    public void CreatePathPointsEditor()
    {
        mainRoadPath = new List<PTK_ModPathPoint>();
        mainRoadPath.Add(mainPath_BeforeFinishLinePoint);

        for (int iPath=0;iPath< sourcePointsParent.childCount;iPath++)
        {
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

                pathPointCurrent.prevPoint = lastPathPoint;

                if (lastPathPoint != null)
                    lastPathPoint.nextPoint = pathPointCurrent;

                if (iPath == 0)
                    mainRoadPath.Add(pathPointCurrent);

                lastPathPoint = pathPointCurrent;
            }
        }
        mainRoadPath.Add(mainPath_AfterFinishLinePoint);
    }

    [EasyButtons.Button]
    public void GeneratePathsEditor()
    {
    }

#endif
}

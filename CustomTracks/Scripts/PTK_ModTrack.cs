using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModTrack : MonoBehaviour
{
    [Header("Track Minimap")]
    public Texture2D minimapImage;
    public Vector2 minimapImage_WorldPos_BL;
    public Vector2 minimapImage_WorldPos_TR;

    [Header("Track Loop")]
    public bool bTrackIsLooped = true;

    [Header("Flip Intro animation side")]
    public bool[] bFlipIntroCamFromRightToLeftForPlayer = new bool[8];

    [Space(30)]
    [Header("--- Do not edit variables below ---")]
    [Space(30)]
    public Camera cameraWithEffects;

    [Header("Start Positions")]
    public Transform[] mapStartPositions;
    [Header("Finish Line")]
    public Transform finishLineBoundary;

    [Header("Item Boxes Parent")]
    public Transform ItemBoxesParent;
    [HideInInspector]
    public List<PTK_ModItemBox> itemBoxes = new List<PTK_ModItemBox>();

    [Header("Boostpads")]
    public Transform BoostPadsParent;
    [SerializeField]
    Transform boostpadsParent_1stLapOnly;
    [SerializeField]
    Transform boostpadsParent_2ndLapOnly;
    [SerializeField]
    Transform boostpadsParent_3rdapOnly;
    [SerializeField]
    Transform boostpadsParent_AlwaysVisibley;
    public List<PTK_ModBoostpad> boostpads = new List<PTK_ModBoostpad>();

    [Header("Driving Paths")]
    [SerializeField]
    public PTK_ModPathsCreator modPathsCreator;
    [SerializeField]
    public Transform aiBezierSplinesPaths;
    [Header("RacePositionCalc Paths")]
    [SerializeField]
    public Transform racePositionCalcPathsPointsParent;
    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    bool bIsInitialized = false;
    public void Initialize()
    {
        if (bIsInitialized == true)
            return;

        itemBoxes.AddRange(ItemBoxesParent.GetComponentsInChildren<PTK_ModItemBox>());

        var lap1 = boostpadsParent_1stLapOnly.GetComponentsInChildren<PTK_ModBoostpad>(); foreach (var lap in lap1) lap.iEnabledInLapNr = 1;
        var lap2 = boostpadsParent_2ndLapOnly.GetComponentsInChildren<PTK_ModBoostpad>(); foreach (var lap in lap2) lap.iEnabledInLapNr = 2;
        var lap3 = boostpadsParent_3rdapOnly.GetComponentsInChildren<PTK_ModBoostpad>(); foreach (var lap in lap3) lap.iEnabledInLapNr = 3;
        var lapAll = boostpadsParent_AlwaysVisibley.GetComponentsInChildren<PTK_ModBoostpad>(); foreach (var lap in lapAll) lap.iEnabledInLapNr = -1;

        boostpads.AddRange(lap1);
        boostpads.AddRange(lap2);
        boostpads.AddRange(lap3);
        boostpads.AddRange(lapAll);

        bIsInitialized = true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    [EasyButtons.Button]
    void GenerateModTrackDataConfig()
    {
#if UNITY_EDITOR
        modPathsCreator.GeneratePathsEditor();
#endif
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModTrack : MonoBehaviour
{
    [Header("Music Soundbank Name")]
    public string strMusicSoundBank = "";

    [Header("Track Loop")]
    public bool bTrackIsLooped = true;

    [Header("Flip Intro animation side")]
    public bool[] bFlipIntroCamFromRightToLeftForPlayer = new bool[8];

    [Space(30)]
    [Header("--- Do not edit variables below ---")]
    [Space(30)]
    public Camera cameraWithEffects;

    [Header("Track Minimap")]
    public PTK_MinimapSO minimapInfo;

    [Header("Start Positions")]
    public Transform[] mapStartPositions;
    [Header("Finish Line")]
    public Transform finishLineBoundary;

    [Header("Item Boxes Parent")]
    public Transform ItemBoxesParent;
    [HideInInspector]
    public List<PTK_ModItemBox> itemBoxes = new List<PTK_ModItemBox>();

    [Header("Road Blocker")]
    [SerializeField]
    public GameObject roadBlockerParent;

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

    [SerializeField]
    public GameObject extraCollidersParent;

    [SerializeField]
    public GameObject aiJumpTriggers;
    [SerializeField]
    public GameObject aiJumpBlockTriggers;
    [SerializeField]
    public GameObject humanLongJumpTriggers;

    [Header("RacePositionCalc Paths")]
    [SerializeField]
    public PTK_CheckpointParent checkpointParent;

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    private void OnDisable()
    {
        Debug.LogError("Disabled");
    }

    bool bIsInitialized = false;
    public void Initialize()
    {
        if (bIsInitialized == true)
            return;

        if(minimapInfo == null)
        {
            Debug.LogError("Minimap Info is empty. Please generate minimap using button");
            return;
        }

        var minimapGen = this.GetComponentInChildren<PTK_MinimapRenderAndSave>(true);
        minimapGen.gameObject.SetActive(false);

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


    [EasyButtons.Button]
    void GenerateMinimap()
    {
#if UNITY_EDITOR
        if(Application.isPlaying == true)
        {
            Debug.LogError("Can generate only in editor mode!");
            return;
        }

       var minimapGen = this.GetComponentInChildren<PTK_MinimapRenderAndSave>();
       bool bRendered = minimapGen.Editor_RenderMinimapAndSaveFileInChoosenDirectory();

        if (bRendered == true)
        {
            minimapInfo = minimapGen.lastCreatedMinimapInfoSO;

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(minimapInfo);
            UnityEditor.AssetDatabase.SaveAssets();
        }
        else
        {
            minimapInfo = null;
        }
#endif
    }
}

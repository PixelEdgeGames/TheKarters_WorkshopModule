using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModTrack : MonoBehaviour
{
    [Header("Track Minimap")]
    public Texture2D minimapImage;
    public Vector2 minimapImage_WorldPos_BL;
    public Vector2 minimapImage_WorldPos_TR;

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

    [Header("Driving Paths")]
    [SerializeField]
    PTK_ModPathsCreator modPathsCreator;
    [SerializeField]
    public Transform aiBezierSplinesPaths;
    [Header("RacePositionCalc Paths")]
    [SerializeField]
    public Transform racePositionCalcPathsPointsParent;
    // Start is called before the first frame update
    void Awake()
    {
    }


    // Update is called once per frame
    void Update()
    {
        
    }


#if UNITY_EDITOR
    [EasyButtons.Button]
     void CreatePathPoints()
    {
        modPathsCreator.CreatePathPointsEditor();
    }

    [EasyButtons.Button]
    void GeneratePaths()
    {
        modPathsCreator.GeneratePathsEditor();
    }

#endif
}

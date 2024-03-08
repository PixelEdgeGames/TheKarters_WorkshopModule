using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModPathPoint : MonoBehaviour
{
    [Header("Setup by hand")]
    public bool bNoGroundBelow_DisableRespawnOnPoint = false; // aby nie resetowac tutaj gracza

    [Header("Initialized On Awake")]
    [HideInInspector]
    public PTK_ModPathPoint nextPoint;
    [HideInInspector]
    public PTK_ModPathPoint prevPoint;
    [HideInInspector]
    public bool bIsMainRoadPoint = false;
    [HideInInspector]
    public float fRoadWidthForAI = 30.0f;

    public float fDistanceFromFinishLine = 0;
    public float fDistanceFromMain_EntryExitMatched = 0; // not real distance but good for positioning : distance aligned to entry and exit main road, this way dist won't change during entry and exit compared to main road - player will just move on it faster
    public float fSegmentLengthDiffFromMain = 0;

    [HideInInspector]
    public Renderer[] pointRenderers;

    // Start is called before the first frame update
    void Start()
    {
    }



    [EasyButtons.Button]
    public void MovePointsToLeft()
    {
        MovePointRecusive(true);
    }


    [EasyButtons.Button]
    public void MovePointsToRight()
    {
        MovePointRecusive(false);
    }
    void MovePointRecusive(bool bLeft)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(transform.parent, "Mod Path Point");

        float fStrength = 1.0f;
        float fStepPerPoint = 0.1f;
        MovePointToward(this, fStrength, fStepPerPoint, bLeft, false, false);
        MovePointToward(this.nextPoint, fStrength, fStepPerPoint, bLeft, true, false);
        MovePointToward(this.prevPoint, fStrength, fStepPerPoint, bLeft, false, true);
#endif
    }

    public void MovePointToward(PTK_ModPathPoint pointToMove,float fCurrentStrength,float fStepPerPoint,bool bLeft,bool bMoveNextPoint,bool bMovePrevPoint)
    {
        if (fCurrentStrength <= 0)
            return;

        if (pointToMove == null)
            return;

        float fDir = bLeft ? -1.0f : 1.0f;
        UnityEditor.Undo.RecordObject(pointToMove.transform, "Mod Path Point");
        pointToMove.transform.position += pointToMove.transform.right * fCurrentStrength* fDir;

        if(bMoveNextPoint == true)
            MovePointToward(pointToMove.nextPoint, fCurrentStrength - fStepPerPoint, fStepPerPoint, bLeft, bMoveNextPoint, bMovePrevPoint);

        if (bMovePrevPoint == true)
            MovePointToward(pointToMove.prevPoint, fCurrentStrength - fStepPerPoint, fStepPerPoint, bLeft, bMoveNextPoint, bMovePrevPoint);
    }

    [EasyButtons.Button]
    public void DecreasePointsWidth()
    {
        ChangePointsWidth(true);
    }

    [EasyButtons.Button]
    public void IncreasePointsWidth()
    {
        ChangePointsWidth(false);
    }



    void ChangePointsWidth(bool bLowerWidth)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(transform.parent, "WidthChange");

        float fStrength = 2.0f;
        float fStepPerPoint = 0.1f;
        ChangePointsWidth(this, fStrength, fStepPerPoint, bLowerWidth, false, false);
        ChangePointsWidth(this.nextPoint, fStrength, fStepPerPoint, bLowerWidth, true, false);
        ChangePointsWidth(this.prevPoint, fStrength, fStepPerPoint, bLowerWidth, false, true);

        this.gameObject.GetComponentInParent<PTK_ModPathsCreator>().GeneratePathsEditor();
#endif
    }

    public void ChangePointsWidth(PTK_ModPathPoint pointToWidthChange, float fCurrentStrength, float fStepPerPoint, bool bLowerWidth, bool bMoveNextPoint, bool bMovePrevPoint)
    {
        if (fCurrentStrength <= 0)
            return;

        if (pointToWidthChange == null)
            return;

        float fDir = bLowerWidth ? -1.0f : 1.0f;
        UnityEditor.Undo.RecordObject(pointToWidthChange, "WidthChange");

       pointToWidthChange.fRoadWidthForAI +=  fCurrentStrength * fDir;

        if (pointToWidthChange.fRoadWidthForAI < 9)
            pointToWidthChange.fRoadWidthForAI = 9;

        if (pointToWidthChange.fRoadWidthForAI > 30)
            pointToWidthChange.fRoadWidthForAI = 30;

        if (bMoveNextPoint == true)
            ChangePointsWidth(pointToWidthChange.nextPoint, fCurrentStrength - fStepPerPoint, fStepPerPoint, bLowerWidth, bMoveNextPoint, bMovePrevPoint);

        if (bMovePrevPoint == true)
            ChangePointsWidth(pointToWidthChange.prevPoint, fCurrentStrength - fStepPerPoint, fStepPerPoint, bLowerWidth, bMoveNextPoint, bMovePrevPoint);
    }
}

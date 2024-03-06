using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModPathPoint : MonoBehaviour
{
    [Header("Setup by hand")]
    public bool bPointAboveTrackWithoutGroundBelow_PlayerRespawnOnPointDisabled = false; // aby nie resetowac tutaj gracza

    [Header("Initialized On Awake")]
    public float fDistanceFromFinishLine_Global = 0.0f;
    public float fDistanceToNextPoint = 0.0f;
    public PTK_ModPathPoint nextPoint;
    public PTK_ModPathPoint prevPoint;
    public bool bIsMainRoadPoint = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    public PTK_ModPathPoint GetNextPoint(int iNextPointNr = 1)
    {
        PTK_ModPathPoint pointNext = this;
        for (int i = 0; i < iNextPointNr; i++)
        {
            pointNext = pointNext.nextPoint;
        }

        return pointNext;
    }
    public PTK_ModPathPoint GetpREVPoint(int iNextPointNr = 1)
    {
        PTK_ModPathPoint pointPrev = this;
        for (int i = 0; i < iNextPointNr; i++)
        {
            pointPrev = pointPrev.prevPoint;
        }

        return pointPrev;
    }

    public PTK_ModPathPoint GetNextPointWithDist(float fDistanceFromThisOne)
    {
        PTK_ModPathPoint pointNext = this;

        float fCurDist = 0;
        for (int i = 0; i < 100; i++)
        {
            fCurDist += pointNext.fDistanceToNextPoint;
            pointNext = pointNext.nextPoint;

            if (fCurDist > fDistanceFromThisOne)
                return pointNext;
        }

        return null;
    }
    public PTK_ModPathPoint GetPrevPointWithDist(float fDistanceFromThisOne)
    {
        PTK_ModPathPoint pointPrev = this;

        float fCurDist = 0;
        for (int i = 0; i < 100; i++)
        {
            fCurDist += pointPrev.fDistanceToNextPoint;
            pointPrev = pointPrev.prevPoint;

            if (fCurDist > fDistanceFromThisOne)
                return pointPrev;
        }

        return null;
    }
}

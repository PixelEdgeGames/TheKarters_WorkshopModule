using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CPC_BezierPath))]
public class PTK_BezierSpline : MonoBehaviour
{
    [HideInInspector]
    public CPC_BezierPath bezierPath;

    bool bWasPlayOnAwake = false;
    // Start is called before the first frame update
    void Awake()
    {
        if (bezierPath == null)
            bezierPath = this.GetComponent<CPC_BezierPath>();

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += RaceRestarted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += RaceTimerStart;

        if (bezierPath != null)
            bWasPlayOnAwake = bezierPath.playOnAwake;
    }

    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= RaceRestarted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= RaceTimerStart;
    }

    private void RaceRestarted()
    {
        if (bezierPath == null)
            return;

        if(bWasPlayOnAwake == true)
        {
            bezierPath.PlayPath(bezierPath.fBezierSpeed, false, true);
            bezierPath.PausePath();
        }
    }

    private void RaceTimerStart()
    {
        if (bezierPath == null)
            return;

        // play again to ensure events at 0 are triggered when match started
        bezierPath.PlayPath(bezierPath.fBezierSpeed, false, true);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

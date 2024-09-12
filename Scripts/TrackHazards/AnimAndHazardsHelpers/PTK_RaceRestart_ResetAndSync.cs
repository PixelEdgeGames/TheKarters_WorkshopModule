using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_RaceRestart_ResetAndSync : MonoBehaviour
{
    [Header("Used to reset objects to default state")]
    public bool bCallToResetToDefaultOnRaceRestart = true;
    public bool bCallToSyncToRaceBegin = true;

    public UnityEngine.Events.UnityEvent eventsToCall;
    // Start is called before the first frame update
    void Awake()
    {

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

        OnRaceRestart();
    }

    private void OnRaceRestart()
    {
        if(bCallToResetToDefaultOnRaceRestart == true)
        {
            eventsToCall?.Invoke();
        }
    }

    private void OnRaceTimerStart()
    {
        if (bCallToSyncToRaceBegin == true)
        {
            eventsToCall?.Invoke();
        }
    }

    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceRestart;
    }


}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PTK_ModGameplayData_GameEventsRegister 
{
    internal abstract void OnGameEvent_RaceFinished();
    internal abstract void OnGameEvent_RaceRestarted();
    internal abstract void OnGameEvent_RaceTimerStart();

    internal abstract void OnGameEvent_GamePaused();

    internal abstract void OnGameEvent_GameUnpaused();
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PTK_ModGameplayData_AllEventsRegister 
{
    internal abstract void OnGameEvent_RaceFinished();
    internal abstract void OnGameEvent_RaceRestarted();
    internal abstract void OnGameEvent_RaceTimerStart();

    internal abstract void OnPlayerEvent_JustJumped(int iGlobalPlayerIndex);
    internal abstract void OnPlayerEvent_JustLanded(int iGlobalPlayerIndex, float fTimeInAir);
    internal abstract void OnPlayerEvent_JustDied(int iGlobalPlayerIndex);
    internal abstract void OnPlayerEvent_FinishedRace(int iGlobalPlayerIndex,int iPosIndex);
    internal abstract void OnPlayerEvent_KilledOpponent(int iGlobalPlayerIndex);
    internal abstract void OnPlayerEvent_JustReceivedWeapon(int iGlobalPlayerIndex, int iWeaponType);
    internal abstract void OnPlayerEvent_UsedWeapon(int iGlobalPlayerIndex,int iWeaponType);
    internal abstract void OnPlayerEvent_MadeTrick(int iGlobalPlayerIndex);
    internal abstract void OnPlayerEvent_BoostFired(int iGlobalPlayerIndex, int iBoostType, float fBoostStrength, float fBoostDuration);

    internal abstract void OnGameEvent_GamePaused();

    internal abstract void OnGameEvent_GameUnpaused();
}

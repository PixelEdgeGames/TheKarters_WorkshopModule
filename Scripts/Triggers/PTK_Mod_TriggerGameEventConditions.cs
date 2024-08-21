using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PTK_Mod_TriggerGameEventConditions : PTK_ModGameplayData_GameEventsRegister
{
    [System.Serializable]
    public class CEventTypeTrigger
    {
        public enum EAutoTriggerType
        {
            E_GAME_RACE_FINISHED_EVENT,
            E_GAME_RACE_RESTARTED_EVENT,
            E_GAME_RACE_RACE_TIMER_START_EVENT,
            E_GAME_PAUSE_EVENT,
            E_GAME_UNPAUSE_EVENT,


            __COUNT
        }

        public EAutoTriggerType triggerOnEventType = EAutoTriggerType.__COUNT;
    }

    PTK_ModGameEventTriggerType parentModTrigger;
    bool bRegisteredToEvents = false;
    public void Awake_InitializeAndAttachToEvents(PTK_ModGameEventTriggerType _parentModTrigger)
    {
        parentModTrigger = _parentModTrigger;

        if (parentModTrigger.eventTypesConditionsToCheck.Count > 0)
        {
            PTK_ModGameplayDataSync.Instance.RegisterToGameTypeEventsOnly(this);
            bRegisteredToEvents = true;
        }
    }

    public void Destroy_Deinitialize()
    {
        if (bRegisteredToEvents == true)
        {
            PTK_ModGameplayDataSync.Instance.UnRegisterFromGameTypeEventsOnly(this);
        }
    }


    internal override void OnGameEvent_RaceFinished()
    {
    }


    internal override void OnGameEvent_RaceRestarted()
    {
    }

    internal override void OnGameEvent_RaceTimerStart()
    {
    }

    internal override void OnGameEvent_GamePaused()
    {
    }

    internal override void OnGameEvent_GameUnpaused()
    {
    }

}

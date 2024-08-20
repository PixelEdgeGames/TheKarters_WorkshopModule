using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PTK_Mod_TriggerVariableConditions : PTK_ModGameplayData_AllEventsRegister
{
    

    public enum ECompareType
    {
        E0_EVENT_TYPE_IGNORE_COMPARISION,

        E_LESS_THAN,         // <
        E_LESS_THAN_OR_EQUAL,// <=
        E_EQUAL,             // ==
        E_NOT_EQUAL,         // !=
        E_GREATER_THAN,      // >
        E_GREATER_THAN_OR_EQUAL // >=
    }

    [System.Serializable]
    public class CGameTypeCondition
    {
        public enum EGameConditionType
        {
            E_DISTANCE_TO_PLAYER_CAMERA_VALUE,
            E_DISTANCE_TO_ANY_PLAYER_VALUE,

            E_GAME_CURRENT_RACE_TIME_VALUE,
            E_GAME_CURRENT_PLAYERS_COUNT_VALUE,
            E_GAME_LOCAL_PLAYERS_COUNT_VALUE,
            E_GAME_TRACK_CONFIG_ID_VALUE,
            E_GAME_MODE_CONFIG_ID_VALUE,
            E_GAME_IS_RACE_GAME_MODE_VALUE,
            E_GAME_IS_TIME_TRIAL_GAME_MODE_VALUE,

            E_GAME_CURRENT_GAME_MAX_HUD_LAP_NR_VALUE,
            E_GAME_PLAYER_RACE_RESTARTED_COUNT_VALUE,


            E_GAME_RACE_FINISHED_EVENT,
            E_GAME_FIRST_PLAYER_MOVED_THROUGH_FINISH_LINE_EVENT,
            E_GAME_FIRST_PLAYER_DIED_EVENT,
            E_GAME_PLAYER_DIED_EVENT,

            E_GAME_RACE_RESTARTED_EVENT,
            E_GAME_RACE_RACE_TIMER_START_EVENT,

            E_GAME_LAP_NR_INCREASED_EVENT,
            E_GAME_FINAL_LAP_STARTED_EVENT,

            __COUNT
        }


        [Header("Game Condition Type")]
        public EGameConditionType eGameConditionType = EGameConditionType.__COUNT;
        [Header("Compare")]
        public ECompareType eGameConditionCompareType = ECompareType.E_EQUAL;
        [Header("0 = FALSE/ 1 = TRUE / Number")]
        public float fGameConditionCompareValue = 0.0f;


        [Header("Param")]
        public bool bDisableCondition = false;

    }


    [System.Serializable]
    public class CPlayerTypeCondition
    {
        public enum EPlayerWithinRangeConditionType
        {
            E_ANY_PLAYER_DIED_EVENT,
            E_ANY_PLAYER_JUMPED_EVENT,
            E_ANY_PLAYER_LANDED_EVENT,
            E_ANY_PLAYER_USED_WEAPON_EVENT,
            E_ANY_PLAYER_KILLED_SOMEONE_EVENT,
            E_ANY_PLAYER_KILLED_MADE_TRICK_EVENT,
            E_ANY_PLAYER_KILLED_USED_BOOST_EVENT,

            E_ANY_PLAYER_USED_WEAPON_OF_TYPE,
            E_ANY_PLAYER_VELOCITY_MAGNITUDE_VALUE,
            E_ANY_PLAYER_CONTAINS_ITEM_OF_TYPE,
            E_ANY_PLAYER_RESERVES_VALUE,
            E_ANY_PLAYER_HEALTH_VALUE,
            E_ANY_PLAYER_IS_IMMUNE_BOOL,
            E_ANY_PLAYER_IS_LOCAL_WITH_CAMERA_BOOL,

            __COUNT
        }

        [Header("Player Condition Type")]
        public EPlayerWithinRangeConditionType ePlayerConditionType = EPlayerWithinRangeConditionType.__COUNT;
        [Header("Compare")]
        public ECompareType ePlayerConditionCompareType = ECompareType.E_EQUAL;
        [Header("0 = FALSE/ 1 = TRUE / Number")]
        public float fPlayerConditionCompareValue = 0.0f;

        [Header("Param")]
        public bool bDisableCondition = false;

    }

    public string strConditionName = "";
    public bool bIgnoreConditions = false;
    [Header("ALL below conditions need to pass for trigger event")]
    public List<CGameTypeCondition> gameTypeConditionsToCheck = new List<CGameTypeCondition>();

    public List<CPlayerTypeCondition> playerTypeConditionsToCheck = new List<CPlayerTypeCondition>();
    
    bool bRegisteredToEvents = false;

    PTK_Mod_Trigger parentModTrigger;
    public void Awake_InitializeAndAttachToEvents(PTK_Mod_Trigger _parentModTrigger)
    {
        parentModTrigger = _parentModTrigger;

        if (gameTypeConditionsToCheck.Count > 0)
        {
            PTK_ModGameplayDataSync.Instance.RegisterToGameTypeEventsOnly(this);
            bRegisteredToEvents = true;
        }
    }

    public void Destroy_Deinitialize()
    {
        if(bRegisteredToEvents == true)
        {
            PTK_ModGameplayDataSync.Instance.UnRegisterFromGameTypeEventsOnly(this);
        }
    }


    public void CheckConditions()
    {
        if (bIgnoreConditions == true)
            return;

        for(int i=0;i< gameTypeConditionsToCheck.Count;i++)
        {
            bool bApproved = CheckConditionType(gameTypeConditionsToCheck[i]);
        }
    }

    bool bConditionAlreadyPassed = false;

    private bool CheckConditionType(CGameTypeCondition condition)
    {
        if (condition.bDisableCondition == true)
            return false;

        bool bIsEventSendTriggerEachTime = false;

        bool bConditionPassed = false;
        switch(condition.eGameConditionType)
        {
            case CGameTypeCondition.EGameConditionType.E_DISTANCE_TO_ANY_PLAYER_VALUE:
                break;
        }

        if (bConditionPassed == false)
            bConditionAlreadyPassed = false;

        // this way it will work like trigger - when the condition is passed we will return true, and we will wait until condition is no longer valid before sending trigger again
        // for example condition player velocity higher than 30 - we will send it but we will send it again only if it will go back to < 30
        // for triggers we will need to pass it every time

        if (bIsEventSendTriggerEachTime == true)
        {
            return bConditionPassed;
        }
        else
        {
            // we wont send trigger about value again until it will no go back again
            if (bConditionAlreadyPassed == true)
                return false;

            return bConditionPassed;
        }
    }

    [System.Serializable]
    public class CEventReceived
    {
        public CGameTypeCondition.EGameConditionType eventConditionType;

    }

    Dictionary<CGameTypeCondition.EGameConditionType, CEventReceived> eventsReceived = new Dictionary<CGameTypeCondition.EGameConditionType, CEventReceived>();

    internal override void OnGameEvent_RaceFinished()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_RACE_FINISHED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType,eventInfo);
        }
    }

    internal override void OnGameEvent_FirstPlayerMovedThroughFinishLine()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_FIRST_PLAYER_MOVED_THROUGH_FINISH_LINE_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_FirstPlayerDied()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_FIRST_PLAYER_DIED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_RaceRestarted()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_RACE_RESTARTED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_RaceTimerStart()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_RACE_RACE_TIMER_START_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_AnyPlayerDied()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_PLAYER_DIED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_LapNrIncreased()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_LAP_NR_INCREASED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }

    internal override void OnGameEvent_FinalLapStarted()
    {
        var eType = CGameTypeCondition.EGameConditionType.E_GAME_FINAL_LAP_STARTED_EVENT;

        if (eventsReceived.ContainsKey(eType) == false)
        {
            CEventReceived eventInfo = new CEventReceived();
            eventInfo.eventConditionType = eType;
            eventsReceived.Add(eType, eventInfo);
        }
    }


    // PLAYER EVENTS NOT USED
    internal override void OnPlayerEvent_JustJumped(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_JustLanded(int iGlobalPlayerIndex, float fTimeInAir)
    {
    }

    internal override void OnPlayerEvent_JustDied(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_FinishedRace(int iGlobalPlayerIndex,int iFinishedRacePosIndex)
    {
    }

    internal override void OnPlayerEvent_KilledOpponent(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_UsedWeapon(int iGlobalPlayerIndex,int iWeaponType)
    {
    }

    internal override void OnPlayerEvent_MadeTrick(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_DestroyedItemBox(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_BoostFired(int iGlobalPlayerIndex, int iBoostType, float fBoostStrength, float fBoostDuration)
    {
    }

}

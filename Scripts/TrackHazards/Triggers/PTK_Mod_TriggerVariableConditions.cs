using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PTK_Mod_TriggerVariableConditions 
{
    

    public enum ECompareType
    {
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
            E_GAME_CURRENT_RACE_TIME_VALUE,
            E_GAME_CURRENT_PLAYERS_COUNT_VALUE,
            E_GAME_LOCAL_PLAYERS_COUNT_VALUE,
            E_GAME_TRACK_CONFIG_ID_VALUE,
            E_GAME_MODE_CONFIG_ID_VALUE,
            E_GAME_IS_RACE_GAME_MODE_VALUE,
            E_GAME_IS_TIME_TRIAL_GAME_MODE_VALUE,

            E_GAME_CURRENT_GAME_MAX_HUD_LAP_NR_VALUE,
            E_GAME_PLAYER_RACE_RESTARTED_COUNT_VALUE,


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

    public string strConditionName = "";
    public bool bIgnoreConditions = false;
    [Header("ALL below conditions need to pass for trigger to pass")]
    public List<CGameTypeCondition> gameTypeConditionsToCheck = new List<CGameTypeCondition>();
    
    bool bRegisteredToEvents = false;

    PTK_ModGameVariableConditionsTriggerType parentModTrigger;
    public void Awake_InitializeAndAttachToEvents(PTK_ModGameVariableConditionsTriggerType _parentModTrigger)
    {
        parentModTrigger = _parentModTrigger;

        if (gameTypeConditionsToCheck.Count > 0)
        {
            bRegisteredToEvents = true;
        }
    }

    public void Destroy_Deinitialize()
    {
        if(bRegisteredToEvents == true)
        {
        }
    }


    // this way it will work like trigger - when the condition is passed we will return true, and we will wait until condition is no longer valid before sending trigger again
    // for example condition player velocity higher than 30 - we will send it but we will send it again only if it will go back to < 30
    // for triggers we will need to pass it every time

    // we wont send trigger about value again until it will no go back again
    bool bConditionAlreadyPassed_TriggerEventSent = false;

    public void CheckConditions(PTK_ModGameVariableConditionsTriggerType pTK_Mod_Trigger)
    {
        if (bIgnoreConditions == true)
            return;

        bool bConditionPassed = true;
        bool bContainsAnyCondition = false;
        for(int i=0;i< gameTypeConditionsToCheck.Count;i++)
        {
            if (gameTypeConditionsToCheck[i].bDisableCondition == true)
                continue;

            bConditionPassed &= CheckConditionType(gameTypeConditionsToCheck[i], pTK_Mod_Trigger);

            bContainsAnyCondition = true;
        }

        if (bContainsAnyCondition == true)
        {
            if(bConditionAlreadyPassed_TriggerEventSent == false && bConditionPassed == true)
            {
                bConditionAlreadyPassed_TriggerEventSent = true;

                if(pTK_Mod_Trigger.eTriggerActivationType == PTK_ModGameVariableConditionsTriggerType.ETriggerActivationType.E0_CONDITION_MET ||
                    pTK_Mod_Trigger.eTriggerActivationType == PTK_ModGameVariableConditionsTriggerType.ETriggerActivationType.E2_BOTH_MET_AND_NO_LONGER_MET)
                {
                    pTK_Mod_Trigger.InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E0_GAME_VARIABLE));
                }

            }
            else if (bConditionAlreadyPassed_TriggerEventSent == true && bConditionPassed == false)
            {
                // we passed condition before but now it is no longer valid - we are setting bConditionAlreadyPassed for trigger to happen again
                bConditionAlreadyPassed_TriggerEventSent = false;

                if (pTK_Mod_Trigger.eTriggerActivationType == PTK_ModGameVariableConditionsTriggerType.ETriggerActivationType.E1_CONDITION_NO_LONGER_MET ||
                    pTK_Mod_Trigger.eTriggerActivationType == PTK_ModGameVariableConditionsTriggerType.ETriggerActivationType.E2_BOTH_MET_AND_NO_LONGER_MET)
                {
                    pTK_Mod_Trigger.InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E0_GAME_VARIABLE));
                }
            }
        }
    }


    private bool CheckConditionType(CGameTypeCondition condition, PTK_ModGameVariableConditionsTriggerType pTK_Mod_Trigger)
    {
        bool bConditionPassed = false;
        switch(condition.eGameConditionType)
        {
            case CGameTypeCondition.EGameConditionType.E_GAME_CURRENT_RACE_TIME_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType,PTK_ModGameplayDataSync.Instance.gameInfo.fCurrentRaceTime, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_CURRENT_PLAYERS_COUNT_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iCurrentPlayingPlayersCount, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_LOCAL_PLAYERS_COUNT_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iLocalPlayersCount, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_TRACK_CONFIG_ID_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iTrackConfigID, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_MODE_CONFIG_ID_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iGameModeConfigID, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_IS_RACE_GAME_MODE_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.bIsRaceGameMode ? 1.0f : 0.0f, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_IS_TIME_TRIAL_GAME_MODE_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.bIsTimeTrialGameMode ? 1.0f : 0.0f, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_CURRENT_GAME_MAX_HUD_LAP_NR_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iCurrentBestLapNrOnHud, condition.fGameConditionCompareValue);
                break;
            case CGameTypeCondition.EGameConditionType.E_GAME_PLAYER_RACE_RESTARTED_COUNT_VALUE:
                bConditionPassed = CompareValue(condition.eGameConditionCompareType, PTK_ModGameplayDataSync.Instance.gameInfo.iPlayerRaceRestartCount, condition.fGameConditionCompareValue);
                break;
            default:
                Debug.LogError("Unknown condition " + condition.eGameConditionType);
                break;
        }

        return bConditionPassed;
    }

    bool CompareValue(ECompareType compare, float fCompareVal, float fCompareTo)
    {
        switch (compare)
        {
            case ECompareType.E_LESS_THAN:
                return fCompareVal < fCompareTo;
            case ECompareType.E_LESS_THAN_OR_EQUAL:
                return fCompareVal <= fCompareTo;
            case ECompareType.E_EQUAL:
                return fCompareVal == fCompareTo;
            case ECompareType.E_NOT_EQUAL:
                return fCompareVal != fCompareTo;
            case ECompareType.E_GREATER_THAN:
                return fCompareVal > fCompareTo;
            case ECompareType.E_GREATER_THAN_OR_EQUAL:
                return fCompareVal >= fCompareTo;
            default:
                Debug.LogError("Unknown compare type " + compare);
                break;
        }
        return false;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Mod_TriggerPlayerInRangeEventConditions : PTK_ModGameplayData_PlayerEventsRegister
{
    [System.Serializable]
    public class CEventTypeTrigger
    {
        public enum EAutoTriggerType
        {
            E_ANY_PLAYER_IN_RANGE_DIED_EVENT,
            E_ANY_PLAYER_IN_RANGE_JUMPED_EVENT,
            E_ANY_PLAYER_IN_RANGE_LANDED_EVENT,
            E_ANY_PLAYER_IN_RANGE_USED_WEAPON_EVENT,
            E_ANY_PLAYER_IN_RANGE_KILLED_SOMEONE_EVENT,
            E_ANY_PLAYER_IN_RANGE_MADE_TRICK_EVENT,
            E_ANY_PLAYER_IN_RANGE_USED_BOOST_EVENT,

            __COUNT
        }

        public EAutoTriggerType triggerOnEventType = EAutoTriggerType.__COUNT;
    }

    PTK_ModPlayerInRangeEventTriggerType parentModTrigger;
    bool bRegisteredToEvents = false;
    public void Awake_InitializeAndAttachToEvents(PTK_ModPlayerInRangeEventTriggerType _parentModTrigger)
    {
        parentModTrigger = _parentModTrigger;

        if (parentModTrigger.eventTypesConditionsToCheck.Count > 0)
        {
            PTK_ModGameplayDataSync.Instance.RegisterToPlayerAllEvents(this);
            bRegisteredToEvents = true;
        }
    }

    public void Destroy_Deinitialize()
    {
        if (bRegisteredToEvents == true)
        {
            PTK_ModGameplayDataSync.Instance.UnRegisterFromAllPlayerEvents(this);
        }
    }

    internal override void OnPlayerEvent_JustJumped(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_JustLanded(int iGlobalPlayerIndex, float fTimeInAir)
    {
    }

    internal override void OnPlayerEvent_JustDied(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_FinishedRace(int iGlobalPlayerIndex, int iPosIndex)
    {
    }

    internal override void OnPlayerEvent_KilledOpponent(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_JustReceivedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
    }

    internal override void OnPlayerEvent_UsedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
    }

    internal override void OnPlayerEvent_MadeTrick(int iGlobalPlayerIndex)
    {
    }

    internal override void OnPlayerEvent_BoostFired(int iGlobalPlayerIndex, int iBoostType, float fBoostStrength, float fBoostDuration)
    {
    }
}

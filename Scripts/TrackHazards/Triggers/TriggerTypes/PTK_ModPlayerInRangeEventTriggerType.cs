using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModPlayerInRangeEventTriggerType : PTK_ModBaseTrigger
{
    public enum EPlayerEventType
    {
        E_VOLUME_HAS_AT_LEAST_ONE_PLAYER_EVENT,
        E_VOLUME_EMPTY_LAST_PLAYER_LEFT,

        E_ANY_PLAYER_IN_RANGE_DIED_EVENT,
        E_ANY_PLAYER_IN_RANGE_JUMPED_EVENT,
        E_ANY_PLAYER_IN_RANGE_LANDED_EVENT,
        E_ANY_PLAYER_IN_RANGE_RECEIVED_WEAPON_EVENT,
        E_ANY_PLAYER_IN_RANGE_USED_WEAPON_EVENT,
        E_ANY_PLAYER_IN_RANGE_KILLED_SOMEONE_EVENT,
        E_ANY_PLAYER_IN_RANGE_MADE_TRICK_EVENT,
        E_ANY_PLAYER_IN_RANGE_USED_BOOST_EVENT,

        __COUNT
    }

    public List<PTK_PlayersInRangeVolume_Base> checkPlayersInVolumes = new List<PTK_PlayersInRangeVolume_Base>();
    public List<EPlayerEventType> eventTypesConditionsToCheck = new List<EPlayerEventType>();

    public bool[] bAreGlobalPlayersWithinVolumesRange = new bool[8];
    public int GetPlayersInVolumesCount()
    {
        int iMaxCount = 0;
        for (int i = 0; i < checkPlayersInVolumes.Count; i++)
        {
            iMaxCount = Mathf.Max(iMaxCount, checkPlayersInVolumes[i].GetPlayersInsideVolumeCount());
        }

        return iMaxCount;
    }

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_PLAYER_IN_RANGE_EVENT_TYPE;
    }

    public override void Start()
    {
        base.Start();

        var playerEvents = PTK_ModGameplayDataSync.Instance.playerEvents;

        playerEvents.OnPlayerEvent_JustJumped += OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied += OnPlayerEvent_JustDied;
        playerEvents.OnPlayerEvent_JustKilledSomeone += OnPlayerEvent_JustKilledSomeone;
        playerEvents.OnPlayerUsedWeapon += OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon += OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick += OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired += OnPlayerEvent_BoostFired;
    }
    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnTriggerEnabledDetected()
    {
    }
    public override void OnTriggerDisabledDetected()
    {
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        var playerEvents = PTK_ModGameplayDataSync.Instance.playerEvents;

        playerEvents.OnPlayerEvent_JustJumped -= OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied -= OnPlayerEvent_JustDied;
        playerEvents.OnPlayerEvent_JustKilledSomeone -= OnPlayerEvent_JustKilledSomeone;
        playerEvents.OnPlayerUsedWeapon -= OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon -= OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick -= OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired -= OnPlayerEvent_BoostFired;
    }

    int iPlayersInsideVolumeCount = 0;

    int iFirstLastGlobalPlayerIndex = -1;
    public override void Update()
    {
        base.Update();

        // we want to check if players are in volume all the time to dected correctly after trigger is enabled again (and not have old  players count)
      //  if (bIsTriggerEnabled == false)
      //      return;

        int iThisFramePlayersInsideVolume = 0;
        int iPlayerInsideVolume = -1;
        for (int iPlayerIndex = 0; iPlayerIndex < bAreGlobalPlayersWithinVolumesRange.Length; iPlayerIndex++)
        {
            bAreGlobalPlayersWithinVolumesRange[iPlayerIndex] = false;

            for (int iVolume = 0; iVolume < checkPlayersInVolumes.Count; iVolume++)
            {
                bAreGlobalPlayersWithinVolumesRange[iPlayerIndex] |= checkPlayersInVolumes[iVolume].bAreGlobalPlayersWithinRange[iPlayerIndex];

            }

            if (bAreGlobalPlayersWithinVolumesRange[iPlayerIndex] == true)
            {
                if (iPlayerInsideVolume == -1)
                    iPlayerInsideVolume = iPlayerIndex;

                iThisFramePlayersInsideVolume++;
            }
        }

        if(iPlayersInsideVolumeCount == 0 && iThisFramePlayersInsideVolume > 0)
        {
            iFirstLastGlobalPlayerIndex = iPlayerInsideVolume;

            if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_VOLUME_HAS_AT_LEAST_ONE_PLAYER_EVENT))
            {
                InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_VOLUME_HAS_AT_LEAST_ONE_PLAYER_EVENT, iFirstLastGlobalPlayerIndex));
            }
        }

        if (iPlayersInsideVolumeCount != 0 && iThisFramePlayersInsideVolume == 0)
        {
            if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_VOLUME_EMPTY_LAST_PLAYER_LEFT))
            {
                InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_VOLUME_EMPTY_LAST_PLAYER_LEFT, iFirstLastGlobalPlayerIndex));
            }

            iFirstLastGlobalPlayerIndex = -1;
        }

        iPlayersInsideVolumeCount = iThisFramePlayersInsideVolume;
    }


    internal void OnPlayerEvent_JustJumped(int iGlobalPlayerIndex)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_JUMPED_EVENT))
        {
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT,EPlayerEventType.E_ANY_PLAYER_IN_RANGE_JUMPED_EVENT, iGlobalPlayerIndex));
        }
    }

    internal void OnPlayerEvent_JustLanded(int iGlobalPlayerIndex, float fTimeInAir)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_LANDED_EVENT))
        {
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_LANDED_EVENT, iGlobalPlayerIndex));
        }
    }

    internal void OnPlayerEvent_JustDied(int iGlobalPlayerIndex)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_DIED_EVENT))
        {
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_DIED_EVENT, iGlobalPlayerIndex));
        }
    }

    private void OnPlayerEvent_JustKilledSomeone(int iGlobalPlayerIndexWhoKilled, int iGlobalPlayerIndexWhoDied)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndexWhoKilled] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_KILLED_SOMEONE_EVENT))
        {
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_KILLED_SOMEONE_EVENT, iGlobalPlayerIndexWhoKilled));
        }
    }

    internal void OnPlayerEvent_JustReceivedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_RECEIVED_WEAPON_EVENT))
        {
            CTriggerEventType.CPlayerEventData playerEventData = new CTriggerEventType.CPlayerEventData();
            playerEventData.iWeaponTypeIndex = iWeaponType;

            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_RECEIVED_WEAPON_EVENT, iGlobalPlayerIndex, playerEventData));
        }
    }

    internal void OnPlayerEvent_UsedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_USED_WEAPON_EVENT))
        {
            CTriggerEventType.CPlayerEventData playerEventData = new CTriggerEventType.CPlayerEventData();
            playerEventData.iWeaponTypeIndex = iWeaponType;

            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_USED_WEAPON_EVENT, iGlobalPlayerIndex, playerEventData));
        }
    }

    internal void OnPlayerEvent_MadeTrick(int iGlobalPlayerIndex)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_MADE_TRICK_EVENT))
        {
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_MADE_TRICK_EVENT, iGlobalPlayerIndex));
        }
    }

    internal void OnPlayerEvent_BoostFired(int iGlobalPlayerIndex, int iBoostType, float fBoostStrength, float fBoostDuration)
    {
        // in range
        if (bAreGlobalPlayersWithinVolumesRange[iGlobalPlayerIndex] == false)
            return;

        if (eventTypesConditionsToCheck.Contains(EPlayerEventType.E_ANY_PLAYER_IN_RANGE_USED_BOOST_EVENT))
        {
            CTriggerEventType.CPlayerEventData playerEventData = new CTriggerEventType.CPlayerEventData();
            playerEventData.iBoostType = iBoostType;
            playerEventData.fBoostStrength = fBoostStrength;
            playerEventData.fBoostDuration = fBoostDuration;
            InvokeTriggerAction(new CTriggerEventType(CTriggerEventType.ETriggerType.E2_PLAYER_EVENT, EPlayerEventType.E_ANY_PLAYER_IN_RANGE_USED_BOOST_EVENT, iGlobalPlayerIndex, playerEventData));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModPlayerInRangeEventTriggerType : PTK_ModBaseTrigger
{
    public enum EPlayerEventType
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

    [Header("Event Type Conditions")]
    public List<PTK_PlayersInRangeVolume_Base> checkPlayersInVolumes = new List<PTK_PlayersInRangeVolume_Base>();
    public List<EPlayerEventType> eventTypesConditionsToCheck = new List<EPlayerEventType>();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_EVENT_TYPE;
    }

    public override void Start()
    {
        base.Start();

        var playerEvents = PTK_ModGameplayDataSync.Instance.playerEvents;

        playerEvents.OnPlayerEvent_JustJumped += OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied += OnPlayerEvent_JustDied;
        playerEvents.OnPlayerUsedWeapon += OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon += OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick += OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired += OnPlayerEvent_BoostFired;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        var playerEvents = PTK_ModGameplayDataSync.Instance.playerEvents;

        playerEvents.OnPlayerEvent_JustJumped -= OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied -= OnPlayerEvent_JustDied;
        playerEvents.OnPlayerUsedWeapon -= OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon -= OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick -= OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired -= OnPlayerEvent_BoostFired;
    }

    private void Update()
    {
      
    }


    internal void OnPlayerEvent_JustJumped(int iGlobalPlayerIndex)
    {
    }

    internal void OnPlayerEvent_JustLanded(int iGlobalPlayerIndex, float fTimeInAir)
    {
    }

    internal void OnPlayerEvent_JustDied(int iGlobalPlayerIndex)
    {
    }

    internal void OnPlayerEvent_FinishedRace(int iGlobalPlayerIndex, int iPosIndex)
    {
    }

    internal void OnPlayerEvent_KilledOpponent(int iGlobalPlayerIndex)
    {
    }

    internal void OnPlayerEvent_JustReceivedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
    }

    internal void OnPlayerEvent_UsedWeapon(int iGlobalPlayerIndex, int iWeaponType)
    {
    }

    internal void OnPlayerEvent_MadeTrick(int iGlobalPlayerIndex)
    {
    }

    internal void OnPlayerEvent_BoostFired(int iGlobalPlayerIndex, int iBoostType, float fBoostStrength, float fBoostDuration)
    {
    }
}

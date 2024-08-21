using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModGameEventTriggerType : PTK_ModBaseTrigger
{
    public enum EGameEventType
    {
        E_GAME_RACE_FINISHED_EVENT,
        E_GAME_RACE_RESTARTED_EVENT,
        E_GAME_RACE_RACE_TIMER_START_EVENT,
        E_GAME_PAUSE_EVENT,
        E_GAME_UNPAUSE_EVENT,


        __COUNT
    }

    [Header("Event Type Conditions")]
    public List<EGameEventType> eventTypesConditionsToCheck = new List<EGameEventType>();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_EVENT_TYPE;
    }

    public override void Start()
    {
        base.Start();

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        gameEvents.OnGameEvent_RaceFinished += OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_RaceRestarted += OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart += OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused += OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused += OnGameEvent_GameUnpaused;

    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        gameEvents.OnGameEvent_RaceFinished -= OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_RaceRestarted -= OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart -= OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused -= OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused -= OnGameEvent_GameUnpaused;
    }


    internal void OnGameEvent_RaceFinished()
    {
    }


    internal void OnGameEvent_RaceRestarted()
    {
    }

    internal void OnGameEvent_RaceTimerStart()
    {
    }

    internal void OnGameEvent_GamePaused()
    {
    }

    internal void OnGameEvent_GameUnpaused()
    {
    }
}

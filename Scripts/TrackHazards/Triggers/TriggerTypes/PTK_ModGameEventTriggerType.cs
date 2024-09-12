using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModGameEventTriggerType : PTK_ModBaseTrigger
{
    public enum EGameEventType
    {
        E_GAME_FIRST_PLAYER_FINISHED_RACE,
        E_GAME_WHOLE_RACE_FINISHED_EVENT,
        E_GAME_RACE_RESTARTED_EVENT,
        E_GAME_RACE_RACE_TIMER_START_EVENT,
        E_GAME_PAUSE_EVENT,
        E_GAME_UNPAUSE_EVENT,


        __COUNT
    }

    public List<EGameEventType> eventTypesConditionsToCheck = new List<EGameEventType>();

    [Header("Allows to use Player Logic Effects Commannds")]
    public bool bTriggerWithPlayerEvents = false;
    public PTK_ModAutoTriggerType.CTriggerOnPlayersSettings triggerTargetPlayersSettings = new PTK_ModAutoTriggerType.CTriggerOnPlayersSettings();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_EVENT_TYPE;
    }

    public override void Start()
    {
        base.Start();

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        gameEvents.OnGameEvent_WholeRaceFinished += OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_FirstPlayerFinishedRace += OnGameEvent_FirstPlayerFinishedRace;
        gameEvents.OnGameEvent_RaceRestarted += OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart += OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused += OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused += OnGameEvent_GameUnpaused;

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

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        gameEvents.OnGameEvent_WholeRaceFinished -= OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_FirstPlayerFinishedRace -= OnGameEvent_FirstPlayerFinishedRace;
        gameEvents.OnGameEvent_RaceRestarted -= OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart -= OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused -= OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused -= OnGameEvent_GameUnpaused;
    }

    public override void Update()
    {
        base.Update();
    }

    private void OnGameEvent_FirstPlayerFinishedRace()
    {
        if(eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_FIRST_PLAYER_FINISHED_RACE))
        {
            if(bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_FIRST_PLAYER_FINISHED_RACE));
            }
        }
    }

    internal void OnGameEvent_RaceFinished()
    {
        if (eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_WHOLE_RACE_FINISHED_EVENT))
        {
            if (bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }
            else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_WHOLE_RACE_FINISHED_EVENT));
            }
        }
    }


    internal void OnGameEvent_RaceRestarted()
    {
        if (eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_RACE_RESTARTED_EVENT))
        {
            if (bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }
            else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_RACE_RESTARTED_EVENT));
            }
        }
    }

    internal void OnGameEvent_RaceTimerStart()
    {
        if (eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_RACE_RACE_TIMER_START_EVENT))
        {
            if (bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }
            else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_RACE_RACE_TIMER_START_EVENT));
            }
        }
    }

    internal void OnGameEvent_GamePaused()
    {
        if (eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_PAUSE_EVENT))
        {
            if (bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }
            else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_PAUSE_EVENT));
            }
        }
    }

    internal void OnGameEvent_GameUnpaused()
    {
        if (eventTypesConditionsToCheck.Contains(EGameEventType.E_GAME_UNPAUSE_EVENT))
        {
            if (bTriggerWithPlayerEvents == true)
            {
                triggerTargetPlayersSettings.CheckAndTriggerEventsForPlayers(this, PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT);
            }
            else
            {
                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E1_GAME_EVENT, EGameEventType.E_GAME_UNPAUSE_EVENT));
            }
        }
    }
}

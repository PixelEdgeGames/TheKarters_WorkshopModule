using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PTK_Command_06_CustomCommands : PTK_TriggerCommandBase
{
    [Header("Events to call")]
    public UnityEvent eventsToTrigger;

    [Header("Use this to reset to default state")]
    public UnityEvent raceRestarted_TriggerEvents;

    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E06_CUSTOM_COMMANDS;
    }
    public override void Awake()
    {
    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }

    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal)
    {
        CommandExecuted();
    }

    void CommandExecuted()
    {
        eventsToTrigger?.Invoke();
    }


    protected override void RaceResetted_RevertToDefault()
    {
        raceRestarted_TriggerEvents?.Invoke();
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }

}

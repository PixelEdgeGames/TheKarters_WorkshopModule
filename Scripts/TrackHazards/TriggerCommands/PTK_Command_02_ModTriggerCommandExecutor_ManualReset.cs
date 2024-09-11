using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_02_ModTriggerCommandExecutor_ManualReset : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E02_COMMANDS_EXECUTOR_MANUAL_RESET;
    }

    public PTK_TriggerArrayCommandsExecutor[] commandsExecutorsToReset;


    public override void Awake()
    {
       
    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }
    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        CommandExecuted();
    }

    void CommandExecuted()
    {
        foreach (PTK_TriggerArrayCommandsExecutor go in commandsExecutorsToReset)
        {
            if (go == null)
                continue;

            go.ManualResetAllowSendingAgain();
        }

    }


    protected override void RaceResetted_RevertToDefault()
    {
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

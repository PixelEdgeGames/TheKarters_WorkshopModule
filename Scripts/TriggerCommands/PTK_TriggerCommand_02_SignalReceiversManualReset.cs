using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommand_02_SignalReceiversManualReset : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E02_SIGNAL_RECEIVER_MANUAL_RESET;
    }

    public PTK_TriggersCommandsLauncher[] signalsToReset;


    public override void Awake()
    {
       
    }
    public override void Start()
    {
    }

    protected override void ExecuteImpl(List<PTK_TriggersCommandsLauncher.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggersCommandsLauncher.CRecivedTriggerWithData recivedTriggerSignal)
    {
        CommandExecuted();
    }

    void CommandExecuted()
    {
        foreach (PTK_TriggersCommandsLauncher go in signalsToReset)
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

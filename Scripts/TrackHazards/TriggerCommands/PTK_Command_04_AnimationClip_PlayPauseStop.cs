using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[Obsolete("Deprecated, please use Command: Animator Commands")]
public class PTK_Command_04_AnimationClip_PlayPauseStop : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.__COUNT;
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

    }

    protected override void RaceResetted_RevertToDefault()
    {
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

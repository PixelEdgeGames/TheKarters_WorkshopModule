using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_03_ModTriggerCommandExecutor_EnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E03_ENABLE_DISABLE_COMMANDS_EXECUTOR;
    }

    public PTK_TriggerArrayCommandsExecutor[] gameObjectsToEnable;
    public PTK_TriggerArrayCommandsExecutor[] gameObjectsToDisable;

    Dictionary<PTK_TriggerArrayCommandsExecutor, bool> defaultEnabledState = new Dictionary<PTK_TriggerArrayCommandsExecutor, bool>();

    public override void Awake()
    {
        foreach (PTK_TriggerArrayCommandsExecutor go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.bModTriggerCommandExecutorEnabled);
        }

        foreach (PTK_TriggerArrayCommandsExecutor go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.bModTriggerCommandExecutorEnabled);
        }
    }
    public override void Start()
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
        foreach (PTK_TriggerArrayCommandsExecutor go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            go.bModTriggerCommandExecutorEnabled = true;
        }

        foreach (PTK_TriggerArrayCommandsExecutor go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            go.bModTriggerCommandExecutorEnabled = false;
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach (PTK_TriggerArrayCommandsExecutor go in defaultEnabledState.Keys)
        {
            if (go == null)
                continue;

            go.bModTriggerCommandExecutorEnabled = defaultEnabledState[go];
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

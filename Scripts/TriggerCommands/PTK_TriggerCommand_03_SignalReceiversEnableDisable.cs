using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommand_03_SignalReceiversEnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E03_SIGNAL_RECEIVER_ENABLE_DISABLE;
    }

    public PTK_TriggersCommandsLauncher[] gameObjectsToEnable;
    public PTK_TriggersCommandsLauncher[] gameObjectsToDisable;

    Dictionary<PTK_TriggersCommandsLauncher, bool> defaultEnabledState = new Dictionary<PTK_TriggersCommandsLauncher, bool>();

    public override void Awake()
    {
        foreach (PTK_TriggersCommandsLauncher go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.gameObject.activeInHierarchy);
        }

        foreach (PTK_TriggersCommandsLauncher go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.gameObject.activeInHierarchy);
        }
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
        foreach (PTK_TriggersCommandsLauncher go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(true);
        }

        foreach (PTK_TriggersCommandsLauncher go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(false);
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach (PTK_TriggersCommandsLauncher go in defaultEnabledState.Keys)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(defaultEnabledState[go]);
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

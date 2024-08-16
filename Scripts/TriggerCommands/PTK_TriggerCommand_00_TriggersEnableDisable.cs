using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommand_00_TriggersEnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E00_TRIGGER_ENABLE_DISABLE;
    }

    public PTK_Mod_Trigger[] triggersToEnable;
    public PTK_Mod_Trigger[] triggersToDisable;

    Dictionary<PTK_Mod_Trigger, bool> defaultEnabledState = new Dictionary<PTK_Mod_Trigger, bool>();

    public override void Awake()
    {
        foreach(PTK_Mod_Trigger trigger in triggersToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            if (defaultEnabledState.ContainsKey(trigger) == false)
                defaultEnabledState.Add(trigger, trigger.gameObject.activeInHierarchy);
        }

        foreach (PTK_Mod_Trigger trigger in triggersToDisable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            if (defaultEnabledState.ContainsKey(trigger) == false)
                defaultEnabledState.Add(trigger, trigger.gameObject.activeInHierarchy);
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
        foreach (PTK_Mod_Trigger trigger in triggersToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.gameObject.SetActive(true);
        }

        foreach (PTK_Mod_Trigger trigger in triggersToDisable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.gameObject.SetActive(false);
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach(PTK_Mod_Trigger trigger in defaultEnabledState.Keys)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.gameObject.SetActive(defaultEnabledState[trigger]);
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_00_TriggersEnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E00_ENABLE_DISABLE_TRIGGER;
    }

    public PTK_ModBaseTrigger[] triggersToEnable;
    public PTK_ModBaseTrigger[] triggersToDisable;

    Dictionary<PTK_ModBaseTrigger, bool> defaultEnabledState = new Dictionary<PTK_ModBaseTrigger, bool>();

    public override void Awake()
    {
        foreach(PTK_ModBaseTrigger trigger in triggersToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            if (defaultEnabledState.ContainsKey(trigger) == false)
                defaultEnabledState.Add(trigger, trigger.gameObject.activeInHierarchy);
        }

        foreach (PTK_ModBaseTrigger trigger in triggersToDisable)
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
        foreach (PTK_ModBaseTrigger trigger in triggersToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.gameObject.SetActive(true);
        }

        foreach (PTK_ModBaseTrigger trigger in triggersToDisable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.gameObject.SetActive(false);
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach(PTK_ModBaseTrigger trigger in defaultEnabledState.Keys)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_00_TriggerEvents_EnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E00_ENABLE_DISABLE_TRIGGER;
    }

    [Header("Trigger Parents")]
    public GameObject[] triggerParentsToEnable;
    public GameObject[] triggerParentsToDisable;

    [Header("Trigger Events")]
    public PTK_ModBaseTrigger[] triggersToEnable;
    public PTK_ModBaseTrigger[] triggersToDisable;

    [Header("Automation - After Command Executed")]
    public bool bPostExecute_RevertToDefaultStateWithDelay = false;
    public float fPostExecute_RevertToDefaulteDelay = 1.0f;

    List<PTK_ModBaseTrigger> triggersToEnableAllPriv = new List<PTK_ModBaseTrigger>();
    List<PTK_ModBaseTrigger> triggersToDisableAllPriv = new List<PTK_ModBaseTrigger>();

    Dictionary<PTK_ModBaseTrigger, bool> defaultEnabledState = new Dictionary<PTK_ModBaseTrigger, bool>();

    public override void Awake()
    {
        foreach(PTK_ModBaseTrigger trigger in triggersToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            if (defaultEnabledState.ContainsKey(trigger) == false)
            {
                defaultEnabledState.Add(trigger, trigger.bIsTriggerEnabled);
            }

            if (triggersToEnableAllPriv.Contains(trigger) == false)
                triggersToEnableAllPriv.Add(trigger);
        }


        foreach (GameObject trigger in triggerParentsToEnable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            var modTriggers = trigger.GetComponentsInChildren<PTK_ModBaseTrigger>();

            foreach(var modTrigger in modTriggers)
            {
                if (defaultEnabledState.ContainsKey(modTrigger) == false)
                {
                    defaultEnabledState.Add(modTrigger, modTrigger.bIsTriggerEnabled);
                }

                if (triggersToEnableAllPriv.Contains(modTrigger) == false)
                    triggersToEnableAllPriv.Add(modTrigger);
            }
        }


        foreach (PTK_ModBaseTrigger trigger in triggersToDisable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            if (defaultEnabledState.ContainsKey(trigger) == false)
            {
                defaultEnabledState.Add(trigger, trigger.bIsTriggerEnabled);
            }

            if (triggersToDisableAllPriv.Contains(trigger) == false)
                triggersToDisableAllPriv.Add(trigger);
        }

        foreach (GameObject trigger in triggerParentsToDisable)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            var modTriggers = trigger.GetComponentsInChildren<PTK_ModBaseTrigger>();

            foreach (var modTrigger in modTriggers)
            {
                if (defaultEnabledState.ContainsKey(modTrigger) == false)
                {
                    defaultEnabledState.Add(modTrigger, modTrigger.bIsTriggerEnabled);
                }

                if(triggersToDisableAllPriv.Contains(modTrigger) == false)
                    triggersToDisableAllPriv.Add(modTrigger);
            }
        }
    }
    public override void Start()
    {
    }

    float fTimeSinceExecuted = 0.0f;
    public override void OnDestroy()
    {
        if (bPostExecute_RevertToDefaultStateWithDelay == true)
        {
            if (fTimeSinceExecuted != 0.0f)
            {
                if ((Time.time - fTimeSinceExecuted) > fPostExecute_RevertToDefaulteDelay)
                {
                    RaceResetted_RevertToDefault();
                }
            }
        }
    }

    public override void Update()
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
        foreach (PTK_ModBaseTrigger trigger in triggersToEnableAllPriv)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.bIsTriggerEnabled = (true);
        }

        foreach (PTK_ModBaseTrigger trigger in triggersToDisableAllPriv)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.bIsTriggerEnabled = (false);
        }

        fTimeSinceExecuted = Time.time;
    }


    protected override void RaceResetted_RevertToDefault()
    {
        fTimeSinceExecuted = 0.0f;

        foreach (PTK_ModBaseTrigger trigger in defaultEnabledState.Keys)
        {
            if (trigger == null || trigger.gameObject == null)
                continue;

            trigger.bIsTriggerEnabled = (defaultEnabledState[trigger]);
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

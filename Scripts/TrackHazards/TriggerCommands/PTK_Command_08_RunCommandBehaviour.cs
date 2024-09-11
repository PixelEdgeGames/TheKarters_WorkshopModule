using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_08_RunCommandBehaviour : PTK_TriggerCommandBase
{
    public List<PTK_TriggerCommandsBehaviour> commandBehavioursToRun = new List<PTK_TriggerCommandsBehaviour>();

    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E08_RUN_COMMAND_BEHAVIOUR;
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

    public void Update()
    {
    }

    // to avoid infinite loop
    float fTimeSinceLastTimeCalled = 0.0f;

    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
       bool bInfiniteLoopDetected = EnsureNoRecursiveCall();

        // will call ourselfs without any delay otherwise resulting in stack overflow. I can add delay to this behaviour to avoid that but creator should use it carfully and I dont want to fix it for him
        if (bInfiniteLoopDetected == true)
            return;

        for (int i = 0; i < commandBehavioursToRun.Count; i++)
        {
            if (commandBehavioursToRun[i] == null)
                continue;

            if (commandBehavioursToRun[i] == _parentCommandBehaviour)
            {
                Debug.LogError("Infinite loop detected - can't execute the same command behaviour");
                continue;
            }

            commandBehavioursToRun[i].Execute(recivedTriggerSignals);
        }
    }

    bool EnsureNoRecursiveCall()
    {
        bool bInfiniteLoopDetected = false;

        if (Time.time == fTimeSinceLastTimeCalled)
        {
            Debug.LogError("Infinite loop detected! " + this.gameObject.name + " Stopping next calls. Please add delay to avoid this situation");
            bInfiniteLoopDetected = true;
        }

        fTimeSinceLastTimeCalled = Time.time;

        bInfiniteLoopDetected = false;

        return bInfiniteLoopDetected;
    }
    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        bool bInfiniteLoopDetected = EnsureNoRecursiveCall();

        // will call ourselfs without any delay otherwise resulting in stack overflow. I can add delay to this behaviour to avoid that but creator should use it carfully and I dont want to fix it for him
        if (bInfiniteLoopDetected == true)
            return;

        for (int i = 0; i < commandBehavioursToRun.Count; i++)
        {
            if (commandBehavioursToRun[i] == null)
                continue;

            if (commandBehavioursToRun[i] == _parentCommandBehaviour)
            {
                Debug.LogError("Infinite loop detected - can't execute the same command behaviour");
                continue;
            }

            commandBehavioursToRun[i].Execute(recivedTriggerSignal);
        }
    }




    protected override void RaceResetted_RevertToDefault()
    {
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

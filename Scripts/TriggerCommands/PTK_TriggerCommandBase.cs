using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PTK_TriggerCommandsBehaviour))]
public abstract class PTK_TriggerCommandBase : MonoBehaviour
{
    public enum ETriggerCommandType
    {
        E00_ENABLE_DISABLE_TRIGGER,
        E01_ENABLE_DISABLE_GAME_OBJECT,
        E02_COMMANDS_EXECUTOR_MANUAL_RESET,
        E03_ENABLE_DISABLE_COMMANDS_EXECUTOR,

        __COUNT
    }

    protected abstract ETriggerCommandType GetCommandType();

    public abstract void Awake();
    public abstract void Start();

    protected abstract void RaceResetted_RevertToDefault();
    protected abstract void OnRaceTimerJustStarted_SyncAndRunAnimsImpl();
    protected abstract void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals);
    protected abstract void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal);

    public void Execute(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        ExecuteImpl(recivedTriggerSignals);
    }

    public void Execute(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal)
    {
        ExecuteImpl(recivedTriggerSignal);
    }

    // multiple triggers can send multiple events and we don't want to run this command multiple times
    bool bAlreadyResetted = false;
    bool bAlreadyTimerStartEventLaunched = false;

    public void RaceResetted()
    {
        if(bAlreadyResetted == false)
        {
            RaceResetted_RevertToDefault();
            bAlreadyResetted = true;
            bAlreadyTimerStartEventLaunched = false;
        }
    }

    public void OnRaceTimerJustStarted_SyncAndRunAnims()
    {
        if(bAlreadyTimerStartEventLaunched == false)
        {
            OnRaceTimerJustStarted_SyncAndRunAnimsImpl();
            bAlreadyResetted = false;
            bAlreadyTimerStartEventLaunched = true;
        }
    }


}
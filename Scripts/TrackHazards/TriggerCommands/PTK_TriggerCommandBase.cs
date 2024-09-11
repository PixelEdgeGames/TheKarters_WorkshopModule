using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PTK_TriggerCommandsBehaviour))]
public abstract class PTK_TriggerCommandBase : MonoBehaviour
{
    public float fExecuteDelay = 0.0f;

    public enum ETriggerCommandType
    {
        E00_ENABLE_DISABLE_TRIGGER,
        E01_ENABLE_DISABLE_GAME_OBJECT,
        E02_COMMANDS_EXECUTOR_MANUAL_RESET,
        E03_ENABLE_DISABLE_COMMANDS_EXECUTOR,
        E05_PLAYER_LOGIC_EFFECTS,
        E06_CUSTOM_COMMANDS,
        E07_ANIMATOR_COMMANDS,
        E08_RUN_COMMAND_BEHAVIOUR,

        __COUNT
    }

    protected abstract ETriggerCommandType GetCommandType();

    public abstract void Awake();
    public abstract void Start();
    public abstract void OnDestroy();
   // public abstract void Update(); // we will use it only in commands that need it

    protected abstract void RaceResetted_RevertToDefault();
    protected abstract void OnRaceTimerJustStarted_SyncAndRunAnimsImpl();
    protected abstract void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour);
    protected abstract void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour);

    public void Execute(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        if(fExecuteDelay > 0)
        {
            StartCoroutine(ExecuteDelayed(recivedTriggerSignals, _parentCommandBehaviour));
        }else
        {
            ExecuteImpl(recivedTriggerSignals, _parentCommandBehaviour);
        }
    }

    public void Execute(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        if (fExecuteDelay > 0)
        {
            StartCoroutine(ExecuteDelayed(recivedTriggerSignal, _parentCommandBehaviour));
        }
        else
        {
            ExecuteImpl(recivedTriggerSignal, _parentCommandBehaviour);
        }
    }

    IEnumerator ExecuteDelayed(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        yield return new WaitForSeconds(fExecuteDelay);

        ExecuteImpl(recivedTriggerSignals, _parentCommandBehaviour);
    }
    IEnumerator ExecuteDelayed(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        yield return new WaitForSeconds(fExecuteDelay);

        ExecuteImpl(recivedTriggerSignal, _parentCommandBehaviour);
    }

    // multiple triggers can send multiple events and we don't want to run this command multiple times
    bool bAlreadyResetted = false;
    bool bAlreadyTimerStartEventLaunched = false;

    public void RaceResetted()
    {
        if(bAlreadyResetted == false)
        {
            StopAllCoroutines();
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

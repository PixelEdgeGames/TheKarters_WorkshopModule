using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_TriggerCommandsBehaviour : MonoBehaviour
{
    PTK_TriggerCommandBase[] ptkCommands;
    public string strBehaviourInfo = "";
    public float fExecuteDelay = 0.0f;
    private void Awake()
    {
        ptkCommands = this.GetComponents<PTK_TriggerCommandBase>();
    }

    internal void OnRaceTimerJustStarted_SyncAndRunAnims()
    {
        for(int i=0;i< ptkCommands.Length;i++)
        {
            ptkCommands[i].OnRaceTimerJustStarted_SyncAndRunAnims();
        }
    }

    internal void RaceResetted()
    {
        StopAllCoroutines();

        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].RaceResetted();
        }
    }

    internal void Execute(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignalsPreview)
    {
        if(fExecuteDelay > 0.0f)
        {
            StartCoroutine(ExecuteDelayedCoroutine(recivedTriggerSignalsPreview));
        }else
        {
            ExecuteInternal(recivedTriggerSignalsPreview);
        }
    }

    private void ExecuteInternal(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignalsPreview)
    {
        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].Execute(recivedTriggerSignalsPreview,this);
        }
    }

    IEnumerator ExecuteDelayedCoroutine(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignalsPreview)
    {
        yield return new WaitForSeconds(fExecuteDelay);

        ExecuteInternal(recivedTriggerSignalsPreview);
    }

    IEnumerator ExecuteDelayedCoroutine(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData firstTriggerData)
    {
        yield return new WaitForSeconds(fExecuteDelay);

        ExecuteInternal(firstTriggerData);
    }

    internal void Execute(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData firstTriggerData)
    {

        if (fExecuteDelay > 0.0f)
        {
            StartCoroutine(ExecuteDelayedCoroutine(firstTriggerData));
        }
        else
        {
            ExecuteInternal(firstTriggerData);
        }
    }

    private void ExecuteInternal(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData firstTriggerData)
    {
        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].Execute(firstTriggerData,this);
        }
    }
}

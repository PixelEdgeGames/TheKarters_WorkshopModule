using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_TriggerCommandsBehaviour : MonoBehaviour
{
    PTK_TriggerCommandBase[] ptkCommands;
    public string strBehaviourInfo = "";

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
        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].RaceResetted();
        }
    }

    internal void Execute(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignalsPreview)
    {
        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].Execute(recivedTriggerSignalsPreview);
        }
    }
    internal void Execute(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData firstTriggerData)
    {
        for (int i = 0; i < ptkCommands.Length; i++)
        {
            ptkCommands[i].Execute(firstTriggerData);
        }
    }
}
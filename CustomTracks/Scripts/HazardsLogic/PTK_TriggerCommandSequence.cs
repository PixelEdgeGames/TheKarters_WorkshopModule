using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommandSequence : MonoBehaviour
{
    public enum ETriggerEventType
    {
        E0_ENTER,
        E1_EXIT,
        E2_BOTH
    }

    public string sequenceName = "Command Sequence";
    public bool bIsCommandSequenceEnabled = true;
    public ETriggerEventType eCallSequenceOnTriggerState = ETriggerEventType.E0_ENTER;

    public List<PTK_TriggerCommandBase> sequenceCommands = new List<PTK_TriggerCommandBase>();

    bool bDefaultEnabledState = false;

    public void Start()
    {
        bDefaultEnabledState = bIsCommandSequenceEnabled;
        Ant_MainGame.Instance.RaceResettedPrepeareForNewOne += RaceResetted;
    }

    private void RaceResetted()
    {
        bIsCommandSequenceEnabled = bDefaultEnabledState;
    }

    public void Execute()
    {
        ExecuteImpl();
    }

    protected virtual void ExecuteImpl()
    {
        for(int i=0;i< sequenceCommands.Count;i++)
        {
            sequenceCommands[i].Execute();
        }
    }
}

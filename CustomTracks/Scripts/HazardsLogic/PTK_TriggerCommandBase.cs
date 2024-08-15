using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommandBase : MonoBehaviour
{
    public string commandName = "Command name";
    public bool bIsCommandEnabled = true;

    bool bDefaultEnabledState = false;
    public virtual void Start()
    {
        bDefaultEnabledState = bIsCommandEnabled;
        Ant_MainGame.Instance.RaceResettedPrepeareForNewOne += RaceResetted;
    }

    private void RaceResetted()
    {
        bIsCommandEnabled = bDefaultEnabledState;
    }

    public void Execute()
    {
        if(bIsCommandEnabled == true)
            ExecuteImpl();
    }

    protected virtual void ExecuteImpl()
    {

    }
}

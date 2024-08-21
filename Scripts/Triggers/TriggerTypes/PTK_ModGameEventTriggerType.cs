using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModGameEventTriggerType : PTK_ModBaseTrigger
{
    [Header("Event Type Conditions")]
    public List<PTK_Mod_TriggerGameEventConditions.CEventTypeTrigger> eventTypesConditionsToCheck = new List<PTK_Mod_TriggerGameEventConditions.CEventTypeTrigger>();
    PTK_Mod_TriggerGameEventConditions eventConditions = new PTK_Mod_TriggerGameEventConditions();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_EVENT_TYPE;
    }

    public override void Start()
    {
        base.Start();

        eventConditions.Awake_InitializeAndAttachToEvents(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();


        eventConditions.Destroy_Deinitialize();
    }
}

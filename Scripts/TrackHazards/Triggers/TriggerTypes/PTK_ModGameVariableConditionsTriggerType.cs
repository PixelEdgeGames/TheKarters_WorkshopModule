using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModGameVariableConditionsTriggerType : PTK_ModBaseTrigger
{
    public enum ETriggerActivationType
    {
        E0_CONDITION_MET,        // When the condition is met
        E1_CONDITION_NO_LONGER_MET,    // When the condition is no longer met
        E2_BOTH_MET_AND_NO_LONGER_MET  // Both when the condition is met and when it is no longer met
    }


    [Header("Trigger if ANY of these conditions are correct")]
    public ETriggerActivationType eTriggerActivationType = ETriggerActivationType.E0_CONDITION_MET;
    public List<PTK_Mod_TriggerVariableConditions> variableTypeConditions = new List<PTK_Mod_TriggerVariableConditions>();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_VARIABLE_CONDITION;
    }
    public override void Start()
    {
        base.Start();


        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            variableTypeConditions[i].Awake_InitializeAndAttachToEvents(this);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();


        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            variableTypeConditions[i].Destroy_Deinitialize();
        }

    }

    public override void Update()
    {
        base.Update();

        if (bIsTriggerEnabled == false)
            return;

        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            if (variableTypeConditions[i].bIgnoreConditions == true)
                continue;

            variableTypeConditions[i].CheckConditions(this);
        }
    }

}

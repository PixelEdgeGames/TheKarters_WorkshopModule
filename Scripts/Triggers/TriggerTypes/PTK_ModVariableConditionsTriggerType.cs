using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModVariableConditionsTriggerType : PTK_ModBaseTrigger
{
    public enum ETriggerActivationType
    {
        E0_CONDITION_MET,        // When the condition is met
        E1_CONDITION_NO_LONGER_MET,    // When the condition is no longer met
        E2_BOTH_MET_AND_NO_LONGER_MET  // Both when the condition is met and when it is no longer met
    }

    public ETriggerActivationType eTriggerActivationType = ETriggerActivationType.E0_CONDITION_MET;

    [Header("Variable Conditions - Check for Players Within Distance Range")]
    public List<PTK_PlayersInRangeVolume_Base> checkPlayersInVolumes = new List<PTK_PlayersInRangeVolume_Base>();
    [Header("Trigger if ANY of these conditions are correct")]
    public List<PTK_Mod_TriggerVariableConditions> variableTypeConditions;

   
    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_VARIABLE_CONDITION;
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

    private void Update()
    {
        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            if (variableTypeConditions[i].bIgnoreConditions == true)
                continue;

            variableTypeConditions[i].CheckConditions(this);
        }
    }
}

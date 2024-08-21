using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModVariableConditionsTriggerType : PTK_ModBaseTrigger
{
    [Header("Variable Conditions - Check for Players Within Distance Range")]
    public float fDistanceToSearchForAnyPlayerInRange = 9999;
    public bool bInRangeCheckOnlyForLocalPlayersWithCamera = true;
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

    [HideInInspector]
    public bool[] bAreGlobalPlayersWithinRange = new bool[8];

    private void Update()
    {
        for (int i = 0; i < bAreGlobalPlayersWithinRange.Length; i++)
        {
            if (PTK_ModGameplayDataSync.Instance.playersInfo[i].bIsPlayerEnabled == true && Vector3.Magnitude(transform.position - PTK_ModGameplayDataSync.Instance.playersInfo[i].vPosition) < fDistanceToSearchForAnyPlayerInRange)
            {
                if (bInRangeCheckOnlyForLocalPlayersWithCamera == true && PTK_ModGameplayDataSync.Instance.playersInfo[i].iLocalCameraIndex == -1)
                    bAreGlobalPlayersWithinRange[i] = false; // no camera
                else
                    bAreGlobalPlayersWithinRange[i] = true;
            }
            else
            {
                bAreGlobalPlayersWithinRange[i] = false;
            }
        }

        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            if (variableTypeConditions[i].bIgnoreConditions == true)
                continue;

            variableTypeConditions[i].CheckConditions(this);
        }
    }
}

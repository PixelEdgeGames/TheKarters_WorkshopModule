using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModPlayerInRangeEventTriggerType : PTK_ModBaseTrigger
{
    [Header("Event Type Conditions")]
    public bool bInRangeCheckOnlyForLocalPlayersWithCamera = true;
    public float fDistanceToSearchForAnyPlayerInRange = 9999;
    public List<PTK_Mod_TriggerPlayerInRangeEventConditions.CEventTypeTrigger> eventTypesConditionsToCheck = new List<PTK_Mod_TriggerPlayerInRangeEventConditions.CEventTypeTrigger>();
    PTK_Mod_TriggerPlayerInRangeEventConditions eventConditions = new PTK_Mod_TriggerPlayerInRangeEventConditions();

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_GAME_EVENT_TYPE;
    }

    [HideInInspector]
    public bool[] bAreGlobalPlayersWithinRange = new bool[8];
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
    }
}

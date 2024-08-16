using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[ExecuteInEditMode]
public class PTK_Mod_Trigger : MonoBehaviour
{

    public enum ETriggerActivationType
    {
        E0_ON_ENTER,
        E1_ON_EXIT,
        E2_ON_BOTH
    }

    public enum ETriggerFrequency
    {
        E0_ALWAYS_EACH_DETECTION,
        E1_ONCE_UNTIL_ENABLED_AGAIN
    }

    [Header("Core")]
    public bool bIsTriggerEnabled = true;
    public ETriggerFrequency eTriggerFrequency = ETriggerFrequency.E0_ALWAYS_EACH_DETECTION;
    public ETriggerActivationType eActivationType = ETriggerActivationType.E0_ON_ENTER;

    public static string strHazardTriggerTagName = "PTK_HazardTrigger";

    [Header("Params")]
    public bool bDetectPlayers = true;
    public bool bTargettedByMissiles = false;
    public bool bAffectedByAreaEffectWeapons = false;

    [Header("Player Ignore Settings")]
    public bool bIgnorePlayerWhenTeleporting = false;
    public bool bIgnorePlayerWhenImmune = false;
    public bool bIgnorePlayerWhenUsingBoostingItem = false;

    public PTK_TriggerCommandSequence[] commandSequencesToCall;

    [Header("Auto Generated - Do not change")]
    public int iUniqueTriggerID = -1;
    [HideInInspector]
    public int iUniqueTriggerIDCratedForObjectInstance = 0;
    private void Start()
    {
        this.tag = PTK_Mod_Trigger.strHazardTriggerTagName;

    }

    private void OnEnable()
    {
        if (iUniqueTriggerID == -1 || iUniqueTriggerID == 0 || (iUniqueTriggerIDCratedForObjectInstance != this.gameObject.GetInstanceID()))
            GenerateUniqueID();
    }

    void GenerateUniqueID()
    {
        int iUniqueIDToSet = UnityEngine.Random.Range(1, int.MaxValue - 1);
        var existingTriggers = GameObject.FindObjectsOfType<PTK_Mod_Trigger>();

        int iSafeguard = 0;
        for(int i=0;i< existingTriggers.Length;i++)
        {
            if(existingTriggers[i].iUniqueTriggerID == iUniqueIDToSet)
            {
                iUniqueIDToSet = UnityEngine.Random.Range(1, int.MaxValue - 1);
                i = 0;
                iSafeguard++;
            }

            if(iSafeguard>10)
            {
                Debug.LogError("Cant generate trigger unique id -WTF");
                break;
            }
        }

        iUniqueTriggerID = iUniqueIDToSet;
        iUniqueTriggerIDCratedForObjectInstance = this.gameObject.GetInstanceID();
    }

    private void OnDestroy()
    {
        this.tag = "Untagged";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = new Color(1.0f, 0.75f, 0.8f);

        // Draw a wireframe box
        Handles.DrawWireCube(transform.position , Vector3.one);

        // Draw the text inside the box
        Vector3 textPosition = transform.position ;
        Handles.Label(textPosition, "Hazard Trigger");
    }
#endif

}

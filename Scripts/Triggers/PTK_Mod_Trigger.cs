using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class PTK_Mod_Trigger : MonoBehaviour
{
    public enum ETriggerActivationType
    {
        E0_ON_ENTER,
        E1_ON_EXIT,
        E2_ON_BOTH
    }

    [Header("Core")]
    public ETriggerActivationType eActivationType = ETriggerActivationType.E0_ON_ENTER;

    public static string strHazardTriggerTagName = "PTK_HazardTrigger";

    [Header("Trigger Events From")]
    [SerializeField]
    bool _bTriggerByPlayerCollision = true;
    [SerializeField]
    bool _bTiggerByBulletCollision = false;
    [SerializeField]
    bool _bTriggerByRangedWeaponsDamage = false;

    [Header("Auto Trigger Events")]
    [SerializeField]
    bool _bAutoTriggerOnSceneLoad = false;
    [SerializeField]
    bool _bAutoTriggerOnRaceRestart = false;
    [SerializeField]
    bool _bAutoTriggerOnRaceTimerStart = false;

    public bool bEnabledAndDetectedByPlayers
    {
        get
        {
            return bIsTriggerEnabled == true && _bTriggerByPlayerCollision;
        }
    }
    public bool bEnabledAndTargettedByMissiles
    {
        get
        {
            return bIsTriggerEnabled == true && _bTiggerByBulletCollision;
        }
    }
    public bool bEnabledAndAffectedByAreaWeaponsEffects
    {
        get
        {
            return bIsTriggerEnabled == true && _bTriggerByRangedWeaponsDamage;
        }
    }
    public bool bIsTriggerEnabled
    {
        get
        {
            return this.gameObject.activeInHierarchy == true;
        }
    }


    [Header("Player Ignore Settings")]
    public bool bIgnorePlayerWhenTeleporting = false;
    public bool bIgnorePlayerWhenImmune = false;
    public bool bIgnorePlayerWhenUsingBoostingItem = false;

    [Header("Variable Conditions - Check for Players Within Distance Range")]
    public float fDistanceToSearchForAnyPlayer = 9999;
    [Header("Trigger if ANY of these conditions are correct")]
    public List<PTK_Mod_TriggerVariableConditions> variableTypeConditions;

    [Header("Auto Generated - Do not change")]
    public int iUniqueTriggerID = -1;
    [HideInInspector]
    public int iUniqueTriggerIDCratedForObjectInstance = 0;


    public class CTriggerEventData
    {
        public bool bTriggerEntered = false;
        public bool bTriggerExited = false;
        public bool bManualTriggerLaunched = false;

        public bool bTriggeredByPlayer = false;
        public int iGlobalPlayerIndex = 0;

        public bool bTriggeredByWeapon = false;
        public int iWeaponType = -1;
        public float fMissileDmgStrengthNormalized = 1.0f;
    }

    public Action<CTriggerEventData> OnTriggerEventNetSynced;
    public Action OnRaceResettedEvent;
    public Action OnRaceTimerSyncedJustStarted;

    private void Start()
    {
        EnsureContainsTriggerID();

        this.tag = PTK_Mod_Trigger.strHazardTriggerTagName;

        for(int i=0;i< variableTypeConditions.Count;i++)
        {
            variableTypeConditions[i].Awake_InitializeAndAttachToEvents(this);
        }

        var commandBehaviour = this.GetComponent<PTK_TriggerCommandsBehaviour>();
        if (commandBehaviour != null)
        {
            // player added  command to this trigger - we will auto commands launcher so player dont need to assign it by hand
            
            if(this.GetComponent<PTK_TriggerArrayCommandsExecutor>() == null)
            {
                var commandLauncher = this.gameObject.AddComponent<PTK_TriggerArrayCommandsExecutor>();

                commandLauncher.triggersToReceiveDataOnTriggerEvent.Add(this);
                commandLauncher.commandBehavioursToRun.Add(commandBehaviour);
            }
        }

        OnRaceResettedEvent += OnRaceResetted;
        OnRaceTimerSyncedJustStarted += OnRaceTimerJustStarted;

        if(_bAutoTriggerOnSceneLoad)
        {
            if(OnTriggerEventNetSynced != null)
            {
                CTriggerEventData triggerData = new CTriggerEventData();
                triggerData.bManualTriggerLaunched = true;
                OnTriggerEventNetSynced(triggerData);
            }
        }
    }


    private void OnDestroy()
    {
        this.tag = "Untagged";

        for (int i = 0; i < variableTypeConditions.Count; i++)
        {
            variableTypeConditions[i].Destroy_Deinitialize();
        }
    }

    private void OnRaceTimerJustStarted()
    {
        if (_bAutoTriggerOnRaceTimerStart)
        {
            if (OnTriggerEventNetSynced != null)
            {
                CTriggerEventData triggerData = new CTriggerEventData();
                triggerData.bManualTriggerLaunched = true;
                OnTriggerEventNetSynced(triggerData);
            }
        }
    }

    private void OnRaceResetted()
    {
        if (_bAutoTriggerOnRaceRestart)
        {
            if (OnTriggerEventNetSynced != null)
            {
                CTriggerEventData triggerData = new CTriggerEventData();
                triggerData.bManualTriggerLaunched = true;
                OnTriggerEventNetSynced(triggerData);
            }
        }
    }

    private void OnValidate()
    {
        EnsureContainsTriggerID();
    }

    public void EnsureContainsTriggerID()
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

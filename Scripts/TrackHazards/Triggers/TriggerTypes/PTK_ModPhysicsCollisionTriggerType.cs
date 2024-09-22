using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[DisallowMultipleComponent]
public class PTK_ModPhysicsCollisionTriggerType : PTK_ModBaseTrigger
{
    public enum ETriggerActivationType
    {
        E0_ON_PLAYER_ENTER,
        E1_ON_PLAYER_EXIT,
        E2_TWICE_ON_PLAYER_ENTER_AND_EXIT
    }

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_PHYSICS_COLLISION;
    }

    public static string strPhysicsTriggerTagName = "PTK_PhysicsTrigger";


    [Header("Player Trigger")]
    [SerializeField]
    bool _bTriggerByPlayerCollision = true;
    public ETriggerActivationType ePlayerTriggerActivationType = ETriggerActivationType.E0_ON_PLAYER_ENTER;
    public bool bIgnorePlayerWhenTeleporting = false;
    public bool bIgnorePlayerWhenImmune = false;
    public bool bIgnorePlayerWhenUsingBoostingItem = false;

    [Header("Weapons Trigger")]
    [SerializeField]
    bool _bTiggerByBulletCollision = false;
    [SerializeField]
    bool _bTriggerByRangedWeaponsDamage = false;

    [Header("[Optional] - Multiple Hits from single weapon")]
    public bool bTriggerFromEachBulletDamageHit = false;


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



   
    [Header("Auto Generated - Do not change")]
    public int iUniquePhysicsTriggerID = -1;
    public Transform extraCollidersParent;

    public override void Start()
    {
        base.Start();

        EnsureContainsTriggerID();

        this.tag = PTK_ModPhysicsCollisionTriggerType.strPhysicsTriggerTagName;

        if(extraCollidersParent != null)
        {
            var childObjects = extraCollidersParent.GetComponentsInChildren<Collider>();

            for (int i = 0; i < childObjects.Length; i++)
                childObjects[i].tag = PTK_ModPhysicsCollisionTriggerType.strPhysicsTriggerTagName;
        }

    }
    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnTriggerEnabledDetected()
    {
    }
    public override void OnTriggerDisabledDetected()
    {
    }



    public override void OnDestroy()
    {
        base.OnDestroy();

        this.tag = "Untagged";



    }

    public override void Update()
    {
        base.Update();
    }


    private void OnValidate()
    {
        if(Application.isPlaying == false)
        {
            PTK_ModPhysicsCollisionTriggerType[] allObjects = FindObjectsOfType<PTK_ModPhysicsCollisionTriggerType>(true);
            foreach (PTK_ModPhysicsCollisionTriggerType obj in allObjects)
            {
                if (obj != this && obj.iUniquePhysicsTriggerID == iUniquePhysicsTriggerID)
                {
                    GenerateUniqueID();
                    Debug.LogWarning("Duplicate detected! " + name + " has the same Unique ID as " + obj.name);
                    break;
                }
            }
        }

        EnsureContainsTriggerID();
    }

    public void EnsureContainsTriggerID()
    {
        if (iUniquePhysicsTriggerID == -1 || iUniquePhysicsTriggerID == 0 )
            GenerateUniqueID();
    }
    void GenerateUniqueID()
    {
        int iUniqueIDToSet = UnityEngine.Random.Range(1, int.MaxValue - 1);
        var existingTriggers = GameObject.FindObjectsOfType<PTK_ModPhysicsCollisionTriggerType>();

        int iSafeguard = 0;
        for(int i=0;i< existingTriggers.Length;i++)
        {
            if(existingTriggers[i].iUniquePhysicsTriggerID == iUniqueIDToSet)
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

        iUniquePhysicsTriggerID = iUniqueIDToSet;
    }

  

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = new Color(1.0f, 0.75f, 0.8f);

        // Draw a wireframe box
        Handles.DrawWireCube(transform.position , Vector3.one);

        // Draw the text inside the box
        Vector3 textPosition = transform.position ;
        Handles.Label(textPosition, "Trigger");
    }

#endif

}

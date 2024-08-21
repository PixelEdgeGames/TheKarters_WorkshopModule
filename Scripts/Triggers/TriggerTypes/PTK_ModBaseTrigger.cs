using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PTK_ModBaseTrigger : MonoBehaviour
{
    public enum ETriggerType
    {
        E_PHYSICS_COLLISION,
        E_GAME_EVENT_TYPE,
        E_PLAYER_IN_RANGE_EVENT_TYPE,
        E_VARIABLE_CONDITION
    }

    public class CTriggerEventData_Player
    {
        public bool bTriggeredByPlayer = false;
        public int iGlobalPlayerIndex = 0;
        public bool bTriggerEntered = false;
        public bool bTriggerExited = false;
    }

    public class CTriggerEventData_Weapon
    {
        public int iGlobalPlayerIndex = 0;
        public bool bTriggeredByWeapon = false;
        public int iWeaponType = -1;
        public float fMissileDmgStrengthNormalized = 1.0f;
    }


    public Action OnTriggerEvent;
    public Action OnTriggerEvent_ByVariableConditions;
    public Action OnTriggerEvent_ByEventCondition;
    public Action<CTriggerEventData_Player> OnTriggerEvent_ByPlayerCollision;
    public Action<CTriggerEventData_Weapon> OnTriggerEvent_ByWeaponCollision;


    public abstract ETriggerType GetTriggerType();

    public virtual void Start()
    {
        var commandBehaviour = this.GetComponent<PTK_TriggerCommandsBehaviour>();
        if (commandBehaviour != null)
        {
            // player added  command to this trigger - we will auto commands launcher so player dont need to assign it by hand

            if (this.GetComponent<PTK_TriggerArrayCommandsExecutor>() == null)
            {
                var commandLauncher = this.gameObject.AddComponent<PTK_TriggerArrayCommandsExecutor>();

                commandLauncher.triggersToReceiveDataOnTriggerEvent.Add(this);
                commandLauncher.commandBehavioursToRun.Add(commandBehaviour);
            }
        }

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceResetted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerJustStarted;
    }

    public virtual void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceResetted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerJustStarted;
    }


    public virtual void OnRaceTimerJustStarted()
    {
    }

    public virtual void OnRaceResetted()
    {
    }
}

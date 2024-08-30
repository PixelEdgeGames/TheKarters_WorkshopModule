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
        E_GAME_VARIABLE_CONDITION
    }

    [System.Serializable]
    public class CTriggerEventData_Player
    {
        public bool bTriggeredByPlayer = false;
        public int iGlobalPlayerIndex = 0;
        public bool bTriggerEntered = false;
        public bool bTriggerExited = false;
    }

    [System.Serializable]
    public class CTriggerEventData_Weapon
    {
        public int iGlobalPlayerIndex = 0;
        public bool bTriggeredByWeapon = false;
        public int iWeaponType = -1;
        public float fMissileDmgStrengthNormalized = 1.0f;
    }

    [System.Serializable]
    public class CTriggerEventType
    {
        public enum ETriggerType
        {
            E0_GAME_VARIABLE,
            E1_GAME_EVENT,
            E2_PLAYER_EVENT,
            E3_PLAYER_COLLISION_WITH_DATA,
            E4_PLAYER_WEAPON_WITH_DATA,
        }

        public ETriggerType eTriggerType = ETriggerType.E0_GAME_VARIABLE;

        public PTK_ModGameEventTriggerType.EGameEventType e1_GameEventType;
        public PTK_ModPlayerInRangeEventTriggerType.EPlayerEventType e2_PlayerEventType;
        public CPlayerEventData e2_PlayerEventData;

        public int iPlayerType_GlobalPlayerIndex = -1;
        public CTriggerEventData_Player e3_playerCollisionData;
        public CTriggerEventData_Weapon e4_playerWeaponData;

        [System.Serializable]
        public class CPlayerEventData
        {
            public int iWeaponTypeIndex;
            public int iBoostType;
            public float fBoostStrength;
            public float fBoostDuration;
        }
        public CTriggerEventType(ETriggerType _triggerType)
        {
            eTriggerType = _triggerType;
            iPlayerType_GlobalPlayerIndex = -1;
        }
        public CTriggerEventType(ETriggerType _triggerType, PTK_ModGameEventTriggerType.EGameEventType _e1_GameEventType)
        {
            eTriggerType = _triggerType;
            e1_GameEventType = _e1_GameEventType;
            iPlayerType_GlobalPlayerIndex = -1;
        }
        public CTriggerEventType(ETriggerType _triggerType, PTK_ModPlayerInRangeEventTriggerType.EPlayerEventType _PlayerEventType, int iGlobalPlayerIndex)
        {
            eTriggerType = _triggerType;
            e2_PlayerEventType = _PlayerEventType;
            iPlayerType_GlobalPlayerIndex = iGlobalPlayerIndex;
        }
        public CTriggerEventType(ETriggerType _triggerType, PTK_ModPlayerInRangeEventTriggerType.EPlayerEventType _PlayerEventType, int iGlobalPlayerIndex, CPlayerEventData _playerEventData)
        {
            eTriggerType = _triggerType;
            e2_PlayerEventType = _PlayerEventType;
            iPlayerType_GlobalPlayerIndex = iGlobalPlayerIndex;
            e2_PlayerEventData = _playerEventData;
        }
        

        public CTriggerEventType(ETriggerType _triggerType, int iGlobalPlayerIndex)
        {
            eTriggerType = _triggerType;
            iPlayerType_GlobalPlayerIndex = iGlobalPlayerIndex;
        }
        public CTriggerEventType(ETriggerType _triggerType, int iGlobalPlayerIndex, CTriggerEventData_Player _playerCollisionData)
        {
            eTriggerType = _triggerType;
            iPlayerType_GlobalPlayerIndex = iGlobalPlayerIndex;
            e3_playerCollisionData = _playerCollisionData;
        }

        public CTriggerEventType(ETriggerType _triggerType, int iGlobalPlayerIndex, CTriggerEventData_Weapon _playerWeaponData)
        {
            eTriggerType = _triggerType;
            iPlayerType_GlobalPlayerIndex = iGlobalPlayerIndex;
            e4_playerWeaponData = _playerWeaponData;
        }

        private CTriggerEventType()
        {
        }
    }

    public Action<CTriggerEventType> OnTriggerEvent;


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

                commandLauncher.receiveEventsFromTriggers.Add(this);
                commandLauncher.receiveEventsFromAllTriggerInGameObjects.Add(this.gameObject);
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


    protected virtual void OnRaceTimerJustStarted()
    {
    }

    protected virtual void OnRaceResetted()
    {
    }
}

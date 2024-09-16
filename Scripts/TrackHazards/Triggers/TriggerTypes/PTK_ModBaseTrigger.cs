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
        E_GAME_VARIABLE_CONDITION,
        E_AUTO_TRIGGER
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
            E5_AUTO_CALLED_TRIGGER,
            E6_TRIGGER_ENABLE_DISABLE_ACTION
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

    public Action<CTriggerEventType> OnTriggerActivated;

    [Header("Base Settings ")]
    public bool bIsTriggerEnabled = true;
    [Header("Show Debug Mesh - Unused : set global setting in ModTrackData object in scene")]
    public bool bShowDebugCollider = true;
    private bool bTriggerEnabledInitialState = true;

    [Header("Automation - Auto Enable/Disable Settings")]
    public CAutomationSettings autoEnableDisableSettings = new CAutomationSettings();

    [Header("Trigger Enabled/Disabled Event Commands")]
    public COnEnableDisableCommands onTriggerEnabledDisabledCommands = new COnEnableDisableCommands();

    [System.Serializable]
    public class CAutomationSettings
    {
        public bool bAutoDisableAfterTriggerEvent = false;
        public bool bAutoEnableWhenTriggerIsDisabled = false;
        public float fAutoEnableAfterTime = 5.0f;
        [HideInInspector]
        public float fTimeSinceTriggerIsDisabled = 0.0f;
    }

    [System.Serializable]
    public class COnEnableDisableCommands
    {
        public CTriggerEnabledDisabledCommands onTriggerEnabledCommands = new CTriggerEnabledDisabledCommands();
        public CTriggerEnabledDisabledCommands onTriggerDisabledCommands = new CTriggerEnabledDisabledCommands();


        [System.Serializable]
        public class CTriggerEnabledDisabledCommands
        {
            public bool bAutoCall_OnRaceRestart_ToResetState = false;
            public bool bAutoCall_WithRaceBegin_ToResetSynchronized = false;
            public List<PTK_TriggerCommandsBehaviour> CommandsToCall = new List<PTK_TriggerCommandsBehaviour>();
        }
    }


    public abstract ETriggerType GetTriggerType();


    PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData info = new PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData();
    public virtual void Start()
    {
        bTriggerEnabledInitialState = bIsTriggerEnabled;
        info.trigger = this;
        info.bSignalReceived = true;
        info.triggerTypeAndData = new CTriggerEventType(CTriggerEventType.ETriggerType.E6_TRIGGER_ENABLE_DISABLE_ACTION);


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

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceResettedPriv;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerJustStartedPriv;

    }


    public virtual void OnDestroy()
    {

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceResettedPriv;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerJustStartedPriv;

        OnTriggerActivated = null;
    }


    bool bWasGameObjectDisabled = false;


    public virtual void OnEnable()
    {
        // because it will be called on start anyway - so we need if it was disabled or not
        if(bWasGameObjectDisabled == true)
        {
            TriggerEnabledDetected();
        }

        bWasGameObjectDisabled = false;
    }

    public virtual void OnDisable()
    {
        bWasGameObjectDisabled = true;

        TriggerDisabledDetected();
    }

    bool bRaceStarted = false;
    bool bLastWasEnabled = false;

    private void OnRaceResettedPriv()
    {
        bRaceStarted = false;
        autoEnableDisableSettings.fTimeSinceTriggerIsDisabled = 0.0f;
        bIsTriggerEnabled = bTriggerEnabledInitialState;
        bLastWasEnabled = bIsTriggerEnabled;

        if (onTriggerEnabledDisabledCommands.onTriggerEnabledCommands.bAutoCall_OnRaceRestart_ToResetState == true)
        {
            foreach(var behaviour in onTriggerEnabledDisabledCommands.onTriggerEnabledCommands.CommandsToCall)
            {
                behaviour.Execute(info);
            }
        }

        if (onTriggerEnabledDisabledCommands.onTriggerDisabledCommands.bAutoCall_OnRaceRestart_ToResetState == true)
        {
            foreach (var behaviour in onTriggerEnabledDisabledCommands.onTriggerDisabledCommands.CommandsToCall)
            {
                behaviour.Execute(info);
            }
        }


        OnRaceResetted();
    }

    private void OnRaceTimerJustStartedPriv()
    {

        if (onTriggerEnabledDisabledCommands.onTriggerEnabledCommands.bAutoCall_WithRaceBegin_ToResetSynchronized == true)
        {
            foreach (var behaviour in onTriggerEnabledDisabledCommands.onTriggerEnabledCommands.CommandsToCall)
            {
                behaviour.Execute(info);
            }
        }

        if (onTriggerEnabledDisabledCommands.onTriggerDisabledCommands.bAutoCall_WithRaceBegin_ToResetSynchronized == true)
        {
            foreach (var behaviour in onTriggerEnabledDisabledCommands.onTriggerDisabledCommands.CommandsToCall)
            {
                behaviour.Execute(info);
            }
        }

        OnRaceTimerJustStarted();
        bRaceStarted = true;
        autoEnableDisableSettings.fTimeSinceTriggerIsDisabled = 0.0f;
    }

    public abstract void OnTriggerEnabledDetected();
    public abstract void OnTriggerDisabledDetected();


    void TriggerEnabledDetected()
    {
        foreach (var behaviour in onTriggerEnabledDisabledCommands.onTriggerEnabledCommands.CommandsToCall)
        {
            behaviour.Execute(info);
        }

        OnTriggerEnabledDetected();
    }

    void TriggerDisabledDetected()
    {
        foreach (var behaviour in onTriggerEnabledDisabledCommands.onTriggerDisabledCommands.CommandsToCall)
        {
            behaviour.Execute(info);
        }

        OnTriggerDisabledDetected();

    }

    public virtual void Update()
    {
        if(bRaceStarted == true)
        {
            if(bIsTriggerEnabled != bLastWasEnabled )
            {

                if (bIsTriggerEnabled == true)
                {
                    TriggerEnabledDetected();
                }
                else
                {
                    OnTriggerDisabledDetected();
                }
            }

            bLastWasEnabled = bIsTriggerEnabled;

            if (bIsTriggerEnabled == false)
            {
                autoEnableDisableSettings.fTimeSinceTriggerIsDisabled += Time.deltaTime;

                if(autoEnableDisableSettings.bAutoEnableWhenTriggerIsDisabled == true)
                {
                    if(autoEnableDisableSettings.fTimeSinceTriggerIsDisabled > autoEnableDisableSettings.fAutoEnableAfterTime)
                    {
                        bIsTriggerEnabled = true;
                    }
                }
            }else
            {
                autoEnableDisableSettings.fTimeSinceTriggerIsDisabled = 0.0f;
            }
        }
    }

    public void InvokeTriggerAction(CTriggerEventType obj)
    {
        if (this.gameObject.activeInHierarchy == false || bIsTriggerEnabled == false)
            return;

        if (OnTriggerActivated != null)
            OnTriggerActivated(obj);

        autoEnableDisableSettings.fTimeSinceTriggerIsDisabled = Time.time;

        if(autoEnableDisableSettings.bAutoDisableAfterTriggerEvent == true)
        {
            bIsTriggerEnabled = false;
            autoEnableDisableSettings.fTimeSinceTriggerIsDisabled = 0.0f;
        }
    }

    protected virtual void OnRaceTimerJustStarted()
    {
    }

    protected virtual void OnRaceResetted()
    {
    }
}

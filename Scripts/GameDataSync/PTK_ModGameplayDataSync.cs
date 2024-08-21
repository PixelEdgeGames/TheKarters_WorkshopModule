using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModGameplayDataSync : MonoBehaviour
{
    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CGameInfo
    {
        public enum ERaceState
        {
            E_WAITING_FOR_RACE_START,
            E_RACE_IS_RUNNING_COUNTDOWN_COMPLETE,
            E_AT_LEAST_ONE_PLAYER_FINISHED_RACE_WAITING_FOR_END,
            E_RACE_FINISHED_PRESENTING_SUMMARY
        }

        public bool bIsWholeRaceFinished = false;
        public ERaceState eRaceState = ERaceState.E_WAITING_FOR_RACE_START;
        public float fCurrentRaceTime = 0.0f;

        public bool bIsOnlinePlay = false;

        public int iCurrentPlayingPlayersCount = 0;
        public int iLocalPlayersCount = 0;

        public int iCurrentCupRoundIndex = 0;
        public int iCurrentCupRoundsCount = 0;

        public string strTrackName = "";
        public int iTrackConfigID = 0;

        public string strGameModeName = "";
        public int iGameModeConfigID = 0;
        public bool bIsRaceGameMode = false;
        public bool bIsTimeTrialGameMode = false;

        public int iCurrentBestLapNrOnHud = 0;
    }

    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CCameraInfo
    {
        public bool bCameraEnabled = false;
        public Vector3 vCameraPosition = Vector3.zero;
        public Vector3 vCameraDirection = Vector3.zero;
        public CPlayerInfo parentPlayerInfo;
    }


    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CPlayerInfo
    {
        public bool bIsPlayerEnabled = false;
        public int iLocalCameraIndex = -1;

        public Vector3 vPosition;
        public Vector3 vVelocity;
        public Quaternion qCurrentRotation;

        public int iHUDLapNr = 1;

        public bool bRaceFinished = false;
        public int iRacePositionIndex = 0;
        public int iPlayerScore = 0;
        public float fPlayerFinishedRaceFinalTime = 0.0f;

        public bool bIsPlayerGrouned = true;
        public Vector3 vPlayerVisualGrounedNormal = Vector3.up;

        public bool bIsDrifting = false;
        public bool bIsBreaking = false;
        public bool bIsMovingBackward = false;
        public bool bIsBoosting = false;
        public float fSteerDir = 0.0f;
        public float fDriftDir = 0.0f;
        public float fAccelInput = 0.0f;

        public float fReservesPercentage = 0.0f;
        public bool bIsWheelTouchingGround_BL = false;
        public bool bIsWheelTouchingGround_BR = false;
        public bool bIsWheelTouchingGround_FL = false;
        public bool bIsWheelTouchingGround_FR = false;
        public bool bAreBoostbarsReadyToFire = false;
        public float fBeforeRaceBurningWheelsLoadedValNormalized = 0;


        public int iPlayerHP = 100;
        public bool bIsPlayerRespawning = false;
        public bool bIsImmune = false;
        public bool bIsDeathImmune = false;
        public bool bIsPoisoned = false;

        public PixelWeaponObject.EWeaponType eCurrentWeaponType;

        public PTK_GroundType.CGroundSettings currentDrivingGroundData;

    }

    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CGameEvents
    {
        // global events
        public Action OnGameEvent_RaceFinished;

        public Action OnGameEvent_RaceRestarted;
        public Action OnGameEvent_RaceTimerStart;

        public Action OnGameEvent_GamePaused;
        public Action OnGameEvent_GameUnpaused;

    }


    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CPlayerEvents
    {
        // player events
        // first int is global player index
        public Action<int> OnPlayerEvent_JustJumped;
        public Action<int, float> OnPlayerEvent_JustLanded;
        public Action<int> OnPlayerEvent_JustDied;
        public Action<int, int> OnPlayerUsedWeapon;
        public Action<int, int> OnPlayerJustReceivedWeapon;
        public Action<int> OnPlayerMadeTrick;
        public Action<int, int, float, float> OnPlayerBoostFired;
    }

    public CGameInfo gameInfo = new CGameInfo();

    public CPlayerInfo[] playersInfo = new[] { new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo() };
    public CCameraInfo[] localCamsInfo = new CCameraInfo[] { new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo() };

    public CPlayerEvents playerEvents = new CPlayerEvents();
    public CGameEvents gameEvents = new CGameEvents();



    // better performance if we don't receive events from all players that are fired constantly
    internal void RegisterToGameTypeEventsOnly(PTK_ModGameplayData_GameEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        gameEvents.OnGameEvent_RaceFinished += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_RaceRestarted += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_GameUnpaused;
    }

    internal void UnRegisterFromGameTypeEventsOnly(PTK_ModGameplayData_GameEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        gameEvents.OnGameEvent_RaceFinished -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceFinished;

        gameEvents.OnGameEvent_RaceRestarted -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_GamePaused -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_GamePaused;
        gameEvents.OnGameEvent_GameUnpaused -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_GameUnpaused;
    }

    internal void RegisterToPlayerAllEvents(PTK_ModGameplayData_PlayerEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {

        playerEvents.OnPlayerEvent_JustJumped += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustDied;
        playerEvents.OnPlayerUsedWeapon += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_BoostFired;
    }



    internal void UnRegisterFromAllPlayerEvents(PTK_ModGameplayData_PlayerEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        playerEvents.OnPlayerEvent_JustJumped -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustDied;
        playerEvents.OnPlayerUsedWeapon -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerJustReceivedWeapon -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustReceivedWeapon;
        playerEvents.OnPlayerMadeTrick -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerBoostFired -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_BoostFired;
    }


    //---------------------------------------------

    static PTK_ModGameplayDataSync _Instance;
    public static PTK_ModGameplayDataSync Instance
    {
        get
        {
            if(_Instance == null)
            {
                var dataSync = new GameObject();
                dataSync.SetActive(false); // so Awake wont be called before we set instance
                dataSync.name = "PTK_ModGameplayDataSync";
                _Instance = dataSync.AddComponent<PTK_ModGameplayDataSync>();
                GameObject.DontDestroyOnLoad(dataSync);
                dataSync.SetActive(true); // so Awake wont be called before we set instance
            }

            return _Instance;
        }
    }

    private void Awake()
    {
        // in case someone added it by hand to game object
        if(_Instance != this)
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModGameplayDataSync : MonoBehaviour
{
    public class CGameInfo
    {
        public float fCurrentRaceTime = 0.0f;
        public int iCurrentPlayingVehicles = 0;
        public int iLocalPlayersCount = 0;


        public string strTrackName = "";
        public int iTrackConfigID = 0;

        public string strGameModeName = "";
        public int iGameModeConfigID = 0;
        public bool bIsRaceGameMode = false;
        public bool bIsTimeTrialGameMode = false;

        public int iCurrentBestLapNrOnHud = 0;
    }

    [System.Serializable]
    public class CCameraInfo
    {
        public bool bCameraEnabled = false;
        public Vector3 vCamPos = Vector3.zero;
        public Vector3 vCameraDirection = Vector3.zero;
    }


    [System.Serializable]
    public class CPlayerInfo
    {
        public bool bIsPlaying = false;

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
        public float fBoostBarLoadedValNormalized = 0.0f;
        public int iBoostBarLoadingIndex = 0;
        public float fBeforeRaceBurningWheelsLoadedValNormalized = 0;


        public int iPlayerHP = 100;
        public bool bIsPlayerRespawning = false;
        public bool bIsImmune = false;
        public bool bIsDeathImmune = false;
        public bool bIsPoisoned = false;

        public PixelWeaponObject.EWeaponType eCurrentWeaponType;

        public PTK_GroundType.CGroundSettings currentDrivingGroundData;

    }

    [System.Serializable]
    public class CGameEvents
    {
        // global events
        public Action OnGameEvent_RaceFinished;
        public Action OnGameEvent_FirstPlayerMovedThroughFinishLine;
        public Action OnGameEvent_FirstPlayerDied;

        public Action OnGameEvent_RaceRestarted;
        public Action OnGameEvent_RaceTimerStart;
        public Action OnGameEvent_AnyPlayerDied;
        public Action OnGameEvent_LapNrIncreased;
        public Action OnGameEvent_FinalLapStarted;
    }


    [System.Serializable]
    public class CPlayerEvents
    {
        // player events
        // first int is global player index
        public Action<int> OnPlayerEvent_JustJumped;
        public Action<int, float> OnPlayerEvent_JustLanded;
        public Action<int> OnPlayerEvent_JustDied;
        public Action<int, int> OnPlayerEvent_FinishedRace;
        public Action<int> OnPlayerKilledOpponent;
        public Action<int, int> OnPlayerUsedWeapon;
        public Action<int> OnPlayerMadeTrick;
        public Action<int> OnPlayerDestroyedItemBox;
        public Action<int, int, float, float> OnPlayerBoostFired;

        public Action<int> OnPlayerLapNrIncrease;
    }

    public CGameInfo gameInfo = new CGameInfo();

    public CPlayerInfo[] playersInfo = new[] { new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo() };
    public CCameraInfo[] camerasInfo = new CCameraInfo[] { new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo() };

    public CPlayerEvents playerEvents = new CPlayerEvents();
    public CGameEvents gameEvents = new CGameEvents();



    // better performance if we don't receive events from all players that are fired constantly
    internal void RegisterToGameTypeEventsOnly(PTK_ModGameplayData_AllEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        gameEvents.OnGameEvent_RaceFinished += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_FirstPlayerMovedThroughFinishLine += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FirstPlayerMovedThroughFinishLine;
        gameEvents.OnGameEvent_FirstPlayerDied += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FirstPlayerDied;

        gameEvents.OnGameEvent_RaceRestarted += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_AnyPlayerDied += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_AnyPlayerDied;
        gameEvents.OnGameEvent_LapNrIncreased += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_LapNrIncreased;
        gameEvents.OnGameEvent_FinalLapStarted += pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FinalLapStarted;
    }

   internal void RegisterToAllEvents(PTK_ModGameplayData_AllEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        RegisterToGameTypeEventsOnly(pTK_ModGameplayData_AllEventsRegister);

        playerEvents.OnPlayerEvent_JustJumped += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustDied;
        playerEvents.OnPlayerEvent_FinishedRace += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_FinishedRace;
        playerEvents.OnPlayerKilledOpponent += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_KilledOpponent;
        playerEvents.OnPlayerUsedWeapon += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerMadeTrick += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerDestroyedItemBox += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_DestroyedItemBox;
        playerEvents.OnPlayerBoostFired += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_BoostFired;
    }


    internal void UnRegisterFromGameTypeEventsOnly(PTK_ModGameplayData_AllEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        gameEvents.OnGameEvent_RaceFinished -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceFinished;
        gameEvents.OnGameEvent_FirstPlayerMovedThroughFinishLine -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FirstPlayerMovedThroughFinishLine;
        gameEvents.OnGameEvent_FirstPlayerDied -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FirstPlayerDied;

        gameEvents.OnGameEvent_RaceRestarted -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceRestarted;
        gameEvents.OnGameEvent_RaceTimerStart -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_RaceTimerStart;
        gameEvents.OnGameEvent_AnyPlayerDied -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_AnyPlayerDied;
        gameEvents.OnGameEvent_LapNrIncreased -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_LapNrIncreased;
        gameEvents.OnGameEvent_FinalLapStarted -= pTK_ModGameplayData_AllEventsRegister.OnGameEvent_FinalLapStarted;
    }

    internal void UnRegisterFromAllEvents(PTK_ModGameplayData_AllEventsRegister pTK_ModGameplayData_AllEventsRegister)
    {
        UnRegisterFromGameTypeEventsOnly(pTK_ModGameplayData_AllEventsRegister);

        playerEvents.OnPlayerEvent_JustJumped -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustJumped;
        playerEvents.OnPlayerEvent_JustLanded += pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustLanded;
        playerEvents.OnPlayerEvent_JustDied -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_JustDied;
        playerEvents.OnPlayerEvent_FinishedRace -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_FinishedRace;
        playerEvents.OnPlayerKilledOpponent -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_KilledOpponent;
        playerEvents.OnPlayerUsedWeapon -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_UsedWeapon;
        playerEvents.OnPlayerMadeTrick -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_MadeTrick;
        playerEvents.OnPlayerDestroyedItemBox -= pTK_ModGameplayData_AllEventsRegister.OnPlayerEvent_DestroyedItemBox;
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
                dataSync.name = "PTK_ModGameplayDataSync";
                GameObject.DontDestroyOnLoad(dataSync);
                _Instance = dataSync.AddComponent<PTK_ModGameplayDataSync>();
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

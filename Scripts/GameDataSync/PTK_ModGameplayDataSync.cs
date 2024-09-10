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
        public int iPlayerRaceRestartCount = 0;
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
        public PTK_Command_05_PlayerLogicEffects.EPlayerDamageVFXType eCurrentDamageEffect = PTK_Command_05_PlayerLogicEffects.EPlayerDamageVFXType.E0_NONE;
        public bool bShowingSquishEffect = false;
        public bool bShowingHealingEffect = false;


        public int iCurrentWeaponType;

        public PTK_GroundType.CGroundSettings currentDrivingGroundData;

    }

    // values need to use network synced data - use properties if possible instead of adding event types like PlayerRaceFinished (it can be triggered by checking player property instead)
    [System.Serializable]
    public class CGameEvents
    {
        // global events
        public Action OnGameEvent_WholeRaceFinished;
        public Action OnGameEvent_FirstPlayerFinishedRace;

        public Action OnGameEvent_RaceRestarted;
        public Action OnGameEvent_RaceTimerStart;

        public Action OnGameEvent_GamePaused;
        public Action OnGameEvent_GameUnpaused;

        public Action OnGameEvent_RaceLeftLoadingMainMenu;
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
        public Action<int,int> OnPlayerEvent_JustKilledSomeone;
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModGameplayDataSync : MonoBehaviour
{
    public float fCurrentRaceTime = 0.0f;
    public int iCurrentPlayingVehicles = 0;
    public int iLocalPlayersCount = 0;

    public string strTrackName = "";
    public int iTrackConfigID = 0;

    public string strGameModeName = "";
    public int iGameModeConfigID = 0;

    public int iCurrentBestLapNrOnHud = 0;

    [System.Serializable]
    public class CCameraInfo
    {
        public bool bCameraEnabled = false;
        public Vector3 vCamPos = Vector3.zero;
        public Vector3 vCameraDirection = Vector3.zero;
    }

    public CCameraInfo[] camerasInfo = new CCameraInfo[] { new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo(), new CCameraInfo() };

    [System.Serializable]
    public class CPlayerInfo
    {
        public bool bIsPlaying = false;
        public bool bIsLocalPlayerWithCamera = false;
        public bool bIsNetworkPlayer = false;
        public bool bIsAI = false;
        public int iLocalPlayerIndex = 0;
        public Vector3 vPosition;
        public Vector3 vVelocity;
        public float fReservesPercentage = 0.0f;
        public Quaternion qCurrentRotation;

        public int iHUDLapNr = 1;
        public bool bRaceFinished = false;
        public int iRacePositionIndex = 0;
        public int iPlayerScore = 0;

        public bool bIsPlayerGrouned = true;
        public bool bIsDrifting = false;
        public bool bIsBreaking = false;
        public bool bIsMovingBackward = false;
        public bool bIsBoosting = false;
        public float fSteerDir = 0.0f;
        public float fDriftDir = 0.0f;
        public float fAccelInput = 0.0f;

        public float fBoostBarLoadedValNormalized = 0.0f;
        public int iBoostBarLoadingIndex = 0;
        public float fBeforeRaceBurningWheelsLoadedValNormalized = 0;

        public bool bIsPlayerRespawning = false;

        public int iPlayerHP = 100;
        public bool bIsImmune = false;
        public bool bIsDeathImmune = false;
        public bool bIsPoisoned = false;

        public PTK_GroundType.CGroundSettings currentDrivingGroundData;

        public Action OnJustJumped;
        public Action OnJustDied;
        public Action OnFinishedRace;
        public Action OnKilledOpponent;
        public Action<int> OnUsedWeapon;
        public Action OnMadeTrick;
        public Action OnDestroyedItemBox;
        public Action<int,float,float> OnBoostFired;

        public Action OnPlayerLapNrIncrease;
    }

    public CPlayerInfo[] gamePlayers = new[] { new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo(), new CPlayerInfo() };

    public Action OnRaceFinished;
    public Action OnFirstPlayerMovedThroughFinishLine;
    public Action OnFirstPlayerDied;


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

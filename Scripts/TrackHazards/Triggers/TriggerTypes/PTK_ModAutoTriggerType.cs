using UnityEngine;

public class PTK_ModAutoTriggerType : PTK_ModBaseTrigger
{
    [Header("How soon after race begin to call trigger")]
    public float fMinimumRaceTimeToTrigger = 0.0f;
    public float fDelayAfterGameObjectEnabled = 0.0f;

    [Header("Repeat per Race count (-1 for infinite)")]
    public int iTriggerRepeatCount = 1;

    public enum ECounterResetMode
    {
        E0_ON_RACE_RESTART,
        E1_ON_RACE_RESTART_AND_EACH_TRIGGER_ENABLED_EVENT
    }
    public ECounterResetMode eRepeatCounterResetMode = ECounterResetMode.E0_ON_RACE_RESTART;

    [Header("Wait before next trigger")]
    public bool bUseRandomRepeatWaitTime = false;
    public float fRepeatWaitTime = 1.0f;
    public Vector2 v2RandomWaitRange = new Vector2(1.0f, 1.0f);
    private float fFinalWaitBeforeNextTrigger = 0.0f;

    [Header("Settings")]
    public bool bStopTriggerAtRaceEnd = false;
    public bool bStopOnFirstPlayerRaceEnd = false;
    public bool bOnlyInRaceGameMode = false;
    public bool bOnlyInTimeTrialGameMode = false;
    public enum ELapCondition
    {
        E0_NONE_DISABLED,
        E1_HIGHER_OR_EQUAL,
        E2_LOWER_OR_EQUAL,
        E3_EQUAL
    };
    public ELapCondition eLapCondition = ELapCondition.E0_NONE_DISABLED;
    public int iLapConditionNr = 1;

    [Header("Allows to use Player Logic Effects Commannds")]
    public bool bTriggerWithPlayerEvents = false;
    public CTriggerOnPlayersSettings triggerTargetPlayersSettings = new CTriggerOnPlayersSettings();

    [System.Serializable]
    public class CTriggerOnPlayersSettings
    {
        public enum ETriggerOnPlayersType
        {
            E0_ALL_PLAYERS,
            E1_LOCAL_WITH_CAMERA_ONLY
        }

        [Header("Players Type")]
        public ETriggerOnPlayersType eTriggerOnPlayers = ETriggerOnPlayersType.E0_ALL_PLAYERS;

        public enum EPlayerRaceState
        {
            E0_ANY,
            E1_CURRENTLY_RACING,
            E2_FINISHED_RACE
        }
        [Header("Playing Players")]
        public EPlayerRaceState ePlayerRacingState = EPlayerRaceState.E0_ANY;

        public enum EPlayersRacePosType
        {
            E0_ANY_RACE_POS,
            E1_WITH_RACE_POS_EQUAL_TO_NR,
            E2_WITH_RACE_POS_NOT_EQUAL_TO_NR,
            E3_WITH_RACE_POS_HIGHER_THAN_NR,
            E4_WITH_RACE_POS_HIGHER_OR_EQUAL_THAN_NR,
            E5_WITH_RACE_POS_LOWER_THAN_NR,
            E6_WITH_RACE_POS_LOWER_OR_EQUAL_THAN_NR,
        }
        [Header("Players Race Pos Condition")]
        public EPlayersRacePosType ePlayerRacePosMode = EPlayersRacePosType.E0_ANY_RACE_POS;

        [Header("Race Pos Nr ")]
        [Range(1, 8)]
        public int iRacePosNr = 1;
    }
   

    public override ETriggerType GetTriggerType()
    {
        return ETriggerType.E_AUTO_TRIGGER;
    }

    private float fLastRaceTimeTrigger = -1;
    private int iCurrentRepeatCount = 0;
    private bool bIsRaceActive = true;

    public override void Start()
    {
        base.Start();

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestarted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_WholeRaceFinished += OnRaceEnded;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_FirstPlayerFinishedRace += OnFirstPlayerFinishedRace;
    }

    float fEnabledTime = 0.0f;
    private void OnEnable()
    {
        fEnabledTime = Time.time;

        if (eRepeatCounterResetMode == ECounterResetMode.E1_ON_RACE_RESTART_AND_EACH_TRIGGER_ENABLED_EVENT && bIsTriggerEnabled == true)
            ResetCounter();
    }
    private void OnRaceRestarted()
    {
        ResetCounter();

        bIsRaceActive = true;
        bWasTriggerEnabled = true;
    }

    void ResetCounter()
    {
        fLastRaceTimeTrigger = -1;
        iCurrentRepeatCount = 0;

        fFinalWaitBeforeNextTrigger = fRepeatWaitTime;
        if (bUseRandomRepeatWaitTime)
        {
            fFinalWaitBeforeNextTrigger = v2RandomWaitRange.x;
        }
    }

    private void OnRaceEnded()
    {
        if (bStopTriggerAtRaceEnd)
        {
            bIsRaceActive = false;
        }
    }

    private void OnFirstPlayerFinishedRace()
    {
        if (bStopOnFirstPlayerRaceEnd)
        {
            bIsRaceActive = false;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        var gameEvents = PTK_ModGameplayDataSync.Instance.gameEvents;

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceRestarted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_WholeRaceFinished -= OnRaceEnded;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_FirstPlayerFinishedRace -= OnFirstPlayerFinishedRace;
    }

    bool bWasTriggerEnabled = false;

    public override void Update()
    {
        base.Update();

        if (bIsTriggerEnabled == true && bWasTriggerEnabled == false)
        {
            if (eRepeatCounterResetMode== ECounterResetMode.E1_ON_RACE_RESTART_AND_EACH_TRIGGER_ENABLED_EVENT)
                ResetCounter();
        }

        float fCurrentRaceTime = PTK_ModGameplayDataSync.Instance.gameInfo.fCurrentRaceTime;

        if (PTK_ModGameplayDataSync.Instance.gameInfo.eRaceState == PTK_ModGameplayDataSync.CGameInfo.ERaceState.E_WAITING_FOR_RACE_START)
            return;

        if (bIsTriggerEnabled == false)
            return;


        // Check if race is still active
        if (!bIsRaceActive)
            return;

        if (PTK_ModGameplayDataSync.Instance.gameInfo.bIsRaceGameMode == false && bOnlyInRaceGameMode == true)
            return;

        if (PTK_ModGameplayDataSync.Instance.gameInfo.bIsTimeTrialGameMode == false && bOnlyInTimeTrialGameMode == true)
            return;

        if (eLapCondition == ELapCondition.E1_HIGHER_OR_EQUAL && PTK_ModGameplayDataSync.Instance.gameInfo.iCurrentBestLapNrOnHud < iLapConditionNr)
            return;

        if (eLapCondition == ELapCondition.E2_LOWER_OR_EQUAL && PTK_ModGameplayDataSync.Instance.gameInfo.iCurrentBestLapNrOnHud > iLapConditionNr)
            return;

        if (eLapCondition == ELapCondition.E3_EQUAL && PTK_ModGameplayDataSync.Instance.gameInfo.iCurrentBestLapNrOnHud != iLapConditionNr)
            return;

        // Only trigger if the race time has passed the minimum threshold
        if ( (Time.time-fEnabledTime) >= fDelayAfterGameObjectEnabled &&  fCurrentRaceTime >= fMinimumRaceTimeToTrigger )
        {
            // Check if we can trigger more events based on repeat count
            if (iCurrentRepeatCount < iTriggerRepeatCount || iTriggerRepeatCount == -1)
            {
                // Check if enough time has passed since the last trigger call
                if (fLastRaceTimeTrigger < 0 || fCurrentRaceTime >= (fLastRaceTimeTrigger + fFinalWaitBeforeNextTrigger))
                {
                    AutoTriggerEvent();
                    fLastRaceTimeTrigger = fCurrentRaceTime;
                    iCurrentRepeatCount++;
                }
            }
        }
    }

    private void AutoTriggerEvent()
    {
        if(bTriggerWithPlayerEvents == true)
        {
            for(int i=0;i< PTK_ModGameplayDataSync.Instance.playersInfo.Length;i++)
            {
                var playerInfo = PTK_ModGameplayDataSync.Instance.playersInfo[i];

                if (playerInfo.bIsPlayerEnabled == false)
                    continue;

                if (triggerTargetPlayersSettings.eTriggerOnPlayers == CTriggerOnPlayersSettings.ETriggerOnPlayersType.E1_LOCAL_WITH_CAMERA_ONLY && playerInfo.IsLocalPlayerWithCam() == false)
                    continue;

                // only finished players
                if (triggerTargetPlayersSettings.ePlayerRacingState ==  CTriggerOnPlayersSettings.EPlayerRaceState.E2_FINISHED_RACE  && playerInfo.bRaceFinished == false)
                    continue;

                // only still racing players
                if (triggerTargetPlayersSettings.ePlayerRacingState == CTriggerOnPlayersSettings.EPlayerRaceState.E1_CURRENTLY_RACING && playerInfo.bRaceFinished == true)
                    continue;

                bool bIsRacePosConditionCorrect = false;

                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E0_ANY_RACE_POS)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E1_WITH_RACE_POS_EQUAL_TO_NR && (playerInfo.iRacePositionIndex + 1) == triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E2_WITH_RACE_POS_NOT_EQUAL_TO_NR && (playerInfo.iRacePositionIndex + 1) != triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E3_WITH_RACE_POS_HIGHER_THAN_NR && (playerInfo.iRacePositionIndex + 1) > triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E4_WITH_RACE_POS_HIGHER_OR_EQUAL_THAN_NR && (playerInfo.iRacePositionIndex + 1) >= triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E5_WITH_RACE_POS_LOWER_THAN_NR && (playerInfo.iRacePositionIndex + 1) < triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;
                if (triggerTargetPlayersSettings.ePlayerRacePosMode == CTriggerOnPlayersSettings.EPlayersRacePosType.E6_WITH_RACE_POS_LOWER_OR_EQUAL_THAN_NR && (playerInfo.iRacePositionIndex + 1) <= triggerTargetPlayersSettings.iRacePosNr)
                    bIsRacePosConditionCorrect = true;

                if (bIsRacePosConditionCorrect == false)
                    continue;


                InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E5_AUTO_CALLED_TRIGGER, i));
            }
        }
        else
        {
            InvokeTriggerAction(new PTK_ModBaseTrigger.CTriggerEventType(PTK_ModBaseTrigger.CTriggerEventType.ETriggerType.E5_AUTO_CALLED_TRIGGER));
        }

        if (bUseRandomRepeatWaitTime)
        {
            fFinalWaitBeforeNextTrigger = GetDeterministicRandom(PTK_ModGameplayDataSync.Instance.gameInfo.fCurrentRaceTime, v2RandomWaitRange.x, v2RandomWaitRange.y);
        }else
        {
            fFinalWaitBeforeNextTrigger = fRepeatWaitTime;
        }
    }

    private float GetDeterministicRandom(float fRaceTime, float fMin, float fMax)
    {
        // Use Mathf.Sin to generate a pseudo-random value based on the race time
        float fNormalized = Mathf.Sin(fRaceTime * 10.0f); // Scale the input to vary the "randomness"
        fNormalized = Mathf.Abs(fNormalized); // Ensure the value is positive

        // Map the value to the desired range (fMin, fMax)
        return Mathf.Lerp(fMin, fMax, fNormalized);
    }
}

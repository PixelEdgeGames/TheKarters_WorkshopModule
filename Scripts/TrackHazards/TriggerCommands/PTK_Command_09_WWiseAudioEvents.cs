using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_09_WWiseAudioEvents : PTK_TriggerCommandBase
{

    [System.Serializable]
    public class CWwiseEvents
    {
        public enum EPlayOnTarget
        {
            E0_ON_TARGET_GAME_OBJECT,
            E1_ON_PLAYER_FROM_TRIGGER,
            E2_ON_PLAYER_FROM_TRIGGER_LOCAL_PLAYER_WITH_CAM_ONLY
        }


        [Header("Play on Position ")]
        public EPlayOnTarget eOrigin = EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT;
        [Header("Target Game Object (ignore for PlayerType)")]
        public GameObject targetGameObject;

        [Header("Delay")]
        public float fDelay = 0.0f;

        [System.Serializable]
        public abstract class CWwiseEventBase
        {
            public float fDelay = 0.0f;

            public enum EType
            {
                E_EVENT,
                E_SWITCH,
                E_STATE,
                E_FVALUE,
                E_IVALUE
            }

            public bool ShouldSendEventThisFrame(float fCurrentTime)
            {
                if(fCurrentTime <= fDelay && (fCurrentTime+Time.deltaTime) > fDelay)
                {
                    return true;
                }

                return false;
            }

            public abstract EType GetWwiseEventType();
        }

        [System.Serializable]
        public class CEvent : CWwiseEventBase
        {
            [Header("Wwise Data")]
            public string strWwise_EventName = "";

            public override EType GetWwiseEventType()
            {
                return EType.E_EVENT;
            }
        }

        [System.Serializable]
        public class CSwitch : CWwiseEventBase
        {
            [Header("Wwise Data")]
            public string strWwise_SwitchName = "";
            public string strWwise_SwitchValue = "";
            public override EType GetWwiseEventType()
            {
                return EType.E_SWITCH;
            }
        }

        [System.Serializable]
        public class CState : CWwiseEventBase
        {
            [Header("Wwise Data")]
            public string strWwise_StateName = "";
            public string strWwise_StateValue = "";
            public override EType GetWwiseEventType()
            {
                return EType.E_STATE;
            }
        }

        [System.Serializable]
        public class CFValue : CWwiseEventBase
        {
            [Header("Wwise Data")]
            public string strWwise_ValueName = "";
            public float fValueToSet;
            public override EType GetWwiseEventType()
            {
                return EType.E_FVALUE;
            }
        }

        [System.Serializable]
        public class CIValue : CWwiseEventBase
        {
            [Header("Wwise Data")]
            public string strWwise_ValueName = "";
            public int iValueToSet;
            public override EType GetWwiseEventType()
            {
                return EType.E_IVALUE;
            }
        }

        [Header("Main")]
        public List<CEvent> audioEvents = new List<CEvent>();
        [Header("Advanced")]
        public List<CSwitch> switches = new List<CSwitch>();
        public List<CState> states = new List<CState>();
        public List<CFValue> fValues = new List<CFValue>();
        public List<CIValue> iValues = new List<CIValue>();

        [HideInInspector]
        public List<CWwiseEvents.CWwiseEventBase> allEvents = new List<CWwiseEvents.CWwiseEventBase>();

        public void InitAllEventsList()
        {
            allEvents.Clear();

            allEvents.AddRange(fValues);
            allEvents.AddRange(iValues);
            allEvents.AddRange(states);
            allEvents.AddRange(switches);
            allEvents.AddRange(audioEvents); // audio play events at the end so we will play correct item
        }
    }

    public List<CWwiseEvents> wwiseTargets = new List<CWwiseEvents>();
    public static Action<int, CWwiseEvents.CWwiseEventBase, CWwiseEvents.EPlayOnTarget,GameObject> OnModWwiseEventTriggered;

    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E09_WWISE_AUDIO_EVENT;
    }


    public override void Awake()
    {

    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }

    public void Update()
    {
    }


    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        List<int> iPlayerIndexes = new List<int>();
        for (int i = 0; i < recivedTriggerSignals.Count; i++)
        {
            iPlayerIndexes.Add(recivedTriggerSignals[i].triggerTypeAndData.iPlayerType_GlobalPlayerIndex);
        }

        ExecuteWWiseEvents(iPlayerIndexes);
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
       ExecuteWWiseEvents(new List<int>() { recivedTriggerSignal.triggerTypeAndData.iPlayerType_GlobalPlayerIndex });
    }



    void ExecuteWWiseEvents(List<int> _iGlobalPlayerIndex)
    {
        if (wwisePostingEvents != null)
            StopCoroutine(wwisePostingEvents);

        wwisePostingEvents = StartCoroutine(PostWwiseEvents(_iGlobalPlayerIndex));
    }

    Coroutine wwisePostingEvents = null;

    IEnumerator PostWwiseEvents(List<int> _iGlobalPlayerIndex)
    {
        for (int i = 0; i < wwiseTargets.Count; i++)
            wwiseTargets[i].InitAllEventsList();

        float fTimeSinceTargetEventStarted = 0;
        float fTimeSinceTargetEventStartedWithoutDelay = 0;

        while (true)
        {
            for (int iEventsTarget = 0; iEventsTarget < wwiseTargets.Count; iEventsTarget++)
            {
                var wwiseTargetWithEvents = wwiseTargets[iEventsTarget];

                if (fTimeSinceTargetEventStarted >= wwiseTargetWithEvents.fDelay)
                {
                    for (int i = 0; i < wwiseTargetWithEvents.allEvents.Count; i++)
                    {
                        if (wwiseTargetWithEvents.allEvents[i].ShouldSendEventThisFrame(fTimeSinceTargetEventStartedWithoutDelay) == true)
                        {
                            for (int iPlayer = 0; iPlayer < _iGlobalPlayerIndex.Count; iPlayer++)
                            {
                                OnModWwiseEventTriggered?.Invoke(_iGlobalPlayerIndex[iPlayer], wwiseTargetWithEvents.allEvents[i], wwiseTargetWithEvents.eOrigin, wwiseTargetWithEvents.targetGameObject);

                            }
                        }
                    }

                    // so it will be 0 after fTimeSinceTargetEventStarted delay
                    fTimeSinceTargetEventStartedWithoutDelay += Time.deltaTime;

                }

                yield return new WaitForEndOfFrame();

                // important at the end!
                fTimeSinceTargetEventStarted += Time.deltaTime;
            }   
        }

    }

    protected override void RaceResetted_RevertToDefault()
    {
        if (wwisePostingEvents != null)
            StopCoroutine(wwisePostingEvents);
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

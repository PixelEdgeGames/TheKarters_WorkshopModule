using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class PTK_AnimationEventsHelper : MonoBehaviour
{
    [SerializeField]
    public CEvent animRestartedDefaultStateEvents;

    [SerializeField]
    public List<CEvent> events = new List<CEvent>();

    void Awake()
    {
        foreach (var e in events)
        {
            foreach (var psEvent in e.particleSystemEvents)
            {
                psEvent.Initialize();
            }
        }

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

        OnRaceRestart();

    }
    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceRestart;
    }

    private void OnRaceRestart()
    {
        animRestartedDefaultStateEvents.TriggerEvents(0);
    }
    private void OnRaceTimerStart()
    {
    }

    void Update()
    {
        for (int i = 0; i < events.Count; i++)
        {
            events[i].Update();
        }
    }

    [Serializable]
    public class BaseEvent
    {
        public BaseEvent() { }
    }
    
    [Serializable]
    public class AnimatorEvent : BaseEvent
    {
        public enum AnimatorActionType { SetTrigger, SetBool, SetFloat }
        public Animator animator;
        public AnimatorActionType actionType;
        public string parameterName;
        public bool boolValue;
        public float floatValue; // For SetFloat action
        public void Execute()
        {
            if (animator == null) return;

            switch (actionType)
            {
                case AnimatorActionType.SetTrigger:
                    animator.SetTrigger(parameterName);
                    break;
                case AnimatorActionType.SetBool:
                    animator.SetBool(parameterName, boolValue);
                    break;
                case AnimatorActionType.SetFloat:
                    animator.SetFloat(parameterName, floatValue);
                    break;
            }
        }
    }

    [Serializable]
    public class AnimatorAnimPlayEvent : BaseEvent
    {
        public Animator animator;
        [Header("Use Name or AnimClip")]
        public string strAnimClipName;
        public AnimationClip animClip;
        [Header("Cross-Fade Blend")]
        public float fCrossFadeBlendTime = 0.1f;

        public void Execute()
        {
            if (animator == null) return;

            if(fCrossFadeBlendTime == 0.0f)
            {
                if (strAnimClipName != "")
                    animator.Play(strAnimClipName);

                if (animClip != null)
                    animator.Play(animClip.name);
            }
            else
            {
                if (strAnimClipName != "")
                    animator.CrossFade(strAnimClipName, fCrossFadeBlendTime, 0, 0);

                if (animClip != null)
                    animator.CrossFade(animClip.name, fCrossFadeBlendTime, 0, 0);
            }

        }
    }


    [Serializable]
    public class ParticleSystemEvent : BaseEvent
    {
        public enum ParticleSystemActionType { PlayParticles, PauseParticles, StopParticles, EmitParticles , StopEmitting }

        public ParticleSystem particleSystem;
        public ParticleSystemActionType actionType;
        public bool bIncludeChildrens = true;
        private List<ParticleSystem> childParticleSystems;
        public int emitCount; // For EmitParticles action

        public void Initialize()
        {
            if (bIncludeChildrens && particleSystem != null)
            {
                childParticleSystems = new List<ParticleSystem>(particleSystem.GetComponentsInChildren<ParticleSystem>());
            }
        }

        public void Execute()
        {
            if (particleSystem == null) return;

            ExecuteAction(particleSystem);

            if (bIncludeChildrens && childParticleSystems != null)
            {
                foreach (var ps in childParticleSystems)
                {
                    ExecuteAction(ps);
                }
            }
        }

        private void ExecuteAction(ParticleSystem ps)
        {
            switch (actionType)
            {
                case ParticleSystemActionType.PlayParticles:
                    ps.Play();
                    break;
                case ParticleSystemActionType.PauseParticles:
                    ps.Stop(); // pause looks weird when particles are frozen
                    break;
                case ParticleSystemActionType.StopParticles:
                    ps.Stop();
                    break;
                case ParticleSystemActionType.EmitParticles:
                    ps.Emit(emitCount);
                    break;
                case ParticleSystemActionType.StopEmitting:
                    var emission = ps.emission;
                    emission.enabled = false;
                    break;
            }
        }
    }

    [System.Serializable]
    public class CWWiseAudioEvent: BaseEvent
    {
        // public float fDelay = 0.0f; // not used - if you decide to use, ensure ShouldSendEventThisFrame wont use fCurrentTimeSinceTriggered because it will be > 0 after delay, so we need time that passed after time for this delay ended (so another time value that will be increased after this delay)
        public GameObject playInTargetGameObjectPosition;

        [Header("Main")]
        public List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CEvent> audioEvents = new List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CEvent>();
        [Header("Advanced")]
        public List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CSwitch> switches = new List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CSwitch>();
        public List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CState> states = new List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CState>();
        public List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CFValue> fValues = new List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CFValue>();
        public List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CIValue> iValues = new List<PTK_Command_09_WWiseAudioEvents.CWwiseEvents.CIValue>();
    }

    [Serializable]
    public class TimelineEvent : BaseEvent
    {
        public enum TimelineActionType { Play, Stop, Pause, Restart }

        public PlayableDirector timeline;
        public TimelineActionType actionType;

        public void Execute()
        {
            if (timeline == null) return;

            switch (actionType)
            {
                case TimelineActionType.Play:
                    timeline.Play();
                    break;
                case TimelineActionType.Stop:
                    timeline.Stop();
                    break;
                case TimelineActionType.Pause:
                    timeline.Pause();
                    break;
                case TimelineActionType.Restart:
                    timeline.Stop();
                    timeline.time = 0;
                    timeline.Play();
                    break;
            }
        }
    }

    [Serializable]
    public class GameObjectEvent : BaseEvent
    {
        public GameObject gameObject;
        public bool setActive;

        public void Execute()
        {
            if (gameObject == null) return;

            gameObject.SetActive(setActive);
        }
    }

    [Serializable]
    public class CEvent
    {
        [Header("Name ID - Used in AnimationClip Event - function TK2_RunAnimationEvent")]
        public string strEventNameID;


        [Header("Game Objects")]
        [SerializeField]
        public GameObjectEvent[] gameObjectEvents;

        [Header("Particle Systems")]
        [SerializeField]
        public ParticleSystemEvent[] particleSystemEvents;

        [Header("Animator Params")]
        [SerializeField]
        public AnimatorAnimPlayEvent[] animatorPlayAnimations;
        [SerializeField]
        public AnimatorEvent[] animatorEvents;

        [Header("Audio Events")]
        public CWWiseAudioEvent[] wwiseAudioEvents;

        [Header("Timeline")]
        [SerializeField]
        public TimelineEvent[] timelineEvents;

        [Header("AllTypes Unity Events")]
        public UnityEngine.Events.UnityEvent eventsToCall;

        // Method to trigger events based on time type
        public void TriggerEvents(float normalizedTime)
        {
            TriggerAnimatorEvents();
            TriggerParticleSystemEvents();
            TriggerTimelineEvents();
            TriggerGameObjectEvents();

            fTimeSinceAudioTriggerEvent = 0.0f;
            bTriggerEventReceived = true;

            eventsToCall?.Invoke();
        }

        bool bTriggerEventReceived = false;
        float fTimeSinceAudioTriggerEvent = 0.0f;
        public void Update()
        {
            if(bTriggerEventReceived == true)
            {
                CheckWWiseTriggerEvents(fTimeSinceAudioTriggerEvent);

                // important that it is at the end
                fTimeSinceAudioTriggerEvent += Time.deltaTime;
            }
        }

        private void CheckWWiseTriggerEvents(float fCurrentTimeSinceTriggered)
        {
            if (wwiseAudioEvents == null)
                return;

            foreach(var wwiseAudioGO in wwiseAudioEvents)
            {
                if (wwiseAudioGO.playInTargetGameObjectPosition == null)
                    continue;


                foreach (var objEvent in wwiseAudioGO.fValues)
                {
                    if (objEvent.ShouldSendEventThisFrame(fCurrentTimeSinceTriggered) == false)
                        continue;

                    PTK_Command_09_WWiseAudioEvents.OnModWwiseEventTriggered?.Invoke(-1, objEvent, PTK_Command_09_WWiseAudioEvents.CWwiseEvents.EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT, wwiseAudioGO.playInTargetGameObjectPosition);
                }

                foreach (var objEvent in wwiseAudioGO.iValues)
                {
                    if (objEvent.ShouldSendEventThisFrame(fCurrentTimeSinceTriggered) == false)
                        continue;

                    PTK_Command_09_WWiseAudioEvents.OnModWwiseEventTriggered?.Invoke(-1, objEvent, PTK_Command_09_WWiseAudioEvents.CWwiseEvents.EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT, wwiseAudioGO.playInTargetGameObjectPosition);
                }

                foreach (var objEvent in wwiseAudioGO.states)
                {
                    if (objEvent.ShouldSendEventThisFrame(fCurrentTimeSinceTriggered) == false)
                        continue;

                    PTK_Command_09_WWiseAudioEvents.OnModWwiseEventTriggered?.Invoke(-1, objEvent, PTK_Command_09_WWiseAudioEvents.CWwiseEvents.EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT, wwiseAudioGO.playInTargetGameObjectPosition);
                }

                foreach (var objEvent in wwiseAudioGO.switches)
                {
                    if (objEvent.ShouldSendEventThisFrame(fCurrentTimeSinceTriggered) == false)
                        continue;

                    PTK_Command_09_WWiseAudioEvents.OnModWwiseEventTriggered?.Invoke(-1, objEvent, PTK_Command_09_WWiseAudioEvents.CWwiseEvents.EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT, wwiseAudioGO.playInTargetGameObjectPosition);
                }

                foreach (var objEvent in wwiseAudioGO.audioEvents)
                {
                    if (objEvent.ShouldSendEventThisFrame(fCurrentTimeSinceTriggered) == false)
                        continue;

                    PTK_Command_09_WWiseAudioEvents.OnModWwiseEventTriggered?.Invoke(-1, objEvent, PTK_Command_09_WWiseAudioEvents.CWwiseEvents.EPlayOnTarget.E0_ON_TARGET_GAME_OBJECT, wwiseAudioGO.playInTargetGameObjectPosition);
                }
            }
            
        }

        private void TriggerAnimatorEvents()
        {
            if(animatorEvents != null)
            {
                foreach (var animEvent in animatorEvents)
                {
                    animEvent.Execute();
                }
            }

            if(animatorPlayAnimations != null)
            {
                foreach (var animEvent in animatorPlayAnimations)
                {
                    animEvent.Execute();
                }
            }

            
        }

        private void TriggerParticleSystemEvents()
        {
            if (particleSystemEvents == null)
                return;

            foreach (var psEvent in particleSystemEvents)
            {
                psEvent.Execute();
            }
        }

        private void TriggerTimelineEvents()
        {
            if (timelineEvents == null)
                return;

            foreach (var timelineEvent in timelineEvents)
            {
                timelineEvent.Execute();
            }
        }

        private void TriggerGameObjectEvents()
        {
            if (gameObjectEvents == null)
                return;

            foreach (var goEvent in gameObjectEvents)
            {
                goEvent.Execute();
            }
        }
    }

    public void TK2_RunAnimationEventID(string strEventNameID)
    {
        if (events == null)
            return;

      //  Debug.LogError("Show log");
        for (int i = 0; i < events.Count; i++)
        {
            if(events[i].strEventNameID == strEventNameID)
            {
                events[i].TriggerEvents(0);
            }
        }
    }


}

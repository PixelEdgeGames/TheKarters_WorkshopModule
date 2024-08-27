using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class PTK_AnimationEventsHelper : MonoBehaviour
{
    [SerializeField]
    private Animation animationComponent; // Animation component to check

    [SerializeField]
    private AnimationClip animationClip; // Animation clip to check against

    [SerializeField]
    public CEvent animRestartedDefaultStateEvents;

    [SerializeField]
    public List<CEvent> events = new List<CEvent>();


    private float previousTime;
    private float animationLength;

    void Awake()
    {
        if (animationClip != null)
        {
            animationLength = animationClip.length;
        }
        else
        {
            Debug.LogWarning("AnimationClip is not set!");
        }

        previousTime = -1;

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
        if (animationComponent != null && animationClip != null)
        {
            animationClip.legacy = true;

            if (animationComponent.GetClip(animationClip.name) == null)
                animationComponent.AddClip(animationClip, animationClip.name);

            AnimationState animState = animationComponent[animationClip.name];

            if (animState != null && animState.enabled && animState.weight > 0)
            {
                float currentTime = animState.time % (animState.length > 0 ? animState.length : 0.001f) ; // divide by length for looped anims

                if (currentTime < previousTime) // Handle looping
                {
                    TriggerPassedEvents(0, animationLength);
                }
                else
                {
                    TriggerPassedEvents(previousTime, currentTime);
                }

                previousTime = currentTime;
            }
        }
    }

    private void TriggerPassedEvents(float startTime, float endTime)
    {
        float normalizedStartTime = startTime / animationLength;
        float normalizedEndTime = endTime / animationLength;

        foreach (var e in events)
        {
            float eventTimeNormalized = e.fEventTime;

            switch (e.timeType)
            {
                case CEvent.EventTimeType.E0_FRAME_NR:
                    // Assuming we are checking by frame number, convert it to normalized time.
                    // This would depend on the frame rate. Here, we assume 60 FPS as an example.
                    eventTimeNormalized = e.fEventTime / (animationLength * 60f);
                    break;

                case CEvent.EventTimeType.E1_TIME:
                    eventTimeNormalized = e.fEventTime / animationLength;
                    break;

                case CEvent.EventTimeType.E2_NORMALIZED_TIME:
                    eventTimeNormalized = e.fEventTime;
                    break;
            }

            if (eventTimeNormalized >= normalizedStartTime && eventTimeNormalized <= normalizedEndTime)
            {
                e.TriggerEvents(eventTimeNormalized);
            }
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
                    ps.Pause();
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
        public enum EventTimeType { E0_FRAME_NR, E1_TIME, E2_NORMALIZED_TIME }

        public EventTimeType timeType;
        public float fEventTime;


        [Header("Game Objects")]
        [SerializeField]
        public GameObjectEvent[] gameObjectEvents;

        [Header("Particle Systems")]
        [SerializeField]
        public ParticleSystemEvent[] particleSystemEvents;

        [Header("Extra")]
        [SerializeField]
        public AnimatorEvent[] animatorEvents;

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

            eventsToCall?.Invoke();
        }

        private void TriggerAnimatorEvents()
        {
            foreach (var animEvent in animatorEvents)
            {
                animEvent.Execute();
            }
        }

        private void TriggerParticleSystemEvents()
        {
            foreach (var psEvent in particleSystemEvents)
            {
                psEvent.Execute();
            }
        }

        private void TriggerTimelineEvents()
        {
            foreach (var timelineEvent in timelineEvents)
            {
                timelineEvent.Execute();
            }
        }

        private void TriggerGameObjectEvents()
        {
            foreach (var goEvent in gameObjectEvents)
            {
                goEvent.Execute();
            }
        }
    }
}

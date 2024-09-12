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

            eventsToCall?.Invoke();
        }

        private void TriggerAnimatorEvents()
        {
            foreach (var animEvent in animatorEvents)
            {
                animEvent.Execute();
            }

            foreach (var animEvent in animatorPlayAnimations)
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

    public void TK2_RunAnimationEventID(string strEventNameID)
    {
        Debug.LogError("Show log");
        for (int i = 0; i < events.Count; i++)
        {
            if(events[i].strEventNameID == strEventNameID)
            {
                events[i].TriggerEvents(0);
            }
        }
    }
}

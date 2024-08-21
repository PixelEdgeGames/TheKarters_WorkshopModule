using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_04_AnimationClip_PlayPauseStop : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E03_ENABLE_DISABLE_COMMANDS_EXECUTOR;
    }
    [System.Serializable]
    public class CAnimation
    {
        public Animation animationObject;
        public EActionType eActionType;
        public AnimationClip clipToPlay;
        public bool bAutoPlayOnRaceStart = false;
        [Header("Should we use this anim first frame to bring object to initial state")]
        public bool bUseFirstFrameForDefaultStateOnRaceRestart = false;

        public enum EActionType
        {
            E_PLAY,
            E_PLAY_REVERSED,
            E_STOP
        }
    }

    public CAnimation[] animationObjects;


    public override void Awake()
    {
    }
    public override void Start()
    {
    }

    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal)
    {
        CommandExecuted();
    }

    void CommandExecuted()
    {
        foreach(var clip in animationObjects)
        {
            if (clip == null || clip.animationObject == null)
                continue;

            if(clip.eActionType == CAnimation.EActionType.E_PLAY)
            {
                if (clip.clipToPlay != null)
                    clip.animationObject.clip = clip.clipToPlay;

                if (clip.animationObject.clip != null)
                {
                    if (clip.animationObject[clip.animationObject.clip.name].normalizedTime == 1)
                        clip.animationObject[clip.animationObject.clip.name].normalizedTime = 0.0f; 

                    clip.animationObject[clip.animationObject.clip.name].speed = 1.0f;
                }

                if(clip.animationObject.isPlaying == false)
                    clip.animationObject.Play();
            }

            if (clip.eActionType == CAnimation.EActionType.E_PLAY_REVERSED)
            {
                if (clip.clipToPlay != null)
                    clip.animationObject.clip = clip.clipToPlay;

                if (clip.animationObject.clip != null)
                {
                    if(clip.animationObject[clip.animationObject.clip.name].normalizedTime == 0)
                        clip.animationObject[clip.animationObject.clip.name].normalizedTime = 1.0f;
                    
                    clip.animationObject[clip.animationObject.clip.name].speed = -1.0f;
                }

                if (clip.animationObject.isPlaying == false)
                    clip.animationObject.Play();
            }

            if (clip.eActionType == CAnimation.EActionType.E_STOP)
            {
                if (clip.clipToPlay != null)
                    clip.animationObject.clip = clip.clipToPlay;

                clip.animationObject.Stop();
            }
        }

    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach (var clip in animationObjects)
        {
            if (clip == null || clip.animationObject == null)
                continue;

            if(clip.clipToPlay != null)
                clip.animationObject.clip = clip.clipToPlay;

            if (clip.animationObject.clip != null)
                clip.animationObject[clip.animationObject.clip.name].speed = 1.0f;

            clip.animationObject.Rewind();

            if (clip.bUseFirstFrameForDefaultStateOnRaceRestart == true)
            {
                clip.clipToPlay.SampleAnimation(clip.animationObject.gameObject, 0.0f);
            }
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
        foreach (var clip in animationObjects)
        {
            if (clip == null || clip.animationObject == null)
                continue;

            if(clip.bAutoPlayOnRaceStart == true)
            {
                clip.animationObject.Rewind();
                clip.animationObject.Play();
            }

        }
    }
}

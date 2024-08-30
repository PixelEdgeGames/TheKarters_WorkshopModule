using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_04_AnimationClip_PlayPauseStop : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E04_ANIMATION_CLIP_PLAY_PAUSE_STOP;
    }
    [System.Serializable]
    public class CAnimation
    {
        public Animation animationObject;
        public EActionType eActionType;
        public AnimationClip clipToPlay;
        [HideInInspector]
        public float fTransitionTime = 0.3f;

        public enum EActionType
        {
            E_PLAY,
            E_PLAY_REVERSED,
            E_PAUSE,
            E_UNPAUSE,
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

    public override void OnDestroy()
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
        foreach(var objAnimLogic in animationObjects)
        {
            if (objAnimLogic == null || objAnimLogic.animationObject == null)
                continue;

            if(objAnimLogic.eActionType == CAnimation.EActionType.E_PLAY)
            {
                objAnimLogic.clipToPlay.legacy = true;

                if (objAnimLogic.animationObject.GetClip(objAnimLogic.clipToPlay.name) == null)
                    objAnimLogic.animationObject.AddClip(objAnimLogic.clipToPlay, objAnimLogic.clipToPlay.name);

                if (objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].normalizedTime == 1)
                    objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].normalizedTime = 0.0f;

                objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].speed = 1.0f;

                if (objAnimLogic.animationObject.isPlaying == false)
                {
                    if (objAnimLogic.animationObject.clip == null)
                        objAnimLogic.animationObject.clip = objAnimLogic.clipToPlay;

                    objAnimLogic.animationObject.Play();
                }

                objAnimLogic.animationObject.CrossFade(objAnimLogic.clipToPlay.name, objAnimLogic.fTransitionTime);
            }

            if (objAnimLogic.eActionType == CAnimation.EActionType.E_PLAY_REVERSED)
            {
                objAnimLogic.clipToPlay.legacy = true;

                if (objAnimLogic.animationObject.GetClip(objAnimLogic.clipToPlay.name) == null)
                    objAnimLogic.animationObject.AddClip(objAnimLogic.clipToPlay, objAnimLogic.clipToPlay.name);

                if (objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].normalizedTime == 0 )
                    objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].normalizedTime = 1.0f;

                objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].speed = -1.0f;


                if (objAnimLogic.animationObject.isPlaying == false)
                {
                    if(objAnimLogic.animationObject.clip == null)
                        objAnimLogic.animationObject.clip = objAnimLogic.clipToPlay;

                    objAnimLogic.animationObject.Play();
                }

                objAnimLogic.animationObject.CrossFade(objAnimLogic.clipToPlay.name, objAnimLogic.fTransitionTime);
            }

            if (objAnimLogic.eActionType == CAnimation.EActionType.E_PAUSE)
            {
                foreach(AnimationState animClipInList in objAnimLogic.animationObject)
                {
                    if (objAnimLogic.animationObject.IsPlaying(animClipInList.name))
                    {
                        fLastSpeedBeforePause = objAnimLogic.animationObject[animClipInList.name].speed;
                        objAnimLogic.animationObject[animClipInList.name].speed = 0.0f;
                        Debug.LogError("Playing clip name: " + animClipInList.name);
                        break;
                    }
                }
            }

            if (objAnimLogic.eActionType == CAnimation.EActionType.E_UNPAUSE)
            {
                foreach (AnimationState animClipInList in objAnimLogic.animationObject)
                {
                    if (objAnimLogic.animationObject.IsPlaying(animClipInList.name))
                    {
                        objAnimLogic.animationObject[animClipInList.name].speed = fLastSpeedBeforePause;
                        break;
                    }
                }
            }

            if (objAnimLogic.eActionType == CAnimation.EActionType.E_STOP)
            {
                objAnimLogic.animationObject.Stop();
            }
        }

    }

    float fLastSpeedBeforePause = 1.0f;

    protected override void RaceResetted_RevertToDefault()
    {
        fLastSpeedBeforePause = 1.0f;
        foreach (var objAnimLogic in animationObjects)
        {
            if (objAnimLogic == null || objAnimLogic.animationObject == null)
                continue;

            objAnimLogic.clipToPlay.legacy = true;

            var clipInside = objAnimLogic.animationObject.GetClip(objAnimLogic.clipToPlay.name);
            if (clipInside == null)
                objAnimLogic.animationObject.AddClip(objAnimLogic.clipToPlay, objAnimLogic.clipToPlay.name);

            objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].speed = 1.0f;
        }
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

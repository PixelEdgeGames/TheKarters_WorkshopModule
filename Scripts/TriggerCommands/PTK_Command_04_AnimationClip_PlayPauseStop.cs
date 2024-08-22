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
        public bool bRaceRestartUseFirstFrameAsDefault = false;
        [HideInInspector]
        public float fTransitionTime = 0.3f;

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

                if (objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].normalizedTime == 0)
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

            if (objAnimLogic.eActionType == CAnimation.EActionType.E_STOP)
            {
                objAnimLogic.animationObject.Stop();
            }
        }

    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach (var objAnimLogic in animationObjects)
        {
            if (objAnimLogic == null || objAnimLogic.animationObject == null)
                continue;

            objAnimLogic.clipToPlay.legacy = true;

            var clipInside = objAnimLogic.animationObject.GetClip(objAnimLogic.clipToPlay.name);
            if (clipInside == null)
                objAnimLogic.animationObject.AddClip(objAnimLogic.clipToPlay, objAnimLogic.clipToPlay.name);

            objAnimLogic.animationObject[objAnimLogic.clipToPlay.name].speed = 1.0f;


          //  objAnimLogic.animationObject.Rewind();

            if (objAnimLogic.bRaceRestartUseFirstFrameAsDefault == true)
            {
                // set this one as default
                objAnimLogic.animationObject.clip = objAnimLogic.clipToPlay;

                objAnimLogic.animationObject.Stop();
                objAnimLogic.clipToPlay.SampleAnimation(objAnimLogic.animationObject.gameObject, 0.0f);
            }
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
        foreach (var objAnimLogic in animationObjects)
        {
            if (objAnimLogic == null || objAnimLogic.animationObject == null)
                continue;

            if(objAnimLogic.bAutoPlayOnRaceStart == true)
            {
                objAnimLogic.animationObject.Rewind();
                objAnimLogic.animationObject.Play();
            }

        }
    }
}

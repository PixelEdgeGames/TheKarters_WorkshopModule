using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class PTK_Command_04_AnimationClip_PlayPauseStop : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E04_ANIMATION_CLIP_PLAY_PAUSE;
    }
    [System.Serializable]
    public class CAnimation
    {
        public Animator animatorTypeObject;

        public EActionType eActionType;
        public AnimationClip clipToPlay;
        [HideInInspector]
        public float fTransitionTime_1 = 0.1f;

        public enum EActionType
        {
            E_PLAY,
            E_PAUSE,
            E_UNPAUSE
        }
    }

    public CAnimation[] animationObjects;


    public override void Awake()
    {
        foreach (var objAnimLogic in animationObjects)
        {
            if (objAnimLogic == null)
                continue;

            if (objAnimLogic.animatorTypeObject != null && objAnimLogic.clipToPlay != null)
            {
                if(DoesAnimationClipExistInAnimator(objAnimLogic.animatorTypeObject, objAnimLogic.clipToPlay.name) == false)
                {
                    AddClipToAnimatorController(objAnimLogic.animatorTypeObject, objAnimLogic.clipToPlay);
                }
            }
        }
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
            if (objAnimLogic == null )
                continue;

            if (objAnimLogic.animatorTypeObject != null)
            {
                ProcessAnimatorTypeObject(objAnimLogic);
            }
        }

    }

    bool DoesAnimationClipExistInAnimator(Animator animator, string clipName)
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        // Loop through all layers in the AnimatorController
        foreach (AnimatorControllerLayer layer in animatorController.layers)
        {
            // Loop through all states in the layer
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                // Check if the state's motion is an AnimationClip and matches the name
                if (state.state.motion is AnimationClip clip && clip.name == clipName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void AddClipToAnimatorController(Animator animator, AnimationClip clip)
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;

        // Create a new state with the animation clip
        AnimatorState newState = animatorController.AddMotion(clip);

        // Optionally, you can set the state's name, or set transitions, etc.
        newState.name = clip.name;
    }

    private void ProcessAnimatorTypeObject(CAnimation objAnimLogic)
    {
        if (objAnimLogic.animatorTypeObject == null)
            return;

        if (objAnimLogic.eActionType == CAnimation.EActionType.E_PLAY)
        {
            objAnimLogic.animatorTypeObject.speed = 1.0f;

            if (objAnimLogic.clipToPlay != null)
            {
                AnimatorStateInfo currentStateInfo = objAnimLogic.animatorTypeObject.GetCurrentAnimatorStateInfo(0);

                objAnimLogic.animatorTypeObject.CrossFade(objAnimLogic.animatorTypeObject.GetLayerName(0) + "." + objAnimLogic.clipToPlay.name, objAnimLogic.fTransitionTime_1, 0, 0);
            }
        }

        if (objAnimLogic.eActionType == CAnimation.EActionType.E_PAUSE)
        {
            fLastSpeedBeforePause = objAnimLogic.animatorTypeObject.speed;
            objAnimLogic.animatorTypeObject.speed = 0.0f;
        }

        if (objAnimLogic.eActionType == CAnimation.EActionType.E_UNPAUSE)
        {
            objAnimLogic.animatorTypeObject.speed = fLastSpeedBeforePause;
        }
    }

    
    float fLastSpeedBeforePause = 1.0f;

    protected override void RaceResetted_RevertToDefault()
    {
        fLastSpeedBeforePause = 1.0f;
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[AddComponentMenu("_PixelTools/Modding/Animation/PTK_RaceRestart_AnimationSync", 0)]
public class PTK_RaceRestart_AnimationSync : MonoBehaviour
{
    public enum EAnimType
    {
        E0_AUTO_DETECTED,
        E1_ANIMATOR,
        E2_LEGACY_ANIMATION
    }

    public enum EMode
    {
        E0_RESET_AND_AUTO_PLAY,
        E1_ONLY_RESET_USING_FIRST_FRAME,
        E2_ONLY_RESET_USING_LAST_FRAME
    }


    public EAnimType eAnimType = EAnimType.E0_AUTO_DETECTED;
    [Header("Reset Mode")]
    public EMode eResetMode = EMode.E0_RESET_AND_AUTO_PLAY;

    [Header("Assign Animation or Aniamtor")]
    public Animation animationTarget;
    public Animator animatorTarget;
    public AnimationClip animationClip;

    [Header("Not Required")]
    public bool bPlayBlendTree = false;
    public string strBlendTreeName = "Blend Tree";

    public bool bUseAnimatorTriggers = false;
    public string strAnimatorTriggerName_OnRaceRestart;
    public string strAnimatorTrigger_OnRaceBeginAfterCountdown;


    public static string strEmptyAnimationClipName = "PTK_EmptyAnimClip";

    // Start is called before the first frame update
    void Awake()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

        // if for some reason animation was removed and user didnt noticed that
        if (animationClip == null)
            AssignFromObject();


        if(animatorTarget != null)
        {

           if (ContainsEmptyAnimationClip(animatorTarget) == false)
            {
                AnimationClip  emptyAnimatorClip = new AnimationClip();
                emptyAnimatorClip.name = strEmptyAnimationClipName;
                emptyAnimatorClip.wrapMode = WrapMode.Once;

                AddClipToAnimatorController(animatorTarget, emptyAnimatorClip);
            }

        }

        OnRaceRestart();
    }

    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceRestart;
    }

    [EasyButtons.Button]
    public void AssignFromObject()
    {
        animationTarget = this.GetComponent<Animation>();

        if (animationTarget != null)
        {
            animationClip = animationTarget.clip;

            if (animationClip == null)
            {
                foreach (AnimationState anim in animationTarget)
                {
                    animationClip = anim.clip;
                    break;
                }
            }
        }

        animatorTarget = this.GetComponent<Animator>();

        if (animatorTarget != null)
        {
            // Get the Animator component attached to this GameObject
            Animator animator = animatorTarget;


            // Get the runtime animator controller
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            // Ensure we have a valid controller
            if (controller == null)
            {
                Debug.LogError("No RuntimeAnimatorController found on the Animator.");
                return;
            }

            // Check if there are any animation clips
            if (controller.animationClips.Length > 0)
            {
                // Assuming the first animation clip is the default one

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                AnimationClip defaultClip = controller.animationClips[0];
                foreach (AnimationClip clip in controller.animationClips)
                {
                    // Compare the clip name with the current state's fullPathHash or shortNameHash
                    if (stateInfo.IsName(clip.name))
                    {
                        defaultClip = clip; // Return the matching clip
                        break;
                    }
                }

                animationClip = defaultClip;
            }
            else
            {
                Debug.LogError("No animation clips found in the Animator Controller.");
            }
        }
    }


    void AddClipToAnimatorController(Animator animator, AnimationClip clip)
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;

        // Create a new state with the animation clip
        AnimatorState newState = animatorController.AddMotion(clip);

        // Optionally, you can set the state's name, or set transitions, etc.
        newState.name = clip.name;
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

   public static bool ContainsEmptyAnimationClip(Animator animator)
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        // Loop through all layers in the AnimatorController
        foreach (AnimatorControllerLayer layer in animatorController.layers)
        {
            // Loop through all states in the layer
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                // Check if the state's motion is an AnimationClip and matches the name
                if (state.state.name == PTK_RaceRestart_AnimationSync.strEmptyAnimationClipName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnRaceRestart()
    {
        if (animatorTarget != null)
        {
            animatorTarget.speed = 1.0f;
            animatorTarget.StopPlayback();
            animatorTarget.Rebind();
            //animatorTarget.Update(0f);
            animatorTarget.StopPlayback();

        }

        if (animationTarget != null)
        {
            animationTarget.Stop();
        }

        if (animationTarget != null)
        {
            if (animationClip != null)
            {
                if (animationClip != null)
                    animationClip.legacy = true;

                if (animationTarget.GetClip(animationClip.name) == null)
                    animationTarget.AddClip(animationClip, animationClip.name);

                animationTarget.clip = animationClip;
            }
        }

        if (animationClip != null)
        {
            if(animationTarget != null)
                animationClip.SampleAnimation(animationTarget.gameObject, eResetMode == EMode.E2_ONLY_RESET_USING_LAST_FRAME ? 1.0f : 0.0f);

            if (animatorTarget != null)
            {
                // to avoid playing other animation
                animatorTarget.Play(animatorTarget.GetLayerName(0) + "." + strEmptyAnimationClipName);
                animatorTarget.StopPlayback();

                bool bEnabledBeforeSample = animatorTarget.enabled;
                animatorTarget.enabled = false;

                animationClip.SampleAnimation(animatorTarget.gameObject, eResetMode == EMode.E2_ONLY_RESET_USING_LAST_FRAME ? 1.0f : 0.0f);

                animatorTarget.enabled = bEnabledBeforeSample;

            }
        }


        if (bUseAnimatorTriggers == true && strAnimatorTriggerName_OnRaceRestart != "")
        {
            if(animatorTarget != null)
                animatorTarget.SetTrigger(strAnimatorTriggerName_OnRaceRestart);
        }
    }

    [EasyButtons.Button]
    public void SampleAnim()
    {
        animationClip.SampleAnimation(animatorTarget.gameObject, eResetMode == EMode.E2_ONLY_RESET_USING_LAST_FRAME ? 1.0f : 0.0f);
    }
    private void OnRaceTimerStart()
    {
        if (eResetMode != EMode.E0_RESET_AND_AUTO_PLAY)
            return;

        if (animationTarget != null)
        {
            animationTarget.clip = animationClip;

            animationTarget.Stop();
            animationTarget.Play();
        }


        if (animatorTarget != null)
        {
            string strAnimToPlay = "";
            if (bPlayBlendTree == true)
            {
                strAnimToPlay = strBlendTreeName;
            }
            else if (animationClip != null)
            {
                strAnimToPlay = animationClip.name;
            }

            if (strAnimToPlay != "")
            {
                string strLayerNameWithAnim = animatorTarget.GetLayerName(0) + "." + strAnimToPlay;
                animatorTarget.Play(strLayerNameWithAnim);
            }

            if (bUseAnimatorTriggers == true && strAnimatorTrigger_OnRaceBeginAfterCountdown != "")
            {
                animatorTarget.SetTrigger(strAnimatorTrigger_OnRaceBeginAfterCountdown);
            }
        }
    }
}

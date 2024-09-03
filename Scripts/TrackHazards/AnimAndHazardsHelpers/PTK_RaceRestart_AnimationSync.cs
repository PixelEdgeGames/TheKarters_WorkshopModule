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
    public bool bUseAnimatorTriggers = false;
    public string strAnimatorTriggerName_OnRaceRestart;
    public string strAnimatorTrigger_OnRaceBeginAfterCountdown;

    AnimationClip emptyAnimatorClip;
    // Start is called before the first frame update
    void Awake()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

        // if for some reason animation was removed and user didnt noticed that
        if (animationClip == null)
            AssignFromObject();

        emptyAnimatorClip = new AnimationClip();
        emptyAnimatorClip.name = "Empty";

        if(animatorTarget != null)
        {
            AddClipToAnimatorController(animatorTarget, emptyAnimatorClip);
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

    private void OnRaceRestart()
    {
        if (animatorTarget != null)
        {
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
                animationClip.SampleAnimation(animatorTarget.gameObject, eResetMode == EMode.E2_ONLY_RESET_USING_LAST_FRAME ? 1.0f : 0.0f);

                // to avoid playing other animation
                animatorTarget.Play(animatorTarget.GetLayerName(0) + "." + emptyAnimatorClip.name);

            }
        }


        if (bUseAnimatorTriggers == true && strAnimatorTriggerName_OnRaceRestart != "")
        {
            if(animatorTarget != null)
                animatorTarget.SetTrigger(strAnimatorTriggerName_OnRaceRestart);
        }
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
            if (animationClip != null)
            {
                string strLayerNameWithAnim = animatorTarget.GetLayerName(0) + "." + animationClip.name;
                animatorTarget.Play(strLayerNameWithAnim);
            }

            if (bUseAnimatorTriggers == true && strAnimatorTrigger_OnRaceBeginAfterCountdown != "")
            {
                animatorTarget.SetTrigger(strAnimatorTrigger_OnRaceBeginAfterCountdown);
            }
        }
    }
}

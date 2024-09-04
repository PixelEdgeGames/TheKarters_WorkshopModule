using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[AddComponentMenu("_PixelTools/Modding/Animation/PTK_RaceRestart_AnimationSync", 0)]
public class PTK_RaceRestart_AnimationSync : MonoBehaviour
{

    public Animator animatorTarget;
    public enum EPlayMode
    {
        E0_ANIM_CLIP,
        E1_BLEND_TREE
    }
    public EPlayMode ePlayMode = EPlayMode.E0_ANIM_CLIP;

    public AnimationClip animationClip;

    [Header("Not Required")]
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

        if(animatorTarget == null)
        {
            Debug.LogError("Animator target was not assigned - trying to get it automatically");
            animatorTarget = this.GetComponent<Animator>();
        }
        // if for some reason animation was removed and user didnt noticed that
        if (animationClip == null && ePlayMode == EPlayMode.E0_ANIM_CLIP)
        {
            Debug.LogError("Set to play anim clip but anim clip not found - trying to get it automatically");

            AssignFromObject();
        }

        /*
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
        */

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
                AnimationClip defaultClip = null;
                foreach (AnimationClip clip in controller.animationClips)
                {
                    // Compare
                    // the clip name with the current state's fullPathHash or shortNameHash
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

    /*
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
    */

    private void OnRaceRestart()
    {

        if (animatorTarget != null)
        {
            animatorTarget.speed = 1.0f;
            animatorTarget.StopPlayback();
            animatorTarget.Rebind();
            //animatorTarget.Update(0f);
            animatorTarget.StopPlayback();


            string strAnimToPlay = strEmptyAnimationClipName;
            if (ePlayMode == EPlayMode.E1_BLEND_TREE)
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


            if (bUseAnimatorTriggers == true && strAnimatorTriggerName_OnRaceRestart != "")
            {
                if (animatorTarget != null)
                    animatorTarget.SetTrigger(strAnimatorTriggerName_OnRaceRestart);
            }

            animatorTarget.speed = 0.0f;
        }

    }

    private void OnRaceTimerStart()
    {
        if (animatorTarget != null)
        {
            animatorTarget.speed = 1.0f; // resume

            if (bUseAnimatorTriggers == true && strAnimatorTrigger_OnRaceBeginAfterCountdown != "")
            {
                animatorTarget.SetTrigger(strAnimatorTrigger_OnRaceBeginAfterCountdown);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("_PixelTools/Modding/Animation/PTK_RaceRestart_AnimationResetUsingSingleFrameData", 0)]
public class PTK_RaceRestart_AnimationResetUsingSingleFrameData : MonoBehaviour
{
    public Animation animationTarget;
    public AnimationClip animationClipForRaceRestart;

    public enum EFrameType
    {
        E0_FIRST_FRAME,
        E1_LAST_FRAME
    }

    public EFrameType eFrameType = EFrameType.E0_FIRST_FRAME;

    // Start is called before the first frame update
    void Awake()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

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
            animationClipForRaceRestart = animationTarget.clip;
    }

    private void OnRaceRestart()
    {
        if (animationTarget == null || animationClipForRaceRestart == null)
            return;

        if (animationClipForRaceRestart != null)
            animationClipForRaceRestart.legacy = true;

        if (animationTarget.GetClip(animationClipForRaceRestart.name) == null)
            animationTarget.AddClip(animationClipForRaceRestart, animationClipForRaceRestart.name);

        animationTarget.clip = animationClipForRaceRestart;
        animationTarget.Stop();

        animationClipForRaceRestart.SampleAnimation(animationTarget.gameObject, eFrameType == EFrameType.E0_FIRST_FRAME ? 0.0F : 1.0F);
    }


    private void OnRaceTimerStart()
    {
      
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PTK_ProceduralAnimSynced : MonoBehaviour
{
    public enum EMoveType
    {
        E0_LINEAR,
        E1_SIN,
        E2_QUAD,
        E3_QUART,
        E4_EXPO,
    }

    [System.Serializable]
    public class AnimationSettings
    {
        public bool enabled = false;
        public Vector3 from = Vector3.zero;
        public Vector3 to = Vector3.up;
        public float speed = 1.0f;
        public LoopType loopType = LoopType.Yoyo;
        public EMoveType easeType = EMoveType.E1_SIN;

        public bool HasChanged(AnimationSettings other)
        {
            return enabled != other.enabled ||
                   from != other.from ||
                   to != other.to ||
                   speed != other.speed ||
                   loopType != other.loopType ||
                   easeType != other.easeType;
        }

        public void CopyFrom(AnimationSettings other)
        {
            enabled = other.enabled;
            from = other.from;
            to = other.to;
            speed = other.speed;
            loopType = other.loopType;
            easeType = other.easeType;
        }
    }

    [Header("Ensure Initial LocalRot is 0,0,0")]
    public AnimationSettings localRotation = new AnimationSettings();

    [Header("Ensure Initial LocalPos is 0,0,0")]
    public AnimationSettings localMoveBetweenAB = new AnimationSettings();

    private AnimationSettings previousLocalRotation = new AnimationSettings();
    private AnimationSettings previousMoveBetweenAB = new AnimationSettings();

    private Tween moveTween;
    private Tween localRotateTween;

    [Header("Private")]
    private Quaternion initialLocalRotation = Quaternion.identity;
    private Vector3 initialLocalPosition = Vector3.zero;
    private Vector3 initialForward = Vector3.zero;

    private bool canRunAnimations = true;

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

        if (moveTween != null)
            moveTween.Kill();

        if (localRotateTween != null)
            localRotateTween.Kill();
    }

    private void OnDisable()
    {
        localRotateTween?.Pause();
        moveTween?.Pause();
    }

    private void OnEnable()
    {
        localRotateTween?.Play();
        moveTween?.Play();
    }
    private void Update()
    {
        if (!canRunAnimations) return;

        UpdateAnimation(localRotation, previousLocalRotation);
        UpdateAnimation(localMoveBetweenAB, previousMoveBetweenAB);
    }

    private void UpdateAnimation(AnimationSettings current, AnimationSettings previous)
    {
        if (current.HasChanged(previous))
        {
            previous.CopyFrom(current);
            ResetPosAndRot();
            LaunchAnims();
        }
    }

    private void RestartLocalRotation()
    {
        if (localRotateTween != null)
        {
            localRotateTween.Kill();
            localRotateTween = null;
        }

        if(localRotation.enabled)
        {
            localRotateTween = transform.DOLocalRotate(localRotation.to + initialLocalRotation.eulerAngles, 1.0f / Mathf.Max(localRotation.speed, 0.0001f),RotateMode.FastBeyond360)
                .From(localRotation.from + initialLocalRotation.eulerAngles)
                .SetEase(ConvertEMoveTypeToEase(localRotation.easeType))
                .SetLoops(-1, localRotation.loopType)
                .SetAutoKill(false);
        }
    }

    private void RestartMoveBetweenAB()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
            moveTween = null;
        }

        if(localMoveBetweenAB.enabled == true)
        {
            moveTween = transform.DOLocalMove(localMoveBetweenAB.to+ initialLocalPosition, 1.0f / Mathf.Max(localMoveBetweenAB.speed, 0.0001f))
                .From(localMoveBetweenAB.from + initialLocalPosition)
                .SetEase(ConvertEMoveTypeToEase(localMoveBetweenAB.easeType))
                .SetLoops(-1, localMoveBetweenAB.loopType)
                .SetAutoKill(false);
        }
    }

    private void OnRaceRestart()
    {
        if (PTK_ModGameplayDataSync.Instance.gameInfo.fCurrentRaceTime > 0)
        {
            canRunAnimations = true; // already started - component added after race running
            LaunchAnims();
        }
        else
            canRunAnimations = false;

        ResetPosAndRot();
    }

    private void OnRaceTimerStart()
    {
        canRunAnimations = true;
        LaunchAnims();
    }

    void LaunchAnims()
    {
        RestartLocalRotation();
        RestartMoveBetweenAB();
    }


    private void ResetPosAndRot()
    {
       // if (initialLocalRotation == Quaternion.identity)
      //      initialLocalRotation = transform.localRotation;

      //  if (initialLocalPosition == Vector3.zero)
      //     initialLocalPosition = transform.localPosition;

        if (initialForward == Vector3.zero)
            initialForward = transform.forward;

        transform.localPosition = initialLocalPosition + (localMoveBetweenAB.enabled ? localMoveBetweenAB.from : Vector3.zero);
        transform.localRotation = initialLocalRotation * (localRotation.enabled ? Quaternion.Euler(localRotation.from) : Quaternion.identity);

        if (moveTween != null)
        {
            moveTween.Kill();
        }

        if (localRotateTween != null)
        {
            localRotateTween.Kill();
        }

    }

    private Ease ConvertEMoveTypeToEase(EMoveType eMoveType)
    {
        switch (eMoveType)
        {
            case EMoveType.E0_LINEAR: return Ease.Linear;
            case EMoveType.E1_SIN: return Ease.InOutSine;
            case EMoveType.E2_QUAD: return Ease.InOutQuad;
            case EMoveType.E3_QUART: return Ease.InOutQuart;
            case EMoveType.E4_EXPO: return Ease.InOutExpo;
            default: return Ease.InOutSine;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PTK_ProceduralAnimSynced : MonoBehaviour
{
    [Header("Local Rotate Loop")]
    public bool bLocalRotateLoop = false;
    public Vector3 vLocalRotateAxis = new Vector3();
    public float fLocalRotateSpeed = 1.0f;
    [Header("Rotate around world Y")]
    public bool bRotateAroundWorldY = false;
    public float fRotateAroundWorldYSpeed = 1.0f;

    [Header("Rotate Wheel-Like Moving")]
    public bool bApplyRollingMovement = false;
    public float fSphereRadiusScale = 1.0f;

    [Header("Move Between A and B")]
    public bool bMoveBetweenPointAB = false;
    public Vector3 vPointA;
    public Vector3 vPointB;
    public float fMoveSpeed = 1.0f;
    public Space moveSpace = Space.Self;
    public enum EMoveType
    {
        E0_LINEAR,
        E1_SIN,
        E2_QUAD,
        E3_QUART,
        E4_EXPO,

    }
    public EMoveType ease = EMoveType.E1_SIN;
    public LoopType loopType = LoopType.Yoyo;
    // private ot check change 
    private Tween moveTween;
    private Vector3 previousPointA;
    private Vector3 previousPointB;
    private float previousfMoveSpeed = 0.0f;
    private Space previousMoveSpace;
    private EMoveType previousEase;
    private LoopType previousLoopType;

    [EasyButtons.Button]
    public void SetPointA()
    {
        vPointA = transform.position;
    }
    [EasyButtons.Button]
    public void SetPointB()
    {
        vPointB = transform.position;
    }
    [EasyButtons.Button]
    public void MoveToPointA()
    {
         transform.position = vPointA;
    }

    [Header("Private")]
    private Vector3 lastPosition;
    Quaternion qInitialRot = Quaternion.identity;
    Vector3 vInitialPos = Vector3.zero;
    Vector3 initialForward = Vector3.zero;

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

    bool bCanApplyAnimations = false;

    private void Update()
    {
        if (bCanApplyAnimations == false)
            return;

        UpdateRotating();

        UpdateRolling();

        UpdateABMovement();
    }
    void UpdateRotating()
    {
        if (bLocalRotateLoop == true)
        {
            transform.Rotate(vLocalRotateAxis, fLocalRotateSpeed * Time.deltaTime, Space.Self);
        }

        if(bRotateAroundWorldY == true)
        {
            transform.Rotate(Vector3.up, fRotateAroundWorldYSpeed * Time.deltaTime, Space.World);
        }

    }

    void UpdateRolling()
    {
        if (bApplyRollingMovement == true)
        {
            // Calculate the movement direction
            Vector3 movementDirection = transform.position - lastPosition;
            lastPosition = transform.position;

            // Check if there is any movement
            if (movementDirection != Vector3.zero)
            {
                // Calculate the distance moved along the ground
                float distance = movementDirection.magnitude;

                // Calculate the rotation angle based on the distance and sphere radius
                float angle = distance / (2 * Mathf.PI * fSphereRadiusScale) * 360;

                // Determine the rotation axis (using local right axis)
                Vector3 rotationAxis = transform.right;

                // Project movement onto the initial forward plane
                Vector3 movementProjected = Vector3.ProjectOnPlane(movementDirection, transform.up);

                // Determine the direction of rotation based on projected movement
                float dotProduct = Vector3.Dot(movementProjected.normalized, initialForward);

                // Invert the angle if moving backwards relative to initial forward
                if (dotProduct < 0)
                {
                    angle = -angle;
                }

                // Calculate the new rotation
                Quaternion deltaRotation = Quaternion.AngleAxis(angle, rotationAxis);

                // Apply the rotation to the current rotation
                transform.rotation *= deltaRotation;
            }
        }
    }
    void UpdateABMovement()
    {
        if (bMoveBetweenPointAB)
        {
            moveTween?.Play();
        }
        else
        {
            moveTween?.Pause();
        }

        if (bMoveBetweenPointAB == false)
            return;

        if (SettingsChanged())
        {
            UpdatePreviousValues();
            RestartAnim();
        }

    }
    private void OnRaceRestart()
    {
        RestartAnim();
    }


    private void OnRaceTimerStart()
    {
        bCanApplyAnimations = true;
    }

    void RestartAnim()
    {
        if (qInitialRot == Quaternion.identity)
            qInitialRot = transform.rotation;

        if (vInitialPos == Vector3.zero)
            vInitialPos = transform.position;

        if (initialForward == Vector3.zero)
            initialForward = transform.forward;

        if (bMoveBetweenPointAB == true)
        {
            vInitialPos = vPointA;
            CreateTween();
        }

        transform.position = vInitialPos;
        lastPosition = vInitialPos;
        transform.rotation = qInitialRot;

    }

    private bool SettingsChanged()
    {
        return vPointA != previousPointA ||
               vPointB != previousPointB ||
               moveSpace != previousMoveSpace ||
               ease != previousEase ||
               loopType != previousLoopType ||
                previousfMoveSpeed != fMoveSpeed;
    }

    private void UpdatePreviousValues()
    {
        previousPointA = vPointA;
        previousPointB = vPointB;
        previousMoveSpace = moveSpace;
        previousEase = ease;
        previousLoopType = loopType;
        previousfMoveSpeed = fMoveSpeed;
    }

    private void CreateTween()
    {
        if (moveTween != null)
        {
            moveTween.Kill();
        }

        moveTween = transform.DOMove(vPointB, fMoveSpeed)
            .From(vPointA)
            .SetEase(EaseTypeFromEnum(ease))
            .SetLoops(-1, loopType)
            .SetRelative(moveSpace == Space.Self)
            .SetAutoKill(false);
    }

    Ease EaseTypeFromEnum(EMoveType eMoveType)
    {
        switch (eMoveType)
        {
            case EMoveType.E0_LINEAR:
                return Ease.Linear;
            case EMoveType.E1_SIN:
                return Ease.InOutSine;
            case EMoveType.E2_QUAD:
                return Ease.InOutQuad;
            case EMoveType.E3_QUART:
                return Ease.InOutQuart;
            case EMoveType.E4_EXPO:
                return Ease.InOutExpo;
        }

        return Ease.InOutSine;
    }
}

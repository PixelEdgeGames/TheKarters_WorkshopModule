using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PTK_ReplayLookAtCamConfig : MonoBehaviour
{
    public Transform triggersParent;
    public Transform camPosPointsParent;

    [Header("Params")]
    public float fCamDurationSec = 3.0f;

    [Header("FOV Configuration")]
    public AnimationCurve fovTransitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0.01f, 1.0f)]
    public float fFovTransitionDurationPercentage = 1.0f;

    [Header(" StartFov")]
    public EFovMode eStartFovMode = EFovMode.DYNAMIC_CONST_KART_SIZE;
    public float fStartFixedFov = 15.0f;
    [Range(4.0f,25.0f)]
    public float fStartDynamicKartSize = 9.0f;

    [Header("EndFov")]
    public EFovMode eEndFovMode = EFovMode.DYNAMIC_CONST_KART_SIZE;
    public float fEndFixedFov = 35.0f;
    [Range(4.0f, 25.0f)]
    public float fEndDynamicKartSize = 9.0f;

    public enum EFovMode
    {
        DYNAMIC_CONST_KART_SIZE,
        FIXED
    }

    [Header("Follow Target Lag")]
    public bool bEnableTargetFollowLagEffect = false;
    public float fFollowLagDurationSec = 2.0f;
    public float fFollowLagIntensity = 0.25f;

    [HideInInspector]
    public Vector3 vCameraPosition;
    [HideInInspector]
    public Quaternion qCamerRotation;
    [HideInInspector]
    public float fFieldOfView = 75.0f;

    bool useSplinePath = false;
    [Header("Spline Movement")]
    public float fSplineMovementDelay = 0.0f;
    public float fSplineMovementSpeed = 40.0f;
    public enum ESplineMovementEasing
    {
        E0_CONSTANT_SPEED,
        E1_ACCELERATE_AT_START,
        E2_ACCELERATE_AT_START_DECELERATE_AT_END,
        E3_DECELERATE_AT_END
    }
    public ESplineMovementEasing eMovementEasing = ESplineMovementEasing.E3_DECELERATE_AT_END;

    public PTK_BezierMB ptkBezierMB;

    [Header("Private FOV Variables")]
    private float fTransitionStartTime;
    private float fInitialFov;
    private bool bIsFovTransitioning = false;
    private float fCurrentFov = 75.0f;
    private float fTransitionDuration = 1.0f;
    private float fTransitionToFov = 75.0f;
    private bool bIsCameraRunning = false;
    private float fTimeSinceCameraRun = 0.0f;
    private Transform cameraLookAtTarget;
    private Vector3 vSmoothDampTargetPos = Vector3.zero;
    private Vector3 vSmoothDampTargetPosVel = Vector3.zero;

    void Start()
    {
        InitializeAndRefreshBezier();
    }

    private void InitializeAndRefreshBezier()
    {
        useSplinePath = ptkBezierMB.bezierPointsParent.transform.childCount > 1;
        ptkBezierMB.RegenerateSpline();
    }

    public void Initialize()
    {
    }

    void FixedUpdate()
    {
        fTimeSinceCameraRun += Time.deltaTime;
        fTimeSinceCameraRun = Mathf.Clamp(fTimeSinceCameraRun, 0.0f, fCamDurationSec);


        if (bIsCameraRunning)
        {
            UpdateLookAtTarget();
            UpdateFOV();
        }

        if (fTimeSinceCameraRun >= fCamDurationSec && bIsCameraRunning == true)
            StopCamera();
    }

    private void Update()
    {
    }
    private void UpdateCameraPosition()
    {
        if (useSplinePath && ptkBezierMB != null)
        {
            float fReachEndSplineInTime = ptkBezierMB.ptkBezier.GetTotalLength() / fSplineMovementSpeed;
            float normalizedTime = (fTimeSinceCameraRun - fSplineMovementDelay) / fReachEndSplineInTime;
            normalizedTime = Mathf.Clamp01(normalizedTime);

            switch (eMovementEasing)
            {
                case ESplineMovementEasing.E0_CONSTANT_SPEED:
                    break;
                case ESplineMovementEasing.E1_ACCELERATE_AT_START:
                    normalizedTime = InSine(normalizedTime);
                    break;
                case ESplineMovementEasing.E2_ACCELERATE_AT_START_DECELERATE_AT_END:
                    normalizedTime = InOutSine(normalizedTime);
                    break;
                case ESplineMovementEasing.E3_DECELERATE_AT_END:
                    normalizedTime = OutSine(normalizedTime);
                    break;
            }

            float targetDistance = normalizedTime * ptkBezierMB.ptkBezier.GetTotalLength();
            Vector3 vTargetPos = ptkBezierMB.ptkBezier.GetPointAtDistance(targetDistance).Position;
            vCameraPosition = Vector3.Lerp(vCameraPosition, vTargetPos, Time.deltaTime * 25.0f);
        }
        else if (camPosPointsParent.transform.childCount > 0)
        {
            vCameraPosition = camPosPointsParent.transform.GetChild(0).transform.position;
        }
    }

    void UpdateLookAtTarget()
    {
        if (camPosPointsParent.transform.childCount > 0)
        {
            UpdateCameraPosition();
        }
        else
        {
            Debug.LogError("Camera positions are empty");
        }

        Vector3 vTargetPosition = CalculateTargetPosition();

        Vector3 vLookAtDir = vTargetPosition - vCameraPosition;
        float fDistToCamera = vLookAtDir.magnitude;
        vLookAtDir.Normalize();

        qCamerRotation = Quaternion.LookRotation(vLookAtDir);
    }

    private float GetFOVForMode(EFovMode mode, float distanceToCamera, float fixedValue, float kartSize)
    {
        switch (mode)
        {
            case EFovMode.DYNAMIC_CONST_KART_SIZE:
                return CalculateFOV(distanceToCamera, kartSize);
            case EFovMode.FIXED:
                return fixedValue;
            default:
                return 75f;
        }
    }

    private void UpdateFOV()
    {
        Vector3 vTargetPosition = CalculateTargetPosition();
        Vector3 vLookAtDir = vTargetPosition - vCameraPosition;
        float fDistToCamera = vLookAtDir.magnitude;

        if (!bIsFovTransitioning)
        {
            float startFov = GetFOVForMode(eStartFovMode, fDistToCamera, fStartFixedFov, fStartDynamicKartSize);
            float endFov = GetFOVForMode(eEndFovMode, fDistToCamera, fEndFixedFov, fEndDynamicKartSize);
            StartFovTransition(startFov, endFov, fFovTransitionDurationPercentage * fCamDurationSec);
        }


        if (eStartFovMode == EFovMode.DYNAMIC_CONST_KART_SIZE)
            fInitialFov = CalculateFOV(fDistToCamera, fStartDynamicKartSize);

        if (eEndFovMode == EFovMode.DYNAMIC_CONST_KART_SIZE)
            fTransitionToFov = CalculateFOV(fDistToCamera, fEndDynamicKartSize);

        HandleFovTransition();
    }

    private void HandleFovTransition()
    {
        if (!bIsFovTransitioning) return;

        float elapsedTime = fTimeSinceCameraRun - fTransitionStartTime;
        float t = Mathf.Clamp01(elapsedTime / fTransitionDuration);

        fFieldOfView = Mathf.Lerp(fInitialFov, fTransitionToFov, fovTransitionCurve.Evaluate(t));
        fCurrentFov = fFieldOfView;

    }

    private void StartFovTransition(float startFov, float endFov, float duration)
    {
        fInitialFov = startFov;
        fTransitionToFov = endFov;
        fTransitionDuration = duration;
        fTransitionStartTime = fTimeSinceCameraRun;
        bIsFovTransitioning = true;
    }

    public float CalculateFOV(float distanceToCamera, float targetHeight)
    {
        if (distanceToCamera <= 0) return 75f;
        float fovRadians = 2 * Mathf.Atan((targetHeight * 0.5f) / distanceToCamera);
        return Mathf.Rad2Deg * fovRadians;
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 vTargetPosition = cameraLookAtTarget.transform.position;
        float fTargetOffset = 2.5f;
        vTargetPosition += Vector3.up * fTargetOffset;

        if (bEnableTargetFollowLagEffect == true)
        {
            float elapsedTime = Mathf.Clamp(fTimeSinceCameraRun, 0.0f, fFollowLagDurationSec);
            float fElapsedTimeNormalized = elapsedTime / fFollowLagDurationSec;
            fElapsedTimeNormalized = QuadraticEaseOut(fElapsedTimeNormalized);

            float fDelayIntensity = fFollowLagIntensity;
            float fSmoothDampLerped = Mathf.Lerp(fDelayIntensity, 0.0f, fElapsedTimeNormalized);

            vSmoothDampTargetPos = Vector3.SmoothDamp(vSmoothDampTargetPos, vTargetPosition, ref vSmoothDampTargetPosVel, fSmoothDampLerped);
            vTargetPosition = vSmoothDampTargetPos;
        }
        return vTargetPosition;
    }

    public float QuadraticEaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 2f);
    }

    public static float InSine(float t) => (float)-Math.Cos(t * Math.PI / 2);
    public static float OutSine(float t) => (float)Math.Sin(t * Math.PI / 2);
    public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;
    public float InCubic(float t) => t * t * t;
    public float OutCubic(float t) => 1 - InCubic(1 - t);
    public float InOutCubic(float t)
    {
        if (t < 0.5) return InCubic(t * 2) / 2;
        return 1 - InCubic((1 - t) * 2) / 2;
    }

    internal void TiggerDetectedActivateCamera(Transform _cameraLookAtTarget)
    {
        if (bIsCameraRunning == true)
            return;

        bIsCameraRunning = true;
        fTimeSinceCameraRun = 0.0f;
        cameraLookAtTarget = _cameraLookAtTarget;

        vSmoothDampTargetPos = _cameraLookAtTarget.position;

        vCameraPosition = ptkBezierMB.ptkBezier.GetPointAtDistance(0).Position;
    }

    internal void StopCamera()
    {
        bIsCameraRunning = false;
        bIsFovTransitioning = false;
        fFieldOfView = 75.0f;
        fTransitionStartTime = 0.0f;
        fTimeSinceCameraRun = 0.0f;
    }

    internal bool IsCameraRunning()
    {
        return bIsCameraRunning;
    }



    public enum EFovConfig
    {
        F45_D4,
        F60_D12,
        F30_D4,

        D6_F50,
        D9_F20,
        D16_F45,

        F15_F40,
        F45_F15,
        F15_F15,
        F35_F35,

        D4_D16,
        D16_D9,
        D4_D9,
        D10_D10,
        D17_D17,

        __CUSTOM
    }

    private readonly Dictionary<EFovConfig, (EFovMode startMode, float startValue, EFovMode endMode, float endValue)> FOV_CONFIGS = new Dictionary<EFovConfig, (EFovMode, float, EFovMode, float)>
    {
        { EFovConfig.F45_D4, (EFovMode.FIXED, 45.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 4.0f) },
        { EFovConfig.F60_D12, (EFovMode.FIXED, 60.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 12.0f) },
        { EFovConfig.F30_D4, (EFovMode.FIXED, 30.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 4.0f) },
        { EFovConfig.D6_F50, (EFovMode.DYNAMIC_CONST_KART_SIZE, 6.0f, EFovMode.FIXED, 50.0f) },
        { EFovConfig.D9_F20, (EFovMode.DYNAMIC_CONST_KART_SIZE, 9.0f, EFovMode.FIXED, 20.0f) },
        { EFovConfig.D16_F45, (EFovMode.DYNAMIC_CONST_KART_SIZE, 16.0f, EFovMode.FIXED, 45.0f) },
        { EFovConfig.F15_F40, (EFovMode.FIXED, 15.0f, EFovMode.FIXED, 40.0f) },
        { EFovConfig.F45_F15, (EFovMode.FIXED, 45.0f, EFovMode.FIXED, 15.0f) },
        { EFovConfig.F15_F15, (EFovMode.FIXED, 15.0f, EFovMode.FIXED, 15.0f) },
        { EFovConfig.F35_F35, (EFovMode.FIXED, 35.0f, EFovMode.FIXED, 35.0f) },
        { EFovConfig.D4_D16, (EFovMode.DYNAMIC_CONST_KART_SIZE, 4.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 16.0f) },
        { EFovConfig.D16_D9, (EFovMode.DYNAMIC_CONST_KART_SIZE, 16.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 9.0f) },
        { EFovConfig.D4_D9, (EFovMode.DYNAMIC_CONST_KART_SIZE, 4.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 9.0f) },
        { EFovConfig.D10_D10, (EFovMode.DYNAMIC_CONST_KART_SIZE, 10.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 10.0f) },
        { EFovConfig.D17_D17, (EFovMode.DYNAMIC_CONST_KART_SIZE, 17.0f, EFovMode.DYNAMIC_CONST_KART_SIZE, 17.0f) }
    };

    public void ApplyAndUsePredefinedFOVMode(PTK_ReplayLookAtCamConfig.EFovConfig eFovConfig)
    {
        if (FOV_CONFIGS.ContainsKey(eFovConfig))
        {
            var config = FOV_CONFIGS[eFovConfig];
            ApplyConfig(config);
        }
    }

    private void ApplyConfig((EFovMode startMode, float startValue, EFovMode endMode, float endValue) config)
    {
        eStartFovMode = config.startMode;
        eEndFovMode = config.endMode;

        if (config.startMode == EFovMode.FIXED)
            fStartFixedFov = config.startValue;
        else
            fStartDynamicKartSize = config.startValue;

        if (config.endMode == EFovMode.FIXED)
            fEndFixedFov = config.endValue;
        else
            fEndDynamicKartSize = config.endValue;
    }
}
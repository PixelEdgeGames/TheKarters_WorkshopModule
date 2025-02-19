using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ReplayMontageCamConfig : MonoBehaviour
{
    [Header("Settings")]
    public ECameraType eCameraType;
    public int iCamPresetIndex;
    [Header("Duration")]
    public float fCamDurationSec = 4.0f;
    float fTimeSinceCameraRun = 0.0F;
    [Header("Priv")]
    public Transform triggersParent;
    public enum ECameraType
    {
        E_GAMEPLAY_AFTER_RACE_CAM,
        E_GAMEPLAY_DYNAMIC_ROAD_IN_VIEW,
        E_GAMEPLAY_DYNAMIC_ROAD_IN_VIEW_BACK,
        E_GAMEPLAY_NO_HUD,
        E_GAMEPLAY_HUD,
        E_KART,
        E_KART_LOCKED,
        E_SPLINE_FREE,
        E_DIRECTOR_ENVIRO,
        E_DIRECTOR_LOOKAT,

        __COUNT
    }


    private bool bIsCameraRunning = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        fTimeSinceCameraRun += Time.deltaTime;
        fTimeSinceCameraRun = Mathf.Clamp(fTimeSinceCameraRun, 0.0f, fCamDurationSec);


        if (bIsCameraRunning)
        {
            float fNormalizedTime = fTimeSinceCameraRun / fCamDurationSec;
            fNormalizedTime = Mathf.Clamp01(fNormalizedTime);

        }

        if (fTimeSinceCameraRun >= fCamDurationSec && bIsCameraRunning == true)
            StopCamera();
    }

    internal void TiggerDetectedActivateCamera(Transform _cameraLookAtTarget)
    {
        if (bIsCameraRunning == true)
            return;

        bIsCameraRunning = true;
        fTimeSinceCameraRun = 0.0f;
    }

    internal void StopCamera()
    {
        bIsCameraRunning = false;
        fTimeSinceCameraRun = 0.0f;
    }


    internal bool IsCameraRunning()
    {
        return bIsCameraRunning;
    }
}

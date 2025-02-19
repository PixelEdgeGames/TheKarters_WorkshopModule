using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ReplayEnviroCameraConfig : MonoBehaviour
{
    public PTK_BezierMB ptkBezierMB;
    float fSplineMovementSpeed = 35.0f;
    float fCompleteSplineWithinTime = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    float fTimeSinceCameraRun = 0.0f;

    [HideInInspector]
    public Vector3 vCameraPosition = Vector2.zero;
    [HideInInspector]
    public Quaternion qCameraRotation = Quaternion.identity;

    bool bIsCameraRunning = false;

    public void RunCamera()
    {
        fTimeSinceCameraRun = 0.0f;
        bIsCameraRunning = true;
        Vector3 vTargetPos = ptkBezierMB.ptkBezier.GetPointAtDistance(0).Position;
        Quaternion qRot = ptkBezierMB.ptkBezier.GetPointAtDistance(0).Rotation;
        vCameraPosition = vTargetPos;
        qCameraRotation = qRot;

    }

    public void StopCamera()
    {
        bIsCameraRunning = false;
    }
    public bool IsCameraRunning()
    {
        return bIsCameraRunning;
    }

    public Action<PTK_ReplayEnviroCameraConfig> OnCameraMovementEnded;

    // namespace Kryz.Tweening
  
    // Update is called once per frame
    void FixedUpdate()
    {

        if (bIsCameraRunning == true)
        {
            fTimeSinceCameraRun += Time.deltaTime;

            float fReachEndSplineInTime = fCompleteSplineWithinTime;// ptkBezierMB.ptkBezier.GetTotalLength() / fSplineMovementSpeed;
            float normalizedTime = fTimeSinceCameraRun / fReachEndSplineInTime;
            normalizedTime = Mathf.Clamp01(normalizedTime);

            normalizedTime = 1.0f - Mathf.Pow(1.0f - normalizedTime, 1.5f);

            float targetDistance = normalizedTime * ptkBezierMB.ptkBezier.GetTotalLength();
            var bezierPoint = ptkBezierMB.ptkBezier.GetPointAtDistance(targetDistance);
            Vector3 vTargetPos = bezierPoint.Position;
            Quaternion qTargetRot = bezierPoint.Rotation;

            vCameraPosition = Vector3.Lerp(vCameraPosition, vTargetPos,Time.deltaTime*25.0F);
            qCameraRotation =  Quaternion.Lerp(qCameraRotation, qTargetRot, Time.deltaTime * 10.0f);
            

            if(normalizedTime == 1.0f)
            {
                if (OnCameraMovementEnded != null)
                    OnCameraMovementEnded(this);

                StopCamera();
            }
        }
    }
}

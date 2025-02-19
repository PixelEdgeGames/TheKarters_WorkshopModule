using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_EnviroSplineCamerasManager : MonoBehaviour
{
    [HideInInspector]
    public PTK_ReplayEnviroCameraConfig[] enviroSplineCams;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize()
    {
        enviroSplineCams = this.GetComponentsInChildren<PTK_ReplayEnviroCameraConfig>();

        // so before race we will still see camera movement
        ResetAndPlayCamera();
    }

    int iCurrentCamIndex = -1;
    public void ResetAndPlayCamera()
    {
        iCurrentCamIndex = 0;

        for(int i=0;i< enviroSplineCams.Length;i++)
        {
            enviroSplineCams[i].StopCamera();
        }

        if (enviroSplineCams.Length > 0)
        {
            enviroSplineCams[iCurrentCamIndex].RunCamera();
        }

    }

    public Vector3 vCurrentEnviroCamPos = Vector3.zero;
    public Quaternion qCurrentEnviroCamRot = Quaternion.identity;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (enviroSplineCams.Length == 0 && iCurrentCamIndex >= 0)
            return;

        if(enviroSplineCams[iCurrentCamIndex].IsCameraRunning() == false)
        {
            iCurrentCamIndex++; iCurrentCamIndex %= enviroSplineCams.Length;

            enviroSplineCams[iCurrentCamIndex].RunCamera();
        }

        vCurrentEnviroCamPos = enviroSplineCams[iCurrentCamIndex].vCameraPosition;
        qCurrentEnviroCamRot = enviroSplineCams[iCurrentCamIndex].qCameraRotation;

    }
}

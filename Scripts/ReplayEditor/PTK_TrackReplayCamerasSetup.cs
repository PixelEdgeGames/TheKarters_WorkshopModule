using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TrackReplayCamerasSetup : MonoBehaviour
{
    private void Awake()
    {
        var meshRenderers =   this.GetComponentsInChildren<MeshRenderer>();
        var colliders = this.GetComponentsInChildren<Collider>();

        foreach (var renderer in meshRenderers)
            renderer.enabled = false;

        foreach (var col in colliders)
            col.enabled = false;
    }

    [HideInInspector]
    public PTK_ReplayLookAtCamConfig[] lookAtCams;
    [HideInInspector]
    public PTK_ReplayMontageCamConfig[] montageCams;
    [HideInInspector]
    public PTK_ReplayEnviroCameraConfig[] enviroSplineCams;

    [HideInInspector]
    public PTK_EnviroSplineCamerasManager enviroSplineCamerasManager;
    public void Initialize()
    {
         lookAtCams = this.GetComponentsInChildren<PTK_ReplayLookAtCamConfig>();
         montageCams = this.GetComponentsInChildren<PTK_ReplayMontageCamConfig>();
        enviroSplineCams = this.GetComponentsInChildren<PTK_ReplayEnviroCameraConfig>();

        enviroSplineCamerasManager = this.GetComponentInChildren<PTK_EnviroSplineCamerasManager>();

        enviroSplineCamerasManager.Initialize();

        foreach (var cam in lookAtCams)
            cam.Initialize();

        foreach (var cam in montageCams)
            cam.Initialize();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool bWasTrackReplayObjSelected = false;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == true)
            return;

        bool bIsTrackReplayCamObjSelected = false;
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject != null && UnityEditor.Selection.activeGameObject.GetComponentInParent<PTK_TrackReplayCamerasSetup>() != null)
        {
            bIsTrackReplayCamObjSelected = true;
        }
#endif

        if(bWasTrackReplayObjSelected != bIsTrackReplayCamObjSelected)
        {
            var meshRenderers = this.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in meshRenderers)
                renderer.enabled = bIsTrackReplayCamObjSelected;
        }

        bWasTrackReplayObjSelected = bIsTrackReplayCamObjSelected;
    }
}

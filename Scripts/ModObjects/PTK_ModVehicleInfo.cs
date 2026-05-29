using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModVehicleInfo : MonoBehaviour
{
    [Header("Optional - for vehicles that want to hide character")]
    public bool bHideCharacterInVehicle = false;
    [Header("Optional - for vehicles that want to hide wheels")]
    public bool bHideWheels = false;

    Renderer[] childRenderers = new Renderer[0];
    private void Awake()
    {
        HideSuspensionDebugVisuals(transform);

        // get all renderers
        childRenderers = this.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null && originalRendererStatus.ContainsKey(childRenderers[i]) == false)
                originalRendererStatus.Add(childRenderers[i], childRenderers[i].enabled);
        }


    }

    Dictionary<Renderer, bool> originalRendererStatus = new Dictionary<Renderer, bool>();

    bool bRenderersAreVisible = true;
    public void HideRenderers()
    {
        if (bRenderersAreVisible == false)
            return;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null)
                childRenderers[i].enabled = false;
        }

        bRenderersAreVisible = false;
    }

    public void ShowRenderersRevertToDefault()
    {
        if (bRenderersAreVisible == true)
            return;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (childRenderers[i] != null && originalRendererStatus.ContainsKey(childRenderers[i]) == true)
                childRenderers[i].enabled = originalRendererStatus[childRenderers[i]];
        }

        HideSuspensionDebugVisuals(transform);
        bRenderersAreVisible = true;
    }

    public static void HideSuspensionDebugVisuals(Transform root)
    {
        if (root == null)
            return;

        PTK_SuspensionPreviewMesh[] previewMeshes = root.GetComponentsInChildren<PTK_SuspensionPreviewMesh>(true);
        for (int iPreview = 0; iPreview < previewMeshes.Length; iPreview++)
        {
            PTK_SuspensionPreviewMesh previewMesh = previewMeshes[iPreview];
            if (previewMesh == null)
                continue;

            if (previewMesh.previewMeshes != null)
            {
                for (int iMesh = 0; iMesh < previewMesh.previewMeshes.Length; iMesh++)
                {
                    if (previewMesh.previewMeshes[iMesh] != null)
                        previewMesh.previewMeshes[iMesh].enabled = false;
                }
            }

            MeshRenderer[] childPreviewRenderers = previewMesh.GetComponentsInChildren<MeshRenderer>(true);
            for (int iMesh = 0; iMesh < childPreviewRenderers.Length; iMesh++)
            {
                if (childPreviewRenderers[iMesh] != null)
                    childPreviewRenderers[iMesh].enabled = false;
            }
        }

        if (PTK_HideMeshRendererInPlayMode.bForceShowDebugMeshRenderersInPlayMode == true)
            return;

        PTK_HideMeshRendererInPlayMode[] playModeHiders = root.GetComponentsInChildren<PTK_HideMeshRendererInPlayMode>(true);
        for (int iHider = 0; iHider < playModeHiders.Length; iHider++)
        {
            PTK_HideMeshRendererInPlayMode hider = playModeHiders[iHider];
            if (hider == null || hider.bEnabled == false)
                continue;

            MeshRenderer meshRenderer = hider.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            if (hider.bIncludeChildrenMeshRenderers == true)
            {
                MeshRenderer[] childMeshRenderers = hider.GetComponentsInChildren<MeshRenderer>(true);
                for (int iMesh = 0; iMesh < childMeshRenderers.Length; iMesh++)
                {
                    if (childMeshRenderers[iMesh] != null)
                        childMeshRenderers[iMesh].enabled = false;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

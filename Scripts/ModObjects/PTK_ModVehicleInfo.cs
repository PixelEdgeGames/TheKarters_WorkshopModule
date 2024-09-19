using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModVehicleInfo : MonoBehaviour
{
    [Header("Optional - for vehicles that want to hide character")]
    public bool bHideCharacterInVehicle = false;
    [Header("Optional - for vehicles that want to hide wheels")]
    public bool bHideWheels = false;

    Renderer[] childRenderers;
    private void Awake()
    {
        // first disbale debug meshes so they wont be turned on later
        PTK_SuspensionPreviewMesh[] previewMeshes = this.GetComponentsInChildren<PTK_SuspensionPreviewMesh>();

        for (int iPreview = 0; iPreview < previewMeshes.Length; iPreview++)
        {
            for (int iMesh = 0; iMesh < previewMeshes[iPreview].previewMeshes.Length; iMesh++)
            {
                previewMeshes[iPreview].previewMeshes[iMesh].enabled = false;
            }
        }


        // get all renderers
        childRenderers = this.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < childRenderers.Length; i++)
        {
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
            childRenderers[i].enabled = originalRendererStatus[childRenderers[i]];
        }

        bRenderersAreVisible = true;
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

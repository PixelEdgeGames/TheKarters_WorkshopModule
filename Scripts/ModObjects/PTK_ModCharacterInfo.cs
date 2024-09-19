using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModCharacterInfo : MonoBehaviour
{
    [Header("Optional - for characters that dont want to show vehicle")]
    public bool bHideVehicleAndWheels = false;

    Renderer[] childRenderers;

    private void Awake()
    {
        childRenderers = this.GetComponentsInChildren<Renderer>(); 

        for(int i=0;i< childRenderers.Length;i++)
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

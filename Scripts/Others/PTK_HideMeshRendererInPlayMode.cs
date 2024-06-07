using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_HideMeshRendererInPlayMode : MonoBehaviour
{
    public static bool bForceShowDebugMeshRenderersInPlayMode = false;

    public bool bIncludeChildrenMeshRenderers = false;

    // Start is called before the first frame update
    void Start()
    {
        if (bForceShowDebugMeshRenderersInPlayMode == true)
            return;

        var meshRenderer = this.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (bIncludeChildrenMeshRenderers == true)
        {
            var meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
                meshRenderers[i].enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

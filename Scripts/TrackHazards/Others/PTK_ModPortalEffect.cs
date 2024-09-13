using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModPortalEffect : MonoBehaviour
{
    [Header("DisableCollider")]
    public bool bDisableCollider = false;
    [Header("Properties")]
    public MeshRenderer rendererParent;
    public Collider parentCollider;
    // Start is called before the first frame update
    void Awake()
    {
        rendererParent.enabled = false;

        if (bDisableCollider == true)
            parentCollider.enabled = false;
    }

    private void Start()
    {
        rendererParent.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        // to override trigger show mesh
        if (rendererParent.enabled == true)
            rendererParent.enabled = false;
    }
}

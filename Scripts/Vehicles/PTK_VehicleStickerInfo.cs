using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_VehicleStickerInfo : MonoBehaviour
{
    public enum EAttachType
    {
        E_BODY,
        E_ENGINE
    }

    public EAttachType eAttachType = EAttachType.E_BODY;
    [Header("Change if sticker width is big")]
    public float fDefaultTileX = 1.0f;
    [Header("Change if sticker height is big")]
    public float fDefaultTileY = 1.0f;
    [Header("Texture offset")]
    public float fDefaultOffsetX = 0.0f;
    public float fDefaultOffsetY = 0.0f;

    [HideInInspector]
   public MeshFilter[] stickerMeshFilters;
    // Start is called before the first frame update
    void Awake()
    {
        stickerMeshFilters = this.GetComponentsInChildren<MeshFilter>();
        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for(int i=0;i< colliders.Length;i++)
            GameObject.DestroyImmediate(colliders[i]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

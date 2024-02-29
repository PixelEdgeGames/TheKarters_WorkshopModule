using System;
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

    public Material stickerMaterialReference;

    [HideInInspector]
    public MeshFilter[] stickerMeshFilters;
    public MeshFilter[] stickerMeshFiltersSecondLayerCreated;


    private int iStickerSettings_FirstLayer;
    private int iStickerSettings_SecondLayer;
    private Texture2D stickerTexture_wrapMode_Repeat;
    private Texture2D stickerTexture_wrapMode_Clamp;

    private Material firstLayerMaterial;
    private Material secondLayerMaterial;

    // Start is called before the first frame update
    void Awake()
    {
        stickerMeshFilters = this.GetComponentsInChildren<MeshFilter>();
        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for(int i=0;i< colliders.Length;i++)
            GameObject.DestroyImmediate(colliders[i]);

        stickerMeshFiltersSecondLayerCreated = new MeshFilter[stickerMeshFilters.Length];

        for (int i=0;i< stickerMeshFilters.Length;i++)
        {
            MeshRenderer firstLayerMeshREnderer = stickerMeshFilters[i].GetComponent<MeshRenderer>();
            firstLayerMaterial = new Material(stickerMaterialReference);
            firstLayerMeshREnderer.material = firstLayerMaterial;

            firstLayerMeshREnderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            stickerMeshFilters[i].gameObject.SetActive(false);


            stickerMeshFiltersSecondLayerCreated[i] = new GameObject(stickerMeshFilters[i].gameObject.name + "_2ndLayer").GetComponent<MeshFilter>();
            stickerMeshFiltersSecondLayerCreated[i].transform.parent = stickerMeshFilters[i].gameObject.transform.parent;
            stickerMeshFiltersSecondLayerCreated[i].transform.localPosition = stickerMeshFilters[i].gameObject.transform.localPosition;
            stickerMeshFiltersSecondLayerCreated[i].transform.localRotation = stickerMeshFilters[i].gameObject.transform.localRotation;
            stickerMeshFiltersSecondLayerCreated[i].transform.localScale = stickerMeshFilters[i].gameObject.transform.localScale;

            // we need seperate material to set material params
            MeshRenderer secondLayerMeshREnderer = stickerMeshFiltersSecondLayerCreated[i].GetComponent<MeshRenderer>();
            secondLayerMaterial = new Material(stickerMaterialReference);
            secondLayerMeshREnderer.material = secondLayerMaterial;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    internal void SetStickerData(Texture2D _stickerTexture_wrapMode_Repeat, Texture2D _stickerTexture_wrapMode_Clamp, int _iStickerSettingPaacked,bool bIsFirstLayer)
    {
        SetStickerTextures(stickerTexture_wrapMode_Repeat, stickerTexture_wrapMode_Clamp, bIsFirstLayer);
        SetStickerSetting(_iStickerSettingPaacked, bIsFirstLayer);

    }

    internal void SetStickerData(Texture2D _stickerTexture_wrapMode_Repeat, Texture2D _stickerTexture_wrapMode_Clamp, bool bIsFirstLayer)
    {
        SetStickerTextures(_stickerTexture_wrapMode_Repeat, _stickerTexture_wrapMode_Clamp, bIsFirstLayer);
    }

    private void SetStickerSetting(int _iStickerSettingPaacked, bool bIsFirstLayer)
    {

    }

    private void SetStickerTextures(Texture2D _stickerTexture_wrapMode_Repeat, Texture2D _stickerTexture_wrapMode_Clamp, bool bIsFirstLayer)
    {
        // texture removed - set invisible
        if (stickerTexture_wrapMode_Repeat != null && _stickerTexture_wrapMode_Repeat == null)
        {
            SetStickerVisible(false, bIsFirstLayer);
        }

        // current texture was empty but new one is full - set visible
        if (stickerTexture_wrapMode_Clamp == null && _stickerTexture_wrapMode_Clamp != null)
        {
            SetStickerVisible(true, bIsFirstLayer);
        }

        stickerTexture_wrapMode_Repeat = _stickerTexture_wrapMode_Repeat;
        stickerTexture_wrapMode_Clamp = _stickerTexture_wrapMode_Clamp;
    }

    void SetStickerVisible(bool bVisible, bool bIsFirstLayer)
    {
        for (int i = 0; i < stickerMeshFilters.Length; i++)
        {
            if (bIsFirstLayer == true)
            {
                if (stickerMeshFilters[i].gameObject.activeInHierarchy != bVisible)
                    stickerMeshFilters[i].gameObject.SetActive(bVisible);
            }
            else
            {
                if (stickerMeshFiltersSecondLayerCreated[i].gameObject.activeInHierarchy != bVisible)
                    stickerMeshFiltersSecondLayerCreated[i].gameObject.SetActive(bVisible);
            }
        }
    }
}

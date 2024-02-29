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
    public MeshRenderer[] stickerMeshRenderers;
    [HideInInspector]
    public MeshRenderer[] stickerMeshRenderersSecondLayerCreated;


    private int iStickerSettings_FirstLayer;
    private Texture2D stickerTexture_wrapMode_Repeat_FristLayer;
    private Texture2D stickerTexture_wrapMode_Clamp_FirstLayer;

    private int iStickerSettings_SecondLayer;
    private Texture2D stickerTexture_wrapMode_Repeat_SecondLayer;
    private Texture2D stickerTexture_wrapMode_Clamp_SecondLayer;

    private Material firstLayerMaterial_Back;
    private Material secondLayerMaterial_Top;

    public bool IsStickerMeshAvailable()
    {
        return stickerMeshRenderers.Length > 0;
    }
    // Start is called before the first frame update
    void Awake()
    {
        stickerMeshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for(int i=0;i< colliders.Length;i++)
            GameObject.DestroyImmediate(colliders[i]);

        stickerMeshRenderersSecondLayerCreated = new MeshRenderer[stickerMeshRenderers.Length];

        GameObject SecondLayer_Top = new GameObject("SecondLayer - TOP");
        SecondLayer_Top.transform.parent = transform;
        SecondLayer_Top.transform.localPosition = Vector3.zero;
        SecondLayer_Top.transform.localRotation = Quaternion.identity;
        SecondLayer_Top.transform.localScale = Vector3.one;
        SecondLayer_Top.transform.SetSiblingIndex(0);


        for (int i=0;i< stickerMeshRenderers.Length;i++)
        {
            MeshRenderer firstLayerMeshREnderer = stickerMeshRenderers[i].GetComponent<MeshRenderer>();
            firstLayerMaterial_Back = new Material(stickerMaterialReference);
            firstLayerMeshREnderer.material = firstLayerMaterial_Back;
            firstLayerMaterial_Back.renderQueue = 3000;

            firstLayerMeshREnderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            stickerMeshRenderers[i].gameObject.SetActive(false);


            stickerMeshRenderersSecondLayerCreated[i] = GameObject.Instantiate(stickerMeshRenderers[i].gameObject, SecondLayer_Top.transform, false).GetComponent<MeshRenderer>();
          
            stickerMeshRenderersSecondLayerCreated[i].name = (stickerMeshRenderers[i].gameObject.name + "_2ndLayer");
            stickerMeshRenderersSecondLayerCreated[i].transform.localPosition = stickerMeshRenderers[i].gameObject.transform.localPosition;
            stickerMeshRenderersSecondLayerCreated[i].transform.localRotation = stickerMeshRenderers[i].gameObject.transform.localRotation;
            stickerMeshRenderersSecondLayerCreated[i].transform.localScale = stickerMeshRenderers[i].gameObject.transform.localScale;

            // we need seperate material to set material params
            MeshRenderer secondLayerMeshREnderer = stickerMeshRenderersSecondLayerCreated[i].GetComponent<MeshRenderer>();
            secondLayerMaterial_Top = new Material(stickerMaterialReference);
            secondLayerMaterial_Top.renderQueue = 3005;
            secondLayerMeshREnderer.material = secondLayerMaterial_Top;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    internal void SetStickerData(Texture2D texRepeat, Texture2D texClamp, int _iStickerSettingPaacked,bool bIsFirstLayer)
    {
        SetStickerTextures(texRepeat, texClamp, bIsFirstLayer);
        SetStickerSetting(_iStickerSettingPaacked, bIsFirstLayer);

    }

    internal void SetStickerData(Texture2D texRepeat, Texture2D texClamp, bool bIsFirstLayer)
    {
        SetStickerTextures(texRepeat, texClamp, bIsFirstLayer);
    }

    private void SetStickerSetting(int _iStickerSettingPaacked, bool bIsFirstLayer)
    {

    }

    private void SetStickerTextures(Texture2D texRepeat, Texture2D texClamp, bool bIsFirstLayer)
    {
        // texture removed - set invisible
        if (texRepeat == null)
        {
            SetStickerVisible(false, bIsFirstLayer);
        }

        // current texture was empty but new one is full - set visible
        if (texRepeat != null)
        {
            SetStickerVisible(true, bIsFirstLayer);
        }

        if(bIsFirstLayer == true)
        {
            bool bTextureChanged = stickerTexture_wrapMode_Repeat_FristLayer != texRepeat;

            stickerTexture_wrapMode_Repeat_FristLayer = texRepeat;
            stickerTexture_wrapMode_Clamp_FirstLayer = texClamp;

            if (bTextureChanged)
            {
                TextureChanged(bIsFirstLayer);
            }
        }else
        {
            bool bTextureChanged = stickerTexture_wrapMode_Repeat_SecondLayer != texRepeat;

            stickerTexture_wrapMode_Repeat_SecondLayer = texRepeat;
            stickerTexture_wrapMode_Clamp_SecondLayer = texClamp;

            if (bTextureChanged)
            {
                TextureChanged(bIsFirstLayer);
            }
        }

    }

    void TextureChanged(bool bIsFirstLayer)
    {
        // choose if repeat or wrap should be from setting
        if (bIsFirstLayer)
        {
            firstLayerMaterial_Back.mainTexture = stickerTexture_wrapMode_Repeat_FristLayer;
        }
        else
        {
            secondLayerMaterial_Top.mainTexture = stickerTexture_wrapMode_Repeat_SecondLayer;
        }
    }

    void SetStickerVisible(bool bVisible, bool bIsFirstLayer)
    {
        for (int i = 0; i < stickerMeshRenderers.Length; i++)
        {
            if (bIsFirstLayer == true)
            {
                if (stickerMeshRenderers[i].gameObject.activeInHierarchy != bVisible)
                    stickerMeshRenderers[i].gameObject.SetActive(bVisible);
            }
            else
            {
                if (stickerMeshRenderersSecondLayerCreated[i].gameObject.activeInHierarchy != bVisible)
                    stickerMeshRenderersSecondLayerCreated[i].gameObject.SetActive(bVisible);
            }
        }
    }
}

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
    private Vector3[] vMeshAvgLocalPositions; // stickers can be exported with parent vehicle mesh origin so they position in unity will be 0, thats why we need to look into vertices
    [HideInInspector]
    public MeshRenderer[] stickerMeshRenderers_FirstLayer_Back;
    [HideInInspector]
     MeshRenderer[] stickerMeshRenderers_SecondLayer_Front;


    private int iStickerSettings_FirstLayer;
    private Texture2D stickerTexture_wrapMode_Repeat_FristLayer;
    private Texture2D stickerTexture_wrapMode_Clamp_FirstLayer;

    private int iStickerSettings_SecondLayer;
    private Texture2D stickerTexture_wrapMode_Repeat_SecondLayer;
    private Texture2D stickerTexture_wrapMode_Clamp_SecondLayer;

    private Material material_FirstLayer_Back;
    private Material material_SecondLayer_Front;

    public Vector3 GetStickerMeshesWorldPosAvg()
    {
        if (stickerMeshRenderers_FirstLayer_Back.Length == 0)
            return transform.position;

        Vector3 vPos = Vector3.zero;

        for(int i=0;i< stickerMeshRenderers_FirstLayer_Back.Length;i++)
        {
            vPos += stickerMeshRenderers_FirstLayer_Back[i].transform.TransformPoint (vMeshAvgLocalPositions[i]);
        }

        vPos /= (float)stickerMeshRenderers_FirstLayer_Back.Length;

        return vPos;
    }
    public bool IsStickerMeshAvailable()
    {
        return stickerMeshRenderers_FirstLayer_Back.Length > 0;
    }

    bool bOutlineIsCurrentlyPresented_ForceMeshVisible_FirstLayerBack = false;
    bool bOutlineIsCurrentlyPresented_ForceMeshVisible_SecondLayerTop = false;
    public void ForceMeshVisibleForOutline(bool _bVisible, bool bIsFirstLayer)
    {
        if (bIsFirstLayer == true)
            bOutlineIsCurrentlyPresented_ForceMeshVisible_FirstLayerBack = _bVisible;
        else
            bOutlineIsCurrentlyPresented_ForceMeshVisible_SecondLayerTop = _bVisible;

        RefreshStickerVisibility(bIsFirstLayer);
    }

    // Start is called before the first frame update
    void Awake()
    {
        stickerMeshRenderers_FirstLayer_Back = this.GetComponentsInChildren<MeshRenderer>();

        vMeshAvgLocalPositions = new Vector3[stickerMeshRenderers_FirstLayer_Back.Length];


        for (int iMeshRenderer=0;iMeshRenderer< stickerMeshRenderers_FirstLayer_Back.Length;iMeshRenderer++)
        {
            var meshFilter = stickerMeshRenderers_FirstLayer_Back[iMeshRenderer].GetComponent<MeshFilter>();

            int iStepsToMakeMax = 200;
            int iStep = Mathf.CeilToInt(meshFilter.sharedMesh.vertexCount / (float)iStepsToMakeMax);

            int iAvgCount = 0;
            for(int iVertex = 0; iVertex < meshFilter.sharedMesh.vertexCount; iVertex += iStep)
            {
                vMeshAvgLocalPositions[iMeshRenderer] += meshFilter.sharedMesh.vertices[iVertex];
                iAvgCount++;
            }

            vMeshAvgLocalPositions[iMeshRenderer] /= (float)iAvgCount;
        }

        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for(int i=0;i< colliders.Length;i++)
            GameObject.DestroyImmediate(colliders[i]);

        stickerMeshRenderers_SecondLayer_Front = new MeshRenderer[stickerMeshRenderers_FirstLayer_Back.Length];

        GameObject stickersParent_SecondLayerFront = new GameObject("StickersParent_SecondLayerFront");
        stickersParent_SecondLayerFront.transform.parent = transform;
        stickersParent_SecondLayerFront.transform.localPosition = Vector3.zero;
        stickersParent_SecondLayerFront.transform.localRotation = Quaternion.identity;
        stickersParent_SecondLayerFront.transform.localScale = Vector3.one;
        stickersParent_SecondLayerFront.transform.SetSiblingIndex(0);


        for (int i=0;i< stickerMeshRenderers_FirstLayer_Back.Length;i++)
        {
            MeshRenderer firstLayerMeshREnderer_FirstLayerBack = stickerMeshRenderers_FirstLayer_Back[i].GetComponent<MeshRenderer>();

            material_FirstLayer_Back = new Material(stickerMaterialReference);
            material_FirstLayer_Back.mainTexture = null;
            firstLayerMeshREnderer_FirstLayerBack.material = material_FirstLayer_Back;
            material_FirstLayer_Back.renderQueue = 3000;

            firstLayerMeshREnderer_FirstLayerBack.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            stickerMeshRenderers_FirstLayer_Back[i].gameObject.SetActive(false);


            stickerMeshRenderers_SecondLayer_Front[i] = GameObject.Instantiate(stickerMeshRenderers_FirstLayer_Back[i].gameObject, stickersParent_SecondLayerFront.transform, false).GetComponent<MeshRenderer>();
          
            stickerMeshRenderers_SecondLayer_Front[i].name = (stickerMeshRenderers_FirstLayer_Back[i].gameObject.name + "_2ndLayerFront");
            stickerMeshRenderers_SecondLayer_Front[i].transform.localPosition = stickerMeshRenderers_FirstLayer_Back[i].gameObject.transform.localPosition;
            stickerMeshRenderers_SecondLayer_Front[i].transform.localRotation = stickerMeshRenderers_FirstLayer_Back[i].gameObject.transform.localRotation;
            stickerMeshRenderers_SecondLayer_Front[i].transform.localScale = stickerMeshRenderers_FirstLayer_Back[i].gameObject.transform.localScale;

            // we need seperate material to set material params
            MeshRenderer secondLayerMeshREnderer = stickerMeshRenderers_SecondLayer_Front[i].GetComponent<MeshRenderer>();
            material_SecondLayer_Front = new Material(stickerMaterialReference);
            material_SecondLayer_Front.mainTexture = null;
            material_SecondLayer_Front.renderQueue = 3005;
            secondLayerMeshREnderer.material = material_SecondLayer_Front;

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
        RefreshStickerVisibility(bIsFirstLayer);

        if (bIsFirstLayer == true)
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
            material_FirstLayer_Back.mainTexture = stickerTexture_wrapMode_Repeat_FristLayer;
        }
        else
        {
            material_SecondLayer_Front.mainTexture = stickerTexture_wrapMode_Repeat_SecondLayer;
        }
    }

    void RefreshStickerVisibility( bool bIsFirstLayer)
    {
        bool bContainsTexture = bIsFirstLayer ? stickerTexture_wrapMode_Repeat_FristLayer != null : stickerTexture_wrapMode_Repeat_SecondLayer != null;
        bool bIsVisibilityForcedForOutline = bIsFirstLayer ? bOutlineIsCurrentlyPresented_ForceMeshVisible_FirstLayerBack : bOutlineIsCurrentlyPresented_ForceMeshVisible_SecondLayerTop ;

        for (int i = 0; i < stickerMeshRenderers_FirstLayer_Back.Length; i++)
        {
            bool bStickerShouldBeVisible = bContainsTexture == true;

            float fTargetAlpha = 0.0f;
            bool bShouldBeVisible = false;
            
            
            if(bContainsTexture == true)// posiada teksture
            {
                fTargetAlpha = 1.0f;
                bShouldBeVisible = true;
            }
            else // nie posiadamy tekstury
            {
                if(bIsVisibilityForcedForOutline == true)
                {
                    // jesli jest forced bo pokazujemy outline a nie ma tekstury to pokazujemy ale alphe dajemy na 0
                    fTargetAlpha = 0.0f;
                    bShouldBeVisible = true;
                }
                else
                {
                    fTargetAlpha = 0.0f;
                    bShouldBeVisible = false;
                }
            }
            
            if (bIsFirstLayer == true)
            {
                material_FirstLayer_Back.color = new Color(material_FirstLayer_Back.color.r, material_FirstLayer_Back.color.g, material_FirstLayer_Back.color.b, fTargetAlpha);
            }
            else
            {
                material_SecondLayer_Front.color = new Color(material_SecondLayer_Front.color.r, material_SecondLayer_Front.color.g, material_SecondLayer_Front.color.b, fTargetAlpha);
            }

            if (bIsFirstLayer == true)
            {
                if (stickerMeshRenderers_FirstLayer_Back[i].gameObject.activeInHierarchy != bShouldBeVisible)
                    stickerMeshRenderers_FirstLayer_Back[i].gameObject.SetActive(bShouldBeVisible);
            }
            else
            {
                if (stickerMeshRenderers_SecondLayer_Front[i].gameObject.activeInHierarchy != bShouldBeVisible)
                    stickerMeshRenderers_SecondLayer_Front[i].gameObject.SetActive(bShouldBeVisible);
            }
        }
    }
}

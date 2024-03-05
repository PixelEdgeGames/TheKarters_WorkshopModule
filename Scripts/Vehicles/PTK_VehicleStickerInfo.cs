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
        Initialize();
    }

    bool bIsInitialized = false;

    private void Initialize()
    {
        if (bIsInitialized == true)
            return;

        bIsInitialized = true;


        stickerMeshRenderers_FirstLayer_Back = this.GetComponentsInChildren<MeshRenderer>();

        vMeshAvgLocalPositions = new Vector3[stickerMeshRenderers_FirstLayer_Back.Length];


        for (int iMeshRenderer = 0; iMeshRenderer < stickerMeshRenderers_FirstLayer_Back.Length; iMeshRenderer++)
        {
            var meshFilter = stickerMeshRenderers_FirstLayer_Back[iMeshRenderer].GetComponent<MeshFilter>();

            int iStepsToMakeMax = 200;
            int iStep = Mathf.CeilToInt(meshFilter.sharedMesh.vertexCount / (float)iStepsToMakeMax);

            int iAvgCount = 0;
            for (int iVertex = 0; iVertex < meshFilter.sharedMesh.vertexCount; iVertex += iStep)
            {
                vMeshAvgLocalPositions[iMeshRenderer] += meshFilter.sharedMesh.vertices[iVertex];
                iAvgCount++;
            }

            vMeshAvgLocalPositions[iMeshRenderer] /= (float)iAvgCount;
        }

        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
            GameObject.DestroyImmediate(colliders[i]);

        stickerMeshRenderers_SecondLayer_Front = new MeshRenderer[stickerMeshRenderers_FirstLayer_Back.Length];

        GameObject stickersParent_SecondLayerFront = new GameObject("StickersParent_SecondLayerFront");
        stickersParent_SecondLayerFront.transform.parent = transform;
        stickersParent_SecondLayerFront.transform.localPosition = Vector3.zero;
        stickersParent_SecondLayerFront.transform.localRotation = Quaternion.identity;
        stickersParent_SecondLayerFront.transform.localScale = Vector3.one;
        stickersParent_SecondLayerFront.transform.SetSiblingIndex(0);


        for (int i = 0; i < stickerMeshRenderers_FirstLayer_Back.Length; i++)
        {
            MeshRenderer firstLayerMeshREnderer_FirstLayerBack = stickerMeshRenderers_FirstLayer_Back[i].GetComponent<MeshRenderer>();

            material_FirstLayer_Back = new Material(stickerMaterialReference);
            material_FirstLayer_Back.mainTexture = null;
            material_FirstLayer_Back.renderQueue = 3000;
            firstLayerMeshREnderer_FirstLayerBack.material = material_FirstLayer_Back;

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
        SetStickerSetting(_iStickerSettingPaacked, bIsFirstLayer);
        SetStickerTextures(texRepeat, texClamp, bIsFirstLayer);

    }

    internal void SetStickerData(Texture2D texRepeat, Texture2D texClamp, bool bIsFirstLayer)
    {
        SetStickerTextures(texRepeat, texClamp, bIsFirstLayer);
    }

    CStickerIntBitVariable stickerIntVariablePacker = new CStickerIntBitVariable();

    bool bShouldUseRepeatTexture = true;

    public void SetStickerSetting(int _iStickerSettingPaacked, bool bIsFirstLayer)
    {
        if (bIsInitialized == false)
            Initialize();

        // no mesh renderers inside sticker
        if (stickerMeshRenderers_FirstLayer_Back.Length == 0)
            return;

        Material materialToSetParams = null;
        if(bIsFirstLayer == true)
        {
            materialToSetParams = material_FirstLayer_Back;
        }
        else
        {
            materialToSetParams = material_SecondLayer_Front;
        }

        stickerIntVariablePacker.UnpackVariables(_iStickerSettingPaacked);


        bool bRepeat = stickerIntVariablePacker.repeat_BitVar_0_1.Value == 0;

        if(bShouldUseRepeatTexture != bRepeat)
        {
            bShouldUseRepeatTexture = bRepeat;
            TextureChanged(bIsFirstLayer);
        }

        CVariable variableUnpacked = null;
        float fValueNormalizedX = 0;
        float fValueNormalizedY = 0;

        variableUnpacked = stickerIntVariablePacker.positionX_BitVar_0_15;

        fValueNormalizedX = (variableUnpacked.Value + 1) / (float)(variableUnpacked.GetMaxValue() + 1); // +1 so we get 0.5 for middle
        fValueNormalizedX -= 0.5f; // default is 8/16, so we want to show it as 0
        fValueNormalizedX *= 2.0f; // -1 : 1


        variableUnpacked = stickerIntVariablePacker.positionY_BitVar_0_15;

        fValueNormalizedY = (variableUnpacked.Value + 1) / (float)(variableUnpacked.GetMaxValue() + 1); // +1 so we get 0.5 for middle
        fValueNormalizedY -= 0.5f; // default is 8/16, so we want to show it as 0
        fValueNormalizedY *= 2.0f; // -1 : 1

        materialToSetParams.SetTextureOffset("_MainTex", new Vector2(-fValueNormalizedX,-fValueNormalizedY) * 3.0f);

        materialToSetParams.SetFloat("_Angle", stickerIntVariablePacker.AngleDegreesFromRotationValueIndex());

        int iColorIndex = stickerIntVariablePacker.color_BitVar_0_7.Value;
        if(iColorIndex == 0 )
        {
            materialToSetParams.SetFloat("_Hue", 0);
            materialToSetParams.SetFloat("_InverseCol", 0);
            materialToSetParams.SetFloat("_Grayscale", 0);
        }
        else if(iColorIndex == 1) // colorful to dark and white
        {
            materialToSetParams.SetFloat("_Hue", 0);
            materialToSetParams.SetFloat("_Grayscale", 1);
            materialToSetParams.SetFloat("_InverseCol", 0);
        }
        else if (iColorIndex == 2) // white to opposite dark
        {
            materialToSetParams.SetFloat("_Hue", 0);
            materialToSetParams.SetFloat("_Grayscale", 1);
            materialToSetParams.SetFloat("_InverseCol", 1);
        }
        else
        {
            materialToSetParams.SetFloat("_Hue", (iColorIndex-2) * 60.0f);
            materialToSetParams.SetFloat("_Grayscale", 0);
            materialToSetParams.SetFloat("_InverseCol", 0);
        }


        variableUnpacked = stickerIntVariablePacker.size_BitVar_0_15;
        float fScaleNormalized = (variableUnpacked.Value + 1) / (float)(variableUnpacked.GetMaxValue() + 1); // +1 so we dont get 0%
        float fMaxSizePercentage = (CStickerIntBitVariable.fMaxSizePercentage/100.0f); // for max value it will have 200%
        float fScaleTex = fScaleNormalized * fMaxSizePercentage;

        materialToSetParams.SetTextureScale("_MainTex", new Vector2(1.0f/fScaleTex,1.0f/ fScaleTex));


        variableUnpacked = stickerIntVariablePacker.transparency_BitVar_0_3;
        float fTransparencyNormalized  = (variableUnpacked.Value) / (float)(variableUnpacked.GetMaxValue());
        materialToSetParams.SetFloat("_Transparency", fTransparencyNormalized);
    }

    private void SetStickerTextures(Texture2D texRepeat, Texture2D texClamp, bool bIsFirstLayer)
    {
        if (bIsInitialized == false)
            Initialize();

        // no mesh renderers inside sticker
        if (stickerMeshRenderers_FirstLayer_Back.Length == 0)
            return;

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
            if(bShouldUseRepeatTexture == true)
                material_FirstLayer_Back.mainTexture = stickerTexture_wrapMode_Repeat_FristLayer;
            else
                material_FirstLayer_Back.mainTexture = stickerTexture_wrapMode_Clamp_FirstLayer;
        }
        else
        {
            if (bShouldUseRepeatTexture == true)
                material_SecondLayer_Front.mainTexture = stickerTexture_wrapMode_Repeat_SecondLayer;
            else
                material_SecondLayer_Front.mainTexture = stickerTexture_wrapMode_Clamp_SecondLayer;
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

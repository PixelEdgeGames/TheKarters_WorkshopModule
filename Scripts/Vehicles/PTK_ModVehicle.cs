using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PTK_ModVehicle : MonoBehaviour
{ 
	public bool bShowDebugMeshes = true;
    [Header("Config Per Vehicle -30,30")]
    [Tooltip("Increase vehicle angle after boostpad/3rd boost/boost landing.")]
    public float fBoostTiltIncreaseAngle = 5.0f;

    [Range(0.5f,1.5f)]
    [Tooltip("sideways angle multiplier on landing / during drift .")]
    public float fDriftingLeanAngleMultiplier = 1.0f;

    public enum EVehicleType
    {
        E_FOUR_WHEELS,
        E_THREE_WHEELS_SINGLE_IN_FRONT,
        E_THREE_WHEELS_SINGLE_IN_BACK,
        E_TWO_WHEELS_FRONT_BACK,
        E_TWO_WHEELS_LEFT_RIGHT
    }
    public EVehicleType eVehicleType = EVehicleType.E_FOUR_WHEELS;
    public float fl_WheelSize = 1.0f;
    public float fr_WheelSize = 1.0f;
    public float bl_WheelSize = 1.26f;
    public float br_WheelSize = 1.26f;

    public float fl_WheelMaxHangDist= 0.6f;
    public float fr_WheelMaxHangDist = 0.6f;
    public float bl_WheelMaxHangDist = 1.35f;
    public float br_WheelMaxHangDist = 1.35f;


    [Header("Click init button below to init")]
    public PTK_VehicleStickersParent stickersManager;
    public Animator vehicleAnimator;

    public Renderer[] vehicleRenderers;

    [Header("Base")]
    public Transform kartRoot;
    public Transform ikRigRootBone;
    public Transform drivingWheelBone;
    public Transform characterSocketBone;
    public Transform engineBone;

    [Header("Wheels")]
    public Transform fl_Bone;
    public Transform fr_Bone;
    public Transform bl_Bone;
    public Transform br_Bone;

    [Header("Boost Pipes")]
    public Transform pipe1;
    public Transform pipe2;


    [Header("Animation Clips")]
    public AnimationClip backTransform_off;
    public AnimationClip backTransform_on;
    public AnimationClip defaultAnim;
    public AnimationClip idleHighSpeed;
    public AnimationClip idleNormal;
    public AnimationClip damage;
    public AnimationClip damageDeath;
    public AnimationClip turnLeft;
    public AnimationClip turnRight;
    public AnimationClip strongHitBack;
    public AnimationClip strongHitFront;

    public PTK_SuspensionSetupParent suspensionParent;
    // Start is called before the first frame update
    void Awake()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        vehicleAnimator.transform.localPosition = Vector3.zero;
        vehicleAnimator.transform.localRotation = Quaternion.identity;

        stickersManager.Initialize();

        for (int i = 0; i < stickersManager.vehicleStickers.Length; i++)
        {
            if (stickersManager.vehicleStickers[i].eAttachType == PTK_VehicleStickerInfo.EAttachType.E_ENGINE && engineBone != null)
                stickersManager.vehicleStickers[i].transform.parent = engineBone.transform;
            else
                stickersManager.vehicleStickers[i].transform.parent = characterSocketBone.transform;
        }

        if (eVehicleType == PTK_ModVehicle.EVehicleType.E_TWO_WHEELS_LEFT_RIGHT)
        {
            bl_Bone.localPosition -= new Vector3(0.0F, 0.0F, 2.0F); // BL
            br_Bone.localPosition -= new Vector3(0.0F, 0.0F, 2.0F);// BR
        }
    }

    public int GetAvailableStickersCountFrontAndBack()
    {
        int iStickerCount = 0;
        for(int i=0;i< stickersManager.vehicleStickers.Length;i++)
        {
            if (stickersManager.vehicleStickers[i].IsStickerMeshAvailable() == true)
                iStickerCount++;
        }

        return iStickerCount*2;// x2 because front and back (back is created runtime)
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR

    [EasyButtons.Button]
    public void InitModel()
    {
        EnsureCorrectRigSetup();

        InitSkinnedMeshRenderers();

        InitBonesReferences();

        EditorUtility.SetDirty(this.gameObject);
    }
    void EnsureCorrectRigSetup()
    {
        // Find the SkinnedMeshRenderer in the child transforms
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found in child transforms.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh);
        ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

        if (modelImporter == null)
        {
            Debug.LogError("ModelImporter not found for the mesh: " + skinnedMeshRenderer.sharedMesh.name);
            return;
        }


        // Set the rig settings
        modelImporter.animationType = ModelImporterAnimationType.Generic;
        modelImporter.sourceAvatar = null; // Set this to use the model's own avatar

        AssignCorrectAnimationClips(path, modelImporter);


        vehicleAnimator = skinnedMeshRenderer.transform.parent.GetComponent<Animator>();
        if(vehicleAnimator == null)
        {
            Debug.LogError("Something is wrong! Vehicle doesnt have animator?");
        }else
        {
            vehicleAnimator.transform.localPosition = Vector3.zero;
            vehicleAnimator.transform.localRotation = Quaternion.identity;
        }
    }

    void AssignCorrectAnimationClips(string strModelPath, ModelImporter modelImporter)
    {

        System.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(strModelPath);

        AnimationClip[] animationClips = assets.OfType<AnimationClip>().ToArray();
        backTransform_off = animationClips.FirstOrDefault(clip => clip.name == "back_transform_OFF"); if (backTransform_off == null) Debug.LogError("Anim not found back_transform_OFF");
        backTransform_on = animationClips.FirstOrDefault(clip => clip.name == "back_transform_ON"); if (backTransform_on == null) Debug.LogError("Anim not found back_transform_ON");


        defaultAnim = animationClips.FirstOrDefault(clip => clip.name == "default"); if (defaultAnim == null) Debug.LogError("Anim not found default");
        idleHighSpeed = animationClips.FirstOrDefault(clip => clip.name == "idle_high_speed"); if (idleHighSpeed == null) Debug.LogError("Anim not found idle_high_speed");
        idleNormal = animationClips.FirstOrDefault(clip => clip.name == "idle_normal"); if (idleNormal == null) Debug.LogError("Anim not found idle_normal");

        damage = animationClips.FirstOrDefault(clip => clip.name == "damage"); if (damage == null) Debug.LogError("Anim not found damage");
        damageDeath = animationClips.FirstOrDefault(clip => clip.name == "damage_death"); if (damageDeath == null) Debug.LogError("Anim not found damage_death");

        turnLeft = animationClips.FirstOrDefault(clip => clip.name == "turning_left");
        if(turnLeft == null) turnLeft = animationClips.FirstOrDefault(clip => clip.name == "turn_left"); // legacy name
        if (turnLeft == null) Debug.LogError("Anim not found turning_left");

        turnRight = animationClips.FirstOrDefault(clip => clip.name == "turning_right");
        if (turnRight == null) turnRight = animationClips.FirstOrDefault(clip => clip.name == "turn_right");  // legacy name
        if (turnRight == null) Debug.LogError("Anim not found turning_right");

        strongHitBack = animationClips.FirstOrDefault(clip => clip.name == "strong_hit_back"); if (strongHitBack == null) Debug.LogError("Anim not strong_hit_back");
        strongHitFront = animationClips.FirstOrDefault(clip => clip.name == "strong_hit_front"); if (strongHitFront == null) Debug.LogError("Anim not found strong_hit_front");

        SetAnimLoopMode(new string[] { defaultAnim.name, idleHighSpeed.name, idleNormal.name }, modelImporter);

        modelImporter.SaveAndReimport();

    }

    void SetAnimLoopMode(string[] namesForLoop, ModelImporter modelImporter)
    {
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;

        // Set the wrap mode to loop for each clip
        foreach (ModelImporterClipAnimation clip in clipAnimations)
        {
            foreach(string strClipNameForLoop in namesForLoop)
            {
                if (clip.name == strClipNameForLoop)
                {
                    clip.loopTime = true;
                    clip.wrapMode = WrapMode.Loop;
                }
            }
        }

        // Apply the changes
        modelImporter.clipAnimations = clipAnimations;
    }

    private void InitSkinnedMeshRenderers()
    {
        vehicleRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

    }

    [Header("Bones Names")]
    public string strBodyBone = "Body";
    public string strRootBone = "Root_SCRIPT";
    public string strEngineBone = "Engine";
    public string strWheelBone = "Driving_Wheel";
    public string strPipeL = "FX-Pipe L";
    public string strPipeR = "FX-Pipe R";
    void InitBonesReferences()
    {
      Transform[] childTransforms =  this.transform.GetComponentsInChildren<Transform>();

         kartRoot = GetChildTransform(strRootBone, childTransforms);
        engineBone = GetChildTransform(strEngineBone, childTransforms);
        ikRigRootBone = GetChildTransform(strBodyBone, childTransforms);
        drivingWheelBone = GetChildTransform(strWheelBone, childTransforms);
        characterSocketBone = GetChildTransform(strBodyBone, childTransforms);

        fl_Bone = GetChildTransform("Wheel_F L", childTransforms);
        fr_Bone = GetChildTransform("Wheel_F R", childTransforms);
        bl_Bone = GetChildTransform("Wheel_B L", childTransforms);
        br_Bone = GetChildTransform("Wheel_B R", childTransforms);

        pipe1 = GetChildTransform(strPipeL, childTransforms);
        pipe2 = GetChildTransform(strPipeR, childTransforms);
    }

    Transform GetChildTransform(string strName, Transform[] childTransforms)
    {
        foreach(Transform child in childTransforms)
        {
            if (child.name == strName)
                return child;
        }

        Debug.LogError("Child bone not found!");
        return null;
    }

   
#endif

    public void InitAnimOverrideControllerAnims(AnimatorOverrideController animOverrideController)
    {
        animOverrideController["back_transform_OFF"] = backTransform_off;
        animOverrideController["back_transform_ON"] = backTransform_on;

        idleNormal.wrapMode = WrapMode.Loop;
        idleHighSpeed.wrapMode = WrapMode.Loop;

        animOverrideController["default"] = defaultAnim;
        animOverrideController["default_Steer"] = defaultAnim;
        animOverrideController["idle_high_speed"] = idleHighSpeed;
        animOverrideController["idle_normal"] = idleNormal;

        animOverrideController["obrazenia"] = damage;
        animOverrideController["obrazenia_death"] = damageDeath;

        animOverrideController["skrecanie_lewo"] = turnLeft;
        animOverrideController["skrecanie_prawo"] = turnRight;

        animOverrideController["strong_hit_back"] = strongHitBack;
        animOverrideController["strong_hit_front"] = strongHitFront;
    }
}

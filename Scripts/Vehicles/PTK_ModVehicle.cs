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

    [Header("Click init button below to init")]
    public Animator vehicleAnimator;

    public Renderer[] vehicleRenderers;

    [Header("Base")]
    public Transform kartRoot;
    public Transform ikRigRootBone;
    public Transform drivingWheelBone;
    public Transform characterSocketBone;

    [Header("Wheels")]
    public Transform fl_Bone;
    public Transform fl_Hinge;
    public Transform fr_Bone;
    public Transform fr_Hinge;
    public Transform bl_Bone;
    public Transform bl_Hinge;
    public Transform br_Bone;
    public Transform br_Hinge;

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
        backTransform_off = animationClips.FirstOrDefault(clip => clip.name == "back_transform_OFF");
        backTransform_on = animationClips.FirstOrDefault(clip => clip.name == "back_transform_ON");


        defaultAnim = animationClips.FirstOrDefault(clip => clip.name == "default");
        idleHighSpeed = animationClips.FirstOrDefault(clip => clip.name == "idle_high_speed");
        idleNormal = animationClips.FirstOrDefault(clip => clip.name == "idle_normal");

        damage = animationClips.FirstOrDefault(clip => clip.name == "damage");
        damageDeath = animationClips.FirstOrDefault(clip => clip.name == "damage_death");

        turnLeft = animationClips.FirstOrDefault(clip => clip.name == "turning_left");
        turnRight = animationClips.FirstOrDefault(clip => clip.name == "turning_right");

        strongHitBack = animationClips.FirstOrDefault(clip => clip.name == "strong_hit_back");
        strongHitFront = animationClips.FirstOrDefault(clip => clip.name == "strong_hit_front");

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

    void InitBonesReferences()
    {
      Transform[] childTransforms =  this.transform.GetComponentsInChildren<Transform>();

         kartRoot = GetChildTransform("Root_SCRIPT", childTransforms);
        ikRigRootBone = GetChildTransform("Body", childTransforms);
        drivingWheelBone = GetChildTransform("Driving_Wheel", childTransforms);
        characterSocketBone = GetChildTransform("Body", childTransforms);

        fl_Bone = GetChildTransform("Wheel_F L", childTransforms);
        fl_Hinge = GetChildTransform("DEF-Bar_F L", childTransforms);
        fr_Bone = GetChildTransform("Wheel_F R", childTransforms);
        fr_Hinge = GetChildTransform("DEF-Bar_F R", childTransforms);
        bl_Bone = GetChildTransform("Wheel_B L", childTransforms);
        bl_Hinge = GetChildTransform("DEF-Bar_B L", childTransforms);
        br_Bone = GetChildTransform("Wheel_B R", childTransforms);
        br_Hinge = GetChildTransform("DEF-Bar_B R", childTransforms);

        pipe1 = GetChildTransform("FX-Pipe L", childTransforms);
        pipe2 = GetChildTransform("FX-Pipe R", childTransforms);
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
        animOverrideController["idle_high_speed"] = idleHighSpeed;
        animOverrideController["idle_normal"] = idleNormal;

        animOverrideController["damage"] = damage;
        animOverrideController["damage_death"] = damageDeath;

        animOverrideController["turning_left"] = turnLeft;
        animOverrideController["turning_right"] = turnRight;

        animOverrideController["strong_hit_back"] = strongHitBack;
        animOverrideController["strong_hit_front"] = strongHitFront;
    }
}
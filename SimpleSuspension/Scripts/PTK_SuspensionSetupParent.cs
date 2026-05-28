using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_SuspensionSetupParent : MonoBehaviour
{


    [Header("Suspensions Parents")]
    public Transform suspensionParent_BL;
    public Transform suspensionParent_BR;
    public Transform suspensionParent_FL;
    public Transform suspensionParent_FR;


    List<PTK_SimpleSuspension> suspensions = new List<PTK_SimpleSuspension>();

    PTK_ModVehicle parentModVehicle;

    Transform vehicleBodyTiltBone;
    Transform vehicleOriginBone;
    // Start is called before the first frame update
    void Start()
    {
        suspensions.Clear();

        InitSuspensionList();

        vehicleBodyTiltBone = parentModVehicle.ikRigRootBone.transform;
        vehicleOriginBone = parentModVehicle.kartRoot.transform;

        transform.parent = vehicleOriginBone.transform.parent; // one more parent to avoid scaling animation of mesh renderers. Body target transform and wheel transform are attached inside vehicle animation so rotation and scalling will affect target positions for IK (but meshes wont be scaled because that is changing mesh orientation if element is long-tilted and scale is only on Y axis
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        CheckAndInitParentKart();

       

        InstancedMaterialConverter.Instance.Initialize(this.transform); // repalce materials with one shared material + colors

        for (int i = 0; i < suspensions.Count; i++)
            suspensions[i].bodyFixedTransfom.transform.parent = vehicleBodyTiltBone.transform;
    }

    private void OnDestroy()
    {
        if(parentKartVisual != null)
            parentKartVisual.OnKartLoaded -= OnKartInitialized;
    }

    void CheckAndInitParentKart()
    {
        if (parentKartVisual == null)
        {
            parentKartVisual = this.GetComponentInParent<Ant_VisualKart>();
            if (parentKartVisual != null)
            {
                this.GetComponentInParent<Ant_VisualKart>().OnKartLoaded += OnKartInitialized;

                if (parentKartVisual.IsKartLoaded)
                    OnKartInitialized();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckAndInitParentKart();
    }

    private void OnKartInitialized()
    {
        StartCoroutine(InitScaleDelayed());
    }


    Ant_VisualKart parentKartVisual;

    IEnumerator InitScaleDelayed()
    {
        yield return new WaitForSeconds(1.0f);
        // lets force all to use once scale, so no mirror because that breaks gpu instancing
        suspensionParent_BL.localScale = Vector3.one;
        suspensionParent_BR.localScale = Vector3.one;
        suspensionParent_FL.localScale = Vector3.one;
        suspensionParent_FR.localScale = Vector3.one;
    }

    void InitSuspensionList()
    {
        parentModVehicle = this.transform.GetComponentInParent<PTK_ModVehicle>();

        PTK_SimpleSuspension susp = null;
        if (suspensionParent_FL != null)
        {
            susp = suspensionParent_FL.GetComponentInChildren<PTK_SimpleSuspension>();
            if(susp != null)
            {
                susp.suspType = PTK_SimpleSuspension.ESuspType.E_FL;
                suspensions.Add(susp);
            }
        }

        if (suspensionParent_FR != null)
        {
            susp = suspensionParent_FR.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null)
            {
                susp.suspType = PTK_SimpleSuspension.ESuspType.E_FR;
                suspensions.Add(susp);
            }
        }

        if (suspensionParent_BL != null)
        {
            susp = suspensionParent_BL.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null)
            {
                susp.suspType = PTK_SimpleSuspension.ESuspType.E_BL;
                suspensions.Add(susp);
            }
        }

        if (suspensionParent_BR != null)
        {
            susp = suspensionParent_BR.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null)
            {
                susp.suspType = PTK_SimpleSuspension.ESuspType.E_BR;
                suspensions.Add(susp);
            }
        }

    }
    public List<PTK_SimpleSuspension> GetVehicleSuspensions()
    {
        if (suspensions.Count == 0)
            InitSuspensionList();

        return suspensions;
    }

    [EasyButtons.Button]
    public void SetSuspensionsWheelsInWheelBonesPositions()
    {
        parentModVehicle = this.transform.GetComponentInParent<PTK_ModVehicle>();
        PTK_SimpleSuspension susp = null;
        if (suspensionParent_FL != null && parentModVehicle.fl_Bone != null)
        {
            susp = suspensionParent_FL.GetComponentInChildren<PTK_SimpleSuspension>();
            if(susp != null) susp.targetDynamicWheelTransfom.transform.position = parentModVehicle.fl_Bone.position;
        }

        if (suspensionParent_FR != null && parentModVehicle.fr_Bone != null)
        {
            susp = suspensionParent_FR.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null) susp.targetDynamicWheelTransfom.transform.position = parentModVehicle.fr_Bone.position;
        }

        if (suspensionParent_BL != null && parentModVehicle.bl_Bone != null)
        {
            susp = suspensionParent_BL.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null) susp.targetDynamicWheelTransfom.transform.position = parentModVehicle.bl_Bone.position;
        }

        if (suspensionParent_BR != null && parentModVehicle.br_Bone != null)
        {
            susp = suspensionParent_BR.GetComponentInChildren<PTK_SimpleSuspension>();
            if (susp != null) susp.targetDynamicWheelTransfom.transform.position = parentModVehicle.br_Bone.position;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(parentModVehicle);
#endif
    }



}

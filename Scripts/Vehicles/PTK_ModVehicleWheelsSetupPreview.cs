using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_ModVehicleWheelsSetupPreview : MonoBehaviour
{
    public PTK_ModVehicle parentModVehicle;
    public GameObject previewParent;
    public Transform flWheel;
    public Transform frWheel;
    public Transform blWheel;
    public Transform brWheel;
    // Start is called before the first frame update
    void Awake()
    {
        previewParent.SetActive(parentModVehicle.bShowDebugMeshes == true);
    }

    // Update is called once per frame
    void Update()
    {
        if(parentModVehicle.bShowDebugMeshes == false)
        {
            if (previewParent.activeInHierarchy == true)
                previewParent.SetActive(false);
        }else
        {
            if(previewParent.activeInHierarchy == false)
                previewParent.SetActive(true);

            float fl_WheelSize = parentModVehicle.fl_WheelSize;
            float fr_WheelSize = parentModVehicle.fr_WheelSize;
            float bl_WheelSize = parentModVehicle.bl_WheelSize;
            float br_WheelSize = parentModVehicle.br_WheelSize;


            float fDefaultWheelWidthWorldSpace = 0.6122545f;
            float fHalfWidthWorlsSPace = fDefaultWheelWidthWorldSpace*0.5f;

            flWheel.transform.localScale = new Vector3(fl_WheelSize, fl_WheelSize, fl_WheelSize);
            frWheel.transform.localScale = new Vector3(fr_WheelSize, fr_WheelSize, fr_WheelSize);
            blWheel.transform.localScale = new Vector3(bl_WheelSize, bl_WheelSize, bl_WheelSize);
            brWheel.transform.localScale = new Vector3(br_WheelSize, br_WheelSize, br_WheelSize);

            if (parentModVehicle.fl_Bone != null)
                flWheel.transform.position = parentModVehicle.fl_Bone.position;
            if (parentModVehicle.fr_Bone != null)
                frWheel.transform.position = parentModVehicle.fr_Bone.position;
            if (parentModVehicle.bl_Bone != null)
                blWheel.transform.position = parentModVehicle.bl_Bone.position;
            if (parentModVehicle.br_Bone != null)
                brWheel.transform.position = parentModVehicle.br_Bone.position;


            frWheel.gameObject.SetActive(true);
            flWheel.gameObject.SetActive(true);
            brWheel.gameObject.SetActive(true);
            blWheel.gameObject.SetActive(true);

            if (parentModVehicle.eVehicleType == PTK_ModVehicle.EVehicleType.E_THREE_WHEELS_SINGLE_IN_FRONT)
            {
                frWheel.gameObject.SetActive(false);
                flWheel.transform.position += flWheel.right * fHalfWidthWorlsSPace* flWheel.transform.lossyScale.y;// move to center
            }

            if (parentModVehicle.eVehicleType == PTK_ModVehicle.EVehicleType.E_THREE_WHEELS_SINGLE_IN_BACK)
            {
                brWheel.gameObject.SetActive(false);
                blWheel.transform.position += blWheel.right * fHalfWidthWorlsSPace * blWheel.transform.lossyScale.y;// move to center
            }

            if (parentModVehicle.eVehicleType == PTK_ModVehicle.EVehicleType.E_TWO_WHEELS_FRONT_BACK)
            {
                frWheel.gameObject.SetActive(false);
                flWheel.transform.position += flWheel.right * fHalfWidthWorlsSPace * flWheel.transform.lossyScale.y;// move to center
                brWheel.gameObject.SetActive(false);
                blWheel.transform.position += blWheel.right * fHalfWidthWorlsSPace * blWheel.transform.lossyScale.y; // move to center
            }

            if (parentModVehicle.eVehicleType == PTK_ModVehicle.EVehicleType.E_TWO_WHEELS_LEFT_RIGHT)
            {
                brWheel.gameObject.SetActive(false);
                blWheel.gameObject.SetActive(false);
            }
        }
    }
}

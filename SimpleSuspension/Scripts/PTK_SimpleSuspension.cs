using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_SimpleSuspension : MonoBehaviour
{
    public enum ESuspType
    {
        E_BL,
        E_BR,
        E_FL,
        E_FR
    }

    [HideInInspector]
    public ESuspType suspType = ESuspType.E_FL;

    public PTK_SuspensionWheelPivot targetDynamicWheelTransfom;
    public Transform bodyFixedTransfom;

    public GameObject[] helperMeshesToHide;

    // Start is called before the first frame update
    void Awake()
    {
        Collider[] colliders = this.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        if (helperMeshesToHide != null)
        {
            for(int i=0;i< helperMeshesToHide.Length;i++)
            {
                if (helperMeshesToHide[i] != null)
                    helperMeshesToHide[i].SetActive(false);
            }
        }

        PTK_ModVehicleInfo.HideSuspensionDebugVisuals(transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetWheelSize(float fWheelWidth)
    {
        targetDynamicWheelTransfom.outerSideWheelPivotTransform.localPosition = new Vector3(0.0f, fWheelWidth * 0.5f, 0.0f);
        targetDynamicWheelTransfom.innerSideWheelPivotTransform.localPosition = new Vector3(0.0f, -fWheelWidth * 0.5f, 0.0f);
    }
}

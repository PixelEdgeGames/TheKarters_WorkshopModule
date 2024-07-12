using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_CheckpointLogic : MonoBehaviour
{
    [Header("Main Logic Plane")]
    [SerializeField]
    bool bShowCheckpointPlaneMesh = false;
    public GameObject checkpointRangePlane;

    [Header("Settings")]
    [Range(5, 300)]
    [SerializeField]
    float fCheckpointWidth = 50;
    [SerializeField]
    bool bShowMesh_L = true;
    [SerializeField]
    bool bShowMesh_R = true;
    [Header("Extra Settings")]
    [Range(0, 30)]
    [SerializeField]
    public float fExtraBottomHeight = 10.0f;
 

    [Header("Visual")]
    [SerializeField]
    GameObject checkpointModelParent;
    [SerializeField]
    MeshRenderer checkpointRangePlaneMeshRenderer;
    [SerializeField]
    Material checkpointMat1;
    [SerializeField]
    Material checkpointMat2;
    [SerializeField]
    Material checkpointMat3;
    [SerializeField]
    MeshRenderer checkpointModel_L;
    [SerializeField]
    MeshRenderer checkpointModel_R;

    // Start is called before the first frame update
    void Start()
    {
        InitializeAndSetupCheckpoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying == false)
            InitializeAndSetupCheckpoint();
    }

    float GetCheckpointConstantHeight()
    {
        return 25.0f;
    }

    void InitializeAndSetupCheckpoint()
    {
        transform.localScale = Vector3.one;
        transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, transform.eulerAngles.z);

        SetupMainCheckpointLogicPlane();

        Setup3DModelMeshes();
    }

    private void SetupMainCheckpointLogicPlane()
    {
        checkpointRangePlane.transform.localScale = new Vector3(fCheckpointWidth, GetCheckpointConstantHeight() + fExtraBottomHeight, 0.1f);

        checkpointRangePlane.transform.localPosition = new Vector3(0.0f, -fExtraBottomHeight * 0.5f, 0.0f);
        checkpointRangePlane.transform.localRotation = Quaternion.identity;
    }

    private void Setup3DModelMeshes()
    {
        int iShowAsNumber = 1;

        Transform checkpointsParent = transform.parent.parent;
        for (int iCheckpointNr=0;iCheckpointNr< checkpointsParent.childCount;iCheckpointNr++)
        {
          if(transform.parent == checkpointsParent.GetChild(iCheckpointNr))
            {
                iShowAsNumber =( iCheckpointNr+1);
                break;
            }
        }

        float fModelSideOffset = 1.5f;
        float fModelHeightOffset = 1.0f;
        checkpointModel_L.transform.position = transform.position - transform.right * fCheckpointWidth * 0.5f - transform.up * GetCheckpointConstantHeight() * 0.5F - transform.up * fModelHeightOffset + transform.right * fModelSideOffset;
        checkpointModel_L.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 5.0f);

        checkpointModel_R.transform.position = transform.position + transform.right * fCheckpointWidth * 0.5f - transform.up * GetCheckpointConstantHeight() * 0.5F - transform.up * fModelHeightOffset - transform.right * fModelSideOffset;
        checkpointModel_R.transform.localRotation = Quaternion.Euler(0.0f, 0, 5.0f);

        // during gameplay always disable
        if (Application.isPlaying == true)
            bShowCheckpointPlaneMesh = false;

        checkpointRangePlaneMeshRenderer.enabled = bShowCheckpointPlaneMesh;

        switch (iShowAsNumber)
        {
            case 1:
                checkpointModel_L.material = checkpointMat1;
                checkpointModel_R.material = checkpointMat1;
                break;
            case 2:
                checkpointModel_L.material = checkpointMat2;
                checkpointModel_R.material = checkpointMat2;
                break;
            case 3:
                checkpointModel_L.material = checkpointMat3;
                checkpointModel_R.material = checkpointMat3;
                break;
        }

        checkpointModel_L.enabled = bShowMesh_L;
        checkpointModel_R.enabled = bShowMesh_R;
    }


    [EasyButtons.Button]
    void AlignToGround()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position + Vector3.up*20.0f,-Vector3.up,out hit,300))
        {
            transform.position = hit.point + Vector3.up* GetCheckpointConstantHeight() * 0.5F - Vector3.up*2.0f;
        }
    }
}

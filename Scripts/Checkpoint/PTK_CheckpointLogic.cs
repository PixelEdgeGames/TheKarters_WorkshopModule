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
    [Header("Extra Size")]
    [Range(0, 200)]
    [SerializeField]
    public float fExtraSides_Width = 5.0f;
    [Range(0, 200)]
    [SerializeField]
    public float fExtraTop_Height = 200.0f;
    [Range(0, 200)]
    [SerializeField]
    public float fExtraBottom_Height =200.0f;

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
    Material checkpointPlaneMat;
    [SerializeField]
    Material timeDiffOnlyPlaneMat;
    [SerializeField]
    MeshRenderer checkpointModel_L;
    [SerializeField]
    MeshRenderer checkpointModel_R;

    public GameObject forwardParentDebugMesh;

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

        SetupMainCheckpointLogicPlane();

        Setup3DModelMeshes();
    }

    private void SetupMainCheckpointLogicPlane()
    {
        checkpointRangePlane.transform.localScale = new Vector3(fCheckpointWidth + fExtraSides_Width * 2, GetCheckpointConstantHeight() + fExtraBottom_Height + fExtraTop_Height, 0.1f);
        checkpointRangePlane.transform.localPosition = new Vector3(0.0f, (fExtraTop_Height - fExtraBottom_Height) * 0.5f, 0.0f);
        checkpointRangePlane.transform.localRotation = Quaternion.identity;
    }

    PTK_CheckpointParent checkpointParent;
    private void Setup3DModelMeshes()
    {
        int iShowAsNumber = 1;

        if(checkpointParent == null)
        {
            checkpointParent = this.GetComponentInParent<PTK_CheckpointParent>();
        }

        if (checkpointParent != null)
        {
            if (transform.parent == checkpointParent.parentCheckpoint_0)
                iShowAsNumber = 1;
            else if (transform.parent == checkpointParent.parentCheckpoint_1)
                iShowAsNumber = 2;
            else if (transform.parent == checkpointParent.parentCheckpoint_2)
                iShowAsNumber = 3;
            else if (transform.parent == checkpointParent.parentCheckpoint_TimeOnly)
                iShowAsNumber = -1;
            else
                Debug.LogError("Checkpoint type not found");
        }

        bool bIsTimeDiffOnlyCheckpoint = iShowAsNumber == -1;


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

        forwardParentDebugMesh.SetActive(bShowCheckpointPlaneMesh);

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
            case -1:
                break;
        }

        if(bIsTimeDiffOnlyCheckpoint == false)
        {
            if (bShowMesh_L == false && bShowMesh_R == false)
            {
                if (checkpointModel_L.enabled == true) // do not allow to disable both sides
                    bShowMesh_L = true;
                else
                    bShowMesh_R = true;
            }

            checkpointModel_L.enabled = bShowMesh_L;
            checkpointModel_R.enabled = bShowMesh_R;

            checkpointRangePlaneMeshRenderer.sharedMaterial = checkpointPlaneMat;
        }
        else
        {
            checkpointModel_L.enabled = false;
            checkpointModel_R.enabled = false;

            checkpointRangePlaneMeshRenderer.sharedMaterial = timeDiffOnlyPlaneMat;
        }
    }

    [EasyButtons.Button]
    void AlignToGround()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 20.0f, -Vector3.up, out hit, 300))
        {
            transform.position = hit.point + Vector3.up * GetCheckpointConstantHeight() * 0.5F - Vector3.up * 2.0f;
        }
    }
}

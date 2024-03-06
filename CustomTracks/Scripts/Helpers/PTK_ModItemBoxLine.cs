using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModItemBoxLine : MonoBehaviour
{
    [SerializeField]
     GameObject[] itemBoxes;
    // Start is called before the first frame update
    void Awake()
    {
        debugPreviewMesh.enabled = false;
    }
    public float fSeperationDistance = 10.0f;
    public MeshRenderer debugPreviewMesh;
    // Update is called once per frame
    void Update()
    {

    }

    public List<GameObject> GetActiveItemBoxes()
    {
        List<GameObject> activeItemBoxes = new List<GameObject>();
        for (int i = 0; i < itemBoxes.Length; i++)
        {
            if (itemBoxes[i].activeInHierarchy == true)
                activeItemBoxes.Add(itemBoxes[i]);
        }

        return activeItemBoxes;
    }

    [EasyButtons.Button]
    public void RefreshItemBoxesPositions()
    {

        List<GameObject> activeItemBoxes = GetActiveItemBoxes();

        int iItemBoxIndex = 0;
        float fCurrentSeperationIndex = 1;
        if (activeItemBoxes.Count % 2 == 1)
        {
            // nieparzyscie, jeden w srodku
            SetItemBoxPosition(activeItemBoxes[0], transform.position);
            iItemBoxIndex = 1;
        }
        else
        {
            fCurrentSeperationIndex = 0.5f;
        }

        for (; iItemBoxIndex < activeItemBoxes.Count;)
        {
            SetItemBoxPosition(activeItemBoxes[iItemBoxIndex], transform.position + transform.right * fSeperationDistance * fCurrentSeperationIndex); iItemBoxIndex++;

            if (iItemBoxIndex < activeItemBoxes.Count)
                SetItemBoxPosition(activeItemBoxes[iItemBoxIndex], transform.position - transform.right * fSeperationDistance * fCurrentSeperationIndex); iItemBoxIndex++;

            fCurrentSeperationIndex++;
        }
    }


    float fHeightOffset = 2.8f;
    void SetItemBoxPosition(GameObject box, Vector3 position)
    {
        box.transform.position = position;
        box.transform.up = (transform.forward + transform.right).normalized;


        RaycastHit hit;

        if (Physics.Raycast(box.transform.position + Vector3.up * 10.0f, Vector3.down, out hit, 999))
        {
            box.transform.rotation = Quaternion.LookRotation(hit.normal, box.transform.up);

            box.transform.position = hit.point + hit.normal * fHeightOffset;
        }
    }
}

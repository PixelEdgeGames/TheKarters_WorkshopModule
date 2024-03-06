using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_AlignQuadToGround : MonoBehaviour
{
    public Transform[] raycastPoints;
    public LayerMask groundLayerCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    static Plane planeFrom3Points = new Plane();
    private void OnEnable()
    {
    }


    [EasyButtons.Button]
    public void AlignToGround()
    {
        Vector3 pointA = new Vector3(0.0f, -9999.0f, 0.0f);
        Vector3 pointB = new Vector3(1.0f, -9999.0f, 0.0f);
        Vector3 pointC = new Vector3(0.0f, -9999.0f, 1.0f);
        int iSucessHitsCount = 0;

        RaycastHit hit;
        int iLowestPointIndex = 0;

        for (int i = 0; i < raycastPoints.Length; i++)
        {
            if (Physics.Raycast(raycastPoints[i].position + Vector3.up * 10.0f, Vector3.down, out hit, 999, groundLayerCollider))
            {
                if (iSucessHitsCount == 0)
                    pointA = hit.point;
                else if (iSucessHitsCount == 1)
                    pointB = hit.point;
                else if (iSucessHitsCount == 2)
                    pointC = hit.point;
                else
                {
                    switch (iLowestPointIndex)
                    {
                        case 0:
                            if (hit.point.y > pointA.y)
                                pointA = hit.point;
                            break;
                        case 1:
                            if (hit.point.y > pointB.y)
                                pointB = hit.point;
                            break;
                        case 2:
                            if (hit.point.y > pointC.y)
                                pointC = hit.point;
                            break;
                    }
                }

                if (pointA.y < pointB.y && pointA.y < pointC.y)
                    iLowestPointIndex = 0;
                if (pointB.y < pointA.y && pointB.y < pointC.y)
                    iLowestPointIndex = 1;
                if (pointC.y < pointB.y && pointC.y < pointA.y)
                    iLowestPointIndex = 2;

                iSucessHitsCount++;
            }
        }

        planeFrom3Points.Set3Points(pointA, pointB, pointC);
        Vector3 vNormalUp = (Vector3.Dot(planeFrom3Points.normal, Vector3.up) < 0.0f ? -planeFrom3Points.normal : planeFrom3Points.normal);
        transform.rotation = Quaternion.LookRotation(Vector3.Cross( transform.right, vNormalUp).normalized, vNormalUp);
     //   transform.up = -planeFrom3Points.normal;
        float fTargetHeight = transform.position.y;
        if ((pointA - pointB).magnitude > (pointB - pointC).magnitude && (pointA - pointB).magnitude > (pointC - pointA).magnitude)
        {
            fTargetHeight = Vector3.Lerp(pointA, pointB, 0.5f).y;
        }
        else if ((pointB - pointC).magnitude > (pointA - pointB).magnitude && (pointB - pointC).magnitude > (pointC - pointA).magnitude)
        {
            fTargetHeight = Vector3.Lerp(pointB, pointC, 0.5f).y;
        }
        else if ((pointC - pointA).magnitude > (pointA - pointB).magnitude && (pointC - pointA).magnitude > (pointB - pointC).magnitude)
        {
            fTargetHeight = Vector3.Lerp(pointC, pointA, 0.5f).y;
        }

        transform.position = new Vector3(transform.position.x, fTargetHeight, transform.position.z) ;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

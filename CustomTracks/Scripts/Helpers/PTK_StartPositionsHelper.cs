using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_StartPositionsHelper : MonoBehaviour
{
    public enum ETurnType
    {
        E_RIGHT,
        E_LEFT
    }

    [Header("First Turn Setting")]
    public ETurnType eRaceTrackTurnDirectionAfterFinishLine = ETurnType.E_RIGHT;
    [Header("Start Orientation - LookAt Origin")]
    [Range(0, 90)]
    public float fLookAt_DistX = 0.0f;
    [Range(60, 150)]
    public float fLookAt_DistZ = 60.0f;

    [Header("Distances in Row")]
    [Range(10,60)]
    public float fSeperationX = 10.0f;
    [Range(10, 60)]
    public float fSeperationZ = 10.0f;

    [Header("Front-Back Distances")]
    [Range(60, 90)]
    public float fDistanceBetweenFrontAndBackStartPositions = 30.0f;


    [Header("Setup")]
    public Transform[] startPositions;
    public Transform[] startPositionsOrigins;
    float[] groundHitRollAngle = new float[8];
    float[] groundHiPitchAngle = new float[8];
    public Transform orientTowardsPoint;
    public Transform previewInitialDir;


    [Header("Auto Align Ground")]
    public bool bAutoRaycastGroundToAlign = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

#if UNITY_EDITOR
    // Update is called once per frame
    public void Update()
    {
        // do not edit during playmode
        if (Application.isPlaying == true)
            return;


        float fTurnMultiplier = eRaceTrackTurnDirectionAfterFinishLine == ETurnType.E_RIGHT ? 1.0f : -1.0f;

        Vector3 vTurnRightPosOffset = eRaceTrackTurnDirectionAfterFinishLine == ETurnType.E_RIGHT ? (-Vector3.right* fSeperationX * 3.5F) : Vector3.zero;

        orientTowardsPoint.transform.localPosition = Quaternion.Euler(0.0f, fLookAt_DistX * fTurnMultiplier, 0.0f)* new Vector3(fSeperationX * 1.5F * fTurnMultiplier, 0.0f, fLookAt_DistZ) + vTurnRightPosOffset;

        // lock preview pos/rot
        previewInitialDir.transform.localPosition = Vector3.zero;
        previewInitialDir.transform.localRotation = Quaternion.identity;



        // first row
        float fXPosition = 0.0f;
        float fZPosition = 0.0f;
        for (int i = 0; i < 4; i++)
        {
            startPositions[i].transform.localPosition = new Vector3(fXPosition* fTurnMultiplier, startPositions[i].transform.localPosition.y, -fZPosition ) + vTurnRightPosOffset;

            fXPosition += fSeperationX;
            fZPosition += fSeperationZ;
        }

        // second row
        fXPosition = fSeperationX*0.5f; // half X seperation initial offset
        fZPosition = 0.0f;
        for (int i = 4; i < startPositions.Length; i++)
        {
            startPositions[i].transform.localPosition = new Vector3(fXPosition * fTurnMultiplier, startPositions[i].transform.localPosition.y, -fZPosition ) + vTurnRightPosOffset;

            fXPosition += fSeperationX;
            fZPosition += fSeperationZ;

            // 2nd row - add distance between back and front
            startPositions[i].transform.localPosition -= new Vector3(0.0f, 0.0f, fDistanceBetweenFrontAndBackStartPositions);
        }

        RefreshDirection();

        if(bAutoRaycastGroundToAlign == true)
            AlignPointsToGround();
    }

#endif

    void RefreshDirection()
    {
        // start position direction
        for (int i = 0; i < startPositions.Length; i++)
        {
            Vector3 vForward = orientTowardsPoint.transform.position - startPositions[i].position;
            vForward.y = 0.0f; vForward.Normalize();

            startPositions[i].forward = vForward;
            startPositions[i].eulerAngles = new Vector3(groundHiPitchAngle[i], startPositions[i].eulerAngles.y, groundHitRollAngle[i]);

            startPositionsOrigins[i].forward = transform.forward;
            startPositionsOrigins[i].eulerAngles = new Vector3(groundHiPitchAngle[i], 0, groundHitRollAngle[i]);
        }
    }

    [EasyButtons.Button]
    public void AlignPointsToGround()
    {
        // reset to origin
        for (int i = 0; i < startPositions.Length; i++)
        {
            startPositionsOrigins[i].transform.localPosition = Vector3.zero;
            startPositions[i].transform.localPosition = new Vector3(startPositions[i].transform.localPosition.x, 0.0F, startPositions[i].transform.localPosition.z);
        }

        // start position direction
        for (int i = 0; i < startPositions.Length; i++)
        {
            startPositionsOrigins[i].transform.localPosition = Vector3.zero;
            startPositions[i].transform.localPosition = new Vector3(startPositions[i].transform.localPosition.x,0.0F, startPositions[i].transform.localPosition.z);

            RaycastHit hitInfo;
            if (RaycastQuad(startPositions[i].transform.position + Vector3.up*5.0F,Vector3.down,1,out hitInfo) == true)
            {
                startPositions[i].transform.position = new Vector3(startPositions[i].transform.position.x, hitInfo.point.y, startPositions[i].transform.position.z);

                Vector3 originalUp = startPositions[i].transform.up;

                startPositions[i].transform.up = hitInfo.normal;

                // Calculate pitch angle
                groundHiPitchAngle[i] = startPositions[i].transform.eulerAngles.x;

                // Calculate roll angle
                groundHitRollAngle[i] = startPositions[i].transform.eulerAngles.z;

                startPositions[i].transform.up = originalUp;

            }
            else
            {
                groundHitRollAngle[i] = 0;
                groundHiPitchAngle[i] = 0;
            }
        }


        RefreshDirection();
    }

    bool RaycastQuad(Vector3 centerPosition,Vector3 vDir, float size,out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        int hitsCount = 0;
        Vector3 totalNormal = Vector3.zero;
        Vector3 totalPos = Vector3.zero;

        // Directions from center to each corner and center
        Vector3[] directions = new Vector3[] {
        Vector3.zero, // Center
        new Vector3(-size, 0, size), // Top-Left
        new Vector3(size, 0, size), // Top-Right
        new Vector3(-size, 0, -size), // Bottom-Left
        new Vector3(size, 0, -size) // Bottom-Right
    };

        RaycastHit hitInfoQuad = new RaycastHit();
        foreach (Vector3 direction in directions)
        {
            // Calculate the ray's starting position
            Vector3 rayStart = centerPosition + direction ;

            if (Physics.Raycast(rayStart, vDir, out hitInfoQuad, Mathf.Infinity))
            {
                totalNormal += hitInfoQuad.normal;
                totalPos += hitInfoQuad.point;
                hitsCount++;
            }
        }

        if (hitsCount > 0)
        {
            hitInfo.normal = totalNormal / hitsCount; // Return the average normal
            hitInfo.point = totalPos / hitsCount;
            return true;
        }
        else
        {
            return false; // No hits, return null
        }
    }
}

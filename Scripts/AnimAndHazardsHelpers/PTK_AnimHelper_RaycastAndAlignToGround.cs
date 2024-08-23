using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_AnimHelper_RaycastAndAlignToGround : MonoBehaviour
{
    [Header("Use while creating Animation Clip to align to ground")]
    public float fAlignHeightOffset = 0.0f;

    [EasyButtons.Button]
    public void RaycastAndMoveToGround()
    {
        RaycastHit hit;
        if(Physics.Raycast(new Ray(transform.position,-Vector3.up),out hit,9999))
        {
            Vector3 vNewPos = transform.position;
            vNewPos.y = hit.point.y + fAlignHeightOffset;
            transform.position = vNewPos;
        }
    }

    [EasyButtons.Button]
    public void RaycastAndAlignRotationToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, -Vector3.up), out hit, 9999))
        {
            Vector3 groundNormal = hit.normal;
            Vector3 forward = transform.forward;

            // Calculate the right vector
            Vector3 right = Vector3.Cross(forward, groundNormal).normalized;

            // Recalculate forward to ensure it's perpendicular to the ground normal
            forward = Vector3.Cross(groundNormal, right).normalized;

            // Create a rotation that aligns with the ground normal and maintains forward direction
            Quaternion targetRotation = Quaternion.LookRotation(forward, groundNormal);

            // Apply the rotation
            transform.rotation = targetRotation;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_SuspensionElementLogic_AttachToPoint : PTK_SuspensionElementLogic_Base
{
    [Header("Offset based on other element orientation")]
    public bool bOffsetBasedOnDirectionBetweenPoints = false;
    public Transform lineStartPos;
    public Transform lineEndPos;

    // Start is called before the first frame update
    void Start()
    {
    }

  

    // Update is called once per frame
    void Update()
    {
        if (fixedAttachedToPoint == null)
            return;

        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset );

        Quaternion rotToUse = fixedAttachedToPoint.rotation;

        if(bOffsetBasedOnDirectionBetweenPoints == true)
        {
            rotToUse = Quaternion.LookRotation(lineEndPos.position - lineStartPos.position);
        }

        Vector3 vFixedPointPos = fixedAttachedToPoint.position + rotToUse * (fixedPointWorldOffset);
        transform.position = vFixedPointPos;
    }
}

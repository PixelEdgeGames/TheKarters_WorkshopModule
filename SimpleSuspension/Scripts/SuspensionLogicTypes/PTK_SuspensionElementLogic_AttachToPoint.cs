using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_SuspensionElementLogic_AttachToPoint : PTK_SuspensionElementLogic_Base
{
    [Header("Offset based on other element orientation")]
    public bool bOffsetBasedOnDirectionBetweenPoints = false;

    // helpfull when the elements is on line and this line is rotating during gameplay. It will use up from this line
    public bool bOrientationBasedOnStartEndPos = false;
    public Vector3 vEulerAngleAdd = Vector3.zero;

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

        if(bOrientationBasedOnStartEndPos == true)
        {
            Vector3 vDir = (lineEndPos.position - lineStartPos.position).normalized;
            Quaternion qRotLine = Quaternion.LookRotation(vDir) * Quaternion.Euler(vEulerAngleAdd);
            transform.rotation = qRotLine;
        }
    }
}

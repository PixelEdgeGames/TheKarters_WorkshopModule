using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_SuspensionElementFixedPos : MonoBehaviour
{
    public Vector3 fixedPointLocalOffset;
    public Transform fixedAttachedToPoint; // which point should not move / move a little based on distance

    public Vector3 localOrientation;
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
        Vector3 vFixedPointPos = fixedAttachedToPoint.position + fixedAttachedToPoint.rotation * (fixedPointWorldOffset);
        transform.position = vFixedPointPos;
        transform.rotation = fixedAttachedToPoint.rotation * Quaternion.Euler(localOrientation);
    }
}

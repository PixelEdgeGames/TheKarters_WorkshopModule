using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(PTK_SimpleSuspensionElement))]
public class PTK_SuspensionElementLogic_StretchOnly : PTK_SuspensionElementLogic_Base
{
    public PTK_SimpleSuspensionElement suspensionElement;
    [Header("Dynamic (end) Point")]
    public Vector3 stretchDistance;


    Vector3 vFixedPointPos;
    Vector3 vTargetPointPos;

    public void Update()
    {
        if (suspensionElement == null)
        {
            suspensionElement = this.GetComponent<PTK_SimpleSuspensionElement>();

            if (suspensionElement == null)
                return;
        }

        if (fixedAttachedToPoint == null)
            return;



        Quaternion qRotFixed = fixedAttachedToPoint.rotation;// Quaternion.LookRotation(vDir);

        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset);

        vFixedPointPos = fixedAttachedToPoint.position + qRotFixed * (fixedPointWorldOffset);
        vTargetPointPos = fixedAttachedToPoint.position + qRotFixed * (stretchDistance);

        StretchElement();

        // set position in origin point (pivot is in our position, so scaling will expand it in the direction)
        transform.position = vFixedPointPos;

    }

    private void StretchElement()
    {
        float fDistanceToTargetPoint = Vector3.Magnitude(vFixedPointPos - vTargetPointPos);

        float fTargetScale = fDistanceToTargetPoint / suspensionElement.fOriginalSizeZ;

        fTargetScale = fTargetScale * suspensionElement.fOriginalScaleZ;

        fTargetScale = Mathf.Max(fTargetScale, 0.1f);


        transform.localScale = new Vector3(1.0f, 1.0f, fTargetScale);
    }


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Selection.activeGameObject != gameObject)
            return;

        if (fixedAttachedToPoint == null)
            return;



        Vector3 fixedAttachedPointWorld = vFixedPointPos;
        Vector3 dynamicTargetPointWorld = vTargetPointPos;

        // Draw spheres using Handles
        Handles.color = Color.red;
        Handles.SphereHandleCap(0, dynamicTargetPointWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        Handles.color = Color.green;
        Handles.SphereHandleCap(0, fixedAttachedPointWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        // Draw line using Handles
        Handles.DrawBezier(dynamicTargetPointWorld, fixedAttachedPointWorld, dynamicTargetPointWorld, fixedAttachedPointWorld, Color.yellow, null, 5f);

    }
#endif
}

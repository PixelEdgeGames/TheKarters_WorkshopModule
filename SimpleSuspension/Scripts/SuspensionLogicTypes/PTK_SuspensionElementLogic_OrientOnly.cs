using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(PTK_SimpleSuspensionElement))]
public class PTK_SuspensionElementLogic_OrientOnly : PTK_SuspensionElementLogic_Base
{
    public PTK_SimpleSuspensionElement suspensionElement;
    [Header("Dynamic (end) Point")]
    public Transform dynamicTargetPoint; // wheel or other moving element
    public Vector3 dynamicPointLocalOffset;


    [Header("Orientation")]
    public float fOrientStrength = 1.0f;

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

        if (dynamicTargetPoint == null)
            return;

        Vector3 vDir = (dynamicTargetPoint.position - fixedAttachedToPoint.position).normalized;

        Quaternion qRotDynamic = dynamicTargetPoint.rotation;// Quaternion.LookRotation(vDir);
        Quaternion qRotFixed = fixedAttachedToPoint.rotation;// Quaternion.LookRotation(vDir);

        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset);
        Vector3 dynamicPointWorldOffset = Vector3.Scale(dynamicTargetPoint.lossyScale, dynamicPointLocalOffset);

        vFixedPointPos = fixedAttachedToPoint.position + qRotFixed * (fixedPointWorldOffset);
        vTargetPointPos = dynamicTargetPoint.position + qRotDynamic * (dynamicPointWorldOffset);

        

        // set position in origin point (pivot is in our position, so scaling will expand it in the direction)
        transform.position = vFixedPointPos;

        transform.LookAt(vTargetPointPos); transform.localRotation = Quaternion.Lerp(suspensionElement.originalLocalRotation, transform.localRotation, fOrientStrength);
    }


#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (Selection.activeGameObject != gameObject)
            return;

        if (fixedAttachedToPoint == null)
            return;

        if (dynamicTargetPoint == null)
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

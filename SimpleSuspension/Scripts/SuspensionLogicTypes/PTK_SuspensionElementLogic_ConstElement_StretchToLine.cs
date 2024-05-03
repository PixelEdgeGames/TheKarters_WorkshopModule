using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(PTK_SimpleSuspensionElement))]
public class PTK_SuspensionElementLogic_ConstElement_StretchToLine : PTK_SuspensionElementLogic_Base
{
    public PTK_SimpleSuspensionElement suspensionElement;
  
    Vector3 vFixedPointPos;
    Vector3 vTargetPointPos;

    [Header("Stretch to line, keep const pitch angle")]
    public Transform stretchToTargetLine_StartPoint;
    public Transform stretchToTargetLine_EndPoint;
    [Header("Angle Offset")]
    public float fElementDirectionTargetAngle;
    [Header("LocalRot Multiplier")]
    public float localRotationChangeStrengthMultiplier = 1.0f; // if fixed attachement point rotation will change - this will increase effect of this rotation so element will rotate even more. Good if element is attached to vehicle body - it will increase tilt strength (
    [Header("Limits between line")]
    public float fHowCloseWeCanMoveOnLineTo_StartPoint = 0.2f; // how close we can get to start/end point
    public float fHowCloseWeCanMoveOnLineTo_EndPoint = 0.2f;


    float fLineIntersectionDistFromOriginDist;
    Vector3 vIntersectionLineUp;
    Vector3 vIntersecitonLineForward;

    public  void Update()
    {
        if (suspensionElement == null)
        {
            suspensionElement = this.GetComponent<PTK_SimpleSuspensionElement>();

            if (suspensionElement == null)
                return;
        }

        if (stretchToTargetLine_StartPoint == null)
            return;

        if (stretchToTargetLine_EndPoint == null)
            return;

        Quaternion qRotFixed = fixedAttachedToPoint.rotation;

        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset);

        vFixedPointPos = fixedAttachedToPoint.position + qRotFixed * (fixedPointWorldOffset);

        CalculatePointOnTargetLine();

        StretchElement();

        // set position in origin point (pivot is in our position, so scaling will expand it in the direction)
        transform.position = vFixedPointPos;

        transform.LookAt(vTargetPointPos);
    }

    // it will keep the orientation constant - it will just calculate how long element needs to be to reach the target element
    private void CalculatePointOnTargetLine()
    {
        Vector3 vIntersectionLineDirFromStartToEndDir = (stretchToTargetLine_EndPoint.position - stretchToTargetLine_StartPoint.position).normalized;

        vIntersectionLineUp = vIntersectionLineDirFromStartToEndDir;
        vIntersecitonLineForward = (vFixedPointPos - stretchToTargetLine_StartPoint.position).normalized;
        Vector3 vRightCross = Vector3.Cross(vIntersectionLineUp, vIntersecitonLineForward).normalized;
        vIntersecitonLineForward = Vector3.Cross(vRightCross, vIntersectionLineUp).normalized;


        Vector3 vDirFromOriginToTarget = (stretchToTargetLine_StartPoint.position - vFixedPointPos).normalized; vDirFromOriginToTarget.y = 0.0f; vDirFromOriginToTarget.Normalize();
        vDirFromOriginToTarget = Quaternion.AngleAxis(fElementDirectionTargetAngle - fixedAttachedToPoint.eulerAngles.x * localRotationChangeStrengthMultiplier, Vector3.Cross(vDirFromOriginToTarget, Vector3.up).normalized) * vDirFromOriginToTarget;
        Ray ray = new Ray(vFixedPointPos, vDirFromOriginToTarget);

        Plane plane = new Plane(vIntersecitonLineForward, stretchToTargetLine_StartPoint.position);
        fLineIntersectionDistFromOriginDist = 0;
        plane.Raycast(ray, out fLineIntersectionDistFromOriginDist);

        vTargetPointPos = vFixedPointPos + ray.direction * fLineIntersectionDistFromOriginDist;

        // is outside of line start/end
        LimitTargetPositionOnLineStartEnd();
    }

    private void LimitTargetPositionOnLineStartEnd()
    {
        Vector3 directionToEnd = (stretchToTargetLine_EndPoint.position - stretchToTargetLine_StartPoint.position).normalized;
        Vector3 vTargetPointOffsetFromStart = vTargetPointPos - stretchToTargetLine_StartPoint.position;
        float fLengthWithinLimits = Vector3.Dot(vTargetPointOffsetFromStart, directionToEnd);

        // Define limits 
        float minLimit = fHowCloseWeCanMoveOnLineTo_StartPoint* fixedAttachedToPoint.lossyScale.x;
        float maxLimit = Vector3.Distance(stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position) - fHowCloseWeCanMoveOnLineTo_EndPoint* fixedAttachedToPoint.lossyScale.x;



        // Clamp the  length to ensure it remains within the desired bounds
        fLengthWithinLimits = Mathf.Clamp(fLengthWithinLimits, minLimit, maxLimit);

        // Adjust the target position within the limits
        vTargetPointPos = stretchToTargetLine_StartPoint.position + directionToEnd * fLengthWithinLimits;

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

        if (stretchToTargetLine_StartPoint != null && stretchToTargetLine_EndPoint != null)
        {
            Handles.DrawBezier(stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position  , stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position, Color.green, null, 5f);


            Vector3 vDirFromOriginToTarget = (vTargetPointPos - vFixedPointPos).normalized;
            Handles.DrawBezier(vFixedPointPos, vFixedPointPos + vDirFromOriginToTarget * fLineIntersectionDistFromOriginDist, vFixedPointPos, vFixedPointPos + vDirFromOriginToTarget * fLineIntersectionDistFromOriginDist, Color.red, null, 5f);

        }

    }
#endif
}

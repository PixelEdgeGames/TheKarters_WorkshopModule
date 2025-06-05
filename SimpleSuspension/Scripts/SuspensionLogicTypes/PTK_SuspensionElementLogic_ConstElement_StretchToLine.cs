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

    [Header("Performance Mode")]
    public bool bUseSimple = false; // Simple mode for better performance

    [Header("Stretch to line, keep const pitch angle")]
    public Transform stretchToTargetLine_StartPoint;
    public Transform stretchToTargetLine_EndPoint;
    [Header("Angle Offset")]
    public float fElementDirectionTargetAngle;
    [Header("LocalRot Multiplier")]
    public float localRotationChangeStrengthMultiplier = 2.0f;
    [Header("Limits between line")]
    public float fHowCloseWeCanMoveOnLineTo_StartPoint = 0.2f;
    public float fHowCloseWeCanMoveOnLineTo_EndPoint = 0.2f;

    // Simple mode cached values
    Vector3 vSimpleTargetDirection;
    float fSimpleTargetDistance;

    float fLineIntersectionDistFromOriginDist;
    Vector3 vIntersectionLineUp;
    Vector3 vIntersecitonLineForward;

    private void Start()
    {
        bUseSimple = true;
    }

    public void Update()
    {
        if (suspensionElement == null)
        {
            suspensionElement = this.GetComponent<PTK_SimpleSuspensionElement>();
            if (suspensionElement == null)
                return;
        }

        if (stretchToTargetLine_StartPoint == null || stretchToTargetLine_EndPoint == null)
            return;

        Quaternion qRotFixed = fixedAttachedToPoint.rotation;
        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset);
        vFixedPointPos = fixedAttachedToPoint.position + qRotFixed * (fixedPointWorldOffset);

        if (bUseSimple)
        {
            CalculatePointOnTargetLine_Simple();
        }
        else
        {
            CalculatePointOnTargetLine();
        }

        StretchElement();

        // set position in origin point
        transform.position = vFixedPointPos;
        transform.LookAt(vTargetPointPos);
    }

    public float fAngleCurrent = 0.0f;

    // Simple mode that closely matches original behavior with better performance
    private void CalculatePointOnTargetLine_Simple()
    {
        // Follow original logic but with optimized operations
        Vector3 vDirFromOriginToTarget = (stretchToTargetLine_StartPoint.position - vFixedPointPos).normalized;
        vDirFromOriginToTarget.y = 0.0f;
        vDirFromOriginToTarget.Normalize();

        // Apply same angle calculation as original
        fAngleCurrent = fElementDirectionTargetAngle - fixedAttachedToPoint.eulerAngles.x * localRotationChangeStrengthMultiplier;

        // Simplified rotation - use cross with up vector like original but skip normalization steps
        Vector3 rotationAxis = Vector3.Cross(vDirFromOriginToTarget, Vector3.up);
        if (rotationAxis.magnitude > 0.001f) // Avoid issues when parallel to up
        {
            vDirFromOriginToTarget = Quaternion.AngleAxis(fAngleCurrent, rotationAxis.normalized) * vDirFromOriginToTarget;
        }

        // Create simplified plane normal (skip the complex coordinate system setup)
        Vector3 vIntersectionLineDirFromStartToEndDir = (stretchToTargetLine_EndPoint.position - stretchToTargetLine_StartPoint.position).normalized;
        Vector3 vSimplePlaneNormal = (vFixedPointPos - stretchToTargetLine_StartPoint.position).normalized;

        // Direct plane-ray intersection math (replaces Plane.Raycast for better performance)
        float planeD = Vector3.Dot(vSimplePlaneNormal, stretchToTargetLine_StartPoint.position);
        float rayDotNormal = Vector3.Dot(vDirFromOriginToTarget, vSimplePlaneNormal);

        if (Mathf.Abs(rayDotNormal) > 0.0001f)
        {
            float rayDistance = (planeD - Vector3.Dot(vSimplePlaneNormal, vFixedPointPos)) / rayDotNormal;
            fLineIntersectionDistFromOriginDist = Mathf.Max(rayDistance, 0.1f);

            fLastLengthIntersect = fLineIntersectionDistFromOriginDist;
            vLastRayIntersectDitr = vDirFromOriginToTarget;
        }
        else
        {
            fLineIntersectionDistFromOriginDist = fLastLengthIntersect;
            vDirFromOriginToTarget = vLastRayIntersectDitr;
        }

        vTargetPointPos = vFixedPointPos + vDirFromOriginToTarget * fLineIntersectionDistFromOriginDist;

        // Use same limit function as original
        LimitTargetPositionOnLineStartEnd();
    }

    // Original complex calculation
    private void CalculatePointOnTargetLine()
    {
        Vector3 vIntersectionLineDirFromStartToEndDir = (stretchToTargetLine_EndPoint.position - stretchToTargetLine_StartPoint.position).normalized;

        vIntersectionLineUp = vIntersectionLineDirFromStartToEndDir;
        vIntersecitonLineForward = (vFixedPointPos - stretchToTargetLine_StartPoint.position).normalized;
        Vector3 vRightCross = Vector3.Cross(vIntersectionLineUp, vIntersecitonLineForward).normalized;
        vIntersecitonLineForward = Vector3.Cross(vRightCross, vIntersectionLineUp).normalized;

        Vector3 vDirFromOriginToTarget = (stretchToTargetLine_StartPoint.position - vFixedPointPos).normalized;
        vDirFromOriginToTarget.y = 0.0f;
        vDirFromOriginToTarget.Normalize();

        fAngleCurrent = fElementDirectionTargetAngle - fixedAttachedToPoint.eulerAngles.x * localRotationChangeStrengthMultiplier;
        vDirFromOriginToTarget = Quaternion.AngleAxis(fAngleCurrent, Vector3.Cross(vDirFromOriginToTarget, Vector3.up).normalized) * vDirFromOriginToTarget;
        vDirFromOriginToTarget.Normalize();
        Ray ray = new Ray(vFixedPointPos, vDirFromOriginToTarget);

        Plane plane = new Plane(vIntersecitonLineForward, stretchToTargetLine_StartPoint.position);

        bool bRaycasted = plane.Raycast(ray, out fLineIntersectionDistFromOriginDist);

        if (bRaycasted == true)
        {
            fLastLengthIntersect = fLineIntersectionDistFromOriginDist;
            vLastRayIntersectDitr = ray.direction;
        }
        else
        {
            fLineIntersectionDistFromOriginDist = fLastLengthIntersect;
            ray.direction = vLastRayIntersectDitr;
        }

        vTargetPointPos = vFixedPointPos + ray.direction * fLineIntersectionDistFromOriginDist;

        // is outside of line start/end
        LimitTargetPositionOnLineStartEnd();
    }

    float fLastLengthIntersect = 0.0f;
    Vector3 vLastRayIntersectDitr = Vector3.forward;

    private void LimitTargetPositionOnLineStartEnd()
    {
        Vector3 directionToEnd = (stretchToTargetLine_EndPoint.position - stretchToTargetLine_StartPoint.position).normalized;
        Vector3 vTargetPointOffsetFromStart = vTargetPointPos - stretchToTargetLine_StartPoint.position;
        float fLengthWithinLimits = Vector3.Dot(vTargetPointOffsetFromStart, directionToEnd);

        // Define limits 
        float fKartScale = Mathf.Abs(fixedAttachedToPoint.lossyScale.z);
        float minLimit = fHowCloseWeCanMoveOnLineTo_StartPoint * fKartScale;
        float maxLimit = Vector3.Distance(stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position) - fHowCloseWeCanMoveOnLineTo_EndPoint * fKartScale;

        // Clamp the length to ensure it remains within the desired bounds
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

        // Different colors for simple vs complex mode
        Color connectionColor = bUseSimple ? Color.cyan : Color.yellow;
        Color targetColor = bUseSimple ? Color.blue : Color.red;

        // Draw spheres using Handles
        Handles.color = targetColor;
        Handles.SphereHandleCap(0, dynamicTargetPointWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        Handles.color = Color.green;
        Handles.SphereHandleCap(0, fixedAttachedPointWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        // Draw line using Handles
        Handles.DrawBezier(dynamicTargetPointWorld, fixedAttachedPointWorld, dynamicTargetPointWorld, fixedAttachedPointWorld, connectionColor, null, 5f);

        if (stretchToTargetLine_StartPoint != null && stretchToTargetLine_EndPoint != null)
        {
            Handles.DrawBezier(stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position, stretchToTargetLine_StartPoint.position, stretchToTargetLine_EndPoint.position, Color.green, null, 5f);

            if (!bUseSimple)
            {
                Vector3 vDirFromOriginToTarget = (vTargetPointPos - vFixedPointPos).normalized;
                Handles.DrawBezier(vFixedPointPos, vFixedPointPos + vDirFromOriginToTarget * fLineIntersectionDistFromOriginDist, vFixedPointPos, vFixedPointPos + vDirFromOriginToTarget * fLineIntersectionDistFromOriginDist, Color.red, null, 5f);
            }
        }
    }
#endif
}
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class PTK_SimpleSuspensionElement : MonoBehaviour
{
    [Header("Attach Points")]
    public Vector3 fixedPointLocalOffset;
    public Transform fixedAttachedToPoint; // which point should not move / move a little based on distance

    public Vector3 dynamicPointLocalOffset;
    public Transform dynamicTargetPoint; // wheel or other moving element

    [Header("Model Start/End")]
    public Transform modelBottomTransform;
    public Transform modelTopTransform;

    [Header("Stretch/Orient")]
    public bool bStretch = true;
    public bool bOrientTowardWheel = true;
    public float fOrientStrength = 1.0f;

    [Header("Slide/Mover")]
    public bool bSlide = false;
    public float fTargetConstElementScale = 1.0f;
    [Header("Slide/Mover Scale based on Y dist of elements")]
    public bool bExtraShrinkBasedOnDistBetweenTransforms = false;
    public Transform distCheckObjA_Wheel;
    public Transform distCheckObjB_Body;
    public float fWheelDistanceToBody = 0.0f;
    public float fWheelDistanceToBodyNotClamped = 0.0f;
    public float fDistChangeMultiplier = 1.0f;

    [Header("Priv")]
    float fMinDistanceToStartScalingDown = 0.5f;
    float fMinDistanceSliderScale = 0.0f;
    float fOriginalSizeZ = 1.0f;
    float fOriginalScaleZ = 1.0f;
    Quaternion originalLocalRotation;
    float fInitialWheelDistToBody = 0.0f;
    Vector3 vLastDir = Vector3.forward;

    void Start()
    {
            InitOriginalInfo();
    }

    void InitOriginalInfo()
    {
        fInitialWheelDistToBody = 0.0f; ;
        originalLocalRotation = transform.localRotation;
        fOriginalSizeZ = Vector3.Magnitude(modelTopTransform.transform.position - modelBottomTransform.transform.position);
        fOriginalScaleZ = transform.localScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying == false)
            InitOriginalInfo();

        Vector3 vDir = (dynamicTargetPoint.position - fixedAttachedToPoint.position).normalized;

        if (vDir.magnitude == 0)
            vDir = vLastDir;

        vLastDir = vDir;
        Quaternion qRot = Quaternion.LookRotation(vDir);

        Vector3 fixedPointWorldOffset = Vector3.Scale(fixedAttachedToPoint.lossyScale, fixedPointLocalOffset);
        Vector3 dynamicPointWorldOffset = Vector3.Scale(dynamicTargetPoint.lossyScale, dynamicPointLocalOffset);

        Vector3 vFixedPointPos = fixedAttachedToPoint.position + qRot * (fixedPointWorldOffset);
        Vector3 vTargetPointPos = dynamicTargetPoint.position + qRot * (dynamicPointWorldOffset);

        float fDistanceToTargetPoint = Vector3.Magnitude(vFixedPointPos - vTargetPointPos);

        float fTargetScale = fDistanceToTargetPoint / fOriginalSizeZ;

        fTargetScale = fTargetScale * fOriginalScaleZ;

        fTargetScale = Mathf.Max(fTargetScale, 0.1f);

        // set position in origin point (pivot is in our position, so scaling will expand it in the direction)
        transform.position = vFixedPointPos;

        // scale down if transform and target transform are too close ( it will rotate very bad in very close distance if element is long, thats why we are making it slower)
        float fDistanceToTargetLerpUsedValue = Vector3.Magnitude(vFixedPointPos - vTargetPointPos);
        float fMinDistanceCurrentScalerVal = Mathf.Lerp( fMinDistanceSliderScale, 1.0f, fDistanceToTargetLerpUsedValue / fMinDistanceToStartScalingDown);
        float fCurrentTargetConstElementSize = fTargetConstElementScale * fMinDistanceCurrentScalerVal;


        if (bExtraShrinkBasedOnDistBetweenTransforms == true)
        {
            fWheelDistanceToBody = (distCheckObjB_Body.position.y - distCheckObjA_Wheel.transform.position.y) / transform.lossyScale.y;

            if (fInitialWheelDistToBody == 0)
                fInitialWheelDistToBody = fWheelDistanceToBody;
            fWheelDistanceToBody -= fInitialWheelDistToBody;

            fWheelDistanceToBodyNotClamped = fWheelDistanceToBody;

            fWheelDistanceToBody *= fDistChangeMultiplier;

            // we will shrink only if the wheel is above the body
            if (fWheelDistanceToBody > 0.0f)
               fWheelDistanceToBody = 0.0f;

            fWheelDistanceToBody =  Mathf.Abs(fWheelDistanceToBody);

            // scale only top part above attach point
            float fCurrentSizeBasedOnScale = fOriginalSizeZ * fCurrentTargetConstElementSize;
            float fDistanceFromBottomPointToAttachementPoint = fDistanceToTargetPoint;
            float fMaxDistanceThatWeCanScaleDown = fCurrentSizeBasedOnScale - fDistanceFromBottomPointToAttachementPoint;

            // so it wont shrink to 0
            fWheelDistanceToBody = Mathf.Clamp(fWheelDistanceToBody, 0, fMaxDistanceThatWeCanScaleDown- 0.2f);

            float fTargetDistance = fCurrentSizeBasedOnScale - fWheelDistanceToBody;
            float fTargetScaleBasedOnWheelDist = (fTargetDistance * fCurrentTargetConstElementSize) / (fCurrentSizeBasedOnScale);

            fCurrentTargetConstElementSize = fTargetScaleBasedOnWheelDist;
        }

        if (bStretch == true)
            transform.localScale = new Vector3(1.0f, 1.0f, fTargetScale);
        else if(bSlide == true ) // we are not stretching, element is slide has constant size
            transform.localScale = new Vector3(1.0f, 1.0f, fCurrentTargetConstElementSize);

         
        if (bOrientTowardWheel == true || bSlide == true) // slide need to rotate itself in direction and then move from target point
        {
            transform.LookAt(vTargetPointPos);
            transform.localRotation = Quaternion.Lerp(originalLocalRotation, transform.localRotation, fOrientStrength);
        }


        // we are attached to target and stretched
        if (bSlide == true)
        {
            if (fCurrentTargetConstElementSize < fTargetScale) // element target size is lower than required to reach target point. We are stretching to it (so we dont need to move back)
            {
            }
            else // element is longer than distance to target, so we need to move it back
            {
                // musimy zeskalowac element do docelowej dlugosci
                transform.localScale = new Vector3(1.0f, 1.0f, fCurrentTargetConstElementSize);
                float fCurrentSize_FromConstElementScale = Vector3.Magnitude(modelTopTransform.transform.position - modelBottomTransform.transform.position);

                float fMoveOffset = (fCurrentSize_FromConstElementScale - fDistanceToTargetPoint) ;
                transform.position = vFixedPointPos - transform.forward * fMoveOffset;
            }
        }
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


        Vector3 vFixedPointPos = fixedAttachedToPoint.position + transform.rotation * (fixedPointLocalOffset);
        Vector3 vTargetPointPos = dynamicTargetPoint.position + transform.rotation * (dynamicPointLocalOffset);

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

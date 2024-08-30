using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PTK_AnimHelper_RollOnGroundRotation : MonoBehaviour
{
    [Header("Roll Wheel-Like Moving")]
    public bool bApplyRollingMovement = true;
    public enum EAllowedRollDir
    {
        E0_ALL_DIRECTIONS,
        E1_LOCAL_FORWARD_DIRECTION,
        E2_LOCAL_RIGHT_DIRECTION,
        E3_LOCAL_UP_DIRECTION,
    }
    [Header("See Cyan Preview Line in SceneView")]
    public EAllowedRollDir eAllowedRollDir = EAllowedRollDir.E0_ALL_DIRECTIONS;
    public float fRollRadiusSizeMultiplier = 1.0f;
    public float fRollDir = 1.0f;
    private Vector3 lastPosition;
    Quaternion initialRot = Quaternion.identity;



    // Start is called before the first frame update
    void Awake()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceRestart;

        initialRot = transform.localRotation;

        OnRaceRestart();
    }

    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerStart;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceRestart;
    }

    bool bCanPlayAnimation = false;

    bool bInitLastPos = true;
    void OnRaceRestart()
    {
        bInitLastPos = true;
        transform.localRotation = initialRot;

        if (PTK_ModGameplayDataSync.Instance.gameInfo.fCurrentRaceTime > 0)
            bCanPlayAnimation = true; // already started - component added after race running
        else
            bCanPlayAnimation = false;
    }

    void OnRaceTimerStart()
    {
        bCanPlayAnimation = true;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateRolling();
    }

    private Vector3 GetVelocityInDirection(Vector3 vVelocity, Vector3 vDir)
    {
        return vDir * (Vector3.Dot(vVelocity, vDir));
    }
    void UpdateRolling()
    {
        if (bCanPlayAnimation == false)
            return;

        if (bApplyRollingMovement == true)
        {
            if (bInitLastPos == true)
            {
                bInitLastPos = false;
                lastPosition = transform.position;
            }

            // Calculate the distance moved since the last frame
            Vector3 deltaPosition = transform.position - lastPosition;
            if (eAllowedRollDir == EAllowedRollDir.E0_ALL_DIRECTIONS)
            {

            }
            else if (eAllowedRollDir == EAllowedRollDir.E1_LOCAL_FORWARD_DIRECTION)
            {
                deltaPosition = GetVelocityInDirection(deltaPosition, Vector3.Cross(transform.forward, Vector3.up).normalized);
            }
            else if (eAllowedRollDir == EAllowedRollDir.E2_LOCAL_RIGHT_DIRECTION)
            {
                deltaPosition = GetVelocityInDirection(deltaPosition, Vector3.Cross(transform.right, Vector3.up).normalized);
            }
            else if (eAllowedRollDir == EAllowedRollDir.E3_LOCAL_UP_DIRECTION)
            {
                deltaPosition = GetVelocityInDirection(deltaPosition, Vector3.Cross(transform.up, Vector3.up).normalized);
            }

            // Calculate the rotation amount
            float distanceMoved = deltaPosition.magnitude;

            // Calculate the roll amount based on the sphere's circumference
            float rollAmount = (distanceMoved / (fRollRadiusSizeMultiplier * transform.lossyScale.x * 2.0f * Mathf.PI)) * 360.0f * 1.0f;


            // Determine the rotation axis based on movement direction
            Vector3 rotationAxis = Vector3.Cross(-deltaPosition.normalized* fRollDir, Vector3.up);


            // Apply the rotation
            transform.Rotate(rotationAxis, rollAmount, Space.World);

            // Update the last position for the next frame
            lastPosition = transform.position;
        }
    }
#if UNITY_EDITOR

    void DrawBezierArrow(Vector3 start, Vector3 end, float width, Color color)
    {
        Handles.color = color;

        // Calculate the middle point of the curve
        Vector3 middle = (start + end) / 2;
        Vector3 offset = (end - start).normalized * width;

        // Adjust the control points to create a curved arrow
        Vector3 startTangent = start + offset;
        Vector3 endTangent = end - offset;

        // Draw the main bezier line with a width of 5
        Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 15f);

        // Calculate direction for the arrowhead
        Vector3 direction = (end - start).normalized;
        Vector3 rightWing = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward * 0.5f;
        Vector3 leftWing = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward * 0.5f;

        // Draw the arrowhead as two lines
        Handles.DrawLine(end, end + rightWing);
        Handles.DrawLine(end, end + leftWing);
    }

    // Function to visualize the allowed roll directions, only when the object is selected
    void OnDrawGizmosSelected()
    {
        Vector3 position = transform.position;

        if (eAllowedRollDir == EAllowedRollDir.E0_ALL_DIRECTIONS)
        {
        }
        else if (eAllowedRollDir == EAllowedRollDir.E1_LOCAL_FORWARD_DIRECTION)
        {
            DrawBezierArrow(position, position + transform.forward * 2.0f, 55.5f, Color.cyan);
        }
        else if (eAllowedRollDir == EAllowedRollDir.E2_LOCAL_RIGHT_DIRECTION)
        {
            DrawBezierArrow(position, position + transform.right * 2.0f, 55.5f, Color.cyan);
        }
        else if (eAllowedRollDir == EAllowedRollDir.E3_LOCAL_UP_DIRECTION)
        {
            DrawBezierArrow(position, position + transform.up * 2.0f, 55.5f, Color.cyan);
        }
    }

#endif

}

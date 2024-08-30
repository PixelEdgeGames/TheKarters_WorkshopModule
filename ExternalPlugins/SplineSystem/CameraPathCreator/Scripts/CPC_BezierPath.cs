using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CPC_Visual
{
    public Color pathColor = Color.green;
    public Color inactivePathColor = Color.gray;
    public Color frustrumColor = Color.white;
    public Color handleColor = Color.yellow;
}

public enum CPC_ECurveType
{
    EaseInAndOut,
    Linear,
    Custom
}

public enum CPC_EAfterLoop
{
    Continue,
    Stop
}

[System.Serializable]
public class CPC_Point
{
    [HideInInspector]
    public Vector3 _positionLocal;
    public Vector3 positionWorld
    {
        get
        {
            if (parentTransform == null)
                return _positionLocal;

            return parentTransform.TransformPoint(_positionLocal);
        }

        set
        {
            _positionLocal = parentTransform.InverseTransformPoint(value);
        }
    }

    public Vector3 positionLocal
    {
        get
        {
            return _positionLocal;
        }

        set
        {
            _positionLocal = (value);
        }
    }

    [HideInInspector]
    public Vector3 _handlePrevLocal;
    [HideInInspector]
    public Vector3 _handleNextLocal;

    public Vector3 handlePrevWorld
    {
        get
        {
            if (parentTransform == null)
                return _handlePrevLocal;

            return parentTransform.transform.rotation * _handlePrevLocal;// parentTransform.TransformPoint(_handlePrevLocal);
        }

        set
        {
            _handlePrevLocal = Quaternion.Inverse(parentTransform.transform.rotation) * value;
            //_handlePrevLocal = parentTransform.InverseTransformPoint(value);
        }
    }

    public void ResetHandleLocalY()
    {
        _handlePrevLocal.y = 0.0f;
        _handleNextLocal.y = 0.0f;
    }

    public Vector3 handleNextWorld
    {
        get
        {
            if (parentTransform == null)
                return _handleNextLocal;

            return parentTransform.transform.rotation * _handleNextLocal;// parentTransform.TransformPoint(_handleNextLocal);
        }

        set
        {
            _handleNextLocal =  Quaternion.Inverse(parentTransform.transform.rotation) *  value;// parentTransform.InverseTransformPoint(value);
        }
    }

    public Vector3 handleNextLocal
    {
        get
        {
            return _handleNextLocal;
        }

        set
        {
            _handleNextLocal = (value);
        }
    }

    public Vector3 handlePrevLocal
    {
        get
        {
            return _handlePrevLocal;
        }

        set
        {
            _handlePrevLocal = (value);
        }
    }


    public Quaternion rotation;
    public CPC_ECurveType curveTypeRotation;
    public AnimationCurve rotationCurve;
    public CPC_ECurveType curveTypePosition;
    public AnimationCurve positionCurve;
    public bool chained;
    public bool bKeepFlat = true;
    [HideInInspector ]
    public Transform parentTransform;
    public CPC_Point(Transform _parentTransform, Vector3 pos, Quaternion rot)
    {
        parentTransform = _parentTransform;
        positionWorld = pos;
        rotation = rot;
        handlePrevWorld = Vector3.back;
        handleNextWorld = Vector3.forward;
        curveTypeRotation = CPC_ECurveType.EaseInAndOut;
        rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        curveTypePosition = CPC_ECurveType.Linear;
        positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        chained = true; bKeepFlat = true;
    }
}

[System.Serializable]
public class CPC_Follower
{
    // References
    public GameObject gameObject;
    public Transform lookAtTarget = null;

    // Settings
    public float fSpeedMultiplier;
    public float fRotationSmoothness;
    public ERotationMode eRotationMode;
    public ERotLerpMode eRotLerpMode;
    public EWrapMode eWrapMode;

    // Flags
    [HideInInspector] public bool bPathPlayJustStarted;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool bExpandedEditorGUI;
    [HideInInspector] public bool bInitializedWithDefaultVals;
    public bool bPingPoingFlipRotation;

    // Parameters
    public float fStartDistance;
    public float fStartDelay;
    public float fPingPongFlipRotationSpeed;

    // State
    [HideInInspector] public float currentDistance;
    [HideInInspector] public float prevEventUpdateDistance;
    [HideInInspector] public float fSpeedMoveDirection;
    [HideInInspector] public float fPingPongDir ;
    [HideInInspector] public Quaternion flippedRotationPingPong;
    [HideInInspector] public Vector3 vRotSmoothDampVel;
    [HideInInspector] public Quaternion qRotSmoothDampVel;
    [HideInInspector] public Vector3 vSavedPosBeforeEditPath;
    [HideInInspector] public List<bool> bEventTriggered = new List<bool>();

    // Enums
    public enum ERotationMode
    {
        E_BEZIER,
        E_BEZIER_WORLD_Y,
        E_POINT_ORIENTATION,
        E_IGNORE,
        E_LOOK_AT
    }

    public enum ERotLerpMode
    {
        E_LERP,
        E_SMOOTH_DAMP
    }

    public enum EWrapMode
    {
        E_LOOP,
        E_PING_PONG,
        E_ONCE
    }

    // Constructor
    public CPC_Follower()
    {
        InitWithDefaultVals();
    }

    // Initialization method
    public void InitWithDefaultVals()
    {
        if (bInitializedWithDefaultVals)
            return;

        // Initialize fields with default values
        fSpeedMultiplier = 1.0f;
        fRotationSmoothness = 0.2f;
        eRotationMode = ERotationMode.E_BEZIER;
        eRotLerpMode = ERotLerpMode.E_LERP;
        eWrapMode = EWrapMode.E_LOOP;
        fStartDistance = 0f;
        fStartDelay = 0f;
        bPingPoingFlipRotation = false;
        fPingPongFlipRotationSpeed = 2.0f;
        flippedRotationPingPong = Quaternion.identity;
        fSpeedMoveDirection = 1.0f;
        fPingPongDir = 1.0f;
        bPathPlayJustStarted = false;
        isMoving = false;
        bExpandedEditorGUI = true;
        currentDistance = 0f;
        prevEventUpdateDistance = 0f;
        vRotSmoothDampVel = Vector3.zero;
        qRotSmoothDampVel = Quaternion.identity;
        vSavedPosBeforeEditPath = Vector3.zero;
        bEventTriggered = new List<bool>();
        bEventTriggered.Clear(); // Clear any existing data in the list
        bInitializedWithDefaultVals = true;
    }
}

[System.Serializable]
public class CPC_Event
{
    public UnityEvent unityEvent;
    public int iWaypointIndex = 0;
    [Range(0f, 1f)]
    public float fWaypointNormalizedDist = 0f;
    [HideInInspector]
    public bool hasTriggered = false;

    [HideInInspector]
    public float fTriggerEventAtDistnace = 99999.0f;
    [HideInInspector]
    public bool bExpandedEditorGUI = false;

    public List<bool> customEventConditionsToPass = new List<bool>();
    public void ResetCustomConditionsToPass()
    {
        for(int i=0;i< customEventConditionsToPass.Count;i++)
        {
            customEventConditionsToPass[i] = false;
        }
    }
    public bool AreCustomConditionsPassed()
    {
        for (int i = 0; i < customEventConditionsToPass.Count; i++)
        {
            if (customEventConditionsToPass[i] == false)
                return false;
        }

        return true;
    }
    public enum ETriggerByType
    {
        E0_ALL_FOLLOWERS,
        E1_SPECIFIC_PASSED_FOLLOWERS
    }

    public enum ETriggerSendMode
    {
        E0_EACH_PASS_THROUGH,
        E1_ONCE_PER_FOLLOWER,
        E2_ONCE_PER_SPLINE
    }
    public ETriggerByType eTriggerBy = ETriggerByType.E0_ALL_FOLLOWERS;
    public ETriggerSendMode eTriggerSendMode = ETriggerSendMode.E0_EACH_PASS_THROUGH;


    // List of assigned followers for E_TARGET_FOLLOWERS mode
    [HideInInspector]
    public bool bExpandedAssignedFollowersGUI = false;
    public List<GameObject> assignedFollowers = new List<GameObject>();
}

public class CPC_BezierPath : MonoBehaviour
{
    public float fLastCalculatedPathLength = 0;
    
    [HideInInspector]
    public bool bShowPointList = true;
    public bool playOnAwake = true;
    public float fBezierSpeed = 2f;
    public List<CPC_Point> points = new List<CPC_Point>();
    public CPC_Visual visual;
    public bool looped = true;
    public bool alwaysShow = true;
    public bool showInGame = false;
    public float fSplineWidth = 0.3f;

    public List<CPC_Follower> followers = new List<CPC_Follower>();
    public List<CPC_Event> events = new List<CPC_Event>();


    private bool paused = false;
    private bool playing = false;

    [HideInInspector]
    public bool bIsRuntimeEditingPath = false;



    void Start()
    {
        foreach (var point in points)
        {
            if (point.curveTypeRotation == CPC_ECurveType.EaseInAndOut) point.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (point.curveTypeRotation == CPC_ECurveType.Linear) point.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            if (point.curveTypePosition == CPC_ECurveType.EaseInAndOut) point.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (point.curveTypePosition == CPC_ECurveType.Linear) point.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        GenerateVertexPath();

        if (playOnAwake)
            PlayPath(fBezierSpeed,false,false);
    }

    public void ShowEventTriggeredDebugInfo()
    {
        Debug.LogError("Event Triggered!");
    }
   public void Update()
    {
        if (Application.isPlaying == true && lineRenderer != null)
        {
            if (showInGame == true || bIsRuntimeEditingPath || bIsObjectSelectedOnGizmos)
                lineRenderer.gameObject.SetActive(true);
            else
                lineRenderer.gameObject.SetActive(false);
        }

        bIsObjectSelectedOnGizmos = false;
    }

    public void RuntimeEditingEnded()
    {
        PlayPath(fBezierSpeed,true);
    }
    Vector3 GetVertexPositionAtDistance(float distance)
    {
        if (vertexPath == null) return Vector3.zero;
        return vertexPath.GetPointAtDistance(distance);
    }

    IEnumerator StartFollowerAfterDelay(CPC_Follower follower)
    {
        yield return new WaitForSeconds(follower.fStartDelay);
        follower.isMoving = true;
    }


    void UpdateRotation(CPC_Follower follower, float distanceTravelled)
    {
        Quaternion rotation = Quaternion.identity;

        switch (follower.eRotationMode)
        {
            case CPC_Follower.ERotationMode.E_BEZIER:
            case CPC_Follower.ERotationMode.E_BEZIER_WORLD_Y:
                rotation = vertexPath.GetRotationAtDistance(distanceTravelled);
                if (follower.eRotationMode == CPC_Follower.ERotationMode.E_BEZIER_WORLD_Y)
                {
                    Vector3 euler = rotation.eulerAngles;
                    euler.x = 0;
                    euler.z = 0;
                    rotation = Quaternion.Euler(euler);
                }else
                {
                    Vector3 euler = rotation.eulerAngles;
                    euler.z = 0;
                    rotation = Quaternion.Euler(euler);
                }
                break;
            case CPC_Follower.ERotationMode.E_POINT_ORIENTATION:
                int pointIndex = Mathf.FloorToInt(distanceTravelled / vertexPath.length * points.Count);
                int nextIndex = GetNextIndex(pointIndex);
                float segmentT = (distanceTravelled % (vertexPath.length / points.Count)) / (vertexPath.length / points.Count);
                rotation = Quaternion.LerpUnclamped(points[pointIndex].rotation, points[nextIndex].rotation, points[pointIndex].rotationCurve.Evaluate(segmentT));
                break;
            case CPC_Follower.ERotationMode.E_LOOK_AT:
                if (follower.lookAtTarget != null)
                {
                    rotation = Quaternion.LookRotation((follower.lookAtTarget.position - follower.gameObject.transform.position).normalized);
                }
                else
                {
                    Debug.LogWarning("Look At target is not set for " + follower.gameObject.name);
                }
                break;
            case CPC_Follower.ERotationMode.E_IGNORE:
            default:
                rotation = follower.gameObject.transform.rotation;
                break;
        }

        bool bFlipDir = (follower.bPingPoingFlipRotation == true && follower.fPingPongDir == -1.0F);

        if(follower.fSpeedMoveDirection == -1)
        {
         //   (follower.bPingPoingFlipRotation == true && follower.fPingPongDir == -1.0F);
        }

        follower.flippedRotationPingPong = Quaternion.Slerp(follower.flippedRotationPingPong, Quaternion.Euler(0.0f, bFlipDir ? 180.0F : 0.0F, 0.0f), Time.deltaTime * follower.fPingPongFlipRotationSpeed);

        Vector3 vForward = follower.gameObject.transform.rotation * Vector3.forward;
        Vector3 vForwardTo = rotation * follower.flippedRotationPingPong * Vector3.forward;


        if (follower.bPathPlayJustStarted == true)
            follower.gameObject.transform.rotation = rotation * follower.flippedRotationPingPong;
        else
        {
            if(follower.eRotLerpMode == CPC_Follower.ERotLerpMode.E_LERP)
                follower.gameObject.transform.rotation = Quaternion.Slerp(follower.gameObject.transform.rotation, rotation * follower.flippedRotationPingPong, Time.deltaTime * 15.0f);//
            else
                follower.gameObject.transform.rotation = Quaternion.LookRotation(Vector3.SmoothDamp(vForward, vForwardTo, ref follower.vRotSmoothDampVel, 0.2f));
        }


    }
    public  Quaternion SmoothDamp(
        Quaternion current, Quaternion target,
        ref Vector3 velocity, float smoothTime,
        float maxSpeed = Mathf.Infinity)
    {
        // Convert from Quaternion to Euler angles
        Vector3 currentEuler = current.eulerAngles;
        Vector3 targetEuler = target.eulerAngles;

        // Perform smooth damp on each component of the euler angles
        Vector3 smoothEuler = new Vector3();
        smoothEuler.x = Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref velocity.x, smoothTime);
        smoothEuler.y = Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref velocity.y, smoothTime);
        smoothEuler.z = Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref velocity.z, smoothTime);

        // Convert back to Quaternion
        return Quaternion.Euler(smoothEuler);
    }
    void UpdateEvents(CPC_Follower follower)
    {
        foreach (var evt in events)
        {
            if (CanTriggerAndIsTimeToTriggerEvent(evt, follower))
            {
                bool shouldTriggerEvent = false;

                switch (evt.eTriggerBy)
                {
                    case CPC_Event.ETriggerByType.E0_ALL_FOLLOWERS:
                        shouldTriggerEvent = true;
                        break;

                    case CPC_Event.ETriggerByType.E1_SPECIFIC_PASSED_FOLLOWERS:
                        shouldTriggerEvent = evt.assignedFollowers.Contains(follower.gameObject);
                        break;
                }

                if (shouldTriggerEvent)
                {
                    switch (evt.eTriggerSendMode)
                    {
                        case CPC_Event.ETriggerSendMode.E0_EACH_PASS_THROUGH:
                            evt.unityEvent.Invoke();
                            follower.bEventTriggered[events.IndexOf(evt)] = true;
                            break;

                        case CPC_Event.ETriggerSendMode.E1_ONCE_PER_FOLLOWER:
                            evt.unityEvent.Invoke();
                            follower.bEventTriggered[events.IndexOf(evt)] = true;
                            break;
                        case CPC_Event.ETriggerSendMode.E2_ONCE_PER_SPLINE:
                            evt.unityEvent.Invoke();
                            follower.bEventTriggered[events.IndexOf(evt)] = true;
                            evt.hasTriggered = true;
                            break;

                    }
                }
            }
        }
    }

    bool CanTriggerAndIsTimeToTriggerEvent(CPC_Event eventToCheck, CPC_Follower follower)
    {
        if (eventToCheck.hasTriggered == true || follower.bEventTriggered[events.IndexOf(eventToCheck)] == true)
        {
            follower.prevEventUpdateDistance = follower.currentDistance;
            return false;
        }

        if (eventToCheck.AreCustomConditionsPassed() == false)
        {
            follower.prevEventUpdateDistance = follower.currentDistance;
            return false;
        }

        if (follower.isMoving == false || follower.fSpeedMoveDirection == 0.0f || follower.fSpeedMultiplier == 0.0f)
        {
            follower.prevEventUpdateDistance = follower.currentDistance;
            return false; // completted path or not moving anymore
        }

        if (follower.currentDistance > 0 && eventToCheck.fTriggerEventAtDistnace == 0)
            return true;


        bool bReturnResult = false;
        if(follower.fSpeedMoveDirection == 1.0f)
        {
            // Check if the distance travelled has reached or passed the event distance
            bReturnResult =  follower.currentDistance >= eventToCheck.fTriggerEventAtDistnace && follower.prevEventUpdateDistance <= eventToCheck.fTriggerEventAtDistnace;
        }
        else
        {
            // Check if the distance travelled has reached or passed the event distance
            bReturnResult =  follower.currentDistance <= eventToCheck.fTriggerEventAtDistnace &&  follower.prevEventUpdateDistance >= eventToCheck.fTriggerEventAtDistnace;
        }




        follower.prevEventUpdateDistance = follower.currentDistance;
        return bReturnResult;
    }

    void ResetEvents()
    {
        foreach (var evt in events)
        {
            evt.hasTriggered = false;

            evt.ResetCustomConditionsToPass();

            // Reset follower-specific event triggers
            foreach (var follower in followers)
            {
                follower.bEventTriggered[events.IndexOf(evt)] = false;
                follower.prevEventUpdateDistance = 0.0f;
                follower.flippedRotationPingPong = Quaternion.identity;
            }
        }
    }
    [HideInInspector]
    public VertexPath vertexPath;
    BezierPath bezierPath;

   public void GenerateVertexPath()
    {
        bool bShowDebugSpehere = false && Application.isPlaying == true;

        List<Vector3> pointsWithAnchors = new List<Vector3>();
        List<float> angles = new List<float>();

        pointsWithAnchors.Add(points[0].positionLocal);
        pointsWithAnchors.Add(points[0].positionLocal + points[0].handleNextLocal);
        angles.Add(0);

        for (int i = 1; i < points.Count - 1; i++)
        {
            // Add the handleprev as the first control point
            pointsWithAnchors.Add(points[i].positionLocal + points[i].handlePrevLocal);
            // Add the main anchor point
            pointsWithAnchors.Add(points[i].positionLocal);
            pointsWithAnchors.Add(points[i].positionLocal + points[i].handleNextLocal);
            angles.Add(0);
        }

        pointsWithAnchors.Add(points[points.Count - 1].positionLocal + points[points.Count - 1].handlePrevLocal);
        pointsWithAnchors.Add(points[points.Count - 1].positionLocal);
        angles.Add(0);

        if (looped)
        {
            pointsWithAnchors.Add(points[points.Count - 1].positionLocal + points[points.Count - 1].handleNextLocal);
            pointsWithAnchors.Add(points[0].positionLocal + points[0].handlePrevLocal);
        }

        // Now create the BezierPath with these points
        BezierPath bezierPath = new BezierPath(pointsWithAnchors, angles, _controlMode: BezierPath.ControlMode.Free, isClosed: looped, space: PathSpace.xyz);
        vertexPath = new VertexPath(bezierPath, transform);

        if(bShowDebugSpehere == true)
        {
            for (int i = 0; i < bezierPath.NumPoints; i++)
            {
                //     GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = bezierPath.GetPoint(i);
            }
            float fDistPerStep = 0.5f;
            int iSteps = Mathf.CeilToInt(vertexPath.length / fDistPerStep);
            float fDistSum = 0;
            for (int i = 0; i < iSteps; i++)
            {
                if (fDistSum > vertexPath.length)
                    fDistSum = vertexPath.length;

                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = vertexPath.GetPointAtDistance(fDistSum);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                fDistSum += fDistPerStep;
            }
        }

        fLastCalculatedPathLength = vertexPath.length;
    }

    public void SaveClosestPointAtCurrentDistance()
    {
        for(int i=0;i<followers.Count;i++)
        {
            followers[i].vSavedPosBeforeEditPath = followers[i].gameObject.transform.position;
        }
    }

    public void StopEditingMoveFollowersToSavedClosestPoints()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].currentDistance = vertexPath.GetClosestDistanceAlongPath(followers[i].vSavedPosBeforeEditPath);
        }
    }

    public void PlayPath(float fSpeed,bool bRegenerateVertexPath, bool bResetEvents = true)
    {
        if (bRegenerateVertexPath == true)
        {
            GenerateVertexPath();
        }

        if (bResetEvents == true)
            ResetEvents();

        for (int i = 0; i < events.Count; i++)
        {
            Vector3 eventPosition = GetBezierPosition(events[i].iWaypointIndex, events[i].fWaypointNormalizedDist);
            events[i].fTriggerEventAtDistnace = vertexPath.GetClosestDistanceAlongPath(eventPosition);
        }

        if (fSpeed <= 0) fSpeed = 0.001f;
        paused = false;
        playing = true;
        StopAllCoroutines();

        foreach (var follower in followers)
        {
            if (follower.bInitializedWithDefaultVals == false)
                follower.InitWithDefaultVals();

            follower.bPathPlayJustStarted = true;


            // Initialize event trigger list for each follower
            follower.bEventTriggered.Clear();
            for (int i = 0; i < events.Count; i++)
            {
                follower.bEventTriggered.Add(false);
            }

            if (follower.gameObject != null)
            {
                follower.fSpeedMoveDirection = 1.0f;

                follower.fPingPongDir = 1.0f;
                follower.gameObject.transform.position = vertexPath.GetPointAtDistance(follower.fStartDistance);
                UpdateRotation(follower, follower.fStartDistance);
                follower.isMoving = false;
                StartCoroutine(StartFollowerAfterDelay(follower));
            }
        }

        StartCoroutine(FollowPath(fSpeed));
    }

    public void StopPath()
    {
        ResetEvents();
        playing = false;
        paused = false;
        StopAllCoroutines();
    }

    public void UpdateTimeInSeconds(float seconds)
    {
    }

    public void PausePath()
    {
        paused = true;
        playing = false;
    }

    public void ResumePath()
    {
        if (paused)
            playing = true;
        paused = false;
    }

    public bool IsPaused()
    {
        return paused;
    }

    public bool IsPlaying()
    {
        return playing;
    }

  
    IEnumerator FollowPath(float fBaseSpeed)
    {
        float pathLength = vertexPath.length;

        while (true)
        {
            if (!paused )
            {
                foreach (var follower in followers)
                {
                    if ( follower.gameObject != null)
                    {
                        if(follower.bPathPlayJustStarted == true)
                            follower.currentDistance = follower.fStartDistance;  // Set start distance when starting to move

                        // Calculate the distance for this follower based on its speed and wrap mode
                        float deltaDistance = fBaseSpeed*10.0f * Mathf.Abs(follower.fSpeedMultiplier) * Time.deltaTime;

                        // Handle reverse direction 
                        if (follower.fSpeedMultiplier < 0)
                        {
                            follower.fSpeedMoveDirection = -1.0f;
                        }
                        else
                        {
                            follower.fSpeedMoveDirection = 1.0f;
                        }

                        if (follower.isMoving == true)
                        {
                            follower.currentDistance += deltaDistance * follower.fSpeedMoveDirection* follower.fPingPongDir;

                            switch (follower.eWrapMode)
                            {
                                case CPC_Follower.EWrapMode.E_LOOP:
                                    if (follower.currentDistance >= pathLength)
                                    {
                                        follower.currentDistance %= pathLength;
                                        OnFollowerCompletedPath(follower);
                                    }
                                    else if (follower.currentDistance < 0)
                                    {
                                        follower.currentDistance += pathLength;
                                        OnFollowerCompletedPath(follower);
                                    }
                                    break;
                                case CPC_Follower.EWrapMode.E_PING_PONG:
                                    if (follower.currentDistance > pathLength)
                                    {
                                        if (follower.fSpeedMoveDirection == 1)
                                        {
                                            follower.fPingPongDir = -1.0f;
                                            follower.currentDistance = pathLength;
                                        }else
                                        {
                                            follower.fPingPongDir = 1.0f;
                                            follower.currentDistance = pathLength;
                                        }
                                        OnFollowerCompletedPath(follower);
                                    }
                                    else if (follower.currentDistance < 0.0f)
                                    {

                                        if(follower.fSpeedMoveDirection == 1)
                                        {
                                            follower.fPingPongDir = 1.0f;
                                            follower.currentDistance = 0;
                                        }else
                                        {
                                            follower.fPingPongDir = -1.0f;
                                            follower.currentDistance = 0;
                                        }
                                        OnFollowerCompletedPath(follower);
                                    }
                                    break;
                                case CPC_Follower.EWrapMode.E_ONCE:
                                    if (follower.currentDistance >= pathLength && follower.fSpeedMoveDirection == 1.0f)
                                    {
                                        follower.currentDistance = pathLength;
                                        follower.isMoving = false;
                                        OnFollowerCompletedPath(follower);
                                    }
                                    else if (follower.currentDistance < -pathLength &&  follower.fSpeedMoveDirection == -1.0f)
                                    {
                                        follower.currentDistance = 0;
                                        follower.isMoving = false;
                                        OnFollowerCompletedPath(follower);
                                    }
                                    break;
                            }

                            // Update follower position and rotation
                            Vector3 position = vertexPath.GetPointAtDistance(follower.currentDistance);
                            follower.gameObject.transform.position = position;
                            UpdateRotation(follower, follower.currentDistance);
                        }

                        follower.bPathPlayJustStarted = false;
                        UpdateEvents(follower);
                    }
                }

                // Update events based on the follower positions
            }

            yield return null;
        }
    }
    void OnFollowerCompletedPath(CPC_Follower follower)
    {
        foreach (var evt in events)
        {
            if (evt.eTriggerSendMode == CPC_Event.ETriggerSendMode.E1_ONCE_PER_FOLLOWER)
            {
                continue; // one follower can send only once, do not restart event triggered info
            }

            switch (evt.eTriggerBy)
            {
                case CPC_Event.ETriggerByType.E0_ALL_FOLLOWERS:
                    if (evt.eTriggerSendMode == CPC_Event.ETriggerSendMode.E0_EACH_PASS_THROUGH)
                    {
                        follower.bEventTriggered[events.IndexOf(evt)] = false;
                    }
                    break;

                case CPC_Event.ETriggerByType.E1_SPECIFIC_PASSED_FOLLOWERS:
                    if (evt.assignedFollowers.Contains(follower.gameObject))
                    {
                        if (evt.eTriggerSendMode == CPC_Event.ETriggerSendMode.E0_EACH_PASS_THROUGH)
                        {
                            follower.bEventTriggered[events.IndexOf(evt)] = false;
                        }
                    }
                    break;
            }

            if (evt.eTriggerSendMode == CPC_Event.ETriggerSendMode.E2_ONCE_PER_SPLINE)
            {
            }
        }
    }



    int GetNextIndex(int index)
    {
        if (index == points.Count - 1)
            return 0;
        return index + 1;
    }

    Vector3 GetBezierPosition(int pointIndex, float time)
    {
        float t = points[pointIndex].positionCurve.Evaluate(time);
        int nextIndex = GetNextIndex(pointIndex);
        return
            Vector3.Lerp(
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].positionWorld,
                        points[pointIndex].positionWorld + points[pointIndex].handleNextWorld, t),
                    Vector3.Lerp(points[pointIndex].positionWorld + points[pointIndex].handleNextWorld,
                        points[nextIndex].positionWorld + points[nextIndex].handlePrevWorld, t), t),
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].positionWorld + points[pointIndex].handleNextWorld,
                        points[nextIndex].positionWorld + points[nextIndex].handlePrevWorld, t),
                    Vector3.Lerp(points[nextIndex].positionWorld + points[nextIndex].handlePrevWorld,
                        points[nextIndex].positionWorld, t), t), t);
    }

    public float fLastGizmosDrawTime = 0.0f;
#if UNITY_EDITOR
    public LineRenderer lineRenderer;

    void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            GameObject debugLineRendererPreview = new GameObject("debugLineRendererPreview");
            debugLineRendererPreview.transform.parent = this.transform;
            lineRenderer = debugLineRendererPreview.AddComponent<LineRenderer>();
        }

        // Set LineRenderer properties
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.textureMode = LineTextureMode.Tile;
        // Load texture from Resources folder
        Texture2D texture = Resources.Load<Texture2D>("CheckerLineRenderer");
        if (texture != null)
        {
            // Create a new material with the texture
            Material lineMaterial = new Material(Shader.Find("Unlit/Texture"));
            lineMaterial.mainTexture = texture;
            lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("CheckerLineRenderer.png not found in Resources folder.");
        }
        fLastLength = -1;

     //   lineRenderer.startColor = Color.green* 0.5f + Color.white * 0.2f;
     //  lineRenderer.endColor = Color.green*0.5f + Color.white * 0.2f;
    }


    public void RefreshLineRenderer()
    {
        if (bIsRuntimeEditingPath == false && Application.isPlaying == true)
            return;

        if (lineRenderer == null)
        {
            InitializeLineRenderer();
        }


        lineRenderer.sharedMaterial.mainTextureScale = new Vector2(1.0f, (fSplineWidth/0.3f)* 0.33f);
        lineRenderer.sharedMaterial.mainTextureOffset = new Vector2(0.0f,0.0f -( (fSplineWidth / 0.3f)) * 0.5f);

        float fBezierChangedVal = transform.position.magnitude + fSplineWidth + (looped ? 1.0f : 0.0f);

        for (int i = 0; i < points.Count; i++)
        {
            if (i > 0)
            {
                fBezierChangedVal += Vector3.Magnitude(points[i - 1].positionWorld - points[i].positionWorld);
                fBezierChangedVal += Vector3.Magnitude(points[i - 1].handleNextWorld - points[i].handleNextWorld);
                fBezierChangedVal += Vector3.Magnitude(points[i - 1].handlePrevWorld - points[i].handlePrevWorld);
            }
        }

        int sampleCount = (int)(fBezierChangedVal / (2.5f));  // Number of samples along the path

        if (sampleCount < 3)
            sampleCount = 3;

        int iSampleCountBezierLineRenderer = sampleCount * 10;
        // lineRenderer.positionCount = iSampleCountBezierLineRenderer;

       
        if (fLastLength != fBezierChangedVal)
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < iSampleCountBezierLineRenderer; i++)
            {
                float t = i / (float)(iSampleCountBezierLineRenderer - 1);
                Vector3 point = GetBezierPositionAtTime(t);
                Vector3 tangent = GetBezierTangentAtTime(t);

                positions.Add(point);

            }

            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
            lineRenderer.transform.localPosition = Vector3.zero;
            lineRenderer.transform.localRotation = Quaternion.Euler(90.0F, 0.0F, 0.0F); // MAKE IT FLAT
            fLastLength = fBezierChangedVal;
            lineRenderer.startWidth = fSplineWidth;
            lineRenderer.endWidth = fSplineWidth;
        }
    }
    float fLastLength = 0;

    bool bIsObjectSelectedOnGizmos = false;
    public void OnDrawGizmos()
    {
        if (Selection.activeGameObject == lineRenderer.gameObject)
            Selection.activeGameObject = this.gameObject;


        RefreshLineRenderer();

        fLastGizmosDrawTime = Time.time;
        if (UnityEditor.Selection.activeGameObject == gameObject) //// || alwaysShow)
        {
            bIsObjectSelectedOnGizmos = true;
            if (points.Count >= 2)
            {
               
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

                float fDistSum = 0.0f;

                for (int i = 0; i < points.Count; i++)
                {
                    if (false)
                    {
                        if (i < points.Count - 1)
                        {
                            var index = points[i];
                            var indexNext = points[i + 1];
                            UnityEditor.Handles.DrawBezier(index.positionWorld, indexNext.positionWorld, index.positionWorld + index.handleNextWorld,
                                indexNext.positionWorld + indexNext.handlePrevWorld, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                        }
                        else if (looped)
                        {
                            var index = points[i];
                            var indexNext = points[0];
                            UnityEditor.Handles.DrawBezier(index.positionWorld, indexNext.positionWorld, index.positionWorld + index.handleNextWorld,
                                indexNext.positionWorld + indexNext.handlePrevWorld, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                        }
                    }
                   
                    if(i>0)
                    {
                        fDistSum += Vector3.Magnitude(points[i - 1].positionWorld - points[i].positionWorld);
                    }
                }
                float cubeSize = 0.7f; // Size of the cube (width and height)
                float cubeDepth = 4.01f; // Depth of the cube (to make it flat)
                float offsetDistance = 0.001f; // Offset distance for the back face
                int sampleCount = (int)(fDistSum/(2.5f));  // Number of samples along the path

                if (sampleCount < 3)
                    sampleCount = 3;

              


                for (int i = 0; i < sampleCount; i++)
                {
                    float t = i / (float)(sampleCount - 1);
                    Vector3 point = GetBezierPositionAtTime(t);
                    Vector3 tangent = GetBezierTangentAtTime(t);
                    Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized; // Assuming an up vector for 2D/XZ paths
                    Vector3 right = Vector3.Cross(normal, tangent).normalized;

                    // Calculate the rotation for the cube
                    Quaternion rotation = Quaternion.LookRotation(tangent, normal);

                    // Draw the front cube (blue)
                    Matrix4x4 frontMatrix = Matrix4x4.TRS(point, rotation, new Vector3(cubeSize, cubeSize, cubeDepth));
                    Gizmos.matrix = frontMatrix;
                    Color32 frontColor = Color.cyan;
                    frontColor = Color.Lerp(Color.blue,Color.green,Mathf.Pow( Mathf.Abs(Vector3.Dot(rotation*Vector3.forward, Vector3.up)) ,0.9f));
                    frontColor.a = 255; // Set transparency
                    Gizmos.color = frontColor;
                   // Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);



                    // Reset Gizmos matrix and color
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.color = Color.white;
                }

                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            }

            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                Gizmos.matrix = Matrix4x4.TRS(index.positionWorld, index.rotation, Vector3.one*3.0f);
                Gizmos.color = visual.frustrumColor;
                Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);
                Gizmos.matrix = Matrix4x4.identity;
            }

            // Draw waypoint labels
            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                UnityEditor.Handles.Label(index.positionWorld, $"P {i+1}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 12, normal = new GUIStyleState { textColor = Color.white } });
            }

            // Draw event spheres
            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt.iWaypointIndex >= 0 && evt.iWaypointIndex < points.Count)
                {
                    Vector3 eventPosition = GetBezierPosition(evt.iWaypointIndex, evt.fWaypointNormalizedDist);
                    UnityEditor.Handles.color = new Color( Color.cyan.r, Color.cyan.g, Color.cyan.b,0.5f);
                    UnityEditor.Handles.SphereHandleCap(0, eventPosition, Quaternion.identity, 0.4f, EventType.Repaint);
                    UnityEditor.Handles.color = Color.black;

                    float fDistFromCam = (Camera.current.transform.position - eventPosition).magnitude/50.0f;
                    UnityEditor.Handles.Label(eventPosition + Vector3.up* fDistFromCam*0.12f, $"Event {i}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 13, normal = new GUIStyleState { textColor = new Color(0.0f, 0.0f, 0.0f, 1.5f) } });
                    UnityEditor.Handles.Label(eventPosition, $"Event {i}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 13, normal = new GUIStyleState { textColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f) } });
                    UnityEditor.Handles.Label(eventPosition - Vector3.up * fDistFromCam * 0.1f, $"Event {i}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 13, normal = new GUIStyleState { textColor = new Color(1.0f, 1.0f, 1.0f, 1.5f) } }) ;
                }
            }
        }
    }
    Vector3 GetBezierPositionAtTime(float t)
    {
        if (points.Count == 0)
            return Vector3.zero;

        if (points.Count == 1)
            return points[0].positionWorld;

        int segmentCount = points.Count - (looped ? 0 : 1);
        float segmentTime = t * segmentCount;
        int currentSegment = Mathf.FloorToInt(segmentTime);
        float segmentFraction = segmentTime - currentSegment;

        currentSegment = currentSegment % points.Count;
        int nextSegment = (currentSegment + 1) % points.Count;

        return GetBezierPosition(currentSegment, segmentFraction);
    }

    

    Vector3 GetBezierTangentAtTime(float t)
    {
        int segmentCount = points.Count - (looped ? 0 : 1);
        float segmentTime = t * segmentCount;
        int currentSegment = Mathf.FloorToInt(segmentTime);
        float segmentFraction = segmentTime - currentSegment;

        currentSegment = currentSegment % points.Count;
        int nextSegment = (currentSegment + 1) % points.Count;

        // Calculate the derivative of the cubic Bezier curve
        Vector3 p0 = points[currentSegment].positionWorld;
        Vector3 p1 = points[currentSegment].positionWorld + points[currentSegment].handleNextWorld;
        Vector3 p2 = points[nextSegment].positionWorld + points[nextSegment].handlePrevWorld;
        Vector3 p3 = points[nextSegment].positionWorld;

        return -3 * (1 - segmentFraction) * (1 - segmentFraction) * p0 +
                3 * (1 - segmentFraction) * (1 - segmentFraction) * p1 -
                6 * segmentFraction * (1 - segmentFraction) * p1 +
                6 * segmentFraction * (1 - segmentFraction) * p2 -
                3 * segmentFraction * segmentFraction * p2 +
                3 * segmentFraction * segmentFraction * p3;
    }
#endif
}

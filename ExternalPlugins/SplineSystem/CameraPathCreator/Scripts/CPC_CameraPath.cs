using PathCreation;
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
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 handleprev;
    public Vector3 handlenext;
    public CPC_ECurveType curveTypeRotation;
    public AnimationCurve rotationCurve;
    public CPC_ECurveType curveTypePosition;
    public AnimationCurve positionCurve;
    public bool chained;

    public CPC_Point(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
        handleprev = Vector3.back;
        handlenext = Vector3.forward;
        curveTypeRotation = CPC_ECurveType.EaseInAndOut;
        rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        curveTypePosition = CPC_ECurveType.Linear;
        positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        chained = true;
    }
}

[System.Serializable]
public class CPC_Follower
{
    public GameObject gameObject;
    public float fDelay = 0f;
    public float fSpeedMultiplier = 1f;

    public enum ERotationMode
    {
        E_BEZIER,
        E_BEZIER_WORLD_Y,
        E_POINT_ORIENTATION,
        E_IGNORE,
        E_LOOK_AT
    }
    public ERotationMode eRotationMode = ERotationMode.E_BEZIER;
    public Transform lookAtTarget = null;

    [HideInInspector]
    public float currentPathTime = 0f;
    [HideInInspector]
    public bool isMoving = false;

    public CPC_Follower()
    {
        fSpeedMultiplier = 1.0f;
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
}

public class CPC_CameraPath : MonoBehaviour
{
    public bool playOnAwake = false;
    public float playOnAwakeTime = 10f;
    public List<CPC_Point> points = new List<CPC_Point>();
    public CPC_Visual visual;
    public bool looped = false;
    public bool alwaysShow = true;
    public CPC_EAfterLoop afterLoop = CPC_EAfterLoop.Continue;

    public List<CPC_Follower> followers = new List<CPC_Follower>();
    public List<CPC_Event> events = new List<CPC_Event>();

    private int currentWaypointIndex;
    private float currentTimeInWaypoint;
    private float timePerSegment;

    private float normalizedTime = 0f;
    private float lastNormalizedTime = 0f;

    private bool paused = false;
    private bool playing = false;

    void Start()
    {
        foreach (var point in points)
        {
            if (point.curveTypeRotation == CPC_ECurveType.EaseInAndOut) point.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (point.curveTypeRotation == CPC_ECurveType.Linear) point.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            if (point.curveTypePosition == CPC_ECurveType.EaseInAndOut) point.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (point.curveTypePosition == CPC_ECurveType.Linear) point.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }


        if (playOnAwake)
            PlayPath(playOnAwakeTime);
    }

    public void ShowError()
    {
        Debug.LogError("Error time " + normalizedTime);
    }
   public void Update()
    {
        lastNormalizedTime = normalizedTime;
        normalizedTime += Time.deltaTime / playOnAwakeTime;
        if (normalizedTime > 1f)
        {
            normalizedTime = looped ? normalizedTime - 1f : 1f;
        }

    }

    Vector3 GetBezierPositionAtTime(float t)
    {
        if (points.Count == 0)
            return Vector3.zero;

        if (points.Count == 1)
            return points[0].position;

        int segmentCount = points.Count - (looped ? 0 : 1);
        float segmentTime = t * segmentCount;
        int currentSegment = Mathf.FloorToInt(segmentTime);
        float segmentFraction = segmentTime - currentSegment;

        currentSegment = currentSegment % points.Count;
        int nextSegment = (currentSegment + 1) % points.Count;

        return GetBezierPosition(currentSegment, segmentFraction);
    }

    IEnumerator StartFollowerAfterDelay(CPC_Follower follower)
    {
        yield return new WaitForSeconds(follower.fDelay);
        follower.isMoving = true;
    }

    float GetFollowerNormalizedTime(CPC_Follower follower,float fOffsetTime = 0.0f)
    {
        float timeIncrement = (Time.deltaTime / playOnAwakeTime) * follower.fSpeedMultiplier;
        follower.currentPathTime += timeIncrement;
        if (follower.currentPathTime > 1f)
        {
            follower.currentPathTime = looped ? follower.currentPathTime - 1f : 1f;
        }
        return follower.currentPathTime + fOffsetTime;
    }

    Vector3 GetPositionOnPath(float t)
    {
        if (points.Count == 0)
            return Vector3.zero;

        if (points.Count == 1)
            return points[0].position;

        int segmentCount = points.Count - (looped ? 0 : 1);
        float segmentTime = t * segmentCount;
        int currentSegment = Mathf.FloorToInt(segmentTime);
        float segmentFraction = segmentTime - currentSegment;

        CPC_Point p0 = points[currentSegment % points.Count];
        CPC_Point p1 = points[(currentSegment + 1) % points.Count];

        return Vector3.Lerp(p0.position, p1.position, segmentFraction);
    }

    void UpdateRotation(CPC_Follower follower, float t)
    {
        follower.gameObject.transform.rotation = GetRotation(t, follower);
    }

    private Quaternion GetRotation(float normalizedTime, CPC_Follower follower)
    {
        float pathLength = (looped) ? points.Count : points.Count - 1;
        float t = normalizedTime * pathLength;
        int pointIndex = Mathf.FloorToInt(t);
        float segmentT = t - pointIndex;

        pointIndex = pointIndex % points.Count;
        int nextIndex = GetNextIndex(pointIndex);

        float fFutureTime = 0.1f;
        switch (follower.eRotationMode)
        {
            case CPC_Follower.ERotationMode.E_BEZIER:
                {
                    if(looped == false)
                    {
                        if(normalizedTime + fFutureTime > 1.0f)
                        {
                            return follower.gameObject.transform.rotation;
                        }
                    }

                    var vStart = GetBezierPositionAtTime(normalizedTime);
                    var vNext = GetBezierPositionAtTime((normalizedTime + fFutureTime) % 1.0f);
                    Vector3 vDir = (vNext - vStart);
                    return Quaternion.LookRotation(vDir.normalized);
                }
            case CPC_Follower.ERotationMode.E_BEZIER_WORLD_Y:
                {
                    if (looped == false)
                    {
                        if (normalizedTime + fFutureTime > 1.0f)
                        {
                            return follower.gameObject.transform.rotation;
                        }
                    }

                    var vStart = GetBezierPositionAtTime(normalizedTime);
                    var vNext = GetBezierPositionAtTime((normalizedTime + 0.1f)%1.0f);
                    Vector3 vDir = (vNext - vStart);
                    vDir.y = 0.0f;
                    return Quaternion.LookRotation(vDir.normalized);
                }
            case CPC_Follower.ERotationMode.E_POINT_ORIENTATION:
                return Quaternion.LerpUnclamped(points[pointIndex].rotation, points[nextIndex].rotation, points[pointIndex].rotationCurve.Evaluate(segmentT));

            case CPC_Follower.ERotationMode.E_LOOK_AT:
                if (follower.lookAtTarget != null)
                {
                    return Quaternion.LookRotation((follower.lookAtTarget.position - follower.gameObject.transform.position).normalized);
                }
                else
                {
                    Debug.LogWarning("Look At target is not set for " + follower.gameObject.name);
                    return follower.gameObject.transform.rotation;
                }
            case CPC_Follower.ERotationMode.E_IGNORE:
            default:
                return follower.gameObject.transform.rotation;
        }
    }

    void UpdateEvents()
    {
        foreach (var evt in events)
        {
            if (!evt.hasTriggered)
            {
                if (IsTimeToTriggerEvent(evt.iWaypointIndex, evt.fWaypointNormalizedDist))
                {
                    evt.unityEvent.Invoke();
                    evt.hasTriggered = true;
                }
            }
        }
    }

    bool IsTimeToTriggerEvent(int waypointIndex, float waypointNormalizedDist)
    {
        if (currentWaypointIndex == waypointIndex && currentTimeInWaypoint >= waypointNormalizedDist)
        {
            return true;
        }

        // Handle the case when looping and the event should trigger after the loop
        if (looped && currentWaypointIndex < waypointIndex && waypointIndex == 0)
        {
            if (currentTimeInWaypoint >= waypointNormalizedDist)
            {
                return true;
            }
        }

        return false;
    }

    void ResetEvents()
    {
        foreach (var evt in events)
        {
            evt.hasTriggered = false;
        }
    }
    private float distanceTravelled = 0f;
    private VertexPath vertexPath;
    BezierPath bezierPath;

    void GenerateVertexPath()
    {
        bool bShowDebugSpehere = false;

        List<Vector3> pointsWithAnchors = new List<Vector3>();
        List<float> angles = new List<float>();

        pointsWithAnchors.Add(points[0].position);
        pointsWithAnchors.Add(points[0].position + points[0].handlenext);
        angles.Add(0);

        for (int i = 1; i < points.Count - 1; i++)
        {
            // Add the handleprev as the first control point
            pointsWithAnchors.Add(points[i].position + points[i].handleprev);
            // Add the main anchor point
            pointsWithAnchors.Add(points[i].position);
            pointsWithAnchors.Add(points[i].position + points[i].handlenext);
            angles.Add(0);
        }

        pointsWithAnchors.Add(points[points.Count - 1].position + points[points.Count - 1].handleprev);
        pointsWithAnchors.Add(points[points.Count - 1].position);
        angles.Add(0);

        if (looped)
        {
            pointsWithAnchors.Add(points[points.Count - 1].position + points[points.Count - 1].handlenext);
            pointsWithAnchors.Add(points[0].position + points[0].handleprev);
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

    }
    public void PlayPath(float time)
    {
        GenerateVertexPath();

        if (time <= 0) time = 0.001f;
        paused = false;
        playing = true;
        StopAllCoroutines();

        foreach (var follower in followers)
        {
            if (follower.gameObject != null)
            {
                follower.gameObject.transform.position = points[0].position;
                follower.isMoving = false;
                StartCoroutine(StartFollowerAfterDelay(follower));
            }
        }

        StartCoroutine(FollowPath(time));
    }

    public void StopPath()
    {
        playing = false;
        paused = false;
        StopAllCoroutines();
    }

    public void UpdateTimeInSeconds(float seconds)
    {
        timePerSegment = seconds / ((looped) ? points.Count : points.Count - 1);
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

    public int GetCurrentWayPoint()
    {
        return currentWaypointIndex;
    }

    public float GetCurrentTimeInWaypoint()
    {
        return currentTimeInWaypoint;
    }

    public void SetCurrentWayPoint(int value)
    {
        currentWaypointIndex = value;
    }

    bool bTimeChangedDuringPause = false;
    public void SetCurrentTimeInWaypoint(float value)
    {
        currentTimeInWaypoint = value;
        bTimeChangedDuringPause = true;
    }

    IEnumerator FollowPath(float time)
    {
        UpdateTimeInSeconds(time);
        currentWaypointIndex = 0;

        while (currentWaypointIndex < points.Count)
        {
            currentTimeInWaypoint = 0;
            while (currentTimeInWaypoint < 1)
            {
                if (!paused || bTimeChangedDuringPause)
                {
                    bTimeChangedDuringPause = false;

                    currentTimeInWaypoint += Time.deltaTime / timePerSegment;
                    if (currentTimeInWaypoint > 1.0f)
                        currentTimeInWaypoint = 1.0f;

                    // Update position and rotation for each follower
                    foreach (var follower in followers)
                    {
                        if (follower.isMoving && follower.gameObject != null)
                        {
                            UpdateFollowerPositionAndRotation(follower);
                        }
                    }

                    UpdateEvents();
                }

                yield return null;
            }

            ResetEvents();
            ++currentWaypointIndex;
            if (currentWaypointIndex == points.Count - 1 && !looped)
            {
                break;
            }

            if (currentWaypointIndex == points.Count && afterLoop == CPC_EAfterLoop.Continue) currentWaypointIndex = 0;
        }
        StopPath();
    }

    private void UpdateFollowerPositionAndRotation(CPC_Follower follower)
    {
        float normalizedTime = (currentWaypointIndex + currentTimeInWaypoint) / (looped ? points.Count : points.Count - 1);

        if(normalizedTime  > 1.0f)
            normalizedTime = normalizedTime % 1.0f;

        Vector3 position = GetBezierPositionAtTime(normalizedTime);
        follower.gameObject.transform.position = position;
        UpdateRotation(follower, normalizedTime);
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
                    Vector3.Lerp(points[pointIndex].position,
                        points[pointIndex].position + points[pointIndex].handlenext, t),
                    Vector3.Lerp(points[pointIndex].position + points[pointIndex].handlenext,
                        points[nextIndex].position + points[nextIndex].handleprev, t), t),
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].position + points[pointIndex].handlenext,
                        points[nextIndex].position + points[nextIndex].handleprev, t),
                    Vector3.Lerp(points[nextIndex].position + points[nextIndex].handleprev,
                        points[nextIndex].position, t), t), t);
    }

    private Quaternion GetLerpRotation(int pointIndex, float time)
    {
        return Quaternion.LerpUnclamped(points[pointIndex].rotation, points[GetNextIndex(pointIndex)].rotation, points[pointIndex].rotationCurve.Evaluate(time));
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject == gameObject || alwaysShow)
        {
            if (points.Count >= 2)
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                for (int i = 0; i < points.Count; i++)
                {
                    if (i < points.Count - 1)
                    {
                        var index = points[i];
                        var indexNext = points[i + 1];
                        UnityEditor.Handles.DrawBezier(index.position, indexNext.position, index.position + index.handlenext,
                            indexNext.position + indexNext.handleprev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                    else if (looped)
                    {
                        var index = points[i];
                        var indexNext = points[0];
                        UnityEditor.Handles.DrawBezier(index.position, indexNext.position, index.position + index.handlenext,
                            indexNext.position + indexNext.handleprev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                }
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            }

            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                Gizmos.matrix = Matrix4x4.TRS(index.position, index.rotation, Vector3.one*3.0f);
                Gizmos.color = visual.frustrumColor;
                Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);
                Gizmos.matrix = Matrix4x4.identity;
            }

            // Draw waypoint labels
            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                UnityEditor.Handles.Label(index.position, $"P {i}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 12, normal = new GUIStyleState { textColor = Color.white } });
            }

            // Draw event spheres
            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt.iWaypointIndex >= 0 && evt.iWaypointIndex < points.Count)
                {
                    Vector3 eventPosition = GetBezierPosition(evt.iWaypointIndex, evt.fWaypointNormalizedDist);
                    UnityEditor.Handles.color = Color.cyan;
                    UnityEditor.Handles.SphereHandleCap(0, eventPosition, Quaternion.identity, 0.5f, EventType.Repaint);
                    UnityEditor.Handles.Label(eventPosition, $"Event {i}", new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 12, normal = new GUIStyleState { textColor = Color.cyan } });
                }
            }
        }
    }
#endif
}

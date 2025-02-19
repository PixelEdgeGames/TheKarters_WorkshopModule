using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PTK_Bezier 
{
    public float fPointEveryDist = 2.5f;

    private List<Vector3> controlPoints = new List<Vector3>();
    private List<Vector3> originalPoints = new List<Vector3>();
    private List<Quaternion> controlRotations = new List<Quaternion>();
    private List<Quaternion> originalRotations = new List<Quaternion>();
    private List<float> arcLengthTable = new List<float>();
    private float totalArcLength = 0f;
    private int arcResolution = 200;

    [HideInInspector]
    public Transform originalBezierPointsParent;

    public struct SplinePoint
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public SplinePoint(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    public void GenerateBezier(List<Vector3> _points, List<Quaternion> _rotations)
    {
        // Store both positions and rotations
        originalPoints = new List<Vector3>();
        originalRotations = new List<Quaternion>();

        originalPoints.AddRange(_points);
        originalRotations.AddRange(_rotations);

       
        ProcessPoints();
        GenerateArcLengthTable();
    }

    public void GenerateBezier(Transform pointsParentTransform)
    {

        if (pointsParentTransform == null)
            return;

        originalBezierPointsParent = pointsParentTransform;
        // Store both positions and rotations
        originalPoints = new List<Vector3>();
        originalRotations = new List<Quaternion>();

        foreach (Transform point in pointsParentTransform.transform)
        {
            originalPoints.Add(point.position);
            originalRotations.Add(point.rotation);
        }

        ProcessPoints();
        GenerateArcLengthTable();
    }

    private void ProcessPoints()
    {
        // Process positions
        controlPoints = new List<Vector3>(originalPoints);
        int chaikinPasses = 4;
        int smoothPasses = 8;

        if (chaikinPasses > 0)
        {
            controlPoints = ChaikinSmooth(controlPoints, chaikinPasses);
        }

        if (smoothPasses > 0)
        {
            controlPoints = SmoothPoints(controlPoints, smoothPasses);
        }

        // Process rotations
        controlRotations = new List<Quaternion>(originalRotations);
        if (chaikinPasses > 0)
        {
            controlRotations = ChaikinSmoothRotations(controlRotations, chaikinPasses);
        }

        if (smoothPasses > 0)
        {
            controlRotations = SmoothRotations(controlRotations, smoothPasses);
        }
    }

    public SplinePoint GetPointAtDistance(float distance, bool useOriginalRotations = false)
    {
        if(controlPoints.Count == 0)
            return new SplinePoint(Vector3.zero, Quaternion.identity);
        if (controlPoints.Count ==1)
            return new SplinePoint(controlPoints[0],controlRotations[0]);

        distance = Mathf.Clamp(distance, 0f, totalArcLength);

        int index = 1;
        while (index < arcLengthTable.Count && arcLengthTable[index] < distance)
        {
            index++;
        }

        float lengthBefore = arcLengthTable[index - 1];
        float lengthAfter = arcLengthTable[index];
        float segmentFraction = (distance - lengthBefore) / (lengthAfter - lengthBefore);

        float tBefore = (index - 1) / (float)arcResolution;
        float tAfter = index / (float)arcResolution;
        float t = Mathf.Lerp(tBefore, tAfter, segmentFraction);

        Vector3 position = CalculateSplinePoint(t);
        Quaternion rotation;

        if (useOriginalRotations)
        {
            rotation = CalculateRotationAlongSpline(t, originalRotations);
        }
        else
        {
            rotation = CalculateRotationAlongSpline(t, controlRotations);
        }

        return new SplinePoint(position, rotation);
    }

    private Quaternion CalculateRotationAlongSpline(float t, List<Quaternion> rotations)
    {
        if (rotations.Count < 2) return rotations[0];

        float tt = t * (rotations.Count - 1);
        int i = Mathf.FloorToInt(tt);
        float fraction = tt - i;

        if (i >= rotations.Count - 1)
            return rotations[rotations.Count - 1];

        return Quaternion.Slerp(rotations[i], rotations[i + 1], fraction);
    }

    private List<Quaternion> ChaikinSmoothRotations(List<Quaternion> rotations, int iterations)
    {
        if (rotations.Count < 2)
            return rotations;

        List<Quaternion> result = new List<Quaternion>(rotations);
        for (int iter = 0; iter < iterations; iter++)
        {
            List<Quaternion> newRotations = new List<Quaternion>();
            newRotations.Add(result[0]);

            for (int i = 0; i < result.Count - 1; i++)
            {
                Quaternion q0 = result[i];
                Quaternion q1 = result[i + 1];

                Quaternion Q = Quaternion.Slerp(q0, q1, 0.25f);
                Quaternion R = Quaternion.Slerp(q0, q1, 0.75f);

                newRotations.Add(Q);
                newRotations.Add(R);
            }

            newRotations.Add(result[result.Count - 1]);
            result = newRotations;
        }
        return result;
    }

    private List<Quaternion> SmoothRotations(List<Quaternion> rotations, int passes)
    {
        var result = new List<Quaternion>(rotations);

        for (int pass = 0; pass < passes; pass++)
        {
            var tempRotations = new List<Quaternion>(result);

            for (int i = 1; i < result.Count - 1; i++)
            {
                Quaternion prev = result[i - 1];
                Quaternion curr = result[i];
                Quaternion next = result[i + 1];

                // Weighted spherical interpolation
                Quaternion avg = curr;
                avg = Quaternion.Slerp(avg, prev, 0.25f);
                avg = Quaternion.Slerp(avg, next, 0.25f);

                tempRotations[i] = avg;
            }

            result = tempRotations;
        }

        return result;
    }

    public float GetTotalLength()
    {
        return totalArcLength;
    }

    public List<SplinePoint> GetOriginalPoints()
    {
        List<SplinePoint> points = new List<SplinePoint>();
        for (int i = 0; i < originalPoints.Count; i++)
        {
            points.Add(new SplinePoint(originalPoints[i], originalRotations[i]));
        }
        return points;
    }

    public List<SplinePoint> GetControlPoints()
    {
        List<SplinePoint> points = new List<SplinePoint>();
        for (int i = 0; i < controlPoints.Count; i++)
        {
            points.Add(new SplinePoint(controlPoints[i], controlRotations[i]));
        }
        return points;
    }

    private void GenerateArcLengthTable()
    {
        arcLengthTable.Clear();
        totalArcLength = 1f;
        Vector3 prevPoint = CalculateSplinePoint(0f);
        arcLengthTable.Add(0f);

        if (controlPoints.Count <= 1)
            return;

        // Special case for only 2 control points
        if (controlPoints.Count == 2)
        {
            totalArcLength = Vector3.Distance(controlPoints[0], controlPoints[1]);
            arcLengthTable.Add(totalArcLength);
            return;
        }

        float distLinear = 0f;
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            distLinear += Vector3.Magnitude(controlPoints[i] - controlPoints[i + 1]);
        }

        float everyM = fPointEveryDist;
        int minResolution = 2;
        arcResolution = Mathf.Max(minResolution, (int)(distLinear / everyM));

        for (int i = 1; i <= arcResolution; i++)
        {
            float t = i / (float)arcResolution;
            Vector3 currPoint = CalculateSplinePoint(t);
            totalArcLength += Vector3.Distance(prevPoint, currPoint);
            arcLengthTable.Add(totalArcLength);
            prevPoint = currPoint;
        }
    }

    private Vector3 CalculateSplinePoint(float t, float tension = 0.5f)
    {
        if (controlPoints.Count < 2) return controlPoints[0];

        Vector3 p0, p1, p2, p3;
        float tt = t * (controlPoints.Count - 1);
        int i = Mathf.FloorToInt(tt);
        t = tt - i;

        if (i == 0)
        {
            p0 = controlPoints[0];
            p1 = controlPoints[0];
            p2 = controlPoints[1];
            p3 = controlPoints.Count > 2 ? controlPoints[2] : controlPoints[1];
        }
        else if (i == controlPoints.Count - 2)
        {
            p0 = controlPoints[i - 1];
            p1 = controlPoints[i];
            p2 = controlPoints[i + 1];
            p3 = controlPoints[i + 1];
        }
        else if (i >= controlPoints.Count - 1)
        {
            p0 = controlPoints[controlPoints.Count - 2];
            p1 = controlPoints[controlPoints.Count - 1];
            p2 = controlPoints[controlPoints.Count - 1];
            p3 = controlPoints[controlPoints.Count - 1];
        }
        else
        {
            p0 = controlPoints[i - 1];
            p1 = controlPoints[i];
            p2 = controlPoints[i + 1];
            p3 = controlPoints[i + 2];
        }

        float t2 = t * t;
        float t3 = t2 * t;
        float s = (1 - tension) / 2f;

        return (2 * t3 - 3 * t2 + 1) * p1 +
               (t3 - 2 * t2 + t) * (s * (p2 - p0)) +
               (-2 * t3 + 3 * t2) * p2 +
               (t3 - t2) * (s * (p3 - p1));
    }
    private List<Vector3> ChaikinSmooth(List<Vector3> points, int iterations = 2)
    {
        if (points.Count < 2)
            return points;

        // First pass: Add intermediate points to increase resolution
        List<Vector3> densePoints = new List<Vector3>();
        densePoints.Add(points[0]);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[i + 1];

            // Add more intermediate points (can adjust these values for different smoothness)
            for (float t = 0.2f; t < 1.0f; t += 0.2f)
            {
                densePoints.Add(Vector3.Lerp(p0, p1, t));
            }
        }
        densePoints.Add(points[points.Count - 1]);

        // Second pass: Apply Chaikin smoothing with weighted averaging
        List<Vector3> result = new List<Vector3>(densePoints);
        for (int iter = 0; iter < iterations; iter++)
        {
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(result[0]); // Keep first point

            // Enhanced smoothing with wider neighborhood
            for (int i = 0; i < result.Count - 1; i++)
            {
                Vector3 p0 = result[i];
                Vector3 p1 = result[i + 1];

                // Use different ratios for more averaging
                Vector3 Q = Vector3.Lerp(p0, p1, 0.35f); // Changed from 0.25f
                Vector3 R = Vector3.Lerp(p0, p1, 0.65f); // Changed from 0.75f

                // Additional smoothing: average with neighbors if available
                if (i > 0)
                {
                    Vector3 prev = result[i - 1];
                    Q = Vector3.Lerp(Q, (prev + p0 + p1) / 3f, 0.3f);
                }
                if (i < result.Count - 2)
                {
                    Vector3 next = result[i + 2];
                    R = Vector3.Lerp(R, (p0 + p1 + next) / 3f, 0.3f);
                }

                newPoints.Add(Q);
                newPoints.Add(R);
            }

            newPoints.Add(result[result.Count - 1]); // Keep last point
            result = newPoints;

            // Optional: Apply additional moving average smoothing after each iteration
            if (iter < iterations - 1) // Don't smooth on final iteration
            {
                result = MovingAverageSmooth(result, 5);
            }
        }

        return result;
    }

    private List<Vector3> MovingAverageSmooth(List<Vector3> points, int windowSize)
    {
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(points[0]); // Keep first point unchanged

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;

            // Calculate average of neighboring points
            for (int j = -windowSize; j <= windowSize; j++)
            {
                int index = i + j;
                if (index >= 0 && index < points.Count)
                {
                    sum += points[index];
                    count++;
                }
            }

            smoothed.Add(sum / count);
        }

        smoothed.Add(points[points.Count - 1]); // Keep last point unchanged
        return smoothed;
    }
    private List<Vector3> SmoothPoints(List<Vector3> points, int passes)
    {
        var result = new List<Vector3>(points);

        for (int pass = 0; pass < passes; pass++)
        {
            var tempPoints = new List<Vector3>(result);

            for (int i = 1; i < result.Count - 1; i++)
            {
                Vector3 prev = result[i - 1];
                Vector3 curr = result[i];
                Vector3 next = result[i + 1];

                tempPoints[i] = prev * 0.25f + curr * 0.5f + next * 0.25f;
            }

            result = tempPoints;
        }

        return result;
    }

    [System.Serializable]
    public class GizmoSettings
    {
        [Header("Visibility")]
        public bool bShowLinearPath = true;
        public bool bShowBezierPath = true;
        public bool bShowPoints = true;
        public bool bShowForwardVectors = true;
        public bool bShowUpVectors = true;

        [Header("Colors")]
        public Color linearPathColor = Color.red;
        public Color bezierPathColor = Color.cyan;
        public Color pointsColor = Color.yellow;
        public Color forwardVectorColor = Color.blue;
        public Color upVectorColor = Color.green;

        [Header("Sizes")]
        public float pointSize = 0.25f;
        public float vectorLength = 1.0f;
        public float linearPathWidth = 2f;
        public float bezierPathWidth = 2f;
    }

    public GizmoSettings gizmoSettings = new GizmoSettings();

    public void SetGizmoSettings(GizmoSettings settings)
    {
        gizmoSettings = settings;
    }

    public void DrawGizmos()
    {
        if (originalPoints == null || originalPoints.Count == 0)
            return;

        // Draw original linear path
        if (gizmoSettings.bShowLinearPath)
        {
            Gizmos.color = gizmoSettings.linearPathColor;
            var originalSplinePoints = GetOriginalPoints();
            for (int i = 0; i < originalSplinePoints.Count - 1; i++)
            {
                Gizmos.DrawLine(originalSplinePoints[i].Position, originalSplinePoints[i + 1].Position);
            }
        }

        // Draw bezier path
        if (gizmoSettings.bShowBezierPath)
        {
            Gizmos.color = gizmoSettings.bezierPathColor;
            float stepSize = totalArcLength / arcResolution;
            Vector3 prevPos = GetPointAtDistance(0).Position;

            for (float dist = stepSize; dist <= totalArcLength; dist += stepSize)
            {
                Vector3 currentPos = GetPointAtDistance(dist).Position;
                Gizmos.DrawLine(prevPos, currentPos);
                prevPos = currentPos;
            }
        }


        // Draw points and direction vectors
        if (gizmoSettings.bShowPoints || gizmoSettings.bShowForwardVectors || gizmoSettings.bShowUpVectors)
        {
            float step = 1f / arcResolution;
            for (float t = 0; t <= 1f; t += step)
            {
                float distance = t * totalArcLength;
                SplinePoint point = GetPointAtDistance(distance);

                // Draw point
                if (gizmoSettings.bShowPoints)
                {
                    Gizmos.color = gizmoSettings.pointsColor;
                    Gizmos.DrawSphere(point.Position, gizmoSettings.pointSize);
                }

                // Draw forward vector
                if (gizmoSettings.bShowForwardVectors)
                {
                    Gizmos.color = gizmoSettings.forwardVectorColor;
                    Gizmos.DrawRay(point.Position, point.Rotation * Vector3.forward * gizmoSettings.vectorLength);
                }

                // Draw up vector
                if (gizmoSettings.bShowUpVectors)
                {
                    Gizmos.color = gizmoSettings.upVectorColor;
                    Gizmos.DrawRay(point.Position, point.Rotation * Vector3.up * gizmoSettings.vectorLength);
                }
            }
        }
    }


}
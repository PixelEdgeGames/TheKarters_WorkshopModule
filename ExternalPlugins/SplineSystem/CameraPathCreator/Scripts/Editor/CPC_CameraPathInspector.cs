using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

public enum CPC_EManipulationModes
{
    Free,
    SelectAndTransform
}

public enum CPC_ENewWaypointMode
{
    SelectedIndex,
    LastWaypoint,
    SceneCamera,
    WorldCenter
}

[CustomEditor(typeof(CPC_CameraPath))]
public class CPC_CameraPathInspector : Editor
{
    private CPC_CameraPath t;
    private ReorderableList pointReorderableList;
    private ReorderableList followerReorderableList;
    private ReorderableList eventReorderableList;

    // Editor variables
    private bool visualFoldout;
    private bool manipulationFoldout;
    private bool showRawValues;
    private CPC_EManipulationModes cameraTranslateMode;
    private CPC_EManipulationModes cameraRotationMode;
    private CPC_EManipulationModes handlePositionMode;
    private CPC_ENewWaypointMode waypointMode = CPC_ENewWaypointMode.LastWaypoint;
    private int waypointIndex = 1;
    private CPC_ECurveType allCurveType = CPC_ECurveType.Custom;
    private AnimationCurve allAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // GUIContents
    private GUIContent addPointContent = new GUIContent("Add Point", "Adds a waypoint at the scene view camera's position/rotation");
    private GUIContent testButtonContent = new GUIContent("Reset & Init", "Only available in play mode");
    private GUIContent pauseButtonContent = new GUIContent("Pause", "Paused Camera at current Position");
    private GUIContent continueButtonContent = new GUIContent("Continue", "Continues Path at current position");
    private GUIContent stopButtonContent = new GUIContent("Stop", "Stops the playback");
    private GUIContent deletePointContent = new GUIContent("X", "Deletes this waypoint");
    private GUIContent gotoPointContent = new GUIContent("Select", "Teleports the scene camera to the specified waypoint");
    private GUIContent relocateContent = new GUIContent("Relocate", "Relocates the specified camera to the current view camera's position/rotation");
    private GUIContent alwaysShowContent = new GUIContent("Always show", "When true, shows the curve even when the GameObject is not selected - \"Inactive path color\" will be used as path color instead");
    private GUIContent chainedContent_ = new GUIContent("o───o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");
    private GUIContent unchainedContent = new GUIContent("o─x─o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");
    private GUIContent flatContent_3 = new GUIContent("Anchors Y - Locked", "Toggles if the handles of the specified waypoint should be kept flat (y-axis locked)");
    private GUIContent notFlatContent_3 = new GUIContent("Anchors Y - Unlocked", "Toggles if the handles of the specified waypoint should be kept flat (y-axis locked)");
    private GUIContent replaceAllPositionContent = new GUIContent("Replace all position lerps", "Replaces curve types (and curves when set to \"Custom\") of all the waypoint position lerp types with the specified values");
    private GUIContent replaceAllRotationContent = new GUIContent("Replace all rotation lerps", "Replaces curve types (and curves when set to \"Custom\") of all the waypoint rotation lerp types with the specified values");

    // Serialized Properties
    private SerializedObject serializedObjectTarget;
    private SerializedProperty useMainCameraProperty;
    private SerializedProperty selectedCameraProperty;
    private SerializedProperty lookAtTargetProperty;
    private SerializedProperty lookAtTargetTransformProperty;
    private SerializedProperty playOnAwakeProperty;
    private SerializedProperty speedProperty;
    private SerializedProperty visualPathProperty;
    private SerializedProperty visualInactivePathProperty;
    private SerializedProperty visualFrustumProperty;
    private SerializedProperty visualHandleProperty;
    private SerializedProperty loopedProperty;
    private SerializedProperty alwaysShowProperty;
    private SerializedProperty followersProperty;
    private SerializedProperty eventsProperty;

    private int selectedIndex = -1;

    private float currentTime;
    private float previousTime;

    private bool hasScrollBar = false;

    void OnEnable()
    {
        EditorApplication.update += Update;

        t = (CPC_CameraPath)target;
        if (t == null) return;

        SetupEditorVariables();
        GetVariableProperties();
        SetupReorderableList();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObjectTarget.Update();
        DrawPlaybackWindow();

        if(t.bIsRuntimeEditingPath == true || Application.isPlaying == false)
        {
            Rect scale = GUILayoutUtility.GetLastRect();
            hasScrollBar = (Screen.width - scale.width <= 12);
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            GUILayout.Space(5);
            DrawBasicSettings();
            GUILayout.Space(5);
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            DrawVisualDropdown();
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            DrawManipulationDropdown();
            GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
            GUILayout.Space(10);
            DrawWaypointList();
            GUILayout.Space(10);
            DrawFollowerList();
            GUILayout.Space(10);
            DrawEventList();
            GUILayout.Space(10);
            DrawRawValues();
        }
        serializedObjectTarget.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (t.points.Count >= 2)
        {
            for (int i = 0; i < t.points.Count; i++)
            {
                DrawHandles(i);
                Handles.color = Color.white;
            }
        }
    }

    void SelectIndex(int index)
    {
        selectedIndex = index;
        pointReorderableList.index = index;
        Repaint();
    }

    void SetupEditorVariables()
    {
        cameraTranslateMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraTranslateMode", 1);
        cameraRotationMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraRotationMode", 1);
        handlePositionMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_handlePositionMode", 0);
        waypointMode = (CPC_ENewWaypointMode)PlayerPrefs.GetInt("CPC_waypointMode", 0);
    }

    void GetVariableProperties()
    {
        serializedObjectTarget = new SerializedObject(t);
        useMainCameraProperty = serializedObjectTarget.FindProperty("useMainCamera");
        selectedCameraProperty = serializedObjectTarget.FindProperty("selectedCamera");
        lookAtTargetProperty = serializedObjectTarget.FindProperty("lookAtTarget");
        lookAtTargetTransformProperty = serializedObjectTarget.FindProperty("target");
        visualPathProperty = serializedObjectTarget.FindProperty("visual.pathColor");
        visualInactivePathProperty = serializedObjectTarget.FindProperty("visual.inactivePathColor");
        visualFrustumProperty = serializedObjectTarget.FindProperty("visual.frustrumColor");
        visualHandleProperty = serializedObjectTarget.FindProperty("visual.handleColor");
        loopedProperty = serializedObjectTarget.FindProperty("looped");
        alwaysShowProperty = serializedObjectTarget.FindProperty("alwaysShow");
        playOnAwakeProperty = serializedObjectTarget.FindProperty("playOnAwake");
        speedProperty = serializedObjectTarget.FindProperty("fBezierSpeed");
        followersProperty = serializedObjectTarget.FindProperty("followers");
        eventsProperty = serializedObjectTarget.FindProperty("events");
    }

    bool bLastShowPointList = false;
    void SetupReorderableList()
    {
        pointReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("points"), true, true, false, false);

        pointReorderableList.elementHeight *= 2;
        // Adjust element height based on visibility
        pointReorderableList.elementHeight = t.bShowPointList ? EditorGUIUtility.singleLineHeight * 2 : 0;
        bLastShowPointList = t.bShowPointList;

        pointReorderableList.drawElementCallback = (rect, index, active, focused) =>
        {
            if(t.bShowPointList != bLastShowPointList)
            {
                SetupReorderableList();
            }

            if (t.bShowPointList == false)
            {
                return;
            }

            float startRectY = rect.y;
            if (index > t.points.Count - 1) return;
            rect.height -= 2;
            float fullWidth = rect.width - 16 * (hasScrollBar ? 1 : 0);
            rect.width = 40;
            fullWidth -= 40;
            rect.height /= 2;

            if (index % 2 == 0)
                GUI.color = Color.red * 0.7f + Color.yellow * 0.4F + Color.white * 0.4f;
            else
                GUI.color = Color.red * 1.0f + Color.yellow * 0.2F + Color.white * 0.2f;

            GUI.Label(rect, "P" + (index + 1));

            GUI.color = Color.white;

            rect.y += rect.height - 3;
            rect.x -= 14;
            rect.width += 12;
            if (GUI.Button(rect, t.points[index].chained ? chainedContent_ : unchainedContent))
            {
                Undo.RecordObject(t, "Changed chain type");
                t.points[index].chained = !t.points[index].chained;
            }

            // Adjusted width for bKeepFlat button to be 3x wider
            rect.x += rect.width + 2;
            rect.width *= 3; // Triple the width

            if(t.points[index].bKeepFlat)
                GUI.backgroundColor = (Color.green + Color.yellow)*0.5f;
            else
                GUI.backgroundColor = (Color.red + Color.yellow) * 0.5f;

            if (GUI.Button(rect, t.points[index].bKeepFlat ? flatContent_3 : notFlatContent_3))
            {
                Undo.RecordObject(t, "Changed keep flat type");
                t.points[index].bKeepFlat = !t.points[index].bKeepFlat;
                if (t.points[index].bKeepFlat)
                {
                    t.points[index].ResetHandleLocalY();
                }
            }
            GUI.backgroundColor = Color.white;

            rect.x += rect.width + 2;
            rect.y = startRectY;

            // Adjusting the width of the position and rotation lerp sections to compensate
            float adjustedWidth = (fullWidth - 22) / 3 - 1;
            rect.width = adjustedWidth / 2; // Make the position section smaller to accommodate wider bKeepFlat button

            // Position Lerp
            /*
            EditorGUI.BeginChangeCheck();
            CPC_ECurveType tempP = (CPC_ECurveType)EditorGUI.EnumPopup(rect, t.points[index].curveTypePosition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed enum value");
                t.points[index].curveTypePosition = tempP;
            }
            rect.y += pointReorderableList.elementHeight / 2 - 4;
            GUI.enabled = t.points[index].curveTypePosition == CPC_ECurveType.Custom;
            AnimationCurve tempACP = EditorGUI.CurveField(rect, t.points[index].positionCurve);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed curve");
                t.points[index].positionCurve = tempACP;
            }
            GUI.enabled = true;

            rect.x += rect.width + 2;
            rect.y = startRectY;

            // Rotation Lerp - Adjusted to be smaller
            rect.width = adjustedWidth / 2; // Smaller rotation section
            EditorGUI.BeginChangeCheck();
            CPC_ECurveType temp = (CPC_ECurveType)EditorGUI.EnumPopup(rect, t.points[index].curveTypeRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed enum value");
                t.points[index].curveTypeRotation = temp;
            }
            rect.y += pointReorderableList.elementHeight / 2 - 4;
            GUI.enabled = t.points[index].curveTypeRotation == CPC_ECurveType.Custom;
            AnimationCurve tempAC = EditorGUI.CurveField(rect, t.points[index].rotationCurve);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed curve");
                t.points[index].rotationCurve = tempAC;
            }
            */
            GUI.enabled = true;

            rect.y = startRectY+20;
            rect.height *= 2;
            rect.x += rect.width + 2-80;
            rect.width = adjustedWidth*0.8F; // Restore original width for the final section
            rect.height = rect.height / 2 - 1;
            if (GUI.Button(rect, gotoPointContent))
            {
                pointReorderableList.index = index;
                selectedIndex = index;
            }
            rect.y += rect.height + 2;

            rect.height = (rect.height + 1) * 2-20;
            rect.y = startRectY+20;
            rect.x += rect.width + 2+60;
            rect.width = 65;


            if (index % 2 == 0)
                GUI.color = Color.red*0.7f + Color.yellow*0.4F + Color.white*0.4f;
            else
                GUI.color = Color.red * 1.0f + Color.yellow * 0.2F + Color.white * 0.2f;

            if (GUI.Button(rect,   "DEL (P" + (index+1) + ")"))
            {
                Undo.RecordObject(t, "Deleted a waypoint");
                t.points.RemoveAt(index);


                if (index == waypointIndex)
                {
                    waypointIndex--; if (waypointIndex < 0) waypointIndex = 0;
                    SelectIndex( waypointIndex);
                }

                t.GenerateVertexPath(); // so we can get next point on curve if not looped
                SceneView.RepaintAll();
            }
            GUI.color = Color.white;

            if (selectedIndex == index && !active)
            {
                pointReorderableList.index = selectedIndex;
            }
        };

        pointReorderableList.drawHeaderCallback = rect =>
        {
            float fullWidth = rect.width;
            rect.width = 56;
            GUI.Label(rect, "Sum: " + t.points.Count);
            rect.x += rect.width;
            rect.width = (fullWidth - 78) / 3;
            GUI.Label(rect, "");

            rect.x -= 0;

            GUI.backgroundColor = t.bShowPointList == true ? Color.yellow * 0.5f : Color.cyan;
           if( GUI.Button(rect, t.bShowPointList ? "Hide Point List" : "Show Point List"))
            {
             t.  bShowPointList = !t.bShowPointList;
            }

        };

        pointReorderableList.onSelectCallback = l =>
        {
            selectedIndex = l.index;
            SceneView.RepaintAll();
        };
    }

    void DrawPlaybackWindow()
    {
        GUI.enabled = Application.isPlaying;
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
     
        if(t.bIsRuntimeEditingPath == false)
        {
            if (GUILayout.Button("Restart"))
            {
                t.StopPath();
                t.PlayPath(t.fBezierSpeed, false, true);
            }

            GUI.backgroundColor = Color.yellow;
            if(GUILayout.Button("Edit Path"))
            {
                t.bIsRuntimeEditingPath = true;
                t.SaveClosestPointAtCurrentDistance();
                t.StopPath();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.enabled = false;
            if (GUILayout.Button("Restart"))
            {
            }
            GUI.enabled = true;


            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Stop Editing"))
            {
                t.bIsRuntimeEditingPath = false;
                t.PlayPath(t.fBezierSpeed,true,true);
            }
            GUI.backgroundColor = Color.white;
        }
        GUI.enabled = t.bIsRuntimeEditingPath ;
     
        GUILayout.EndHorizontal();
      
        GUI.enabled = false;
        Rect rr = GUILayoutUtility.GetRect(4, 8);
        float endWidth = rr.width - 60;
        rr.y -= 4;
        rr.width = 4;
        int c = t.points.Count + ((t.looped) ? 1 : 0);
        for (int i = 0; i < c; ++i)
        {
            GUI.Box(rr, "");
            rr.x += endWidth / (c - 1);
        }
        GUILayout.EndVertical();
        GUI.enabled = true;
    }

    void DrawBasicSettings()
    {
        if (Mathf.Abs(Time.time - t.fLastGizmosDrawTime) > 0.5f)
        {
            GUI.backgroundColor = Color.red*2.0f;
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Ensure Gizmos in Scene View are enabled to see Bezier Path");
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Path Length: " + t.fLastCalculatedPathLength);
        GUILayout.FlexibleSpace();
        loopedProperty.boolValue = GUILayout.Toggle(loopedProperty.boolValue, "Path Looped");
        GUILayout.Space(10);
        playOnAwakeProperty.boolValue = GUILayout.Toggle(playOnAwakeProperty.boolValue, "Play on awake");
        GUILayout.Space(10);
        GUI.enabled = playOnAwakeProperty.boolValue;
        GUILayout.Label("Speed: ");
        speedProperty.floatValue = EditorGUILayout.FloatField(speedProperty.floatValue,GUILayout.Width(100));
        GUILayout.Space(10);
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

    void DrawVisualDropdown()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        visualFoldout = EditorGUILayout.Foldout(visualFoldout, "Visual");
        alwaysShowProperty.boolValue = GUILayout.Toggle(alwaysShowProperty.boolValue, alwaysShowContent);
        GUILayout.EndHorizontal();
        if (visualFoldout)
        {
            GUILayout.BeginVertical("Box");
            visualPathProperty.colorValue = EditorGUILayout.ColorField("Path color", visualPathProperty.colorValue);
            visualInactivePathProperty.colorValue = EditorGUILayout.ColorField("Inactive path color", visualInactivePathProperty.colorValue);
            visualFrustumProperty.colorValue = EditorGUILayout.ColorField("Frustum color", visualFrustumProperty.colorValue);
            visualHandleProperty.colorValue = EditorGUILayout.ColorField("Handle color", visualHandleProperty.colorValue);
            if (GUILayout.Button("Default colors"))
            {
                Undo.RecordObject(t, "Reset to default color values");
                t.visual = new CPC_Visual();
            }
            GUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void DrawManipulationDropdown()
    {
        manipulationFoldout = EditorGUILayout.Foldout(manipulationFoldout, "Transform manipulation modes");
        EditorGUI.BeginChangeCheck();
        if (manipulationFoldout)
        {
            GUILayout.BeginVertical("Box");
            cameraTranslateMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Translation", cameraTranslateMode);
            cameraRotationMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Rotation", cameraRotationMode);
            handlePositionMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Handle Translation", handlePositionMode);
            GUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("CPC_cameraTranslateMode", (int)cameraTranslateMode);
            PlayerPrefs.SetInt("CPC_cameraRotationMode", (int)cameraRotationMode);
            PlayerPrefs.SetInt("CPC_handlePositionMode", (int)handlePositionMode);
            SceneView.RepaintAll();
        }
    }

    void DrawWaypointList()
    {
        /*
        GUILayout.Label("Replace all lerp types");
        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        allCurveType = (CPC_ECurveType)EditorGUILayout.EnumPopup(allCurveType, GUILayout.Width(Screen.width / 3f));
        if (GUILayout.Button(replaceAllPositionContent))
        {
            Undo.RecordObject(t, "Applied new position");
            foreach (var index in t.points)
            {
                index.curveTypePosition = allCurveType;
                if (allCurveType == CPC_ECurveType.Custom)
                    index.positionCurve.keys = allAnimationCurve.keys;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.enabled = allCurveType == CPC_ECurveType.Custom;
        allAnimationCurve = EditorGUILayout.CurveField(allAnimationCurve, GUILayout.Width(Screen.width / 3f));
        GUI.enabled = true;
        if (GUILayout.Button(replaceAllRotationContent))
        {
            Undo.RecordObject(t, "Applied new rotation");
            foreach (var index in t.points)
            {
                index.curveTypeRotation = allCurveType;
                if (allCurveType == CPC_ECurveType.Custom)
                    index.rotationCurve = allAnimationCurve;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        */
        GUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width / 2f - 20);
        GUILayout.Label("↓");
        GUILayout.EndHorizontal();
        serializedObject.Update();
        pointReorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        Rect r = GUILayoutUtility.GetRect(Screen.width - 16, 18);
        r.y -= 10;
        GUILayout.Space(-30);
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = (Color.green*0.6f + Color.yellow * 0.5f + Color.white * 0.3f)*0.5f;
        if (GUILayout.Button(addPointContent,GUILayout.Width(300)))
        {
            Undo.RecordObject(t, "Added camera path point");

            if(t.points.Count < 2)
            {
                if(t.points.Count == 0)
                    t.points.Add(new CPC_Point(t.transform, t.transform.position + t.transform.rotation * new Vector3(-10.0f,0.0f,0.0f), Quaternion.identity) { handleNextWorld = t.transform.rotation * new Vector3(0.0f, 0.0f, 10.0f), handlePrevWorld = t.transform.rotation * new Vector3(0.0f, 0.0f, -10.0f) });
               
                t.points.Add(new CPC_Point(t.transform, t.transform.position + t.transform.rotation * new Vector3(10.0f, 0.0f, 0.0f), Quaternion.identity) { handleNextWorld = t.transform.rotation * new Vector3(0.0f, 0.0f, -10.0f), handlePrevWorld = t.transform.rotation * new Vector3(0.0f, 0.0f, 10.0f) });
            }
            else
            {
                switch (waypointMode)
                {
                    case CPC_ENewWaypointMode.SceneCamera:
                        t.points.Add(new CPC_Point(t.transform, SceneView.lastActiveSceneView.camera.transform.position, SceneView.lastActiveSceneView.camera.transform.rotation));
                        SelectIndex(t.points.Count - 1);
                        break;
                    case CPC_ENewWaypointMode.LastWaypoint:
                        if (t.points.Count > 0)
                        {
                            t.points.Add(new CPC_Point(t.transform, t.points[t.points.Count - 1].positionWorld, t.points[t.points.Count - 1].rotation) { handleNextWorld = t.points[t.points.Count - 1].handleNextWorld, handlePrevWorld = t.points[t.points.Count - 1].handlePrevWorld });

                            SelectIndex(t.points.Count - 1);
                        }
                        else
                        {
                            t.points.Add(new CPC_Point(t.transform, Vector3.zero, Quaternion.identity));

                            SelectIndex(t.points.Count - 1);
                            Debug.LogWarning("No previous waypoint found to place this waypoint, defaulting position to world center");
                        }
                        break;
                    case CPC_ENewWaypointMode.SelectedIndex:
                        bool bLooped = t.looped;
                        CPC_Point pointCur = t.points[waypointIndex]; Vector3 pointCurPos = pointCur.positionWorld;
                        int iNextPointIndex = ((waypointIndex + 1) % t.points.Count);
                        CPC_Point pointNext = t.points[iNextPointIndex]; Vector3 pointNextPos = pointNext.positionWorld;

                        t.GenerateVertexPath(); // so we can get next point on curve if not looped


                        Vector3 vNewPointPos = Vector3.zero;

                        Vector3 vNewPointPrev = pointCur.handlePrevWorld;
                        Vector3 vNewPointNext = pointCur.handleNextWorld;


                        bool bIsLastPointSelected = waypointIndex == (t.points.Count - 1);
                        if (bIsLastPointSelected == true && bLooped == false)
                        {
                            var vPointDist = t.vertexPath.GetClosestDistanceAlongPath(pointCur.positionWorld);

                            Vector3 vPointBefore = t.vertexPath.GetPointAtDistance(vPointDist - 1.0f);
                            Vector3 vPathDirection = (pointCur.positionWorld - vPointBefore).normalized;

                            // path not looped and last selected - add in direction
                            vNewPointPos = pointCur.positionWorld + vPathDirection*5.0f;


                            // Calculate control points
                            vNewPointPrev = -vPathDirection * 2.5f;
                            vNewPointNext =  vPathDirection * 2.5f;
                        }
                        else
                        {
                            var vPointDist = t.vertexPath.GetClosestDistanceAlongPath(pointCur.positionWorld);
                            var vPointDist2 = t.vertexPath.GetClosestDistanceAlongPath(pointNext.positionWorld);
                            if(iNextPointIndex == 0)
                            {
                                vPointDist2 = t.vertexPath.length;
                            }
                            // path looped or between points - add between
                            float fLookForDist = vPointDist + Mathf.Abs(vPointDist - vPointDist2) * 0.5f;
                            vNewPointPos = t.vertexPath.GetPointAtDistance(fLookForDist);


                            // Calculate control points
                            Vector3 dirToNext = (pointNextPos - pointCurPos).normalized;
                            float distToNext = Vector3.Distance(pointCurPos, pointNextPos);
                            vNewPointPrev =  -dirToNext * (distToNext * 0.25f);
                            vNewPointNext =  dirToNext * (distToNext * 0.25f);
                        }


                        CPC_Point pointAdded = new CPC_Point(t.transform, vNewPointPos, Quaternion.Lerp(pointCur.rotation, pointNext.rotation, 0.5F)) { handleNextWorld = vNewPointNext, handlePrevWorld = vNewPointPrev };

                        if ((waypointIndex + 1) < t.points.Count)
                        {
                            t.points.Insert((waypointIndex + 1), pointAdded);
                            SelectIndex(waypointIndex + 1);
                        }
                        else
                        {
                            t.points.Add(pointAdded);

                            SelectIndex(t.points.Count - 1);
                        }
                        break;
                    case CPC_ENewWaypointMode.WorldCenter:
                        t.points.Add(new CPC_Point(t.transform, Vector3.zero, Quaternion.identity));

                        SelectIndex(t.points.Count - 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            t.GenerateVertexPath(); // so we can get next point on curve if not looped
            SceneView.RepaintAll();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.Label("at", GUILayout.Width(20));
        EditorGUI.BeginChangeCheck();
        waypointMode = (CPC_ENewWaypointMode)EditorGUILayout.EnumPopup(waypointMode, waypointMode == CPC_ENewWaypointMode.SelectedIndex ? GUILayout.Width(Screen.width / 4) : GUILayout.Width(Screen.width / 2));
        if (waypointMode == CPC_ENewWaypointMode.SelectedIndex)
        {
            GUILayout.FlexibleSpace();
            if(t.points.Count > 0)
            waypointIndex = (selectedIndex)% t.points.Count;
        }
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("CPC_waypointMode", (int)waypointMode);
        }
        GUILayout.EndHorizontal();
    }

    void DrawFollowerList()
    {
        Color followerBackgroundColor = new Color(0.9f, 0.85f, 0.8f); // Light background color
        Color buttonColor = new Color(1.0f, 0.6f, 0.4f); // Coral color for buttons

        var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUI.backgroundColor = buttonColor;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Followers", boldLabelStyle);
        GUILayout.BeginVertical("Box");

        serializedObject.Update();
        int followerCount = followersProperty.arraySize;

        // Color scheme for followers

        // Reset to button color and bold style for "Add Follower" button
        GUI.backgroundColor = buttonColor;
        GUI.contentColor = Color.white;
        var boldButtonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };



        for (int i = 0; i < followerCount; i++)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            var element = followersProperty.GetArrayElementAtIndex(i);

            var gameObjectProp = element.FindPropertyRelative("gameObject");
            string gameObjectName = gameObjectProp.objectReferenceValue != null ? gameObjectProp.objectReferenceValue.name : "None";

            bool bExpandedEditorGUI = element.FindPropertyRelative("bExpandedEditorGUI").boolValue;
            //  bExpandedEditorGUI = EditorGUILayout.Foldout(bExpandedEditorGUI, "Event " + (i + 1));
            string strShowHideText = (bExpandedEditorGUI ? "/\\    Collapse" : " \\/           Expand Follower Settings");

            GUI.backgroundColor = buttonColor + Color.white * 0.4f;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(strShowHideText ,GUILayout.Width(200)) == true)
            {
                bExpandedEditorGUI = !bExpandedEditorGUI;
            }
            GUILayout.Label("    (Follower " + (i + 1) + ")" + "  Name: " + gameObjectName);

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = buttonColor + Color.red * 0.5f;
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                followersProperty.DeleteArrayElementAtIndex(i);
                break;
            }

            GUILayout.EndHorizontal();
            element.FindPropertyRelative("bExpandedEditorGUI").boolValue = bExpandedEditorGUI;

            if (bExpandedEditorGUI == true)
            {
                GUILayout.BeginHorizontal();

                // Set background color for each follower entry
                GUI.backgroundColor = followerBackgroundColor;
                GUILayout.BeginVertical("Box");

                // Centered bold label with GameObject name
                GUILayout.Label("Name: " + gameObjectName, boldLabelStyle);

                if (t.followers[i].bInitializedWithDefaultVals == false)
                {
                    t.followers[i].InitWithDefaultVals();
                    t.followers[i].bInitializedWithDefaultVals = true;
                }
                // Display properties with white labels
                GUI.contentColor = Color.white;
                EditorGUILayout.PropertyField(gameObjectProp, new GUIContent("GameObject"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("eWrapMode"), new GUIContent("Wrap Mode"));
                if (t.followers[i].eWrapMode == CPC_Follower.EWrapMode.E_PING_PONG)
                {
                    if (t.followers[i].bPingPoingFlipRotation == true && t.followers[i].fPingPongFlipRotationSpeed == 0)
                        t.followers[i].fPingPongFlipRotationSpeed = 1.0f;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("bPingPoingFlipRotation"), new GUIContent("PingPong Moving Back Revert Rotation"));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("fPingPongFlipRotationSpeed"), new GUIContent(" RevertRot Speed"));
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Rotation Mode");
                EditorGUILayout.PropertyField(element.FindPropertyRelative("eRotationMode"), new GUIContent());
                GUILayout.Label("Interpolation");
                EditorGUILayout.PropertyField(element.FindPropertyRelative("eRotLerpMode"), new GUIContent());
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(element.FindPropertyRelative("fRotationSmoothness"), new GUIContent("Rot Smoothness"));

                GUILayout.Space(5);

                // Display new properties
                EditorGUILayout.PropertyField(element.FindPropertyRelative("fSpeedMultiplier"), new GUIContent("Speed Multiplier"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("fStartDistance"), new GUIContent("Start Distance"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("fStartDelay"), new GUIContent("Start Delay"));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Move To Spline");
                if(GUILayout.Button("Move to Target Pos"))
                {
                    t.GenerateVertexPath();
                    t.followers[i].gameObject.transform.position = t.vertexPath.GetPointAtDistance(t.followers[i].fStartDistance);
                    t.followers[i].gameObject.transform.rotation = t.vertexPath.GetRotationAtDistance(t.followers[i].fStartDistance);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // Conditionally display the lookAtTarget field
                var eRotationModeProp = element.FindPropertyRelative("eRotationMode");
                if (eRotationModeProp.enumValueIndex == (int)CPC_Follower.ERotationMode.E_LOOK_AT)
                {
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("lookAtTarget"), new GUIContent("Look At Target"));
                }

                GUILayout.EndVertical();


                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
           
        }

      

        serializedObject.ApplyModifiedProperties();
        GUILayout.EndVertical();

        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Move All Followers To Spline");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Move All Followers To Spline"))
        {
            t.GenerateVertexPath();
            for (int i = 0; i < followerCount; i++)
            {
                t.followers[i].gameObject.transform.position = t.vertexPath.GetPointAtDistance(t.followers[i].fStartDistance);
                t.followers[i].gameObject.transform.rotation = t.vertexPath.GetRotationAtDistance(t.followers[i].fStartDistance);
            }
        }
        GUILayout.EndHorizontal();


            GUI.backgroundColor = buttonColor;
        GUI.contentColor = Color.white;
        if (GUILayout.Button("Add Follower", boldButtonStyle, GUILayout.Height(30)))
        {
            followersProperty.InsertArrayElementAtIndex(followersProperty.arraySize);
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);

        // Reset GUI colors
        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;
    }

    void RunAndDrawEventFollowersCheckboxList(SerializedProperty element)
    {
        SerializedProperty assignedFollowers = element.FindPropertyRelative("assignedFollowers");
        CPC_CameraPath cameraPath = (CPC_CameraPath)serializedObject.targetObject;

        foreach (var follower in cameraPath.followers)
        {
            // Check if the follower's gameObject is in the assignedFollowers list
            bool isAssigned = Enumerable.Range(0, assignedFollowers.arraySize)
                .Any(j => assignedFollowers.GetArrayElementAtIndex(j).objectReferenceValue == follower.gameObject);

            // Draw the checkbox for each follower
            bool newAssigned = EditorGUILayout.ToggleLeft($"Follower {follower.gameObject.name}", isAssigned);

            if (newAssigned != isAssigned)
            {
                if (newAssigned)
                {
                    // Add follower's gameObject to assignedFollowers
                    assignedFollowers.arraySize++;
                    assignedFollowers.GetArrayElementAtIndex(assignedFollowers.arraySize - 1).objectReferenceValue = follower.gameObject;
                }
                else
                {
                    // Remove follower's gameObject from assignedFollowers
                    for (int j = 0; j < assignedFollowers.arraySize; j++)
                    {
                        if (assignedFollowers.GetArrayElementAtIndex(j).objectReferenceValue == follower.gameObject)
                        {
                            assignedFollowers.DeleteArrayElementAtIndex(j);
                            break;
                        }
                    }
                }
            }
        }

        // Apply changes to the serialized object if needed
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    void DrawEventList()
    {
        // Color scheme for events
        Color eventBackgroundColor = new Color(0.85f, 0.8f, 0.9f); // Light purple background color
        Color buttonColor = new Color(0.6f, 0.4f, 0.8f); // Purple color for buttons

        var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUI.backgroundColor = buttonColor;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Events", boldLabelStyle);
        GUILayout.BeginVertical("Box");

        serializedObject.Update();
        int eventCount = eventsProperty.arraySize;




        // Reset to button color and bold style for "Add Event" button
     

        GUILayout.Space(10);

        for (int i = 0; i < eventCount; i++)
        {

            var element = eventsProperty.GetArrayElementAtIndex(i);
            // Centered bold label for the event



            bool bExpandedEditorGUI = element.FindPropertyRelative("bExpandedEditorGUI").boolValue;
          //  bExpandedEditorGUI = EditorGUILayout.Foldout(bExpandedEditorGUI, "Event " + (i + 1));
            string strShowHideText = (bExpandedEditorGUI ? "/\\     Collapse" : " \\/          Expand Event Settings");

            GUI.backgroundColor = buttonColor + Color.white*0.4f;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(strShowHideText, GUILayout.Width(200)) == true)
            {
                bExpandedEditorGUI = !bExpandedEditorGUI;
            }
            GUILayout.Label("    (Event " + (i + 1) + ")");
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = buttonColor + Color.red * 0.5f;
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                eventsProperty.DeleteArrayElementAtIndex(i);
                break;
            }
            GUILayout.EndHorizontal();

            element.FindPropertyRelative("bExpandedEditorGUI").boolValue = bExpandedEditorGUI;

            if (bExpandedEditorGUI)
            {
                GUILayout.BeginHorizontal();

                // Set background color for each event entry
                GUI.backgroundColor = eventBackgroundColor;
                GUILayout.BeginVertical("Box");

                // Display properties with white labels
                GUI.contentColor = Color.white;
                EditorGUILayout.PropertyField(element.FindPropertyRelative("unityEvent"), new GUIContent("Event"));
                SerializedProperty iWaypointIndexProp = element.FindPropertyRelative("iWaypointIndex");
                iWaypointIndexProp.intValue = EditorGUILayout.IntSlider("Waypoint Index", iWaypointIndexProp.intValue, 0, t.points.Count - 1);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("fWaypointNormalizedDist"), new GUIContent("Normalized Distance"));

                // Display new propertieseRepeatMode

                EditorGUILayout.PropertyField(element.FindPropertyRelative("eTriggerSendMode"), new GUIContent("Send Mode"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("eTriggerBy"), new GUIContent("Triggered By"));

                if (element.FindPropertyRelative("eTriggerBy").enumValueIndex == (int)CPC_Event.ETriggerByType.E1_SPECIFIC_PASSED_FOLLOWERS)
                {

                    bool bExpandedAssignedFollowersGUI = element.FindPropertyRelative("bExpandedAssignedFollowersGUI").boolValue;
                    bExpandedAssignedFollowersGUI = EditorGUILayout.Foldout(bExpandedAssignedFollowersGUI, "Check Passed Followers List");
                    element.FindPropertyRelative("bExpandedAssignedFollowersGUI").boolValue = bExpandedAssignedFollowersGUI;

                    if (bExpandedAssignedFollowersGUI)
                        RunAndDrawEventFollowersCheckboxList(element);

                }

              /*  CPC_CameraPath cameraPath = (CPC_CameraPath)serializedObject.targetObject;

                foreach (var follower in cameraPath.followers)
                {
                    if (Application.isPlaying == false)
                    {
                        if (cameraPath.points.Count > 0)
                            follower.gameObject.transform.position = cameraPath.points[0].positionWorld;

                        if (cameraPath.points.Count > 1)
                        {

                        }
                    }
                }*/

                GUILayout.EndVertical();

                // Set button color for the "Remove" button

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

           

        }

        GUILayout.Space(10);

        GUI.backgroundColor = buttonColor;
        GUI.contentColor = Color.white;
        var boldButtonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
        if (GUILayout.Button("Add Event", boldButtonStyle, GUILayout.Height(30)))
        {
            eventsProperty.InsertArrayElementAtIndex(eventsProperty.arraySize);
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);


        serializedObject.ApplyModifiedProperties();
        GUILayout.EndVertical();

        // Reset GUI colors
        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;
    }


    void DrawHandles(int i)
    {
        if(Application.isPlaying == false && t.bIsRuntimeEditingPath == true)
        {
            t.bIsRuntimeEditingPath = false;
        }

       if(t.bIsRuntimeEditingPath == true || Application.isPlaying == false)
        {
            DrawHandleLines(i);
            Handles.color = t.visual.handleColor;
            
        if (t.points[i].bKeepFlat == false)
            Handles.color = (Color.red + Color.yellow) * 0.5f;

            DrawNextHandle(i);
            DrawPrevHandle(i);
            DrawWaypointHandles(i);
            DrawSelectionHandles(i);
        }
    }

    void DrawHandleLines(int i)
    {
        Handles.color = t.visual.handleColor;
        if (t.points[i].bKeepFlat == false)
            Handles.color = (Color.red + Color.yellow) * 0.5f;

        if (i < t.points.Count - 1 || t.looped)
            Handles.DrawLine(t.points[i].positionWorld, t.points[i].positionWorld + t.points[i].handleNextWorld);
        if (i > 0 || t.looped)
            Handles.DrawLine(t.points[i].positionWorld, t.points[i].positionWorld + t.points[i].handlePrevWorld);
        Handles.color = Color.white;
    }

    void DrawNextHandle(int i)
    {
        if (i < t.points.Count - 1 || loopedProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 posNext = Vector3.zero;
            float size = HandleUtility.GetHandleSize(t.points[i].positionWorld + t.points[i].handleNextWorld) * 0.1f;
            if (handlePositionMode == CPC_EManipulationModes.Free)
            {
#if UNITY_5_5_OR_NEWER
                posNext = Handles.FreeMoveHandle(t.points[i].positionWorld + t.points[i].handleNextWorld, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
#else
                posNext = Handles.FreeMoveHandle(t.points[i].position + t.points[i].handlenext, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);
#endif
            }
            else
            {
                if (selectedIndex == i)
                {
#if UNITY_5_5_OR_NEWER
                    Handles.SphereHandleCap(0, t.points[i].positionWorld + t.points[i].handleNextWorld, Quaternion.identity, size, EventType.Repaint);
#else
                    Handles.SphereCap(0, t.points[i].position + t.points[i].handlenext, Quaternion.identity, size);
#endif
                    posNext = Handles.PositionHandle(t.points[i].positionWorld + t.points[i].handleNextWorld, Quaternion.identity);
                }
                else if (Event.current.button != 1)
                {
#if UNITY_5_5_OR_NEWER
                    if (Handles.Button(t.points[i].positionWorld + t.points[i].handleNextWorld, Quaternion.identity, size, size, Handles.CubeHandleCap))
                    {
                        SelectIndex(i);
                    }
#else
                    if (Handles.Button(t.points[i].position + t.points[i].handlenext, Quaternion.identity, size, size, Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Handle Position");
                t.points[i].handleNextWorld = posNext - t.points[i].positionWorld;
                if (t.points[i].chained)
                    t.points[i].handlePrevWorld = t.points[i].handleNextWorld * -1;

                if (t.points[i].bKeepFlat)
                    t.points[i].ResetHandleLocalY();
            }
        }

    }

    void DrawPrevHandle(int i)
    {
        if (i > 0 || loopedProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 posPrev = Vector3.zero;
            float size = HandleUtility.GetHandleSize(t.points[i].positionWorld + t.points[i].handlePrevWorld) * 0.1f;
            if (handlePositionMode == CPC_EManipulationModes.Free)
            {
#if UNITY_5_5_OR_NEWER
                posPrev = Handles.FreeMoveHandle(t.points[i].positionWorld + t.points[i].handlePrevWorld, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].positionWorld + t.points[i].handlePrevWorld), Vector3.zero, Handles.SphereHandleCap);
#else
                posPrev = Handles.FreeMoveHandle(t.points[i].position + t.points[i].handleprev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].position + t.points[i].handleprev), Vector3.zero, Handles.SphereCap);
#endif
            }
            else
            {
                if (selectedIndex == i)
                {
#if UNITY_5_5_OR_NEWER
                    Handles.SphereHandleCap(0, t.points[i].positionWorld + t.points[i].handlePrevWorld, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].positionWorld + t.points[i].handleNextWorld), EventType.Repaint);
#else
                    Handles.SphereCap(0, t.points[i].position + t.points[i].handleprev, Quaternion.identity,
                        0.1f * HandleUtility.GetHandleSize(t.points[i].position + t.points[i].handlenext));
#endif
                    posPrev = Handles.PositionHandle(t.points[i].positionWorld + t.points[i].handlePrevWorld, Quaternion.identity);
                }
                else if (Event.current.button != 1)
                {
#if UNITY_5_5_OR_NEWER
                    if (Handles.Button(t.points[i].positionWorld + t.points[i].handlePrevWorld, Quaternion.identity, size, size, Handles.CubeHandleCap))
                    {
                        SelectIndex(i);
                    }
#else
                    if (Handles.Button(t.points[i].position + t.points[i].handleprev, Quaternion.identity, size, size,
                        Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Handle Position");
                t.points[i].handlePrevWorld = posPrev - t.points[i].positionWorld;
                if (t.points[i].chained)
                    t.points[i].handleNextWorld = t.points[i].handlePrevWorld * -1;

                if (t.points[i].bKeepFlat)
                    t.points[i].ResetHandleLocalY();
            }
        }
    }

    bool bMovedRefreshDist = false;
    float fLastGeneratedPathOnMovedPoint = 0;
    void DrawWaypointHandles(int i)
    {
        // Ensure custom handles are clearly different
        Color original = Handles.color;
        Vector3 positionOffset = Vector3.zero;  // Initialize an offset variable

        // Check for overlap and set an offset if needed
        if (Vector3.Distance(t.points[i].positionWorld, t.transform.position) < HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.3f)
        {
            // Apply a small offset to avoid overlap
            t.points[i].positionWorld -= Vector3.right * HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.1f;
        }

        if (Tools.current == Tool.Move)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;
            if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform)
            {
                if (i == selectedIndex)
                {
                    // Draw a custom shape for the position handle with an offset
                    Handles.color = Color.magenta;
                    Handles.DrawWireDisc(t.points[i].positionWorld , Vector3.up,
                        HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.5f);
                    Handles.DrawWireDisc(t.points[i].positionWorld , Vector3.up,
                        HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.55f);

                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(0, t.points[i].positionWorld ,
                        (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].rotation : Quaternion.identity,
                        HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.2f, EventType.Repaint);

                    // Handle movement
                    pos = Handles.PositionHandle(t.points[i].positionWorld + positionOffset,
                        (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].rotation : Quaternion.identity) - positionOffset;
                }
            }
            else
            {
                // Use a different handle shape with offset
                pos = Handles.FreeMoveHandle(t.points[i].positionWorld + positionOffset,
                    (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].rotation : Quaternion.identity,
                    HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.2f, Vector3.zero, Handles.SphereHandleCap) - positionOffset;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Moved Waypoint");
                t.points[i].positionWorld = pos;

                if (i == selectedIndex)
                {
                    bMovedRefreshDist = true;
                    fLastGeneratedPathOnMovedPoint = Time.realtimeSinceStartup;
                }

            }else
            {
                if(i==selectedIndex)
                {
                    if(bMovedRefreshDist == true)
                    {
                        if (Mathf.Abs(fLastGeneratedPathOnMovedPoint - Time.realtimeSinceStartup) > 1.0F)
                        {
                            bMovedRefreshDist = false;
                            // to refresh distance
                            t.GenerateVertexPath(); // so we can get next point on curve if not looped
                            fLastGeneratedPathOnMovedPoint = Time.realtimeSinceStartup;
                          //  Debug.LogError("Regenerated path");
                        }
                    }
                }
            }
        }
        else if (Tools.current == Tool.Rotate)
        {
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Quaternion.identity;
            if (cameraRotationMode == CPC_EManipulationModes.SelectAndTransform)
            {
                if (i == selectedIndex)
                {
                    // Draw a custom rotation indicator with offset
                    Handles.color = Color.magenta;
                    Handles.DrawWireDisc(t.points[i].positionWorld , Vector3.up,
                        HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.5f);
                    Handles.DrawWireDisc(t.points[i].positionWorld , Vector3.up,
                        HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.55f);

                    // Handle rotation
                    rot = Handles.RotationHandle(t.points[i].rotation, t.points[i].positionWorld + positionOffset);
                }
            }
            else
            {
                // Free rotation handle with offset
                rot = Handles.FreeRotateHandle(t.points[i].rotation, t.points[i].positionWorld + positionOffset,
                    HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.2f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rotated Waypoint");
                t.points[i].rotation = rot;
            }
        }

        // Reset handle color to white to avoid affecting other handles
        Handles.color = original;

        // Repaint handles to ensure proper rendering
        HandleUtility.Repaint();
    }

    void DrawSelectionHandles(int i)
    {
        if (Event.current.button != 1 && selectedIndex != i || (Event.current.button == 1))
        {
            if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Move
                || cameraRotationMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Rotate)
            {
                float size = HandleUtility.GetHandleSize(t.points[i].positionWorld) * 0.2f;
#if UNITY_5_5_OR_NEWER
                if (Handles.Button(t.points[i].positionWorld, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    SelectIndex(i);
                }
#else
                if (Handles.Button(t.points[i].position, Quaternion.identity, size, size, Handles.CubeCap))
                {
                    SelectIndex(i);
                }
#endif
            }
        }
    }

    void DrawRawValues()
    {
        if (GUILayout.Button(showRawValues ? "Hide raw values" : "Show raw values"))
            showRawValues = !showRawValues;

        if (showRawValues)
        {
            foreach (var i in t.points)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("Box");
                Vector3 pos = EditorGUILayout.Vector3Field("Waypoint Position", i.positionWorld);
                Quaternion rot = Quaternion.Euler(EditorGUILayout.Vector3Field("Waypoint Rotation", i.rotation.eulerAngles));
                Vector3 posp = EditorGUILayout.Vector3Field("Previous Handle Offset", i.handlePrevWorld);
                Vector3 posn = EditorGUILayout.Vector3Field("Next Handle Offset", i.handleNextWorld);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Changed waypoint transform");
                    i.positionWorld = pos;
                    i.rotation = rot;
                    i.handlePrevWorld = posp;
                    i.handleNextWorld = posn;
                    SceneView.RepaintAll();
                }
            }
        }
    }
}

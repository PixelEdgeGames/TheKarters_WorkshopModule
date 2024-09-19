using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(PTK_Command_05_PlayerLogicEffects), true)]
public class PTK_Command_05_PlayerLogicEffectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Create a serialized object so we can handle property fields
        SerializedObject serializedObject = new SerializedObject(target);
        PTK_Command_05_PlayerLogicEffects script = (PTK_Command_05_PlayerLogicEffects)target;

        // Start scroll view
        EditorGUILayout.BeginScrollView(Vector2.zero);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Execute Delay");
        script.fExecuteDelay =  EditorGUILayout.FloatField(script.fExecuteDelay, GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Get all fields of the target object
        FieldInfo[] fields = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Check if the field is a subclass of CPlayerEffectBase
            if (typeof(PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase).IsAssignableFrom(field.FieldType))
            {
                var effect = (PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase)field.GetValue(script);

                if (effect != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    // Draw enabled button
                    bool isEnabled = effect.bExecute;
                    EditorGUILayout.BeginHorizontal();
                    // Display the effect type name

                    GUI.color = isEnabled ? (Color.green + Color.white * 0.3f + Color.yellow * 0.3f) : Color.white*0.9f;
                    string formattedFieldName = AddSpacesToFieldName(field.Name);
                    EditorGUILayout.LabelField(formattedFieldName, EditorStyles.boldLabel);

                    GUI.color = Color.white;
                    GUI.backgroundColor = isEnabled ? (Color.green + Color.white * 0.3f + Color.yellow * 0.3f) : Color.gray;
                    if (GUILayout.Button(isEnabled ?  "ON" : "OFF"))
                    {
                        effect.bExecute = !effect.bExecute;
                    }

                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();

                    // If the effect is enabled, show its properties
                    if (isEnabled)
                    {
                        EditorGUI.indentLevel++;

                        // Convert effect to SerializedObject to handle properties
                        SerializedObject effectSerializedObject = new SerializedObject(script);
                        SerializedProperty effectProperty = effectSerializedObject.FindProperty(field.Name);

                        SerializedProperty property = effectProperty.Copy();
                        SerializedProperty endProperty = property.GetEndProperty();

                        // Iterate over all properties of the effect
                        if (property.NextVisible(true))
                        {
                            do
                            {
                                if (SerializedProperty.EqualContents(property, endProperty))
                                    break;

                                if (property.name == "bExecute") continue;

                                EditorGUILayout.PropertyField(property, true);
                            }
                            while (property.NextVisible(false));

                        }


                        DrawPerEffectGUI(script,effect);

                        // Add Animation Clip Support toggle and display proxy values if enabled
                        DisplayAnimationClipSupport(effectSerializedObject,script, effect);


                        effectSerializedObject.ApplyModifiedProperties();

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();

                    // Space between each effect
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider,GUILayout.Height(8));
                }
            }
        }


        // End the scroll view
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    void DisplayAnimationClipSupport(SerializedObject so, PTK_Command_05_PlayerLogicEffects parentScript, PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase effect)
    {
        GUILayout.Space(5);
        int iOriginalIntent = EditorGUI.indentLevel;

        // Bounce effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E1_Bounce)
        {
            SerializedProperty bUseBounceAnimProxyValues = so.FindProperty("bUseBounceAnimProxyValues");
            SerializedProperty fBounceStrengthProxy = so.FindProperty("fBounceStrengthProxy");

            EditorGUILayout.PropertyField(bUseBounceAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseBounceAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(fBounceStrengthProxy, new GUIContent("Bounce Strength Proxy"));
            }
        }

        // Kill effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E2_Kill)
        {
            SerializedProperty bUseKillAnimProxyValues = so.FindProperty("bUseKillAnimProxyValues");
            SerializedProperty fSquishDurationProxy = so.FindProperty("fSquishDurationProxy");

            EditorGUILayout.PropertyField(bUseKillAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseKillAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fSquishDurationProxy, new GUIContent("Squish Duration Proxy"));
            }
        }

        // Damage effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E3_Damage)
        {
            SerializedProperty bUseDamageAnimProxyValues = so.FindProperty("bUseDamageAnimProxyValues");
            SerializedProperty iDamageProxy = so.FindProperty("iDamageProxy");
            SerializedProperty fSquishDurationDamageProxy = so.FindProperty("fSquishDurationDamageProxy");

            EditorGUILayout.PropertyField(bUseDamageAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseDamageAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(iDamageProxy, new GUIContent("Damage Proxy"));
                EditorGUILayout.PropertyField(fSquishDurationDamageProxy, new GUIContent("Squish Duration Proxy (Damage)"));
            }
        }

        // Continuous Damage effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage)
        {
            SerializedProperty bUseContinuousDamageAnimProxyValues = so.FindProperty("bUseContinuousDamageAnimProxyValues");
            SerializedProperty fContinuousDamageDurationProxy = so.FindProperty("fContinuousDamageDurationProxy");
            SerializedProperty fDamageTickEverySecProxy = so.FindProperty("fDamageTickEverySecProxy");
            SerializedProperty iDamagePerTickProxy = so.FindProperty("iDamagePerTickProxy");

            EditorGUILayout.PropertyField(bUseContinuousDamageAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseContinuousDamageAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fContinuousDamageDurationProxy, new GUIContent("Continuous Damage Duration Proxy"));
                EditorGUILayout.PropertyField(fDamageTickEverySecProxy, new GUIContent("Damage Tick Every Sec Proxy"));
                EditorGUILayout.PropertyField(iDamagePerTickProxy, new GUIContent("Damage Per Tick Proxy"));
            }
        }

        // Heal effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E5_HealPlayer)
        {
            SerializedProperty bUseHealAnimProxyValues = so.FindProperty("bUseHealAnimProxyValues");
            SerializedProperty iHealHPProxy = so.FindProperty("iHealHPProxy");
            SerializedProperty fHealthToRefillProxy = so.FindProperty("fHealthToRefillProxy");
            SerializedProperty fRefillHealthInTimeProxy = so.FindProperty("fRefillHealthInTimeProxy");

            EditorGUILayout.PropertyField(bUseHealAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseHealAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(iHealHPProxy, new GUIContent("Heal HP Proxy"));
                EditorGUILayout.PropertyField(fHealthToRefillProxy, new GUIContent("Health to Refill Proxy"));
                EditorGUILayout.PropertyField(fRefillHealthInTimeProxy, new GUIContent("Refill Health In Time Proxy"));
            }
        }

        // Catapult effect
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E09_Catapult)
        {
            SerializedProperty bUseCatapultAnimProxyValues = so.FindProperty("bUseCatapultAnimProxyValues");
            SerializedProperty fCatapultUpForceProxy = so.FindProperty("fCatapultUpForceProxy");
            SerializedProperty fCatapultForwardForceProxy = so.FindProperty("fCatapultForwardForceProxy");

            EditorGUILayout.PropertyField(bUseCatapultAnimProxyValues, new GUIContent("Enable Variables Animation"));


            if (bUseCatapultAnimProxyValues.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fCatapultUpForceProxy, new GUIContent("Catapult Up Force Proxy"));
                EditorGUILayout.PropertyField(fCatapultForwardForceProxy, new GUIContent("Catapult Forward Force Proxy"));
            }
        }

        // Frozen
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E7_FrozenWheelsEffect)
        {
            SerializedProperty bUseProxy = so.FindProperty("bUseFrozenWheelsAnimProxyValues");
            SerializedProperty fFrozenDurationProxy = so.FindProperty("fFrozenDurationProxy");

            EditorGUILayout.PropertyField(bUseProxy, new GUIContent("Enable Variables Animation"));


            if (bUseProxy.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(fFrozenDurationProxy, new GUIContent("Frozen Duration Proxy"));
            }
        }

        // boostpad
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E8_BoostPad) // ensure you changed class name when adding new
        {
            SerializedProperty bUseProxy = so.FindProperty("bUseBoostPadAnimProxyValues");
            SerializedProperty fBoostpadStrengthProxy = so.FindProperty("fBoostpadStrengthProxy");

            EditorGUILayout.PropertyField(bUseProxy, new GUIContent("Enable Variables Animation"));


            if (bUseProxy.boolValue)
            {
                EditorGUILayout.HelpBox("The values below will override the originals to support animation. They will be used with or without animation. You can Animate these variables in the Animation window", MessageType.Info);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(fBoostpadStrengthProxy, new GUIContent("Boostpad Strength Proxy"));
            }
        }

        // Apply changes to the serialized object
        so.ApplyModifiedProperties();

        EditorGUI.indentLevel = iOriginalIntent;
    }

    void DrawPerEffectGUI(PTK_Command_05_PlayerLogicEffects parentScript, PTK_Command_05_PlayerLogicEffects.CPlayerEffectBase effect)
    {
        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_BezierSpline)
        {
            PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_BezierSpline effectType = effect as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_BezierSpline;
            if (effectType == null)
                return;

            GUILayout.BeginHorizontal();

            if (effectType.bezierSpline == null)
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                GUILayout.BeginHorizontal();

                var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold};
                GUI.color = Color.red;
                GUILayout.Label("Assign or create bezier spline", boldLabelStyle);
                GUI.color = Color.white;

                if (GUILayout.Button("Create Spline", GUILayout.Width(150)))
                {
                    GameObject bezierSpline = new GameObject("QuickDash Bezier Spline");
                    bezierSpline.transform.parent = parentScript.transform;
                    bezierSpline.transform.localPosition = Vector3.zero;
                    bezierSpline.transform.localRotation = Quaternion.identity;
                    var bezierCreated = bezierSpline.AddComponent<PTK_BezierSpline>();
                    bezierSpline.AddComponent<CPC_BezierPath>();

                    // Add undo functionality for object creation
                    Undo.RegisterCreatedObjectUndo(bezierSpline, "Create Bezier Spline");
                    Undo.RecordObject(parentScript, "Assign Bezier Spline");
                    EditorUtility.SetDirty(parentScript);

                    effectType.bezierSpline = bezierCreated;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUI.backgroundColor = original;
            }
            else
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Spline", GUILayout.Width(150)))
                {
                    if (EditorUtility.DisplayDialog( "Delete", "Are you sure?", "ok", "cancel"))
                    {
                        // Add undo functionality for object deletion
                        Undo.DestroyObjectImmediate(effectType.bezierSpline.gameObject); effectType.bezierSpline = null;
                        Undo.RecordObject(parentScript, "Delete Bezier Spline");
                        EditorUtility.SetDirty(parentScript);
                        return;
                    }
                }
                GUI.backgroundColor = original;

                if (GUILayout.Button("Remove Spline", GUILayout.Width(150)))
                {
                    effectType.bezierSpline = null;
                    Undo.RecordObject(parentScript, "");
                    EditorUtility.SetDirty(parentScript);
                    return;
                }

                GUILayout.EndHorizontal();
                GUI.backgroundColor = original;
            }

            GUILayout.EndHorizontal();
        }

        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_InstantTeleport)
        {
            PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_InstantTeleport effectType = effect as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_InstantTeleport;
            if (effectType == null)
                return;

            GUILayout.BeginHorizontal();

            if (effectType.targetTransform == null)
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                GUILayout.BeginHorizontal();

                var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUI.color = Color.red;
                GUILayout.Label("Assign or create teleport target", boldLabelStyle);
                GUI.color = Color.white;

                if (GUILayout.Button("Create Teleport Target", GUILayout.Width(300)))
                {
                    // Create the main teleport target (quad)
                    GameObject teleportTarget = CreateTargetTransformDirectionVisuals(parentScript, "QuickDash Teleport Target");

                    effectType.targetTransform = teleportTarget.transform;
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                GUI.backgroundColor = original;
            }
            else
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Teleport Target", GUILayout.Width(300)))
                {
                    if (EditorUtility.DisplayDialog( "Delete", "Are you sure?", "ok", "cancel"))
                    {
                        // Add undo functionality for object deletion
                        Undo.DestroyObjectImmediate(effectType.targetTransform.gameObject); effectType.targetTransform = null;
                        Undo.RecordObject(parentScript, "Delete Teleport Target");
                        EditorUtility.SetDirty(parentScript);
                        return;
                    }
                }

                GUI.backgroundColor = original;

                if (GUILayout.Button("Remove Teleport Target", GUILayout.Width(300)))
                {
                    effectType.targetTransform = null;
                    Undo.RecordObject(parentScript, "");
                    EditorUtility.SetDirty(parentScript);
                    return;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
        }


        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_Waypoints)
        {
            PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_Waypoints effectType = effect as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E6_QuickDashMovement_Waypoints;
            if (effectType == null)
                return;

            if (effectType.waypointsParent == null)
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                GUILayout.BeginHorizontal();

                var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUI.color = Color.red;
                GUILayout.Label("Assign or create Waypoint Parent", boldLabelStyle);
                GUI.color = Color.white;

                if (GUILayout.Button("Create Waypoint Parent", GUILayout.Width(300)))
                {
                    GameObject waypointParent = new GameObject("QuickDash Waypoint Parent");
                    waypointParent.transform.parent = parentScript.transform;
                    waypointParent.transform.localPosition = Vector3.zero;
                    waypointParent.transform.localRotation = Quaternion.identity;

                    // Add undo functionality for object creation
                    Undo.RegisterCreatedObjectUndo(waypointParent, "Create Waypoint Parent");
                    Undo.RecordObject(parentScript, "Assign Waypoint Parent");

                    effectType.waypointsParent = waypointParent.transform;
                    EditorUtility.SetDirty(parentScript);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUI.backgroundColor = original;
            }
            else
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Waypoint Parent", GUILayout.Width(300)))
                {
                    if (EditorUtility.DisplayDialog( "Delete", "Are you sure?", "ok", "cancel"))
                    {
                        // Add undo functionality for object deletion
                        Undo.DestroyObjectImmediate(effectType.waypointsParent.gameObject); effectType.waypointsParent = null;
                        Undo.RecordObject(parentScript, "Delete Waypoint Parent");
                        EditorUtility.SetDirty(parentScript);
                        return;
                    }
                }
                GUI.backgroundColor = original;

                if (GUILayout.Button("Remove Waypoint Parent", GUILayout.Width(300)))
                {
                    effectType.waypointsParent = null;
                    Undo.RecordObject(parentScript, "Delete Waypoint Parent");
                    EditorUtility.SetDirty(parentScript);
                    return;
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();

                if (effectType.waypointsParent.childCount < 2)
                {
                    GUI.backgroundColor = Color.red;
                    var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                    GUI.color = Color.red;
                    GUILayout.Label("Assign or create at least 2 waypoints", boldLabelStyle);
                    GUI.color = Color.white;
                    GUI.backgroundColor = Color.green;
                }

                if (GUILayout.Button("Create Waypoint", GUILayout.Width(150)))
                {
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GameObject.DestroyImmediate(sphere.GetComponent<Collider>());

                    // Add undo functionality for waypoint creation
                    Undo.RegisterCreatedObjectUndo(sphere, "Create Waypoint");
                    Undo.RecordObject(parentScript, "Assign Waypoint");

                    if (effectType.waypointsParent.childCount > 0)
                    {
                        sphere.transform.position = effectType.waypointsParent.GetChild(effectType.waypointsParent.childCount - 1).transform.position;
                        sphere.transform.rotation = effectType.waypointsParent.GetChild(effectType.waypointsParent.childCount - 1).transform.rotation;
                        sphere.transform.parent = effectType.waypointsParent;
                    }
                    else
                    {
                        sphere.transform.parent = effectType.waypointsParent;
                        sphere.transform.localPosition = Vector3.zero;
                        sphere.transform.localRotation = Quaternion.identity;
                    }
                    EditorUtility.SetDirty(parentScript);
                }
                GUI.backgroundColor = original;

                if (effectType.waypointsParent.childCount > 0)
                {
                    if (GUILayout.Button("Select Waypoint", GUILayout.Width(150)))
                    {
                        Selection.activeGameObject = effectType.waypointsParent.GetChild(effectType.waypointsParent.childCount - 1).gameObject;
                    }
                }

                GUILayout.FlexibleSpace();

                if (effectType.waypointsParent.childCount > 0)
                {
                    if (GUILayout.Button("Delete Last Waypoint", GUILayout.Width(150)))
                    {
                        if (effectType.waypointsParent.childCount > 0)
                        {
                            // Add undo functionality for waypoint deletion
                            Undo.DestroyObjectImmediate(effectType.waypointsParent.GetChild(effectType.waypointsParent.childCount - 1).gameObject);
                            Undo.RecordObject(parentScript, "Delete Waypoint");
                            EditorUtility.SetDirty(parentScript);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage)
        {
            PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage effectType = effect as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage;
            if (effectType == null)
                return;

            EditorGUI.BeginChangeCheck();

            effectType.EditorUpdate_UseValuesFromPreset();

            GUILayout.Space(10);
            Color original = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow*1.5f;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Total Damage: " + (effectType.iDamagePerTick * (effectType.fContinuousDamageDuration / effectType.fDamageTickEverySec)),GUI.skin.box,GUILayout.Width(300));
            GUILayout.EndHorizontal();
            GUI.backgroundColor = original;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(parentScript);
            }
        }


        if (effect is PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E09_Catapult)
        {
            PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E09_Catapult effectType = effect as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E09_Catapult;
            if (effectType == null)
                return;

            GUILayout.BeginHorizontal();

            if (effectType.catapultDirectionPreview == null)
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                GUILayout.BeginHorizontal();

                var boldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUI.color = Color.red;
                GUILayout.Label("Assign or create catapult direction transform", boldLabelStyle);
                GUI.color = Color.white;

                if (GUILayout.Button("Create Catapult Direction Transform", GUILayout.Width(300)))
                {
                    // Create the main teleport target (quad)
                    GameObject targetTransform = CreateTargetTransformDirectionVisuals(parentScript,"Catapult Direction Transform");

                    effectType.catapultDirectionPreview = targetTransform.transform;
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                GUI.backgroundColor = original;
            }
            else
            {
                Color original = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Catapult Direction Transform", GUILayout.Width(300)))
                {
                    if (EditorUtility.DisplayDialog("Delete", "Are you sure?", "ok", "cancel"))
                    {
                        // Add undo functionality for object deletion
                        Undo.DestroyObjectImmediate(effectType.catapultDirectionPreview.gameObject); effectType.catapultDirectionPreview = null;
                        Undo.RecordObject(parentScript, "Delete Catapult Direction Transform");
                        EditorUtility.SetDirty(parentScript);
                        return;
                    }
                }

                GUI.backgroundColor = original;

                if (GUILayout.Button("Remove Catapult Direction Transform", GUILayout.Width(300)))
                {
                    effectType.catapultDirectionPreview = null;
                    Undo.RecordObject(parentScript, "");
                    EditorUtility.SetDirty(parentScript);
                    return;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
        }
    }

    private static GameObject CreateTargetTransformDirectionVisuals(PTK_Command_05_PlayerLogicEffects parentScript,string strName)
    {
        GameObject teleportTarget = GameObject.CreatePrimitive(PrimitiveType.Quad);
        GameObject.DestroyImmediate(teleportTarget.GetComponent<Collider>());
        teleportTarget.name = strName ;
        teleportTarget.transform.parent = parentScript.transform;
        teleportTarget.transform.localPosition = Vector3.zero;
        teleportTarget.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        teleportTarget.transform.localRotation = Quaternion.identity;

        // Create an additional quad rotated 180 degrees
        GameObject additionalQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        GameObject.DestroyImmediate(additionalQuad.GetComponent<Collider>());
        additionalQuad.name = "Additional Quad";
        additionalQuad.transform.parent = teleportTarget.transform;
        additionalQuad.transform.localPosition = Vector3.zero;
        additionalQuad.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        additionalQuad.transform.localRotation = Quaternion.Euler(0, 180, 0);

        // Create the arrow from 3 cubes
        float fArrowLength = 1.0f;
        float fArrowSidesLength = 0.3f;

        // Arrow shaft (long cube)
        GameObject arrowShaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.DestroyImmediate(arrowShaft.GetComponent<Collider>());
        arrowShaft.name = "Arrow Shaft";
        arrowShaft.transform.parent = teleportTarget.transform;
        arrowShaft.transform.localScale = new Vector3(0.05f, 0.05f, fArrowLength);  // Long and thin
        arrowShaft.transform.localPosition = new Vector3(0.0f, 0, fArrowLength / 2.0f);  // Positioning it forward

        // Left arrowhead side
        GameObject leftArrowHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.DestroyImmediate(leftArrowHead.GetComponent<Collider>());
        leftArrowHead.name = "Left Arrow Side";
        leftArrowHead.transform.parent = teleportTarget.transform;
        leftArrowHead.transform.localScale = new Vector3(0.05f, 0.05f, fArrowSidesLength);
        leftArrowHead.transform.localPosition = new Vector3(-fArrowSidesLength / 2.0f, 0, fArrowLength );
        leftArrowHead.transform.localRotation = Quaternion.Euler(0, 45,0.0f );  // Rotate for the arrowhead

        // Right arrowhead side
        GameObject rightArrowHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.DestroyImmediate(rightArrowHead.GetComponent<Collider>());
        rightArrowHead.name = "Right Arrow Side";
        rightArrowHead.transform.parent = teleportTarget.transform;
        rightArrowHead.transform.localScale = new Vector3(0.05f, 0.05f, fArrowSidesLength);
        rightArrowHead.transform.localPosition = new Vector3(+fArrowSidesLength / 2.0f, 0, fArrowLength );
        rightArrowHead.transform.localRotation = Quaternion.Euler(0, -45,0.0f );  // Rotate for the arrowhead

        // Add undo functionality for object creation
        Undo.RegisterCreatedObjectUndo(teleportTarget, "Create Teleport Target");
        Undo.RecordObject(parentScript, "Assign Teleport Target");
        EditorUtility.SetDirty(parentScript);
        return teleportTarget;
    }

    private string AddSpacesToFieldName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;

        // Insert spaces before uppercase letters
        string strResult = string.Concat(fieldName.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));

        // Replace underscores with ' - ' (and remove the underscore)
        strResult = strResult.Replace("_", " -");

        return strResult;
    }
}

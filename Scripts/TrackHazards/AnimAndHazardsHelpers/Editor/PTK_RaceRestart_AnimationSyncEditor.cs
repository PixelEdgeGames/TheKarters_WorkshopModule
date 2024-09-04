using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PTK_RaceRestart_AnimationSync))]
public class PTK_RaceRestart_AnimationSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target script
        PTK_RaceRestart_AnimationSync script = (PTK_RaceRestart_AnimationSync)target;

        // Display the animation type dropdown
        script.eAnimType = (PTK_RaceRestart_AnimationSync.EAnimType)EditorGUILayout.EnumPopup("Animation Type", script.eAnimType);

        // Separator
        EditorGUILayout.Space();

        if(script.animatorTarget == null && script.animationTarget == null)
        {
            script.animatorTarget = script.GetComponent<Animator>();
            script.animationTarget = script.GetComponent<Animation>();
        }

        if(script.animatorTarget == null && script.animationTarget == null)
        {
            EditorGUILayout.HelpBox("No Animator or Animation component found.", MessageType.Warning);
            return;
        }


        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        script.eResetMode = (PTK_RaceRestart_AnimationSync.EMode)EditorGUILayout.EnumPopup("Reset Mode", script.eResetMode);


        GUILayout.Space(10);

        bool bDrawAnimator = false;

        if(script.eAnimType == PTK_RaceRestart_AnimationSync.EAnimType.E0_AUTO_DETECTED)
        {
            bDrawAnimator = script.animatorTarget != null;
        }else if(script.eAnimType == PTK_RaceRestart_AnimationSync.EAnimType.E1_ANIMATOR)
        {
            bDrawAnimator = true;
        }
        else
        {
            bDrawAnimator = false;
        }

        if (bDrawAnimator == true)
        {

            // Show Animator and related fields
            EditorGUILayout.LabelField("Animator Target", EditorStyles.boldLabel);
            script.animatorTarget = (Animator)EditorGUILayout.ObjectField("Animator", script.animatorTarget, typeof(Animator), true);

            DrawAnimClips(script);

            GUILayout.Space(10);
            // Display option to use Animator triggers
            EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
            script.bPlayBlendTree = EditorGUILayout.Toggle("Use Blend Treee", script.bPlayBlendTree);
            if(script.bPlayBlendTree == true)
            {
                GUI.backgroundColor = Color.yellow*2;
                EditorGUILayout.LabelField("Race Rastart Tip: Please set default values used by Blend Tree in animator",GUI.skin.box);
                GUI.backgroundColor = Color.white;
                script.strBlendTreeName = EditorGUILayout.TextField("BlendTree Name", script.strBlendTreeName);
            }

            if(script.bPlayBlendTree == true)
                GUILayout.Space(5);

            script.bUseAnimatorTriggers = EditorGUILayout.Toggle("Use Animator Triggers", script.bUseAnimatorTriggers);
            if (script.bUseAnimatorTriggers)
            {
                script.strAnimatorTriggerName_OnRaceRestart = EditorGUILayout.TextField("TriggerName - On Race Restart", script.strAnimatorTriggerName_OnRaceRestart);
                script.strAnimatorTrigger_OnRaceBeginAfterCountdown = EditorGUILayout.TextField("TriggerName On Race Begin", script.strAnimatorTrigger_OnRaceBeginAfterCountdown);
            }
        }
        else
        {
            // Show Legacy Animation and related fields
            EditorGUILayout.LabelField("Legacy Animation Target", EditorStyles.boldLabel);
            script.animationTarget = (Animation)EditorGUILayout.ObjectField("Animation", script.animationTarget, typeof(Animation), true);
            DrawAnimClips(script);
        }

        

        // Apply changes to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private static void DrawAnimClips(PTK_RaceRestart_AnimationSync script)
    {

        if (script.animationClip == null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);
            script.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", script.animationClip, typeof(AnimationClip), true);

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox("No Default Animation Clip Found.", MessageType.Warning);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Fix - Auto Assign", GUILayout.Height(40), GUILayout.Width(200)))
            {
                script.AssignFromObject();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(5); 
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        else
        {
            script.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", script.animationClip, typeof(AnimationClip), true);
        }

    }

}

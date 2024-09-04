using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PTK_RaceRestart_AnimationSync))]
public class PTK_RaceRestart_AnimationSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target script
        PTK_RaceRestart_AnimationSync script = (PTK_RaceRestart_AnimationSync)target;


        // Separator
        EditorGUILayout.Space();

        if(script.animatorTarget == null )
        {
            script.animatorTarget = script.GetComponent<Animator>();
        }

        if(script.animatorTarget == null )
        {
            EditorGUILayout.HelpBox("No Animator component found.", MessageType.Warning);
            return;
        }


        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);


        GUILayout.Space(10);

        // Show Animator and related fields
        EditorGUILayout.LabelField("Animator Target", EditorStyles.boldLabel);
        script.animatorTarget = (Animator)EditorGUILayout.ObjectField("Animator", script.animatorTarget, typeof(Animator), true);


        GUILayout.Space(10);
        // Display option to use Animator triggers
        script.ePlayMode = (PTK_RaceRestart_AnimationSync.EPlayMode)EditorGUILayout.EnumPopup("PlayMode", script.ePlayMode);
        if (script.ePlayMode == PTK_RaceRestart_AnimationSync.EPlayMode.E1_BLEND_TREE)
        {
            GUI.backgroundColor = Color.green * 2;
            EditorGUILayout.LabelField("Race Rastart Tip: Please set default paramter values used by Blend Tree in animator", GUI.skin.box);
            GUI.backgroundColor = Color.white;
            script.strBlendTreeName = EditorGUILayout.TextField("Play BlendTree Name", script.strBlendTreeName);

            GUILayout.Space(5);
        }else
        {
            DrawAnimClips(script);
        }

        script.bUseAnimatorTriggers = EditorGUILayout.Toggle("Use Animator Triggers", script.bUseAnimatorTriggers);
        if (script.bUseAnimatorTriggers)
        {
            script.strAnimatorTriggerName_OnRaceRestart = EditorGUILayout.TextField("TriggerName - On Race Restart", script.strAnimatorTriggerName_OnRaceRestart);
            script.strAnimatorTrigger_OnRaceBeginAfterCountdown = EditorGUILayout.TextField("TriggerName On Race Begin", script.strAnimatorTrigger_OnRaceBeginAfterCountdown);
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
            script.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Play Animation Clip", script.animationClip, typeof(AnimationClip), true);

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            EditorGUILayout.HelpBox("No Default Animation Clip Found. Assign clip or change to Blend Tree", MessageType.Warning);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Fix - Auto Assign", GUILayout.Height(40), GUILayout.Width(200)))
            {
                if (script.animatorTarget != null)
                {
                    EditorUtility.SetDirty(script.animatorTarget);
                }
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

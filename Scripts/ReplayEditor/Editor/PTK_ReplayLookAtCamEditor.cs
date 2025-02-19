using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PTK_ReplayLookAtCamConfig))]
public class PTK_ReplayLookAtCamEditor : Editor
{
    private PTK_ReplayLookAtCamConfig script;

    private void OnEnable()
    {
        script = (PTK_ReplayLookAtCamConfig)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow;
        EditorGUILayout.LabelField("Apply Predefined FOV Mode", EditorStyles.boldLabel);
        GUI.color = Color.white;

        DrawEnumGroup("Fixed->Dynamic", new[] {
            PTK_ReplayLookAtCamConfig.EFovConfig.F45_D4, PTK_ReplayLookAtCamConfig.EFovConfig.F60_D12, PTK_ReplayLookAtCamConfig.EFovConfig.F30_D4
        });

        DrawEnumGroup("Dynamic->Fixed", new[] {
            PTK_ReplayLookAtCamConfig.EFovConfig.D6_F50, PTK_ReplayLookAtCamConfig.EFovConfig.D9_F20, PTK_ReplayLookAtCamConfig.EFovConfig.D16_F45
        });

        DrawEnumGroup("Fixed -> Fixed", new[] {
            PTK_ReplayLookAtCamConfig.EFovConfig.F15_F40, PTK_ReplayLookAtCamConfig.EFovConfig.F45_F15,
            PTK_ReplayLookAtCamConfig.EFovConfig.F15_F15, PTK_ReplayLookAtCamConfig.EFovConfig.F35_F35
        });

        DrawEnumGroup("Dynamic -> Dynamic", new[] {
            PTK_ReplayLookAtCamConfig.EFovConfig.D4_D16, PTK_ReplayLookAtCamConfig.EFovConfig.D16_D9, PTK_ReplayLookAtCamConfig.EFovConfig.D4_D9
            , PTK_ReplayLookAtCamConfig.EFovConfig.D10_D10, PTK_ReplayLookAtCamConfig.EFovConfig.D17_D17
        });
        EditorGUILayout.EndVertical();
    }

    private void DrawEnumGroup(string label, PTK_ReplayLookAtCamConfig.EFovConfig[] modes)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        int buttonsPerRow = 3;
        for (int i = 0; i < modes.Length; i += buttonsPerRow)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < buttonsPerRow && (i + j) < modes.Length; j++)
            {
                if (GUILayout.Button(modes[i + j].ToString().Replace("_", "   "), GUILayout.Width(100)))
                {
                    ApplyFOVMode(modes[i + j]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void ApplyFOVMode(PTK_ReplayLookAtCamConfig.EFovConfig mode)
    {
        Undo.RecordObject(script, "Change FOV Mode");
        script.ApplyAndUsePredefinedFOVMode(mode);
        EditorUtility.SetDirty(script);
    }
}
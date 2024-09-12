using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PTK_ModPhysicsCollisionTriggerType), true)]
public class PTK_ModPhysicsCollisionTriggerTypeEditor : Editor
{
    SerializedProperty _bTriggerByPlayerCollision;
    SerializedProperty ePlayerTriggerActivationType;
    SerializedProperty bIgnorePlayerWhenTeleporting;
    SerializedProperty bIgnorePlayerWhenImmune;
    SerializedProperty bIgnorePlayerWhenUsingBoostingItem;
    SerializedProperty _bTiggerByBulletCollision;
    SerializedProperty _bTriggerByRangedWeaponsDamage;
    SerializedProperty bTriggerFromEachBulletDamageHit;
    SerializedProperty extraCollidersParent;

    private void OnEnable()
    {
        // Cache serialized properties
        _bTriggerByPlayerCollision = serializedObject.FindProperty("_bTriggerByPlayerCollision");
        ePlayerTriggerActivationType = serializedObject.FindProperty("ePlayerTriggerActivationType");
        bIgnorePlayerWhenTeleporting = serializedObject.FindProperty("bIgnorePlayerWhenTeleporting");
        bIgnorePlayerWhenImmune = serializedObject.FindProperty("bIgnorePlayerWhenImmune");
        bIgnorePlayerWhenUsingBoostingItem = serializedObject.FindProperty("bIgnorePlayerWhenUsingBoostingItem");
        _bTiggerByBulletCollision = serializedObject.FindProperty("_bTiggerByBulletCollision");
        _bTriggerByRangedWeaponsDamage = serializedObject.FindProperty("_bTriggerByRangedWeaponsDamage");
        bTriggerFromEachBulletDamageHit = serializedObject.FindProperty("bTriggerFromEachBulletDamageHit");
        extraCollidersParent = serializedObject.FindProperty("extraCollidersParent");
    }

    public override void OnInspectorGUI()
    {
        // Start tracking property changes
        serializedObject.Update();

        // Draw everything excluding specific fields
        DrawPropertiesExcluding(serializedObject, "_bTriggerByPlayerCollision", "ePlayerTriggerActivationType",
            "bIgnorePlayerWhenTeleporting", "bIgnorePlayerWhenImmune", "bIgnorePlayerWhenUsingBoostingItem",
            "_bTiggerByBulletCollision", "_bTriggerByRangedWeaponsDamage", "bTriggerFromEachBulletDamageHit", "extraCollidersParent");

        GUILayout.Space(10);

        // Player Trigger Settings
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Player Trigger Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(_bTriggerByPlayerCollision, new GUIContent("Trigger By Player Collision"));
        if (_bTriggerByPlayerCollision.boolValue)
        {
            EditorGUILayout.PropertyField(ePlayerTriggerActivationType, new GUIContent("Player Trigger Activation Type"));
            EditorGUILayout.PropertyField(bIgnorePlayerWhenTeleporting, new GUIContent("Ignore When Teleporting"));
            EditorGUILayout.PropertyField(bIgnorePlayerWhenImmune, new GUIContent("Ignore When Immune"));
            EditorGUILayout.PropertyField(bIgnorePlayerWhenUsingBoostingItem, new GUIContent("Ignore When Using Boost"));
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Weapon Trigger Settings
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.yellow * 1.3f;
        EditorGUILayout.LabelField("Weapon Trigger Settings", EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.PropertyField(_bTiggerByBulletCollision, new GUIContent("Trigger By Bullet Collision"));
        EditorGUILayout.PropertyField(_bTriggerByRangedWeaponsDamage, new GUIContent("Trigger By Ranged Weapons"));
        EditorGUILayout.PropertyField(bTriggerFromEachBulletDamageHit, new GUIContent("Trigger From Each Bullet Hit"));
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Collider Settings
        EditorGUILayout.PropertyField(extraCollidersParent, new GUIContent("Extra Colliders Parent"));

        GUILayout.Space(10);

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}

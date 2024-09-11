using System;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_05_PlayerLogicEffects : PTK_TriggerCommandBase
{
    public enum EPlayerDamageVFXType
    {
        E0_NONE,
        E_POISON,
        E_ELECTRICITY,
        E_FIRE,
        E_SMOKE
    }

    public static Action<CPlayerEffectBase, int, PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> OnPlayerLogicEffectExecute;

    [System.Serializable]
    public  abstract class CPlayerEffectBase
    {
        public enum EEffectType
        {
            E0_NONE,

            E1_BOUNCE,

            E2_KILL_PLAYER,
            E3_DAMAGE_PLAYER,
            E4_CONTINOUS_DAMAGE,

            E5_HEAL_PLAYER,

            E6_QUICK_DASH_MOVEMENT_WAYPOINTS,
            E6_QUICK_DASH_MOVEMENT_BEZIER,
            E6_QUICK_DASH_MOVEMENT_TELEPORT,

            E7_FROZEN_WHEELS,

            MORE_COMING_SOON = 9999
        }

        public bool bExecute = false;
        public abstract EEffectType GetEffectType();
        public abstract void AwakeInit();
    }


    [System.Serializable]
    public class CPlayerEffect_E1_Bounce : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public float fBounceStrength = 50.0f;

        public override void AwakeInit()
        {
        }

        public override EEffectType GetEffectType()
        {
            return EEffectType.E1_BOUNCE;
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E2_Kill : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public EPlayerDamageVFXType eDeadParticleEffect = EPlayerDamageVFXType.E0_NONE;

        [Header("Squish")]
        public bool bSquishPlayer = false;
        public float fSquishDuration = 5.0f;

        public override void AwakeInit()
        {
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E2_KILL_PLAYER; // change me to correct one!
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E3_Damage : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        [Header("Damage")]
        public int iDamage = 80;
        public EPlayerDamageVFXType eDamageVFX = EPlayerDamageVFXType.E0_NONE;
        [Header("Squish")]
        public bool bSquishPlayer = false;
        public float fSquishDuration = 5.0f;

        public override void AwakeInit()
        {
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E3_DAMAGE_PLAYER; // change me to correct one! 
        }

        //!! \/ create instance below and add to AddAllEffectsToList
    }

    [System.Serializable]
    public class CPlayerEffect_E4_ContinuousDamage : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {

        public enum EContinousDamageType
        {
            E_POISON_DAMAGE,
            E_FIRE_DAMAGE,
            E_CUSTOM
        }

        [HideInInspector]
        public bool bSkipFirstDamageTick = false;

        [Header("Preset")]
        public EContinousDamageType eContinousPreset = EContinousDamageType.E_POISON_DAMAGE;

        [Header("Custom - Select Custom Preset to edit")]
        public float fContinuousDamageDuration = 5.0f;
        public float fDamageTickEverySec = 1.0f;
        public int iDamagePerTick = 20;


        [Header("Continuous VFX")]
        public EPlayerDamageVFXType eDamageVFX = EPlayerDamageVFXType.E0_NONE;

        EContinousDamageType eContinousPresetLast = EContinousDamageType.E_CUSTOM;

        public void EditorUpdate_UseValuesFromPreset()
        {
            if (eContinousPreset == EContinousDamageType.E_CUSTOM && eContinousPresetLast != EContinousDamageType.E_CUSTOM)
                eDamageVFX = EPlayerDamageVFXType.E0_NONE;

            eContinousPresetLast = eContinousPreset;


            if (eContinousPreset == EContinousDamageType.E_CUSTOM)
                return;

            switch (eContinousPreset)
            {
                case PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage.EContinousDamageType.E_POISON_DAMAGE:
                    fContinuousDamageDuration = 10.0f;
                    fDamageTickEverySec = 1.0f;
                    iDamagePerTick = 10;
                    eDamageVFX = EPlayerDamageVFXType.E_POISON;
                    break;
                case PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage.EContinousDamageType.E_FIRE_DAMAGE:
                    fContinuousDamageDuration = 3.0f;
                    fDamageTickEverySec = 0.05f;
                    iDamagePerTick = 2;
                    eDamageVFX = EPlayerDamageVFXType.E_FIRE;
                    break;
            }

        }

        public override void AwakeInit()
        {
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E4_CONTINOUS_DAMAGE; // change me to correct one! 
        }

        //!! \/ create instance below and add to AddAllEffectsToList
    }

    [System.Serializable]
    public class CPlayerEffect_E5_HealPlayer : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {

        [Header("Instant Heal")]
        public int iHealHP = 100;

        [Header("Continous - Refill")]
        public bool bUseContinousHPRefill = false;
        public float fHealhToRefill = 50.0f;
        public float fRefillHealthInTime = 2.0f;

        [Header("Allow Overflow (Default Max: 300->500)")]
        public bool bMaxHealthOverflowEnabled = true;

        public override void AwakeInit()
        {
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E5_HEAL_PLAYER; // change me to correct one! 
        }

        //!! \/ create instance below and add to AddAllEffectsToList
    }





    [System.Serializable]
    public abstract class CPlayerEffect_E6_QuickDashMovementBase : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public enum EMoveSpeed
        {
            E_FAST_120,
            E_SUPER_FAST_180
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E6_QuickDashMovement_Waypoints : CPlayerEffect_E6_QuickDashMovementBase // make sure to add instance to playerEffects in Awake() !
    {
        [Header("Waypoints Parent")]
        public Transform waypointsParent;
        public EMoveSpeed eMoveSpeed = EMoveSpeed.E_FAST_120;
        public bool bMoveReverse = false;

        public override void AwakeInit()
        {
            if(waypointsParent != null)
            {
                for(int i=0;i< waypointsParent.childCount;i++)
                {
                    var meshRenderer = waypointsParent.GetChild(i).GetComponent<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.enabled = false;

                    var collider = waypointsParent.GetChild(i).GetComponent<Collider>();
                    if (collider != null) collider.enabled = false;
                }
            }
           
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E6_QUICK_DASH_MOVEMENT_WAYPOINTS; // change me to correct one!
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E6_QuickDashMovement_BezierSpline : CPlayerEffect_E6_QuickDashMovementBase // make sure to add instance to playerEffects in Awake() !
    {
        [Header("Bezier")]
        public PTK_BezierSpline bezierSpline;
        public EMoveSpeed eMoveSpeed = EMoveSpeed.E_FAST_120;
        public bool bMoveReverse = false;


        public override void AwakeInit()
        {
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E6_QUICK_DASH_MOVEMENT_BEZIER; // change me to correct one!
        }

        [EasyButtons.Button]
        public void CreateBezier()
        {

        }
    }


    [System.Serializable]
    public class CPlayerEffect_E6_QuickDashMovement_InstantTeleport : CPlayerEffect_E6_QuickDashMovementBase // make sure to add instance to playerEffects in Awake() !
    {
        [Header("Target Pos & Dir")]
        public Transform targetTransform;

        public override void AwakeInit()
        {
            if (targetTransform != null)
            {
                var meshRenderers = targetTransform.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    meshRenderers[i].enabled = false;
                }

                var colliders = targetTransform.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E6_QUICK_DASH_MOVEMENT_TELEPORT; // change me to correct one!
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E7_FrozenWheelsEffect : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public float fDuration = 5.0f;

        public override void AwakeInit()
        {
         
        }
        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E7_FROZEN_WHEELS; // change me to correct one!
        }
    }

    [System.Serializable]
    public class CPlayerEffect_MoreComingSoon : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public bool bAreYouThatExcited = false;

        public override void AwakeInit()
        {
        }

        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.MORE_COMING_SOON; // change me to correct one! 
        }

        //!! \/ create instance below and add to AddAllEffectsToList
    }

    public CPlayerEffect_E1_Bounce BounceEffect = new CPlayerEffect_E1_Bounce(); // !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E2_Kill KillPlayer = new CPlayerEffect_E2_Kill(); // !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E3_Damage DamagePlayer = new CPlayerEffect_E3_Damage(); // !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E4_ContinuousDamage DamagePlayer_Continous = new CPlayerEffect_E4_ContinuousDamage();// !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E5_HealPlayer HealPlayer = new CPlayerEffect_E5_HealPlayer();// !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E6_QuickDashMovement_Waypoints QuickDashMovement_Waypoints = new CPlayerEffect_E6_QuickDashMovement_Waypoints();// !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E6_QuickDashMovement_BezierSpline QuickDashMovement_Spline = new CPlayerEffect_E6_QuickDashMovement_BezierSpline();// !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E6_QuickDashMovement_InstantTeleport QuickDashMovement_InstantTeleport = new CPlayerEffect_E6_QuickDashMovement_InstantTeleport();// !! add item to list inside AddAllEffectsToList !!
    public CPlayerEffect_E7_FrozenWheelsEffect FrozenWheels = new CPlayerEffect_E7_FrozenWheelsEffect();

    public CPlayerEffect_MoreComingSoon MoreComingSoon = new CPlayerEffect_MoreComingSoon(); // !! add item to list inside AddAllEffectsToList !!
    void AddAllEffectsToList()
    {
        playerEffects.Add(BounceEffect);

        playerEffects.Add(KillPlayer);
        playerEffects.Add(DamagePlayer);
        playerEffects.Add(DamagePlayer_Continous);

        playerEffects.Add(HealPlayer);

        playerEffects.Add(QuickDashMovement_Waypoints);
        playerEffects.Add(QuickDashMovement_Spline);
        playerEffects.Add(QuickDashMovement_InstantTeleport);

        playerEffects.Add(FrozenWheels);

        playerEffects.Add(MoreComingSoon);

        for (int i = 0; i < playerEffects.Count; i++)
            playerEffects[i].AwakeInit();
    }

    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E05_PLAYER_LOGIC_EFFECTS;
    }

    List<CPlayerEffectBase> playerEffects = new List<CPlayerEffectBase>();

    public override void Awake()
    {
        AddAllEffectsToList();

    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }

    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        for(int i=0;i< recivedTriggerSignals.Count;i++)
        {
            if (recivedTriggerSignals[i].triggerTypeAndData.iPlayerType_GlobalPlayerIndex != -1)
                CommandExecutedForPlayer(recivedTriggerSignals[i].triggerTypeAndData.iPlayerType_GlobalPlayerIndex, recivedTriggerSignals[i]);
        }
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal, PTK_TriggerCommandsBehaviour _parentCommandBehaviour)
    {
        if(recivedTriggerSignal.triggerTypeAndData.iPlayerType_GlobalPlayerIndex != -1)
            CommandExecutedForPlayer(recivedTriggerSignal.triggerTypeAndData.iPlayerType_GlobalPlayerIndex, recivedTriggerSignal);
    }

    void CommandExecutedForPlayer(int iPlayerIndex, PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData triggerData)
    {
        Debug.LogError("Received signal from command behaviour triggers - for player " + iPlayerIndex);

        bool bContainsDamageEffect = false;
        for (int i = 0; i < playerEffects.Count; i++)
        {
            if (playerEffects[i].bExecute == false)
                continue;

            if(playerEffects[i].GetEffectType() == CPlayerEffectBase.EEffectType.E3_DAMAGE_PLAYER )
            {
                var effectDamage = playerEffects[i] as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E3_Damage;

                if(effectDamage != null)
                {
                    if (effectDamage.iDamage > 0)
                        bContainsDamageEffect = true;
                }
            }
        }

        for (int i = 0; i< playerEffects.Count;i++)
        {
            if (playerEffects[i].bExecute == false)
                continue;

            if (playerEffects[i].GetEffectType() == CPlayerEffectBase.EEffectType.E4_CONTINOUS_DAMAGE)
            {
                var effectDamageContinous = playerEffects[i] as PTK_Command_05_PlayerLogicEffects.CPlayerEffect_E4_ContinuousDamage;

                if (effectDamageContinous != null)
                {
                    effectDamageContinous.bSkipFirstDamageTick = bContainsDamageEffect; // so we wont get twice damage at the same time
                }
            }

            OnPlayerLogicEffectExecute?.Invoke(playerEffects[i],iPlayerIndex, triggerData);
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }


}

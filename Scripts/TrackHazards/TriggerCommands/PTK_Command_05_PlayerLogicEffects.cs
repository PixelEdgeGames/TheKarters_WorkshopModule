using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_05_PlayerLogicEffects : PTK_TriggerCommandBase
{
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


            MORE_COMING_SOON = 9999
        }

        public bool bExecute = false;
        public abstract EEffectType GetEffectType();
    }


    [System.Serializable]
    public class CPlayerEffect_E1_Bounce : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public float fBounceStrength = 50.0f;

        public override EEffectType GetEffectType()
        {
            return EEffectType.E1_BOUNCE;
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E2_Kill : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public enum EParticleEffect
        {
            E_NONE,
            COMING_SOON_PLEASE_UPDATE_LATER
            /*
            E_BLACK_SMOKE,
            E_ELECTRICITY,
            E_FIRE,
            E_SQUISH_LIGHT_VFX,
            E_POISON*/
        }

        public EParticleEffect eDeadParticleEffect = EParticleEffect.E_NONE;
        [Header("Squish")]
        public bool bSquishPlayer = false;
        public float fSquishDuration = 5.0f;

        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E2_KILL_PLAYER; // change me to correct one!
        }
    }

    [System.Serializable]
    public class CPlayerEffect_E3_Damage : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        // 1. set GetEffectType enum
        // 2. add effect to AddAllEffectsToList
        public enum EParticleEffect
        {
            E_NONE,
            COMING_SOON_PLEASE_UPDATE_LATER
            /*
            E_BLACK_SMOKE,
            E_ELECTRICITY,
            E_FIRE,
            E_SQUISH_LIGHT_VFX,
            E_POISON*/
        }

        [Header("Damage")]
        public int iDamage = 80;
        public EParticleEffect eDamageParticleEffect = EParticleEffect.E_NONE;
        [Header("Squish")]
        public bool bSquishPlayer = false;
        public float fSquishDuration = 5.0f;

        [Header("Continuous - Coming Soon")]
        public bool bUseContinuousDamage = false;
        /*
        public enum EContinousDamageType
        {
            E_POISON_DAMAGE,
            E_FIRE_DAMAGE,
            E_CUSTOM
        }

        [Header("Continuous VFX")]
        public EParticleEffect eContinousTickFVX = EParticleEffect.E_NONE;
        public EParticleEffect eContinousConstantVFX = EParticleEffect.E_NONE;

        [Header("Continuous Preset")]
        public EContinousDamageType eContinousPreset = EContinousDamageType.E_POISON_DAMAGE;

        [Header("Custom Continuous Settings")]
        public float fContinuousDamageDuration = 5.0f;
        public float fDamageTickEverySec = 1.0f;
        public float fDamagePerTick = 20.0f;
        
        */

        // !!! CHANGE ME TO CORRECT ONE
        public override EEffectType GetEffectType()
        {
            return EEffectType.E3_DAMAGE_PLAYER; // change me to correct one! 
        }

        //!! \/ create instance below and add to AddAllEffectsToList
    }

    [System.Serializable]
    public class CPlayerEffect_MoreComingSoon : CPlayerEffectBase // make sure to add instance to playerEffects in Awake() !
    {
        public bool bAreYouThatExcited = false;
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
    public CPlayerEffect_MoreComingSoon MoreComingSoon = new CPlayerEffect_MoreComingSoon(); // !! add item to list inside AddAllEffectsToList !!
    void AddAllEffectsToList()
    {
        playerEffects.Add(BounceEffect);
        playerEffects.Add(KillPlayer);
        playerEffects.Add(DamagePlayer);
        playerEffects.Add(MoreComingSoon);
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

    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        for(int i=0;i< recivedTriggerSignals.Count;i++)
        {
            if (recivedTriggerSignals[i].triggerTypeAndData.iPlayerType_GlobalPlayerIndex != -1)
                CommandExecutedForPlayer(recivedTriggerSignals[i].triggerTypeAndData.iPlayerType_GlobalPlayerIndex, recivedTriggerSignals[i]);
        }
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal)
    {
        if(recivedTriggerSignal.triggerTypeAndData.iPlayerType_GlobalPlayerIndex != -1)
            CommandExecutedForPlayer(recivedTriggerSignal.triggerTypeAndData.iPlayerType_GlobalPlayerIndex, recivedTriggerSignal);
    }

    void CommandExecutedForPlayer(int iPlayerIndex, PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData triggerData)
    {
        Debug.LogError("Received signal from command behaviour triggers - for player " + iPlayerIndex);

        for (int i = 0; i< playerEffects.Count;i++)
        {
            if (playerEffects[i].bExecute == false)
                continue;

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
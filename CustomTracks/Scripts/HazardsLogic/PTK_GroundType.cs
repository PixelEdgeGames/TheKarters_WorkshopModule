using UnityEngine;

public class PTK_GroundType : MonoBehaviour
{
    public static string strTagName = "PTK_GroundTypeTag";

    [Range(0.0f, 10.0f)]
    public float fGroundFriction = 0.5f;

    [HideInInspector]
    public Collider collider;
   
    [System.Serializable]
    public class CGroundSettings
    {
        [HideInInspector]
        public bool[] bPlayerLandedAndDrivingOnSurface = new bool[8]; // can be used to detect if player landed on surface / if it is driving on it
        [Header("Friction Type (Normal/Offroad/Ice)")]
        public EFrictionType eFrictionType = EFrictionType.E0_NORMAL_DEFAULT_GROUND;
        [Header("Skidemarks Color")]
        public ESkideMarksType eSkideMarksType = ESkideMarksType.E0_GROUND_DEFAULT_BLACK_1;
        [Header("Driving Particle VFX")]
        public EDriveParticleEffect eDrivingParticleEffect_1 = EDriveParticleEffect.E0_NONE_DEFAULT_GROUND;
        public EDriveParticleEffect eDrivingParticleEffect_2 = EDriveParticleEffect.E0_NONE_DEFAULT_GROUND;

        [Header("Logic Effects - Enabled/Disabled")]
        public ELogicEffectType eLogicEffect_1 = ELogicEffectType.E0_NONE;
        public ELogicEffectType eLogicEffect_2 = ELogicEffectType.E0_NONE;
        public ELogicEffectType eLogicEffect_3 = ELogicEffectType.E0_NONE;
        public ELogicEffectType eLogicEffect_4 = ELogicEffectType.E0_NONE;
        public ELogicEffectType eLogicEffect_5 = ELogicEffectType.E0_NONE;
        public ELogicEffectType eLogicEffect_6 = ELogicEffectType.E0_NONE;
        public enum EFrictionType
        {
            E0_NORMAL_DEFAULT_GROUND,
            E1_OFFROAD_MEDIUM,
            E2_OFFROAD_MUD_STRONG,
            E3_ICE_NO_FRICTION,

            __COUNT
        }

        public enum ESkideMarksType
        {
            E0_GROUND_DEFAULT_BLACK_1,
            E1_GROUND_GRAY_2,

            E2_GRASS_GREEN_1,
            E3_GRASS_GREEN_2,

            E4_ICE_1,
            E5_ICE_2,

            E6_WATER_1,
            E7_WATER_2,

            E8_SNOW_WHITE,

            E9_LAVA_1,
            E10_LAVA_2,

            E11_SAND_1,
            E12_SAND_2,

            __COUNT
        }

        public enum EDriveParticleEffect
        {
            E0_NONE_DEFAULT_GROUND,
            E1_GRASS,
            E2_WATER,
            E3_SNOW,
            E4_MUD,
            E5_SAND,
            E6_ROCKS,
            E7_POISON,
            E8_FIRE,
            E9_HEAL,
            E10_BOOST,

            __COUNT

        }

        public enum ELogicEffectType
        {
            E0_NONE,

            E1_BOUNCE_ON_LAND_LOW,
            E2_BOUNCE_ON_LAND_MED,
            E3_BOUNCE_ON_LAND_HIGH,

            E4_DAMAGE_10, // TOUCHING_OR_LANDED
            E5_DAMAGE_25,
            E6_DAMAGE_50,
            E7_DAMAGE_100,
            E8_DAMAGE_KILL,

            E9_HEAL_5, // TOUCHING_OR_LANDED
            E10_HEAL_10,
            E11_HEAL_25,
            E12_HEAL_50,
            E13_HEAL_100,

            E14_BOOSTPAD_CONTINUOUS, // TOUCHING_OR_LANDED

            E15_POISON,

            E16_FLATTENED,

            __COUNT
        }

    }

    public static bool ShouldSkidemarksBeAlwaysVisibleOnGround(CGroundSettings _eGroundType)
    {
        if (_eGroundType == null || _eGroundType.eFrictionType == CGroundSettings.EFrictionType.E0_NORMAL_DEFAULT_GROUND)
            return false;


        return true;
    }

    public CGroundSettings groundSettings = new CGroundSettings();


    // Start is called before the first frame update
    void Awake()
    {
        this.tag = strTagName;
        collider = this.GetComponent<Collider>();


        this.gameObject.layer = LayerMask.NameToLayer("GroundCollider");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

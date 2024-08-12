using UnityEngine;

[ExecuteInEditMode]
public class PTK_GroundType : MonoBehaviour
{
    public static string strTagName = "PTK_GroundTypeTag";

    [HideInInspector]
    public Collider collider;
   
    [System.Serializable]
    public class CGroundSettings
    {
        [Header("Friction Type (Normal/Offroad/Ice)")]
        public EFrictionType_16 eFrictionType = EFrictionType_16.E0_NORMAL_DEFAULT_GROUND; CVariable eFrictionType_4b = new CVariable(CVariable.EType.E_4_BIT_16_CHOICES, 0);
        [Header("Audio SFX")]
        public EAudioEffect_16 eDrivingAudioSFX = EAudioEffect_16.E0_NONE_DEFAULT_GROUND; CVariable eDrivingAudioSFX_4B = new CVariable(CVariable.EType.E_4_BIT_16_CHOICES, 0);

        [Header("Driving Particle VFX")]
        public EDriveParticleEffect_32 eDrivingParticleEffect_1 = EDriveParticleEffect_32.E0_NONE_DEFAULT_GROUND; CVariable eDrivingParticleEffect_1_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public EDriveParticleEffect_32 eDrivingParticleEffect_2 = EDriveParticleEffect_32.E0_NONE_DEFAULT_GROUND; CVariable eDrivingParticleEffect_2_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);


        [Header("Skidemarks Color")]
        public ESkideMarksType_16 eSkideMarksType = ESkideMarksType_16.E0_GROUND_DEFAULT_BLACK_1; CVariable eSkideMarksType_4B = new CVariable(CVariable.EType.E_4_BIT_16_CHOICES, 0);
        public Color previewSkidemarkColor = Color.black;

        [Header("Logic Effects - Enabled/Disabled")]
        public ELogicEffectType_32 eLogicEffect_1 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_1_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public ELogicEffectType_32 eLogicEffect_2 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_2_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public ELogicEffectType_32 eLogicEffect_3 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_3_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public ELogicEffectType_32 eLogicEffect_4 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_4_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public ELogicEffectType_32 eLogicEffect_5 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_5_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);
        public ELogicEffectType_32 eLogicEffect_6 = ELogicEffectType_32.E0_NONE; CVariable eLogicEffect_6_5B = new CVariable(CVariable.EType.E_5_BIT_32_CHOICES, 0);

        public (int,int,int) GetAsInt()
        {
            eFrictionType_4b.SetValue((int)eFrictionType);
            eSkideMarksType_4B.SetValue((int)eSkideMarksType);
            eDrivingParticleEffect_1_5B.SetValue((int)eDrivingParticleEffect_1);
            eDrivingParticleEffect_2_5B.SetValue((int)eDrivingParticleEffect_2);
            eLogicEffect_1_5B.SetValue((int)eLogicEffect_1);
            eLogicEffect_2_5B.SetValue((int)eLogicEffect_2);

            // int 2
            eLogicEffect_3_5B.SetValue((int)eLogicEffect_3); // 5
            eLogicEffect_4_5B.SetValue((int)eLogicEffect_4); // 10
            eLogicEffect_5_5B.SetValue((int)eLogicEffect_5); // 15
            eLogicEffect_6_5B.SetValue((int)eLogicEffect_6); // 20
            eDrivingAudioSFX_4B.SetValue((int)eDrivingAudioSFX); // 24


            return (ptkIntPacker_1.PackVariables(), ptkIntPacker_2.PackVariables(), ptkIntPacker_3.PackVariables());
        }

        PixelSDK_IntVariablePacker ptkIntPacker_1 = new PixelSDK_IntVariablePacker();
        PixelSDK_IntVariablePacker ptkIntPacker_2 = new PixelSDK_IntVariablePacker();
        PixelSDK_IntVariablePacker ptkIntPacker_3 = new PixelSDK_IntVariablePacker();

        public CGroundSettings()
        {
            ptkIntPacker_1.AddVariable(eFrictionType_4b);
            ptkIntPacker_1.AddVariable(eSkideMarksType_4B);
            ptkIntPacker_1.AddVariable(eDrivingParticleEffect_1_5B);//13
            ptkIntPacker_1.AddVariable(eDrivingParticleEffect_2_5B);//18
            ptkIntPacker_1.AddVariable(eLogicEffect_1_5B); // 23
            ptkIntPacker_1.AddVariable(eLogicEffect_2_5B); // 28

            ptkIntPacker_2.AddVariable(eLogicEffect_3_5B); // 5
            ptkIntPacker_2.AddVariable(eLogicEffect_4_5B); // 10
            ptkIntPacker_2.AddVariable(eLogicEffect_5_5B); // 15
            ptkIntPacker_2.AddVariable(eLogicEffect_6_5B); // 20
            ptkIntPacker_2.AddVariable(eDrivingAudioSFX_4B); // 24
        }
        public void ReadFromInt(int iData1,int iData2, int iData3)
        {
            ptkIntPacker_1.UnpackVariables(iData1);
            ptkIntPacker_2.UnpackVariables(iData2);
            ptkIntPacker_3.UnpackVariables(iData3);

            eFrictionType = (EFrictionType_16) eFrictionType_4b.Value;
            eSkideMarksType = (ESkideMarksType_16)eSkideMarksType_4B.Value;
            eDrivingParticleEffect_1 = (EDriveParticleEffect_32)eDrivingParticleEffect_1_5B.Value;
            eDrivingParticleEffect_2 = (EDriveParticleEffect_32)eDrivingParticleEffect_2_5B.Value;
            eLogicEffect_1 = (ELogicEffectType_32)eLogicEffect_1_5B.Value;
            eLogicEffect_2 = (ELogicEffectType_32)eLogicEffect_2_5B.Value;

            eLogicEffect_3 = (ELogicEffectType_32)eLogicEffect_3_5B.Value;
            eLogicEffect_4 = (ELogicEffectType_32)eLogicEffect_4_5B.Value;
            eLogicEffect_5 = (ELogicEffectType_32)eLogicEffect_5_5B.Value;
            eLogicEffect_6 = (ELogicEffectType_32)eLogicEffect_6_5B.Value;

            eDrivingAudioSFX = (EAudioEffect_16)eDrivingAudioSFX_4B.Value;


        }
        public enum EFrictionType_16
        {
            E0_NORMAL_DEFAULT_GROUND,
            E1_OFFROAD_MEDIUM,
            E2_OFFROAD_MUD_STRONG,
            E3_ICE_NO_FRICTION,

            __COUNT // max 16 types! for serialization
        }

        public enum ESkideMarksType_16
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

            __COUNT // max 16 types! for serialization
        }

        public static Color SkidemarkTypeToColor(ESkideMarksType_16 skidemark)
        {
            Color color = Color.black;
            switch (skidemark)
            {
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E0_GROUND_DEFAULT_BLACK_1:
                    color = new Color(0.0f, 0.0f, 0.0f, 1.0f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E1_GROUND_GRAY_2:
                    color = new Color(0.5f, 0.5f, 0.5f, 1.0f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E2_GRASS_GREEN_1:
                    color = new Color(0.0f, 1.0f, 0.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E3_GRASS_GREEN_2:
                    color = new Color(0.0f, 0.5f, 0.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E4_ICE_1:
                    color = new Color(0.6f, 0.8f, 1.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E5_ICE_2:
                    color = new Color(0.5f, 1.0f, 1.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E8_SNOW_WHITE:
                    color = new Color(1.0f, 1.0f, 1.0f, 1.0f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E6_WATER_1:
                    color = new Color(0.25f, 0.44f, 1.0f, 0.75f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E7_WATER_2:
                    color = new Color(0.0f, 0.14f, 1.0f, 0.75f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E9_LAVA_1:
                    color = new Color(1.0f, 0.0f, 0.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E10_LAVA_2:
                    color = new Color(1.0f, 0.4f, 0.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E11_SAND_1:
                    color = new Color(1.0f, 1.0f, 0.0f, 0.65f); break;
                case PTK_GroundType.CGroundSettings.ESkideMarksType_16.E12_SAND_2:
                    color = new Color(1.0f, 1.0f, 0.6f, 0.65f); break;
            }

            return color;
        }

        public enum EDriveParticleEffect_32
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
            E11_DIRT,
            E12_ICE,
            E14_ELECTRICITY,

            __COUNT // max 16 types! for serialization

        }

        public enum ELogicEffectType_32
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

            __COUNT // max 32 types! for serialization
        }

        public enum EAudioEffect_16
        {
            E0_NONE_DEFAULT_GROUND,
            E1_DIRT_OFFROAD_DEFAULT,
            E2_ICE,
            E3_WATER,
            E4_FIRE,


            __COUNT
        }

    }

    public static bool ShouldSkidemarksBeAlwaysVisibleOnGround(CGroundSettings _eGroundType)
    {
        if (_eGroundType == null || _eGroundType.eFrictionType == CGroundSettings.EFrictionType_16.E0_NORMAL_DEFAULT_GROUND)
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
        if (groundSettings.eFrictionType != PTK_GroundType.CGroundSettings.EFrictionType_16.E0_NORMAL_DEFAULT_GROUND && groundSettings.eDrivingAudioSFX == PTK_GroundType.CGroundSettings.EAudioEffect_16.E0_NONE_DEFAULT_GROUND)
        {
            // we have friction but no sound effect - setting to default
            groundSettings.eDrivingAudioSFX = PTK_GroundType.CGroundSettings.EAudioEffect_16.E1_DIRT_OFFROAD_DEFAULT;
        }

        groundSettings.previewSkidemarkColor = PTK_GroundType.CGroundSettings.SkidemarkTypeToColor(groundSettings.eSkideMarksType);
    }
}

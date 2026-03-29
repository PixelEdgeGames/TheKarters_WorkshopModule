using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PTK_Workshop/CharacterAnimationConfig")]
public class PTK_Workshop_CharAnimConfig : ScriptableObject
{
    [HideInInspector]
    public string strLastInitializationTime = "";
    [HideInInspector]
    public int iLastInitializationElementsCount = 0;

    [System.Serializable]
    public class AnimationCategory
    {
        public List<AnimationClip> Driving = new List<AnimationClip>();
        public List<AnimationClip> Events = new List<AnimationClip>();
        public List<AnimationClip> Menu = new List<AnimationClip>();
        public List<AnimationClip> ItemsModelAnim = new List<AnimationClip>();
        public List<AnimationClip> ItemsModelAnim_Common = new List<AnimationClip>();
        public List<AnimationClip> JumpTricks_SuperLong = new List<AnimationClip>();
        public List<AnimationClip> JumpTricks_NormalShort = new List<AnimationClip>();
        public List<AnimationClip> ItemUsage = new List<AnimationClip>();
        public List<AnimationClip> WeaponTargeting = new List<AnimationClip>();

        Dictionary<string, AnimationClip> nameToClip = new Dictionary<string, AnimationClip>();

        void TryAddClipToDictionary(AnimationClip clip, string listName)
        {
            string key = Remove_ABC_Prefix(clip.name);
            if (nameToClip.ContainsKey(key))
            {
                Debug.LogError($"Duplicate anim clip key '{key}' in list '{listName}'. " +
                    $"New clip: '{clip.name}', Existing clip: '{nameToClip[key].name}'");
                return;
            }
            nameToClip.Add(key, clip);
        }

        public AnimationClip GetClipByNameFull(string namePart,string alternative = "")
        {
            if(nameToClip.Count == 0)
            {
                foreach (var clip in Driving)
                    TryAddClipToDictionary(clip, "Driving");

                foreach (var clip in Events)
                    TryAddClipToDictionary(clip, "Events");

                foreach (var clip in Menu)
                    TryAddClipToDictionary(clip, "Menu");

                foreach (var clip in ItemsModelAnim)
                    TryAddClipToDictionary(clip, "ItemsModelAnim");

                foreach (var clip in ItemsModelAnim_Common)
                    TryAddClipToDictionary(clip, "ItemsModelAnim_Common");

                foreach (var clip in JumpTricks_SuperLong)
                    TryAddClipToDictionary(clip, "JumpTricks_SuperLong");

                foreach (var clip in JumpTricks_NormalShort)
                    TryAddClipToDictionary(clip, "JumpTricks_NormalShort");

                foreach (var clip in ItemUsage)
                    TryAddClipToDictionary(clip, "ItemUsage");

                foreach (var clip in WeaponTargeting)
                    TryAddClipToDictionary(clip, "WeaponTargeting");
            }

            namePart = namePart.ToLower();
            alternative = alternative.ToLower();

            if (nameToClip.ContainsKey(namePart))
                return nameToClip[namePart];

            if (nameToClip.ContainsKey(Remove_ABC_Prefix(namePart)))
                return nameToClip[Remove_ABC_Prefix(namePart)];

            if (nameToClip.ContainsKey(alternative))
                return nameToClip[alternative];

            if (nameToClip.ContainsKey(Remove_ABC_Prefix(alternative)))
                return nameToClip[Remove_ABC_Prefix(alternative)];

            Debug.LogError("Clip not found: " + namePart);

            return null;
        }

        // in scriptable object we have names for chaaracter _A and _B and _c , they are used to assign to correct SO, but inside the game we are using names for each character
        // AnimationCategory CharacterA , AnimationCategory CharacterB, AnimationCategory CharacterC and they shouldnt have this prefix because game logic code is not using it
        string Remove_ABC_Prefix(string strName)
        {
            string[] endings = { "_a", "_b", "_c" };

            strName = strName.ToLower();

            foreach (var ending in endings)
            {
                if (strName.EndsWith(ending))
                {
                    string strReturn = strName.Substring(0, strName.Length - ending.Length);
                    return strReturn;
                }
            }

            return strName;
        }
    }



    public AnimationCategory CharacterA = new AnimationCategory();
    public AnimationCategory CharacterB = new AnimationCategory();
    public AnimationCategory CharacterC = new AnimationCategory();
}

/*
Driving
- Drift_left_right
- Driving_idle
- Jump
- Reverse_idle
- turning_left_right
- Transition_to_reverse
- Turning_left_right

Events
- Acceleration
- Before_race
- Damage
- Defeat
- Overtaking_2_LEFT
- Overtaking_2_RIGHT
- Overtaking_LEFT
- Overtaking_RIGHT
- Sneeze
- Victory

Items
- Bazooka_catch
- Bazooka_idle
- Bazooka_shot
- Bazooka_ThrowAway
- Boost_catch
- Boost_ThrowAway
- Hammer_catch
- Hammer_hit
- Hammer_idle
- Hammer_ThrowAway
- HomingMissile_catch
- HomingMissile_ThrowAway
- Minigun_catch
- Minigun_idle
- Minigun_shot
- Minigun_ThrowAway
- PoisonGun_catch
- PoisonGun_idle
- PoisonGun_shot
- PoisonGun_ThrowAway
- PortalGun_catch
- PortalGun_idle
- PortalGun_shot
- PortalGun_ThrowAway
- Shield_catch
- Shield_ThrowAway
- Shotgun_catch
- Shotgun_idle
- Shotgun_shot
- Shotgun_ThrowAway
- SpawnButton_catch
- SpawnButton_idle
- SpawnButton_press
- SpawnButton_ThrowAway
- Star_catch
- Star_idle
- Star_Throw
- TeslaGun_catch
- TeslaGun_idle
- TeslaGun_shot
- TeslaGun_ThrowAway
- TeslaShield_catch
- TeslaShield_ThrowAway

JumpTricks
- Long_jump
- Long_jump_2
- Long_jump_3
- Short_jump
- Short_jump_2
- Short_jump_3
- Super_long_jump
- Super_long_jump_2

Weapons
- Item_Bazooka_catch
- Item_Bazooka_default
- Item_Bazooka_ThrowAway
- Item_Boost_catch
- Item_Boost_default
- Item_Boost_ThrowAway
- Item_Hammer_catch
- Item_Hammer_default
- Item_Hammer_hit
 - Item_Hammer_ThrowAway
- Item_HomingMissile_catch
- Item_HomingMissile_default
- Item_HomingMissile_ThrowAway
- Item_Minigun_catch
- Item_Minigun_default
- Item_Minigun_ThrowAway
- Item_PoisonGun_catch
- Item_PoisonGun_default
- Item_PortalGun_catch
- Item_PortalGun_default
- Item_PortalGun_ThrowAway
- Item_Shield_catch
- Item_Shield_default
- Item_Shield_ThrowAway
- Item_Shotgun_catch
- Item_Shotgun_default
- Item_Shotgun_ThrowAway
- Item_SpawnButton_catch
- Item_SpawnButton_default
- Item_SpawnButton_press
- Item_SpawnButton_ThrowAway
- Item_Star_catch
- Item_Star_default
- Item_TeslaGun_catch
- Item_TeslaGun_default
- Item_TeslaGun_ThrowAway
- Item_TeslaShield_catch
- Item_TeslaShield_ThrowAway

WeaponTargeting
- Bazooka_shot_back
- Shotgun_shot_back
- WeaponBig_pose_(0)
- WeaponBig_pose_(20down)
- WeaponBig_pose_(20up)
- WeaponBig_pose_(40up)
- WeaponBig_pose_(50down)
- WeaponBig_pose_(80up)
- WeaponSmall_pose_(0)
- WeaponSmall_pose_(20down)
- WeaponSmall_pose_(20up)
- WeaponSmall_pose_(40up)
- WeaponSmall_pose_(50down)
- WeaponSmall_pose_(80up)

*/
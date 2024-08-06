using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_OffroadGroundType : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float fGroundFriction = 0.5f;

    [HideInInspector]
    public Collider collider;
    public enum EOffroadGroundType
    {
        GRASS,
        DIRT_ROCKS,
        MUD,
        SAND,
        ICE,
        SNOW,
        WATER,
        LAVA,

        __COUNT,
        __NONE_OFF = 200
    }

    public EOffroadGroundType eOffroadGroundType = EOffroadGroundType.GRASS;

    // Start is called before the first frame update
    void Awake()
    {
        this.tag = "PTK_OffroadGround";
        collider = this.GetComponent<Collider>();


        this.gameObject.layer = LayerMask.NameToLayer("GroundCollider");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

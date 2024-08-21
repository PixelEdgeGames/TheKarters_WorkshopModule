using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PTK_PlayersInRangeVolume_Base : MonoBehaviour
{
    public enum EPlayerType
    {
        E0_LOCAL_PLAYER_WITH_CAMERA,
        E1_ALL_PLAYERS
    }
    public EPlayerType eLookForPlayersOfType = EPlayerType.E0_LOCAL_PLAYER_WITH_CAMERA;
    [HideInInspector]
    public bool[] bAreGlobalPlayersWithinRange = new bool[8];

    // Start is called before the first frame update
    
    public virtual void Awake()
    {
        Collider collider = this.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        var meshRenderer = this.GetComponent<MeshRenderer>();

        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    public virtual void Start()
    {
        
    }
    // Update is called once per frame
    public virtual void Update()
    {
       
    }
}

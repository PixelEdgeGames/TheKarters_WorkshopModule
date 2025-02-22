using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PTK_PlayersInRangeVolume_Base : MonoBehaviour
{
    public bool bShowVolumeMeshInPlaymode = false;

    public enum EPlayerType
    {
        E0_ALL_PLAYERS,
        E1_LOCAL_PLAYER_WITH_CAMERA_ONLY
    }
    public EPlayerType eLookForPlayersOfType = EPlayerType.E0_ALL_PLAYERS;
    [HideInInspector]
    public bool[] bAreGlobalPlayersWithinRange = new bool[8];

    // Start is called before the first frame update
    
    public virtual void Awake()
    {
        Collider collider = this.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;


        if(bShowVolumeMeshInPlaymode == false)
        {
            var meshRenderer = this.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
                meshRenderer.enabled = false;
        }
    }

    public int GetPlayersInsideVolumeCount()
    {
        int iPlayersCountInVolume = 0;
        for(int i=0;i< bAreGlobalPlayersWithinRange.Length;i++)
        {
            if (bAreGlobalPlayersWithinRange[i] == true)
                iPlayersCountInVolume++;
        }

        return iPlayersCountInVolume;
    }

    public virtual void Start()
    {
        
    }
    // Update is called once per frame
    public virtual void Update()
    {
       
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PTK_PlayersInRangeVolume_SphereFast : PTK_PlayersInRangeVolume_Base
{
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();

    }


    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        for (int i = 0; i < bAreGlobalPlayersWithinRange.Length; i++)
        {
            if (PTK_ModGameplayDataSync.Instance.playersInfo[i].bIsPlayerEnabled == true && Vector3.Magnitude(transform.position - PTK_ModGameplayDataSync.Instance.playersInfo[i].vPosition) < this.transform.lossyScale.x)
            {
                if (eLookForPlayersOfType == EPlayerType.E0_LOCAL_PLAYER_WITH_CAMERA && PTK_ModGameplayDataSync.Instance.playersInfo[i].iLocalCameraIndex == -1)
                    bAreGlobalPlayersWithinRange[i] = false; // no camera
                else
                    bAreGlobalPlayersWithinRange[i] = true;
            }
            else
            {
                bAreGlobalPlayersWithinRange[i] = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_PlayersInRangeVolume_BoxSlow : PTK_PlayersInRangeVolume_Base
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
            if (PTK_ModGameplayDataSync.Instance.playersInfo[i].bIsPlayerEnabled == true && IsInside( PTK_ModGameplayDataSync.Instance.playersInfo[i].vPosition) == true)
            {
                if (eLookForPlayersOfType == EPlayerType.E1_LOCAL_PLAYER_WITH_CAMERA_ONLY && PTK_ModGameplayDataSync.Instance.playersInfo[i].iLocalPlayerIndex == -1)
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

    bool IsInside(Vector3 vPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(vPoint);

        // Step 2: Check if the local point is inside the unit box centered at the origin
        // Box dimensions in local space are -0.5 to 0.5 in each axis after adjusting for scale
        Vector3 halfScale = Vector3.one * 0.5f;

        bool isInside = (localPoint.x >= -halfScale.x && localPoint.x <= halfScale.x) &&
                        (localPoint.y >= -halfScale.y && localPoint.y <= halfScale.y) &&
                        (localPoint.z >= -halfScale.z && localPoint.z <= halfScale.z);

        return isInside;
    }
}

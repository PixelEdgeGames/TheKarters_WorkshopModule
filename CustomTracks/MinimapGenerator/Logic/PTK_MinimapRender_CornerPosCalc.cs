using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_MinimapRender_CornerPosCalc : MonoBehaviour
{
    public Camera cameraOrthoRender;
    public GameObject corner_BL;
    public GameObject corner_TR;

    public bool bRefreshCornerPos = false;

    public Vector2 bl_CornerWorldPos;
    public Vector2 tr_CornerWorldPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(bRefreshCornerPos == true)
        {
            bRefreshCornerPos = false;

            RefreshCornerPos();
        }
    }

    public void RefreshCornerPos()
    {
        Vector3 vPosXZ = cameraOrthoRender.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, -1000.0f));

        Vector3 vCornerObjPos = corner_BL.transform.position;
        vCornerObjPos.x = bl_CornerWorldPos.x = vPosXZ.x;
        vCornerObjPos.z = bl_CornerWorldPos.y = vPosXZ.z;


        corner_BL.transform.position = vCornerObjPos;


        vPosXZ = cameraOrthoRender.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, -1000.0f));

        vCornerObjPos = corner_TR.transform.position;
        vCornerObjPos.x = tr_CornerWorldPos.x = vPosXZ.x;
        vCornerObjPos.z = tr_CornerWorldPos.y = vPosXZ.z;

        corner_TR.transform.position = vCornerObjPos;


        if(cameraOrthoRender.targetTexture == null)
        {
            if (Screen.width != Screen.height )
            {
                Debug.LogError("Please make Unity Scene GameView size as ortho 2000x2000 before calculating minimap corners");
                tr_CornerWorldPos = bl_CornerWorldPos = new Vector2(-99999, -99999);
            }
        }
        else
        {
            if (cameraOrthoRender.targetTexture.width != cameraOrthoRender.targetTexture.height )
            {
                Debug.LogError("Please make Unity camera render texture size as ortho 2000x2000 before calculating minimap corners");
                tr_CornerWorldPos = bl_CornerWorldPos = new Vector2(-99999, -99999);
            }
        }
    }

}

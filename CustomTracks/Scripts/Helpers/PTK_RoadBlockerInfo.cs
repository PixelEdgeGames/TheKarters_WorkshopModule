using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_RoadBlockerInfo : MonoBehaviour
{
    public int iHiddenFromLapNrOnHUD = -1;
    public bool bRoadSegment1_Affected = false;
    public bool bRoadSegment2_Affected = false;
    public bool bRoadSegment3_Affected = false;
    public bool bRoadSegment4_Affected = false;
    public bool bRoadSegment5_Affected = false;
    public bool bRoadSegment6_Affected = false;
    public bool bRoadSegment7_Affected = false;
    public bool bRoadSegment8_Affected = false;
    public bool bRoadSegment9_Affected = false;
    public bool bRoadSegment10_Affected = false;

    public bool[] roadSegmentsIsAffected
    {
        get
        {
            return new bool[] { bRoadSegment1_Affected , bRoadSegment2_Affected , bRoadSegment3_Affected , bRoadSegment4_Affected , bRoadSegment5_Affected,
            bRoadSegment6_Affected,bRoadSegment7_Affected,bRoadSegment8_Affected,bRoadSegment9_Affected,bRoadSegment10_Affected};
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

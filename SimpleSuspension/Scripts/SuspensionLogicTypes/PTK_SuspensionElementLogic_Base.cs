using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class PTK_SuspensionElementLogic_Base : MonoBehaviour
{
    [Header("Fixed (start) Point")]
    public Transform fixedAttachedToPoint; // which point should not move / move a little based on distance
    public Vector3 fixedPointLocalOffset;
}

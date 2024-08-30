using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CPC_BezierPath))]
public class PTK_BezierSpline : MonoBehaviour
{
    [HideInInspector]
    public CPC_BezierPath bezierPath;

    // Start is called before the first frame update
    void Awake()
    {
        if (bezierPath == null)
            bezierPath = this.GetComponent<CPC_BezierPath>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

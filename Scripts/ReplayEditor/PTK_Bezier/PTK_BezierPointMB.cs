using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_BezierPointMB : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    Vector3 vLastPos;
    Quaternion qLastRot;
    PTK_BezierMB lastBezier;

    private void OnDestroy()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            if (vLastPos != transform.position || qLastRot != transform.rotation)
            {
                if (lastBezier == null)
                    lastBezier = this.GetComponentInParent<PTK_BezierMB>();

                if (lastBezier != null)
                    lastBezier.RegenerateSpline();

                UnityEditor.SceneView.RepaintAll();
            }

            vLastPos = transform.position;
            qLastRot = transform.rotation;
        }
#endif
    }
}

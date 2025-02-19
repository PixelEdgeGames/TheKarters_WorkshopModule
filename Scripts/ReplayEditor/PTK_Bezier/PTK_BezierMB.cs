using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PTK_BezierMB : MonoBehaviour
{
    public Transform bezierPointsParent;
    public PTK_Bezier ptkBezier = new PTK_Bezier();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int iLastChildCount = 0;
    // Update is called once per frame
    void Update()
    {
        if(iLastChildCount != bezierPointsParent.childCount)
        {
            RegenerateSpline();
        }

        iLastChildCount = bezierPointsParent.childCount;
    }

    public void RegenerateSpline()
    {
        ptkBezier.GenerateBezier(bezierPointsParent);

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }
    private void OnDrawGizmos()
    {
        if(UnityEditor.Selection.activeGameObject != null && UnityEditor.Selection.activeGameObject.GetComponentInParent<PTK_BezierMB>() != null)
            ptkBezier.DrawGizmos();
    }
}

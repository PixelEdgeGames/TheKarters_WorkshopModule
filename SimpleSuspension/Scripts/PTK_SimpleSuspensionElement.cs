using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class PTK_SimpleSuspensionElement : MonoBehaviour
{
    [Header("Model Start/End")]
    public Transform modelBottomTransform;
    public Transform modelTopTransform;


    [Header("Priv")]
    [HideInInspector]
    public float fOriginalSizeZ = 1.0f;
    [HideInInspector]
    public float fOriginalScaleZ = 1.0f;
    [HideInInspector]
    public Quaternion originalLocalRotation;

    void Start()
    {
        InitOriginalInfo();
    }

    void InitOriginalInfo()
    {
        originalLocalRotation = transform.localRotation;
        fOriginalSizeZ = Vector3.Magnitude(modelTopTransform.transform.position - modelBottomTransform.transform.position);
        fOriginalScaleZ = transform.localScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying == false)
            InitOriginalInfo();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_ModTriggerEditorHelper : MonoBehaviour
{
    public PTK_TriggerArrayCommandsExecutor triggerArrayCommandsExecutorParent;

    public GameObject instantiateInTriggersParent;
    public GameObject commandsBehaviourParent;
    public PTK_TriggerCommandsBehaviour commandsBehaviourPrefab;

    public GameObject PlayerOrBulletPhysicsCollisionBox;
    public GameObject PlayerOrBulletPhysicsCollisionSphere;
    public GameObject PlayerOrBulletPhysicsCollisionMultiple;
    public GameObject GameEventTrigger;
    public GameObject PlayerEventTriggerBox;
    public GameObject PlayerEventTriggerSphere;
    public GameObject GameConditionTrigger;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

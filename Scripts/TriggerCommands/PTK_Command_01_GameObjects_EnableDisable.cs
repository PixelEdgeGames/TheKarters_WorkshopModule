using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_01_GameObjects_EnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E01_ENABLE_DISABLE_GAME_OBJECT;
    }

    public GameObject[] gameObjectsToEnable;
    public GameObject[] gameObjectsToDisable;

    Dictionary<GameObject, bool> defaultEnabledState = new Dictionary<GameObject, bool>();

    public override void Awake()
    {
        foreach (GameObject go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.gameObject.activeInHierarchy);
        }

        foreach (GameObject go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            if (defaultEnabledState.ContainsKey(go) == false)
                defaultEnabledState.Add(go, go.gameObject.activeInHierarchy);
        }
    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }
    protected override void ExecuteImpl(List<PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggerArrayCommandsExecutor.CRecivedTriggerWithData recivedTriggerSignal)
    {
        CommandExecuted();
    }

    void CommandExecuted()
    {
        foreach (GameObject go in gameObjectsToEnable)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(true);
        }

        foreach (GameObject go in gameObjectsToDisable)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(false);
        }
    }


    protected override void RaceResetted_RevertToDefault()
    {
        foreach (GameObject go in defaultEnabledState.Keys)
        {
            if (go == null)
                continue;

            go.gameObject.SetActive(defaultEnabledState[go]);
        }
    }

    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerCommand_01_GameObjectsEnableDisable : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E01_GAME_OBJECT_ENABLE_DISABLE;
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

    protected override void ExecuteImpl(List<PTK_TriggersCommandsLauncher.CRecivedTriggerWithData> recivedTriggerSignals)
    {
        CommandExecuted();
    }

    protected override void ExecuteImpl(PTK_TriggersCommandsLauncher.CRecivedTriggerWithData recivedTriggerSignal)
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

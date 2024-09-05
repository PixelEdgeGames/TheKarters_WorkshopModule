using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerArrayCommandsExecutor : MonoBehaviour
{
    [System.Serializable]
    public class CRecivedTriggerWithData
    {
        public PTK_ModBaseTrigger trigger;
        public PTK_ModBaseTrigger.CTriggerEventType triggerTypeAndData;
        public bool bSignalReceived = false;
    }

    public enum ECommandsRunCondition
    {
        E0_WHEN_RECIVED_EVENT_FROM_ANY_TRIGGER,
        E1_WHEN_RECEIVED_EVENT_DATA_FROM_ALL_TRIGGERS
    }

    public enum ECommandsRunMode
    {
        E0_YES_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN,
        E1_NO_REQUIRE_MANUAL_CLEAR
    }

    public enum ECommandBehaviourExecutionMode
    {
        E0_EXECUTE_ALL_SIMULTANEOUSLY,  // Run all commands at the same time
        E1_EXECUTE_ONE_IN_ORDER,        // Run each command one at a time in order from first to last
        E2_EXECUTE_ONE_IN_ORDER_PING_PONG            // Run each command one at a time, then reverse direction
    }

    public bool bModTriggerCommandExecutorEnabled = true;

    public List<GameObject> receiveEventsFromAllTriggerInGameObjects = new List<GameObject>();
    public List<PTK_ModBaseTrigger> receiveEventsFromTriggers = new List<PTK_ModBaseTrigger>();

    [Header("PreviewOnly - Receive Signals from Triggers")]
     List<CRecivedTriggerWithData> recivedTriggerEventsPreview = new List<CRecivedTriggerWithData>();
    [Header("When to send commands")]
    public ECommandsRunCondition eRunCommandCondition = ECommandsRunCondition.E0_WHEN_RECIVED_EVENT_FROM_ANY_TRIGGER;
    [Header("Allow Sending Commands Again?")]
    public ECommandsRunMode eRunCommandsMode = ECommandsRunMode.E0_YES_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN;
    [Header("Execution Mode")]
    public ECommandBehaviourExecutionMode eCommandBehavioursExecutionMode = ECommandBehaviourExecutionMode.E0_EXECUTE_ALL_SIMULTANEOUSLY;

    [Header("Commands To Send")]
    public List<GameObject> commandBehavioursParentsToRun = new List<GameObject>();

    List<PTK_TriggerCommandsBehaviour> commandBehavioursLogicsToRun = new List<PTK_TriggerCommandsBehaviour>();

    bool bAlreadyCommandSent = false;

    int iBehaviourIndexToRun = 0;
    int iPingPongDirection = 1;

    private void Start()
    {
        for(int i=0;i< commandBehavioursParentsToRun.Count;i++)
        {
          var behaviours = commandBehavioursParentsToRun[i].GetComponentsInChildren<PTK_TriggerCommandsBehaviour>();
            for(int iBehav = 0; iBehav < behaviours.Length;iBehav++)
            {
                if (commandBehavioursLogicsToRun.Contains(behaviours[iBehav]) == false)
                    commandBehavioursLogicsToRun.Add(behaviours[iBehav]);
            }
        }
        recivedTriggerEventsPreview = new List<CRecivedTriggerWithData>();

        for (int i=0;i< receiveEventsFromTriggers.Count;i++)
        {
            if (receiveEventsFromTriggers[i] == null)
                continue;

            var receivedInfo = new CRecivedTriggerWithData();
            receivedInfo.trigger = receiveEventsFromTriggers[i];

            recivedTriggerEventsPreview.Add(receivedInfo);
        }

        for (int i = 0; i < receiveEventsFromAllTriggerInGameObjects.Count; i++)
        {
            var triggersInside = receiveEventsFromAllTriggerInGameObjects[i].GetComponentsInChildren<PTK_ModBaseTrigger>();

            for(int iTrigger = 0; iTrigger < triggersInside.Length;iTrigger++)
            {
                if(receiveEventsFromTriggers.Contains(triggersInside[iTrigger]) == false)
                {
                    if (triggersInside[iTrigger] == null)
                        continue;

                    var receivedInfo = new CRecivedTriggerWithData();
                    receivedInfo.trigger = triggersInside[iTrigger];

                    recivedTriggerEventsPreview.Add(receivedInfo);
                }
            }
        }


        for (int i=0;i< recivedTriggerEventsPreview.Count;i++)
        {
            if (recivedTriggerEventsPreview[i].trigger == null)
                continue;

            int currentIndexLambda = i;
            recivedTriggerEventsPreview[currentIndexLambda].trigger.OnTriggerEvent += (PTK_ModBaseTrigger.CTriggerEventType triggerTypeAndData) =>
           {
               // we will allow to run logic only if the signal receiver is not disabled
               if(this.gameObject.activeInHierarchy == true)
               {
                   recivedTriggerEventsPreview[currentIndexLambda].bSignalReceived = true;
                   recivedTriggerEventsPreview[currentIndexLambda].triggerTypeAndData = triggerTypeAndData;
                   //  recivedTriggerEventsPreview[currentIndexLambda].triggerEventData = triggerData;

                   TriggerEventReceived();
               }
           };
        }

        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted += OnRaceResetted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart += OnRaceTimerJustStarted;
    }

    private void OnDestroy()
    {
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceRestarted -= OnRaceResetted;
        PTK_ModGameplayDataSync.Instance.gameEvents.OnGameEvent_RaceTimerStart -= OnRaceTimerJustStarted;
    }

    private void OnRaceTimerJustStarted()
    {
        for (int iBehaviour = 0; iBehaviour < commandBehavioursLogicsToRun.Count; iBehaviour++)
        {
            if (commandBehavioursLogicsToRun[iBehaviour] != null)
            {
                commandBehavioursLogicsToRun[iBehaviour].OnRaceTimerJustStarted_SyncAndRunAnims();
            }
        }
    }

    private void OnRaceResetted()
    {
        iBehaviourIndexToRun = 0;
        iPingPongDirection = 1;

        ClearReceivedEventsInfo();

        for (int iBehaviour = 0; iBehaviour < commandBehavioursLogicsToRun.Count; iBehaviour++)
        {
            if (commandBehavioursLogicsToRun[iBehaviour] != null)
            {
                commandBehavioursLogicsToRun[iBehaviour].RaceResetted();
            }
        }
    }

    private void TriggerEventReceived()
    {
        if (recivedTriggerEventsPreview.Count == 0)
            return;

        bool bReceivedSignalsFromAllTriggers = true;
        bool bReceivedSignalFromAtLeasOne = false;

        List<CRecivedTriggerWithData> triggersWithReceivedSignals = new List<CRecivedTriggerWithData>();

        for (int i = 0; i < recivedTriggerEventsPreview.Count; i++)
        {
            if (recivedTriggerEventsPreview[i].trigger == null)
                continue;

            bReceivedSignalsFromAllTriggers &= recivedTriggerEventsPreview[i].bSignalReceived;
            bReceivedSignalFromAtLeasOne |= recivedTriggerEventsPreview[i].bSignalReceived;

            // only if we received signal so we have data - we can have E0_WHEN_RECIVED_EVENT_FROM_ANY_TRIGGER and this will have null data if there are for example 2 triggers but only one received data
            if (recivedTriggerEventsPreview[i].bSignalReceived == true)
            {
                triggersWithReceivedSignals.Add(recivedTriggerEventsPreview[i]);
            }
        }

        bool bCanSendCommand = false;
        if (eRunCommandCondition == ECommandsRunCondition.E1_WHEN_RECEIVED_EVENT_DATA_FROM_ALL_TRIGGERS && bReceivedSignalsFromAllTriggers == true)
            bCanSendCommand = true;

        if (eRunCommandCondition == ECommandsRunCondition.E0_WHEN_RECIVED_EVENT_FROM_ANY_TRIGGER && bReceivedSignalFromAtLeasOne == true)
            bCanSendCommand = true;


        if(bCanSendCommand == true && bAlreadyCommandSent == false)
        {
            bAlreadyCommandSent = true; // important to be here, because if any exception in sendcommands then it will have infinite loop

            SendCommands(triggersWithReceivedSignals);

            if (eRunCommandsMode == ECommandsRunMode.E0_YES_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN)
            {
                ClearReceivedEventsInfo();
            }
        }

    }

    void SendCommands(List<CRecivedTriggerWithData> triggersWithReceivedSignals)
    {
        for (int iBehaviour = 0; iBehaviour < commandBehavioursLogicsToRun.Count; iBehaviour++)
        {
            if (eCommandBehavioursExecutionMode != ECommandBehaviourExecutionMode.E0_EXECUTE_ALL_SIMULTANEOUSLY && iBehaviour != iBehaviourIndexToRun)
                continue;

            if (commandBehavioursLogicsToRun[iBehaviour] != null)
            {
                if (eRunCommandCondition == ECommandsRunCondition.E1_WHEN_RECEIVED_EVENT_DATA_FROM_ALL_TRIGGERS)
                {
                    commandBehavioursLogicsToRun[iBehaviour].Execute(triggersWithReceivedSignals);
                }

                if (eRunCommandCondition == ECommandsRunCondition.E0_WHEN_RECIVED_EVENT_FROM_ANY_TRIGGER )
                {
                    commandBehavioursLogicsToRun[iBehaviour].Execute(triggersWithReceivedSignals);
                }
            }
        }


        if (commandBehavioursLogicsToRun.Count > 1) // at least 2 elements!
        {
            if (eCommandBehavioursExecutionMode == ECommandBehaviourExecutionMode.E1_EXECUTE_ONE_IN_ORDER)
            {
                iBehaviourIndexToRun++; iBehaviourIndexToRun %= commandBehavioursLogicsToRun.Count;
            }
            else if (eCommandBehavioursExecutionMode == ECommandBehaviourExecutionMode.E2_EXECUTE_ONE_IN_ORDER_PING_PONG)
            {
                // Move in current direction (forward or backward)
                iBehaviourIndexToRun += iPingPongDirection;

                // If we go past the last element, reverse direction and move to the second last element
                if (iBehaviourIndexToRun >= commandBehavioursLogicsToRun.Count)
                {
                    iPingPongDirection = -1;
                    iBehaviourIndexToRun = commandBehavioursLogicsToRun.Count - 2;
                }

                // If we go before the first element, reverse direction and move to the second element
                else if (iBehaviourIndexToRun < 0)
                {
                    iPingPongDirection = 1;
                    iBehaviourIndexToRun = 1;
                }
            }
        }

    }

     void ClearReceivedEventsInfo()
    {
        for (int i = 0; i < recivedTriggerEventsPreview.Count; i++)
        {
            recivedTriggerEventsPreview[i].bSignalReceived = false;
          //  recivedTriggerEventsPreview[i].triggerEventData = null;
        }

        bAlreadyCommandSent = false;
    }

    public void ManualResetAllowSendingAgain()
    {
        ClearReceivedEventsInfo();
    }
}

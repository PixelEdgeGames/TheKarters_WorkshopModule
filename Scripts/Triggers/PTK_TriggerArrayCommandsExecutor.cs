using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggerArrayCommandsExecutor : MonoBehaviour
{
    [System.Serializable]
    public class CRecivedTriggerWithData
    {
        public PTK_Mod_Trigger trigger;
        public PTK_Mod_Trigger.CTriggerEventData triggerEventData;
        public bool bSignalReceived = false;
    }

    public enum ERunCommandsCondition
    {
        E0_RUN_COMMANDS_IF_RECEIVED_DATA_FROM_ALL_TRIGGERS,
        E1_RUN_COMMANDS_IF_RECIVED_TRIGGER_DATA_FROM_AT_LEAST_ONE
    }

    public enum EAllowSendingCommandsAgain
    {
        E_YES_AUTO_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN,
        E_NO_NEED_TO_WAIT_FOR_MANUAL_CLEAR_FIRST
    }

    public List<PTK_Mod_Trigger> triggersToReceiveDataOnTriggerEvent = new List<PTK_Mod_Trigger>();

    [Header("PreviewOnly - Receive Signals from Triggers")]
     List<CRecivedTriggerWithData> recivedTriggerEventsPreview = new List<CRecivedTriggerWithData>();
    [Header("When to send commands")]
    public ERunCommandsCondition eRunCommandCondition = ERunCommandsCondition.E0_RUN_COMMANDS_IF_RECEIVED_DATA_FROM_ALL_TRIGGERS;
    [Header("Allow Sending Commands Again?")]
    public EAllowSendingCommandsAgain eAutoClearAndAllowSendingAgain = EAllowSendingCommandsAgain.E_YES_AUTO_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN;
    [Header("Commands To Send")]
    public List<PTK_TriggerCommandsBehaviour> commandBehavioursToRun = new List<PTK_TriggerCommandsBehaviour>();

    bool bAlreadyCommandSent = false;

    private void Start()
    {
        recivedTriggerEventsPreview = new List<CRecivedTriggerWithData>();

        for (int i=0;i< triggersToReceiveDataOnTriggerEvent.Count;i++)
        {
            if (triggersToReceiveDataOnTriggerEvent[i] == null)
                continue;

            var receivedInfo = new CRecivedTriggerWithData();
            receivedInfo.trigger = triggersToReceiveDataOnTriggerEvent[i];

            recivedTriggerEventsPreview.Add(receivedInfo);
        }


        for (int i=0;i< recivedTriggerEventsPreview.Count;i++)
        {
            if (recivedTriggerEventsPreview[i].trigger == null)
                continue;

            int currentIndexLambda = i;
            recivedTriggerEventsPreview[currentIndexLambda].trigger.OnTriggerEventNetSynced += (triggerData) =>
           {
               // we will allow to run logic only if the signal receiver is not disabled
               if(this.gameObject.activeInHierarchy == true)
               {
                   recivedTriggerEventsPreview[currentIndexLambda].bSignalReceived = true;
                   recivedTriggerEventsPreview[currentIndexLambda].triggerEventData = triggerData;

                   TriggerEventReceived();
               }
           };

            recivedTriggerEventsPreview[currentIndexLambda].trigger.OnRaceResettedEvent += OnRaceResetted;
            recivedTriggerEventsPreview[currentIndexLambda].trigger.OnRaceTimerSyncedJustStarted += OnRaceTimerJustStarted;
        }
    }

    private void OnRaceTimerJustStarted()
    {
        for (int iBehaviour = 0; iBehaviour < commandBehavioursToRun.Count; iBehaviour++)
        {
            if (commandBehavioursToRun[iBehaviour] != null)
            {
                commandBehavioursToRun[iBehaviour].OnRaceTimerJustStarted_SyncAndRunAnims();
            }
        }
    }

    private void OnRaceResetted()
    {
        ClearReceivedEventsInfo();

        for (int iBehaviour = 0; iBehaviour < commandBehavioursToRun.Count; iBehaviour++)
        {
            if (commandBehavioursToRun[iBehaviour] != null)
            {
                commandBehavioursToRun[iBehaviour].RaceResetted();
            }
        }
    }

    private void TriggerEventReceived()
    {
        if (recivedTriggerEventsPreview.Count == 0)
            return;

        bool bReceivedSignalsFromAllTriggers = true;
        bool bReceivedSignalFromAtLeasOne = false;

        CRecivedTriggerWithData firstReceivedTrigger = null;

        for (int i = 0; i < recivedTriggerEventsPreview.Count; i++)
        {
            if (recivedTriggerEventsPreview[i].trigger == null)
                continue;

            bReceivedSignalsFromAllTriggers &= recivedTriggerEventsPreview[i].bSignalReceived;
            bReceivedSignalFromAtLeasOne |= recivedTriggerEventsPreview[i].bSignalReceived;

            if(firstReceivedTrigger == null && recivedTriggerEventsPreview[i].bSignalReceived == true)
            {
                firstReceivedTrigger = recivedTriggerEventsPreview[i];
            }
        }

        bool bCanSendCommand = false;
        if (eRunCommandCondition == ERunCommandsCondition.E0_RUN_COMMANDS_IF_RECEIVED_DATA_FROM_ALL_TRIGGERS && bReceivedSignalsFromAllTriggers == true)
            bCanSendCommand = true;

        if (eRunCommandCondition == ERunCommandsCondition.E1_RUN_COMMANDS_IF_RECIVED_TRIGGER_DATA_FROM_AT_LEAST_ONE && bReceivedSignalFromAtLeasOne == true)
            bCanSendCommand = true;


        if(bCanSendCommand == true && bAlreadyCommandSent == false)
        {
            SendCommands(firstReceivedTrigger);
            bAlreadyCommandSent = true;

            if (eAutoClearAndAllowSendingAgain == EAllowSendingCommandsAgain.E_YES_AUTO_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN)
            {
                ClearReceivedEventsInfo();
            }
        }

    }

    void SendCommands(CRecivedTriggerWithData firstReceivedTriggerSignal)
    {
        for (int iBehaviour = 0; iBehaviour < commandBehavioursToRun.Count; iBehaviour++)
        {
            if (commandBehavioursToRun[iBehaviour] != null)
            {
                if (eRunCommandCondition == ERunCommandsCondition.E0_RUN_COMMANDS_IF_RECEIVED_DATA_FROM_ALL_TRIGGERS)
                {
                    commandBehavioursToRun[iBehaviour].Execute(recivedTriggerEventsPreview);
                }

                if (eRunCommandCondition == ERunCommandsCondition.E1_RUN_COMMANDS_IF_RECIVED_TRIGGER_DATA_FROM_AT_LEAST_ONE )
                {
                    commandBehavioursToRun[iBehaviour].Execute(firstReceivedTriggerSignal);
                }
            }
        }
    }

     void ClearReceivedEventsInfo()
    {
        for (int i = 0; i < recivedTriggerEventsPreview.Count; i++)
        {
            recivedTriggerEventsPreview[i].bSignalReceived = false;
            recivedTriggerEventsPreview[i].triggerEventData = null;
        }

        bAlreadyCommandSent = false;
    }

    public void ManualResetAllowSendingAgain()
    {
        ClearReceivedEventsInfo();
    }
}

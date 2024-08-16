using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_TriggersCommandsLauncher : MonoBehaviour
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

    public List<PTK_Mod_Trigger> receiveDataFromTriggersOnEnterExitEvent = new List<PTK_Mod_Trigger>();

    [Header("PreviewOnly - Receive Signals from Triggers")]
     List<CRecivedTriggerWithData> recivedTriggerSignalsPreview = new List<CRecivedTriggerWithData>();
    [Header("When to send commands")]
    public ERunCommandsCondition eRunCommandCondition = ERunCommandsCondition.E0_RUN_COMMANDS_IF_RECEIVED_DATA_FROM_ALL_TRIGGERS;
    [Header("Allow Sending Commands Again?")]
    public EAllowSendingCommandsAgain eAutoClearAndAllowSendingAgain = EAllowSendingCommandsAgain.E_YES_AUTO_CLEAR_AND_WAIT_FOR_TRIGGERS_AGAIN;
    [Header("Commands To Send")]
    public List<PTK_TriggerCommandsBehaviour> commandBehavioursToRun = new List<PTK_TriggerCommandsBehaviour>();

    bool bAlreadyCommandSent = false;

    private void Start()
    {
        recivedTriggerSignalsPreview = new List<CRecivedTriggerWithData>();

        for (int i=0;i< receiveDataFromTriggersOnEnterExitEvent.Count;i++)
        {
            if (receiveDataFromTriggersOnEnterExitEvent[i] == null)
                continue;

            var receivedInfo = new CRecivedTriggerWithData();
            receivedInfo.trigger = receiveDataFromTriggersOnEnterExitEvent[i];

            recivedTriggerSignalsPreview.Add(receivedInfo);
        }


        for (int i=0;i< recivedTriggerSignalsPreview.Count;i++)
        {
            if (recivedTriggerSignalsPreview[i].trigger == null)
                continue;

            int currentIndexLambda = i;
            recivedTriggerSignalsPreview[currentIndexLambda].trigger.OnTriggerEventNetSynced += (triggerData) =>
           {
               // we will allow to run logic only if the signal receiver is not disabled
               if(this.gameObject.activeInHierarchy == true)
               {
                   recivedTriggerSignalsPreview[currentIndexLambda].bSignalReceived = true;
                   recivedTriggerSignalsPreview[currentIndexLambda].triggerEventData = triggerData;

                   TriggerSignalReceived();
               }
           };

            recivedTriggerSignalsPreview[currentIndexLambda].trigger.OnRaceResettedEvent += OnRaceResetted;
            recivedTriggerSignalsPreview[currentIndexLambda].trigger.OnRaceTimerSyncedJustStarted += OnRaceTimerJustStarted;
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
        ClearReceivedSignalsInfo();

        for (int iBehaviour = 0; iBehaviour < commandBehavioursToRun.Count; iBehaviour++)
        {
            if (commandBehavioursToRun[iBehaviour] != null)
            {
                commandBehavioursToRun[iBehaviour].RaceResetted();
            }
        }
    }

    private void TriggerSignalReceived()
    {
        if (recivedTriggerSignalsPreview.Count == 0)
            return;

        bool bReceivedSignalsFromAllTriggers = true;
        bool bReceivedSignalFromAtLeasOne = false;

        CRecivedTriggerWithData firstReceivedTrigger = null;

        for (int i = 0; i < recivedTriggerSignalsPreview.Count; i++)
        {
            if (recivedTriggerSignalsPreview[i].trigger == null)
                continue;

            bReceivedSignalsFromAllTriggers &= recivedTriggerSignalsPreview[i].bSignalReceived;
            bReceivedSignalFromAtLeasOne |= recivedTriggerSignalsPreview[i].bSignalReceived;

            if(firstReceivedTrigger == null && recivedTriggerSignalsPreview[i].bSignalReceived == true)
            {
                firstReceivedTrigger = recivedTriggerSignalsPreview[i];
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
                ClearReceivedSignalsInfo();
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
                    commandBehavioursToRun[iBehaviour].Execute(recivedTriggerSignalsPreview);
                }

                if (eRunCommandCondition == ERunCommandsCondition.E1_RUN_COMMANDS_IF_RECIVED_TRIGGER_DATA_FROM_AT_LEAST_ONE )
                {
                    commandBehavioursToRun[iBehaviour].Execute(firstReceivedTriggerSignal);
                }
            }
        }
    }

     void ClearReceivedSignalsInfo()
    {
        for (int i = 0; i < recivedTriggerSignalsPreview.Count; i++)
        {
            recivedTriggerSignalsPreview[i].bSignalReceived = false;
            recivedTriggerSignalsPreview[i].triggerEventData = null;
        }

        bAlreadyCommandSent = false;
    }

    public void ManualResetAllowSendingAgain()
    {
        ClearReceivedSignalsInfo();
    }
}

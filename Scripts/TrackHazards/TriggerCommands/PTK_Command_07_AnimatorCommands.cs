using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PTK_Command_07_AnimatorCommands : PTK_TriggerCommandBase
{
    protected override ETriggerCommandType GetCommandType()
    {
        return ETriggerCommandType.E07_ANIMATOR_COMMANDS;
    }
    [System.Serializable]
    public class CAnimatorCalls
    {
        public Animator animatorTypeObject;

        public List<CTrigger> triggers = new List<CTrigger>();
        public List<CBool> booleans = new List<CBool>();
        public List<CFloat> floats = new List<CFloat>();
        public List<CInt> integers = new List<CInt>();

        [System.Serializable]
        public class CTrigger
        {
            public string strTriggerName = "";
        }

        [System.Serializable]
        public class CBool
        {
            public string strBoolName = "";
            public bool bValue = false;
        }

        [System.Serializable]
        public class CFloat
        {
            public string strFloatName = "";
            public float fValue;
            [Header("Lerp duration (0 = none)")]
            public float fAchiveValueInTime = 0.2f;

            [HideInInspector]
            public Animator parentAnimator;
            [HideInInspector]
            public PTK_Command_07_AnimatorCommands parentUpdateCommand;
        }

        [System.Serializable]
        public class CInt
        {
            public string strIntName = "";
            public int iValue;
        }
    }


    public CAnimatorCalls[] animatorCallsToSend;

    public static List<CAnimatorCalls.CFloat> currentAnimatorsFloatValsInterp = new List<CAnimatorCalls.CFloat>();

    public override void Awake()
    {
       
    }
    public override void Start()
    {
    }

    public override void OnDestroy()
    {
    }

    public  void Update()
    {
        for(int i=0;i< currentAnimatorsFloatValsInterp.Count;i++)
        {
            if(currentAnimatorsFloatValsInterp[i].parentUpdateCommand == this)
            {
                currentAnimatorsFloatValsInterp[i].parentAnimator.SetFloat(currentAnimatorsFloatValsInterp[i].strFloatName, currentAnimatorsFloatValsInterp[i].fValue, currentAnimatorsFloatValsInterp[i].fAchiveValueInTime, Time.deltaTime);
            }
        }
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
        foreach (var objAnimLogic in animatorCallsToSend)
        {
            if (objAnimLogic == null)
                continue;

            if (objAnimLogic.animatorTypeObject == null)
                continue;

            for(int i=0;i<objAnimLogic.triggers.Count;i++)
            {
                objAnimLogic.animatorTypeObject.SetTrigger(objAnimLogic.triggers[i].strTriggerName);
            }

            for (int i = 0; i < objAnimLogic.booleans.Count; i++)
            {
                objAnimLogic.animatorTypeObject.SetBool(objAnimLogic.booleans[i].strBoolName, objAnimLogic.booleans[i].bValue);
            }

            for (int i = 0; i < objAnimLogic.floats.Count; i++)
            {
                // we need to ensure no other command is interpolating this float value
                objAnimLogic.floats[i].parentAnimator = objAnimLogic.animatorTypeObject;
                objAnimLogic.floats[i].parentUpdateCommand = this;
                
                InterpFloatAndEnsureOnlyOneFloatInterpVal(objAnimLogic.floats[i]);
            }

            for (int i = 0; i < objAnimLogic.integers.Count; i++)
            {
                objAnimLogic.animatorTypeObject.SetInteger(objAnimLogic.integers[i].strIntName, objAnimLogic.integers[i].iValue);
            }
        }

    }

  
    void InterpFloatAndEnsureOnlyOneFloatInterpVal(CAnimatorCalls.CFloat thisCommandFloatVal)
    {
        List<CAnimatorCalls.CFloat> itemsToRemove = new List<CAnimatorCalls.CFloat>();
        for (int iInterpVal = 0; iInterpVal < currentAnimatorsFloatValsInterp.Count; iInterpVal++)
        {
            if (currentAnimatorsFloatValsInterp[iInterpVal] == null || currentAnimatorsFloatValsInterp[iInterpVal].parentAnimator == null)
            {
                itemsToRemove.Add(currentAnimatorsFloatValsInterp[iInterpVal]);
                continue;
            }

            if (currentAnimatorsFloatValsInterp[iInterpVal] == thisCommandFloatVal)
            {
                // we already interpolating toward out value , removing to add again
                itemsToRemove.Add(currentAnimatorsFloatValsInterp[iInterpVal]);
            }; 

            if (currentAnimatorsFloatValsInterp[iInterpVal].parentAnimator == thisCommandFloatVal.parentAnimator && currentAnimatorsFloatValsInterp[iInterpVal].strFloatName == thisCommandFloatVal.strFloatName)
            {
                // some other command was already interpolating value - we are stooping it and using ours
                itemsToRemove.Add(currentAnimatorsFloatValsInterp[iInterpVal]);
                continue;
            }
        }

        for(int i=0;i< itemsToRemove.Count;i++)
        {
            currentAnimatorsFloatValsInterp.Remove(itemsToRemove[i]);
        }

        currentAnimatorsFloatValsInterp.Add(thisCommandFloatVal);
    }

    protected override void RaceResetted_RevertToDefault()
    {
    }


    protected override void OnRaceTimerJustStarted_SyncAndRunAnimsImpl()
    {
    }
}

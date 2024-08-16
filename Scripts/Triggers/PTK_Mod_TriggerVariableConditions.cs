using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PTK_Mod_TriggerVariableConditions
{
    [System.Serializable]
    public class CCondition
    {
        public enum EConditionType
        {
            E_00_CAMERA_DIST_FROM_TRIGGER,

            __COUNT
        }

        public enum ECompareType
        {
            E_LESS_THAN,         // <
            E_LESS_THAN_OR_EQUAL,// <=
            E_EQUAL,             // ==
            E_NOT_EQUAL,         // !=
            E_GREATER_THAN,      // >
            E_GREATER_THAN_OR_EQUAL // >=
        }


        [Header("Type")]
        public EConditionType eConditionType = EConditionType.__COUNT;
        [Header("Compare")]
        public ECompareType eCompareType = ECompareType.E_EQUAL;
        [Header("0 = FALSE/ 1 = TRUE / Number")]
        public float fCompareValue = 0.0f;

        [Header("Param")]
        public bool bDisableCondition = false;

    }

    public string strConditionName = "";
    public bool bIgnoreConditions = false;
    [Header("ALL below conditions need to pass for trigger event")]
    public List<CCondition> conditionsToCheck = new List<CCondition>();

    public void InitializeAndAttachToEvents()
    {

    }

    public void CheckConditions()
    {
        if (bIgnoreConditions == true)
            return;

        for(int i=0;i< conditionsToCheck.Count;i++)
        {
            bool bApproved = CheckConditionType(conditionsToCheck[i]);
        }
    }

    private bool CheckConditionType(CCondition condition)
    {
        if (condition.bDisableCondition == true)
            return false;

        switch(condition.eConditionType)
        {
            case CCondition.EConditionType.E_00_CAMERA_DIST_FROM_TRIGGER:
                break;
        }

        return false;
    }
}

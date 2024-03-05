using System.Collections.Generic;
using UnityEngine;

/* Usage:
 public class CStickerIntBitVariable : PixelSDK_IntVariablePacker
{
    // cant exceed 32 bits
    public CVariable repeat_BitVar_0_1 = new CVariable(CVariable.EType.E_BOOL_1_BIT, 1);
    public CVariable siz_BitVar_0_15 = new CVariable(CVariable.EType.E_4_BIT_16_CHOICES, 7);

    public CStickerIntBitVariable()
    {
        AddVariable(repeat_BitVar_0_1);
        AddVariable(siz_BitVar_0_15);
    }
}
*/

public class PixelSDK_IntVariablePacker
{
    private List<CVariable> variables = new List<CVariable>();
    private int currentBitOffset = 0;

    public void AddVariable(CVariable variable)
    {
        if (currentBitOffset + (int)variable.Type > 32)
        {
            Debug.LogError("Adding this variable exceeds the 32-bit limit.");
            return;
        }

        variable.BitOffset = currentBitOffset;
        currentBitOffset += (int)variable.Type;
        variables.Add(variable);
    }

    public int PackVariables()
    {
        int packedValue = 0;
        foreach (var variable in variables)
        {
            packedValue |= (variable.Value << variable.BitOffset);
        }
        return packedValue;
    }

    public void UnpackVariables(int packedValue)
    {
        if (packedValue == -1)
        {
            // -1 is as default
            SetDefaultValues();
            return;
        }

        foreach (var variable in variables)
        {
            int mask = (1 << (int)variable.Type) - 1;
            variable.SetValue((packedValue >> variable.BitOffset) & mask);
        }
    }

    public void SetDefaultValues()
    {
        foreach (var variable in variables)
        {
            variable.SetDefaultOriginalVal();
        }
    }
}

public class CVariable
{
    public enum EType
    {
        E_BOOL_1_BIT = 1,
        E_2_BIT_4_CHOICES = 2,
        E_3_BIT_8_CHOICES = 3,
        E_4_BIT_16_CHOICES = 4,
        E_5_BIT_32_CHOICES = 5
    }

    public EType Type { get; private set; }
    public int Value { get; private set; }
    public int BitOffset { get; set; } // The starting bit position of this variable in the packed int

    public int bitCountVariableTaking
    {
        get
        {
            switch (Type)
            {
                case EType.E_BOOL_1_BIT:
                    return 1;
                case EType.E_2_BIT_4_CHOICES:
                    return 2;
                case EType.E_3_BIT_8_CHOICES:
                    return 3;
                case EType.E_4_BIT_16_CHOICES:
                    return 4;
                case EType.E_5_BIT_32_CHOICES:
                    return 5;
                default:
                    Debug.LogError("Unknow bit count!");
                    break;
            }

            return 6;
        }
    }

    int iOriginalValue = 0;
    public CVariable(EType type, int value, int bitOffset = 0)
    {
        iOriginalValue = value;
        Type = type;
        BitOffset = bitOffset;
        SetValue(value);
    }

    public void SetDefaultOriginalVal()
    {
        SetValue(iOriginalValue);
    }

    public void SetValue(int value)
    {
        int maxValue = GetMaxValue();
        if (value < 0 || value > maxValue)
        {
            Debug.LogError( $"Value must be in the range 0-{maxValue} for type {Type}.");
            return;
        }

        Value = value;
    }
    public int GetMaxValue()
    {
        int maxValue = (1 << (int)Type) - 1;
        return maxValue;
    }
}
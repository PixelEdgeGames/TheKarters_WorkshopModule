using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RenamePhrase", menuName = "PixelTools/SO/AnimFileName_ToGameAnimName", order = 0)]
public class StringToStringCombination : ScriptableObject
{
    [System.Serializable]
    public class PhraseMapping
    {
        public string animName;
        public string gameAnimName;
    }

    public List<PhraseMapping> mappings = new List<PhraseMapping>();
}
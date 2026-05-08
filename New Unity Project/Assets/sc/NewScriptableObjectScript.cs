using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData")]
public class DialogData : ScriptableObject
{


    public LanguageData[] LanguageData;

}

[Serializable]
public class LanguageData
{
    public string language;
    public DialogLineData[] dialog;
}

[Serializable]
public class DialogLineData
{
    public Sprite sprite;
    public string speaker;
    public string dialog;
}
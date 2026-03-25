using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data_Character
{
    public string displayName;
    public string characterId;
    public List<string> voiceIds = new List<string>();
    public string voiceName;
    [TextArea(2, 5)]
    public string notes;
    public int priority = 99;
    public bool isPlayer;
}

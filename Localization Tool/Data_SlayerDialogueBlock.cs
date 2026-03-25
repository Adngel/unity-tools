using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class Data_SlayerDialogueBlock
{
    public string blockId;

    [SerializeReference] public FileData_DialogueLine ryan;   // 7H6N38NbuN
    [SerializeReference] public FileData_DialogueLine amy;    // HXRlo050AG
    [SerializeReference] public FileData_DialogueLine bruno;  // kRt43LoLbQ
    [SerializeReference] public FileData_DialogueLine carla;  // 2do0XXATEI
    [SerializeReference] public FileData_DialogueLine dani;   // w29Vbo6JDQ
    [SerializeReference] public FileData_DialogueLine jacob;  // oBGVRxQnka

    public Data_SlayerDialogueBlock(string id)
    {
        blockId = id;
    }

    // --- MÉTODOS DE APOYO ---

    private List<FileData_DialogueLine> GetAllSlayersList()
    {
        return new List<FileData_DialogueLine> { ryan, amy, bruno, carla, dani, jacob };
    }

    public int GetCount()
    {
        int count = 0;
        foreach (var slayer in GetAllSlayersList())
        {
            if (slayer != null) count++;
        }
        return count;
    }

    public bool IsComplete()
    {
        return GetCount() == 6;
    }

    // --- LÓGICA DE PROCESAMIENTO ---

    public bool AssignLine(FileData_DialogueLine line)
    {
        switch (line.CharacterObjectId)
        {
            case "7H6N38NbuN": 
                if (ryan != null) 
                    return false; 
                ryan = line; 
                break;
            case "HXRlo050AG": 
                if (amy != null) 
                    return false; 
                amy = line; 
                break;
            case "kRt43LoLbQ": 
                if (bruno != null) 
                    return false; 
                bruno = line; 
                break;
            case "2do0XXATEI": 
                if (carla != null) 
                    return false; 
                carla = line; 
                break;
            case "w29Vbo6JDQ": 
                if (dani != null) 
                    return false; 
                dani = line; 
                break;
            case "oBGVRxQnka": 
                if (jacob != null) 
                    return false; 
                jacob = line; 
                break;
            default: return false;
        }
        return true;
    }

    public void WriteToOutput(List<string> output, Character_Database charDb)
    {
        // Crear una lista de slayers ordenada.
        List<FileData_DialogueLine> activeLines = new List<FileData_DialogueLine>();
        foreach (var slayer in GetAllSlayersList())
        {
            if (slayer != null) 
                activeLines.Add(slayer);
        }

        if (activeLines.Count == 0) 
            return;

        activeLines = activeLines.OrderBy(l => 
        {
            var profile = charDb.GetCharacter(l.CharacterObjectId);
            return profile != null ? profile.priority : 99;
        }).ToList();

        // Comprobar si falta alguno.
        if (!IsComplete())
        {
            output.Add($"!!! [DEBUG: Bloque incompleto ({activeLines.Count}/6 Slayers)] !!!");
        }

        // Iterar e imprimir.
        for (int i = 0; i < activeLines.Count; i++)
        {
            var line = activeLines[i];
            var character = charDb.GetCharacter(line.CharacterObjectId);

            if (character == null && !string.IsNullOrEmpty(line.VoiceObjectId))
            {
                character = charDb.GetCharacter(line.VoiceObjectId);
            }

            string name = (character != null) ? character.displayName.ToUpper() : "UNKNOWN";
            string indent = (i == 0) ? "" : "    ";

            output.Add($"{indent}[{name}]: {line.GetFullText()} ");
        }
        output.Add($"");
    }
}

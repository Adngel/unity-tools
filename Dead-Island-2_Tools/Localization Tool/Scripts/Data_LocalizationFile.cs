using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class Data_LocalizationFile
{
    public Dictionary<string, FileData_Sequence> Sequences = new Dictionary<string, FileData_Sequence>();
    public Dictionary<string, FileData_Character> Characters = new Dictionary<string, FileData_Character>();
    public Dictionary<string, FileData_Voice> Voices = new Dictionary<string, FileData_Voice>();
    public List<FileData_DialogueLine> DialogueLines = new List<FileData_DialogueLine>();

    public Data_LocalizationFile(XmlDocument xmlDoc)
    {
        ParseCharacters(xmlDoc);
        ParseVoices(xmlDoc);
        ParseSequences(xmlDoc);
        ParseDialogueLines(xmlDoc);
    }

    private void ParseCharacters(XmlDocument doc)
    {
        foreach (XmlNode n in doc.SelectNodes("//Characters/Character"))
        {
            var item = new FileData_Character
            {
                ObjectId = n.Attributes["ObjectId"]?.Value,
                Name = n.Attributes["Name"]?.Value,
                IsPlayer = n.Attributes["IsPlayer"]?.Value == "1"
            };
            Characters[item.ObjectId] = item;
        }
    }

    private void ParseVoices(XmlDocument doc)
    {
        foreach (XmlNode n in doc.SelectNodes("//Voices/Voice"))
        {
            var item = new FileData_Voice
            {
                ObjectId = n.Attributes["ObjectId"]?.Value,
                Name = n.Attributes["Name"]?.Value,
                DisplayName = n.Attributes["DisplayName"]?.Value,
                CharacterObjectId = n.Attributes["CharacterObjectId"]?.Value
            };
            Voices[item.ObjectId] = item;
        }
    }

    private void ParseSequences(XmlDocument doc)
    {
        foreach (XmlNode n in doc.SelectNodes("//Sequences/Sequence"))
        {
            var item = new FileData_Sequence
            {
                ObjectId = n.Attributes["ObjectId"]?.Value,
                BuildType = n.Attributes["BuildType"]?.Value,
                Type = n.Attributes["Type"]?.Value,
                SequenceOrder = n.Attributes["SequenceOrder"]?.Value,
                Name = n.Attributes["Name"]?.Value,
                PhysicalFileName = n.Attributes["PhysicalFilename"]?.Value
            };
            Sequences[item.ObjectId] = item;
        }
    }

    private void ParseDialogueLines(XmlDocument doc)
    {
        // Buscamos ambos tipos de nodos
        XmlNodeList nodes = doc.SelectNodes("//DialogueLine | //DialogueLineVariation");
        int indexCounter = 0;

        foreach (XmlNode n in nodes)
        {
            var line = new FileData_DialogueLine
            {
                OriginalIndex = indexCounter++,

                CharacterObjectId = n.Attributes["CharacterObjectId"]?.Value,
                VoiceObjectId = n.Attributes["VoiceObjectId"]?.Value,
                SequenceObjectId = n.Attributes["SequenceObjectId"]?.Value,

                ActorLine = n.Attributes["ActorLine"]?.Value,

                Path = n.Attributes["Path"]?.Value,
                LogicalPath = n.Attributes["LogicalPath"]?.Value,

                Event = n.Attributes["Event"]?.Value,
            };

            line.IsSlayer = DetermineIfSlayer(line);

            // Extraer Folder y FileName del Path
            if (!string.IsNullOrEmpty(line.Path))
            {
                string[] parts = line.Path.Split('/');
                if (parts.Length > 0)
                {
                    string fileName = parts[parts.Length - 1];
                    line.VoiceFileName = fileName;

                    //Extracción del Line order
                    string[] nameParts = fileName.Split('_');
                    if (nameParts.Length >= 2)
                    {
                        string orderStr = nameParts[nameParts.Length - 2];
                        int.TryParse(orderStr, out line.LineOrder);
                    }
                }
                if (parts.Length > 1) 
                    line.VoiceFolderName = parts[parts.Length - 2];
            }

            // Procesar Chunks
            XmlNodeList chunks = n.SelectNodes("Chunk");
            foreach (XmlNode c in chunks)
            {
                if (c.Attributes["Female"]?.Value == "1")
                    continue;

                line.Chunks.Add(new FileData_DialogueLine.FileData_Chunk
                {
                    Time = float.TryParse(c.Attributes["Time"]?.Value, out float t) ? t : 0f,
                    Text = c.Attributes["Text"]?.Value,
                    CharacterId = c.Attributes["Character"]?.Value
                });
            }

            DialogueLines.Add(line);
        }
    }

    private bool DetermineIfSlayer (FileData_DialogueLine line)
    {
        string charId = line.CharacterObjectId;


        if (string.IsNullOrEmpty(charId) && !string.IsNullOrEmpty(line.VoiceObjectId))
        {
            if (Voices.TryGetValue(line.VoiceObjectId, out var voice))
            {
                charId = voice.CharacterObjectId;
                line.CharacterObjectId = charId;
            }
        }

        if (!string.IsNullOrEmpty(charId) && Characters.TryGetValue(charId, out var character))
        {
            return character.IsPlayer;
        }

        return false;
    }
}

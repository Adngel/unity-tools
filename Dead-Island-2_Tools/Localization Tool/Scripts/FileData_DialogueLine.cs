using System;
using System.Collections.Generic;

[System.Serializable]
public class FileData_DialogueLine
{
    // Ejemplo 1
    // <DialogueLineVariation Event="DX_3p_Talk_Close_PLAY" ExternalSource="External_Source" Path="Main/Voices/KenW/SerlingHub/AvaKen_AndreaSDComp_CuteRobot_01_001" HasFaceFX="1" Critical="0" VoiceObjectId="V9K627ziIo" DialogueType="normal" Volume="0" HasLargeSubtitle="1">
    // <Chunk Time = "0" Text="Así que querías verme, ¿eh? Qué monada."/>
    // <Chunk Time = "3.51" Text="A ver, quizá me muera de hambre, pero al menos moriré sabiendo que recuperé a mi Ava."/>
    // </DialogueLineVariation>
    //
    // Ejemplo 2
    // <DialogueLine Event = "DX_1p_Player_Talk_Cinematic_PLAY" ExternalSource="External_Source" LogicalPath="EXP1/Sequences/Cinematic/028_exp1_the_other_half/DI2_001_ryan_fireman_pained_groans_what_now" Path="EXP1/Sequences/Cinematic/exp1_the_other_half/DI2_001_ryan_fireman_ugh_what_now_the" SequenceObjectId="EQtHpJOC0i" CharacterObjectId="7H6N38NbuN" ActorLine="[quejidos de dolor] ¿Y ahora qué?" HasFaceFX="1" Volume="0" HasParent="0" RefersToPlayer="0" Critical="0"/>

    public struct FileData_Chunk
    {
        public float Time;
        public string Text;
        public string CharacterId;
    }

    public int OriginalIndex;

    public string CharacterObjectId;
    public string VoiceObjectId;
    public string SequenceObjectId;
    public int LineOrder;

    public string ActorLine;
    [System.NonSerialized]
    public List<FileData_Chunk> Chunks;

    public bool IsSlayer;
    [System.NonSerialized]
    public Data_SlayerDialogueBlock SlayerBlock;

    public string Path;
    public string LogicalPath;
    public string VoiceFolderName;
    public string VoiceFileName;

    public string Event;

    public FileData_DialogueLine()
    {
        Chunks = new List<FileData_Chunk>();
    }

    // --- Lógica de Impresión Inteligente ---

    public void WriteLine(List<string> output, Character_Database charDb)
    {
        if (IsSlayerBlock())
        {
            SlayerBlock.WriteToOutput(output, charDb);
        }
        else if (HasMultipleCharactersInChunks ())
        {
            PrintMultiChunks(output, charDb);
        }
        else
        {
            string text = GetFullText();

            if (string.IsNullOrEmpty(text))
                return;

            var character = charDb.GetCharacter(CharacterObjectId);

            if (character == null && !string.IsNullOrEmpty(VoiceObjectId))
            {
                character = charDb.GetCharacter(VoiceObjectId);
            }

            string name = (character != null) ? character.displayName.ToUpper() : "UNKNOWN";

            output.Add($"[{name}]: {text}");
            output.Add("");
        }
    }

    private void PrintMultiChunks(List<string> output, Character_Database charDb)
    {
        if (Chunks == null || Chunks.Count == 0) 
            return;

        string lastCharacterId = "";
        string combinedText = "";

        for (int i = 0; i < Chunks.Count; i++)
        {
            var chunk = Chunks[i];
            if (string.IsNullOrEmpty(chunk.Text)) 
                continue;

            if (chunk.CharacterId == lastCharacterId)
            {
                combinedText += " " + chunk.Text;
            }
            else
            {
                if (!string.IsNullOrEmpty(lastCharacterId))
                {
                    WriteCombinedChunk(output, charDb, lastCharacterId, combinedText);
                }

                lastCharacterId = chunk.CharacterId;
                combinedText = chunk.Text;
            }
        }

        if (!string.IsNullOrEmpty(lastCharacterId))
        {
            WriteCombinedChunk(output, charDb, lastCharacterId, combinedText);
        }
    }

    private void WriteCombinedChunk(List<string> output, Character_Database charDb, string charId, string text)
    {
        var character = charDb.GetCharacter(charId);
        string name = (character != null) ? character.displayName.ToUpper() : "UNKNOWN";

        output.Add($"[{name}]: {text.Trim().Replace("  ", " ")}");
        output.Add("");
    }


    // --- Métodos Auxiliares ---

    public bool IsSlayerLine()
    {
        return IsSlayer;
    }

    public bool IsSlayerBlock ()
    {
        return SlayerBlock != null;
    }

    public string GetFullText()
    {
        if (!string.IsNullOrEmpty(ActorLine))
            return ActorLine;

        if (Chunks != null && Chunks.Count > 0)
            return string.Join(" ", Chunks.ConvertAll(c => c.Text));

        return "";
    }

    private bool HasMultipleCharactersInChunks()
    {
        if (Chunks == null || Chunks.Count <= 1) 
            return false;

        foreach (var chunk in Chunks)
        {
            if (!string.IsNullOrEmpty(chunk.CharacterId) && chunk.CharacterId != CharacterObjectId)
            {
                return true;
            }
        }
        return false;
    }

    public string GetSlayerGroupId()
    {
        if (!string.IsNullOrEmpty(VoiceFileName))
            return VoiceFileName;

        if (string.IsNullOrEmpty(Path)) 
            return "";

        string cleanPath = Path;
        string[] suffixes = { "__Amy", "__Bruno", "__Carla", "__Dani", "__Jacob", "__Ryan" };

        foreach (var suffix in suffixes)
        {
            if (cleanPath.EndsWith(suffix))
            {
                cleanPath = cleanPath.Substring(0, cleanPath.Length - suffix.Length);
                break;
            }
        }
        return cleanPath;
    }

}

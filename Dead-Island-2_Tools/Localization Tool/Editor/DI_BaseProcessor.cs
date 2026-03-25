using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public abstract class DI_BaseProcessor
{
    protected abstract string FileSuffix { get; }

    protected List<string> _output;
    protected Data_LocalizationFile _fileData;
    protected Character_Database _charDb;

    public void Execute(string targetFolder, string baseName, Data_LocalizationFile fileData, Character_Database charDb)
    {
        _output = new List<string>();
        _fileData = fileData;
        _charDb = charDb;

        WriteFileHeader(_output, "Documento de prueba Base");

        WriteContent();

        string fullPath = Path.Combine(targetFolder, baseName + FileSuffix);
        File.WriteAllLines(fullPath, _output);
        AssetDatabase.ImportAsset(fullPath);

        _output = null;
    }

    protected abstract void WriteContent();

    protected void WriteFileHeader(List<string> output, string title)
    {
        output.Add("");
        output.Add($"|===== {title.ToUpper()} =====|");
        output.Add("");
        output.Add("");
    }

    protected List<FileData_DialogueLine> CompactSlayerLines(List<FileData_DialogueLine> originalLines)
    {
        List<FileData_DialogueLine> compactedList = new List<FileData_DialogueLine>();
        Dictionary<string, Data_SlayerDialogueBlock> slayerBlocks = new Dictionary<string, Data_SlayerDialogueBlock>();

        foreach (var line in originalLines)
        {
            if (line.IsSlayerLine())
            {
                string groupId = line.GetSlayerGroupId();

                if (!slayerBlocks.ContainsKey(groupId))
                {
                    var newBlock = new Data_SlayerDialogueBlock(groupId);
                    slayerBlocks[groupId] = newBlock;

                    FileData_DialogueLine proxyLine = new FileData_DialogueLine
                    {
                        OriginalIndex = line.OriginalIndex,
                        SlayerBlock = newBlock,
                        Path = groupId,
                        IsSlayer = true,
                        LineOrder = line.LineOrder
                    };
                    compactedList.Add(proxyLine);
                }
                slayerBlocks[groupId].AssignLine(line);
            }
            else
            {
                compactedList.Add(line);
            }
        }
        return compactedList;
    }
}
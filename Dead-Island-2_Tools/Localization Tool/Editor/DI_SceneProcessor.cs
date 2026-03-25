using System.Collections.Generic;
using System.Linq;

public class DI_SceneProcessor : DI_BaseProcessor
{
    protected override string FileSuffix => "_Scenes.txt";

    protected override void WriteContent()
    {
        //------------------------------
        //|--- Recolección de datos ---|
        //------------------------------

        Dictionary<string, List<FileData_DialogueLine>> cinematicGroups = new Dictionary<string, List<FileData_DialogueLine>>();
        Dictionary<string, List<FileData_DialogueLine>> misionGroups = new Dictionary<string, List<FileData_DialogueLine>>();
        Dictionary<string, List<FileData_DialogueLine>> journalGroups = new Dictionary<string, List<FileData_DialogueLine>>();

        foreach (var line in _fileData.DialogueLines)
        {
            if (string.IsNullOrEmpty(line.SequenceObjectId))
                continue;

            FileData_Sequence seqObject;
            if (_fileData.Sequences.TryGetValue(line.SequenceObjectId, out seqObject))
            {
                if (seqObject.Type == "Cinematic")
                {
                    if (!cinematicGroups.ContainsKey(line.SequenceObjectId))
                        cinematicGroups[line.SequenceObjectId] = new List<FileData_DialogueLine>();

                    cinematicGroups[line.SequenceObjectId].Add(line);
                }
                else if (seqObject.Type == "Mission")
                {
                    if (!misionGroups.ContainsKey(line.SequenceObjectId))
                        misionGroups[line.SequenceObjectId] = new List<FileData_DialogueLine>();

                    misionGroups[line.SequenceObjectId].Add(line);
                }
                else if (seqObject.Type == "Journal")
                {
                    if (!journalGroups.ContainsKey(line.SequenceObjectId))
                        journalGroups[line.SequenceObjectId] = new List<FileData_DialogueLine>();

                    journalGroups[line.SequenceObjectId].Add(line);
                }
            }
        }

        //-----------------------------
        //|--- Fase de Ordenación ---|
        //-----------------------------

        List<FileData_Sequence> sortedIds = _fileData.Sequences.Values.ToList();
        sortedIds.Sort();

        CompactGroupsLines(cinematicGroups);
        CompactGroupsLines(misionGroups);
        CompactGroupsLines(journalGroups);

        //-----------------------------
        //|--- Impresión de líneas ---|
        //-----------------------------

        PrintSection("SECCIÓN: ESCENAS NARRATIVAS", sortedIds, cinematicGroups);
        PrintSection("SECCIÓN: EVENTOS E INTERLUDIOS", sortedIds, misionGroups);
        PrintSection("SECCIÓN: DOCUMENTOS Y GRABACIONES", sortedIds, journalGroups);
        
    }

    private void CompactGroupsLines(Dictionary<string, List<FileData_DialogueLine>> groups)
    {
        var sceneIds = groups.Keys.ToList();

        foreach (var id in sceneIds)
        {
            List<FileData_DialogueLine> groupLines = groups[id];
            List<FileData_DialogueLine> compactedLines = CompactSlayerLines(groupLines);
            groups[id] = compactedLines;
        }
    }

    private void PrintSection(string title, List<FileData_Sequence> masterOrder, Dictionary<string, List<FileData_DialogueLine>> groups)
    {
        if (groups.Count == 0) 
            return;

        _output.Add("#################################################");
        _output.Add($"# {title}");
        _output.Add("#################################################");
        _output.Add("");

        foreach (FileData_Sequence seq in masterOrder)
        {
            if (groups.TryGetValue(seq.ObjectId, out List<FileData_DialogueLine> lines))
            {
                WriteSceneHeader(seq);

                foreach (var line in lines)
                {
                    line.WriteLine(_output, _charDb);
                }

                _output.Add("");
            }
        }
    }

    private void WriteSceneHeader (FileData_Sequence seq)
    {
        string headerLine = seq.Name;
        _output.Add($"{headerLine}");
        _output.Add(new string('-', 50));
        _output.Add("");
    }
}
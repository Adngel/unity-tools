using System.Collections.Generic;
using System.Linq;

public class DI_ConversationProcessor : DI_BaseProcessor
{
    protected override string FileSuffix => "_Conversations.txt";

    public enum EConversationZone
    {
        // Hubs (Prioridad 1)
        EmmaHub,
        RoxanneHub,
        FranHub,
        FilmSetHub,
        PattonHub,
        BlueCrabHub,
        VeniceTowerHub,
        SerlingHub,
        PierLifeguardHub,
        NewsHub, 
        TishaHub,
        EschatonHub,

        // Misiones (Prioridad 2)
        MainQuests, 
        SideQuests, 
        MissingPeople, 
        TreasureHunts,

        // Otros
        Skope,
        Unknown
    }

    protected override void WriteContent()
    {
        //------------------------------
        //|--- Recolección de datos ---|
        //------------------------------

        int zoneCount = System.Enum.GetNames(typeof(EConversationZone)).Length;
        List<Dictionary<string, List<FileData_DialogueLine>>> organizedData = new List<Dictionary<string, List<FileData_DialogueLine>>>(zoneCount);

        for (int i = 0; i < zoneCount; i++)
        {
            organizedData.Add(new Dictionary<string, List<FileData_DialogueLine>>());
        }

        foreach (var line in _fileData.DialogueLines)
        {
            if (!string.IsNullOrEmpty(line.SequenceObjectId))
                continue;

            string folderName = line.VoiceFolderName;
            if (string.IsNullOrEmpty(folderName))
                continue;

            int zoneIndex = (int)GetZoneFromFolder(folderName);

            if (zoneIndex == (int)EConversationZone.Unknown)
                continue;


            string conversationId = GetCleanConversationId(line.VoiceFileName);

            var zoneDict = organizedData[zoneIndex];
            
            if (!zoneDict.ContainsKey(conversationId))
                zoneDict[conversationId] = new List<FileData_DialogueLine>();

            zoneDict[conversationId].Add(line);
        }

        //-----------------------------
        //|--- Fase de Ordenación ---|
        //-----------------------------

        for (int i = 0; i < organizedData.Count; i++)
        {
            var zoneDict = organizedData[i];
            if (zoneDict.Count == 0) 
                continue;

            var conversationIds = zoneDict.Keys.ToList();
            foreach (var convId in conversationIds)
            {
                List<FileData_DialogueLine> rawLines = zoneDict[convId];

                var orderedLines = rawLines
                    .OrderBy(l => l.LineOrder)
                    .ThenBy(l => l.OriginalIndex)
                    .ToList();

                List<FileData_DialogueLine> processedLines = CompactSlayerLines(orderedLines);
                zoneDict[convId] = processedLines;
            }
        }

        //-----------------------------
        //|--- Impresión de líneas ---|
        //-----------------------------

        PrintTableOfContents(organizedData);

        for (int i = 0; i < organizedData.Count; i++)
        {
            var zoneDict = organizedData[i];
            if (zoneDict.Count == 0) continue;

            EConversationZone zoneType = (EConversationZone)i;
            WriteZoneHeader(zoneType);

            var sortedConversationIds = zoneDict.Keys.OrderBy(k => k).ToList();

            foreach (var convId in sortedConversationIds)
            {
                List<FileData_DialogueLine> speechLines = zoneDict[convId];

                WriteConversationHeader(convId);

                foreach (var line in speechLines)
                {
                    line.WriteLine(_output, _charDb);
                }
            }
            _output.Add("");
        }

    }

    private EConversationZone GetZoneFromFolder(string folder)
    {
        string f = folder.ToUpper();

        // --- PRIORIDAD 1: HUBS ESPECÍFICOS ---
        // Buscamos primero los nombres propios de los Hubs
        if (f.Contains("BLUECRABHUB")) return EConversationZone.BlueCrabHub;
        if (f.Contains("EMMAHUB")) return EConversationZone.EmmaHub;
        if (f.Contains("SERLINGHUB")) return EConversationZone.SerlingHub;
        if (f.Contains("ROXANNEHUB")) return EConversationZone.RoxanneHub;
        if (f.Contains("FRANHUB")) return EConversationZone.FranHub;
        if (f.Contains("FILMSETHUB")) return EConversationZone.FilmSetHub;
        if (f.Contains("PATTONHUB")) return EConversationZone.PattonHub;
        if (f.Contains("NEWSHUB")) return EConversationZone.NewsHub;
        if (f.Contains("TISHAHUB")) return EConversationZone.TishaHub;
        if (f.Contains("VENICETOWERHUB")) return EConversationZone.VeniceTowerHub;
        if (f.Contains("PIERLIFEGUARDHUB")) return EConversationZone.PierLifeguardHub;
        if (f.Contains("ESCHATONHUB")) return EConversationZone.EschatonHub;


        // --- PRIORIDAD 2: MISIONES POR PREFIJO ---
        // Si no es un Hub, miramos si es una misión
        if (f.Contains("MQ")) return EConversationZone.MainQuests;
        if (f.Contains("SM")) return EConversationZone.MainQuests;
        if (f.Contains("SQ")) return EConversationZone.SideQuests;
        if (f.Contains("MP")) return EConversationZone.MissingPeople;
        //if (f.Contains("TH")) return EConversationZone.TreasureHunts; //TH es muy típica, colisiona con otros nombres.

        // --- PRIORIDAD 3: CASOS ESPECIALES ---
        if (f.Contains("SKOPE")) return EConversationZone.Skope;

        // Si no encaja en nada de lo anterior
        return EConversationZone.Unknown;
    }

    private void WriteConversationHeader(string path)
    {
        _output.Add("--------------------------------------------------");
        _output.Add($"CONVERSATION: {path}");
        _output.Add("--------------------------------------------------");
        _output.Add("");
    }

    private void WriteZoneHeader(EConversationZone zone)
    {
        _output.Add("################################################################################");
        _output.Add($"# SECCIÓN: {zone.ToString().ToUpper()}");
        _output.Add("################################################################################");
        _output.Add("");
    }

    private string GetCleanConversationId(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return "Unknown";

        string[] parts = fileName.Split('_');

        if (parts.Length > 2)
        {
            return string.Join("_", parts.Take(parts.Length - 2));
        }
        return fileName;
    }

    private void PrintTableOfContents(List<Dictionary<string, List<FileData_DialogueLine>>> organizedData)
    {
        _output.Add("================================================================================");
        _output.Add("                          ÍNDICE DE CONVERSACIONES                              ");
        _output.Add("================================================================================");
        _output.Add("");

        for (int i = 0; i < organizedData.Count; i++)
        {
            var zoneDict = organizedData[i];
            if (zoneDict.Count == 0) continue;

            EConversationZone zoneType = (EConversationZone)i;

            _output.Add($"[+] SECCIÓN: {zoneType.ToString().ToUpper()} ({zoneDict.Count} Conversaciones)");

            var sortedKeys = zoneDict.Keys.OrderBy(k => k).ToList();

            foreach (var convId in sortedKeys)
            {
                _output.Add($"    |-- {convId}");
            }

            _output.Add("");
        }

        _output.Add("================================================================================");
        _output.Add("                      FIN DEL ÍNDICE - INICIO DE CONTENIDO                      ");
        _output.Add("================================================================================");
        _output.Add("");
        _output.Add("");
    }
}
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class DI_ExtraProcessor
{
    /*public static void Process(string path, List<DI_LocFileProcessor.Data_DialogueLine> lines)
    {
        if (lines.Count == 0) return;
        var groups = lines.GroupBy(l => l.EventType);
        List<string> output = new List<string> { "=== FRASES SUELTAS Y EFECTOS ===\n" };
        foreach (var group in groups)
        {
            output.Add($"\n[GRUPO: {group.Key}]\n" + new string('-', 40));
            DI_LocFileProcessor.WriteLinesToOutput(output, group);
        }
        File.WriteAllLines(path, output);
        AssetDatabase.ImportAsset(path);
    }*/
}
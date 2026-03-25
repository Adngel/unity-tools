using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public static class DI_DebugProcessor
{
    public static void DumpEverything(string targetFolder, string baseName, Data_LocalizationFile fileData)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=================================================");
        sb.AppendLine($"DEBUG DUMP: {baseName}");
        sb.AppendLine($"Total Lines: {fileData.DialogueLines.Count}");
        sb.AppendLine("=================================================\n");

        for (int i = 0; i < fileData.DialogueLines.Count; i++)
        {
            var l = fileData.DialogueLines[i];

            sb.AppendLine($"[INDEX: {i}]");
            sb.AppendLine($"  IsSlayer: {l.IsSlayer.ToString().ToUpper()}");
            sb.AppendLine($"  LineOrder: {l.LineOrder}");
            sb.AppendLine($"  SlayerGroupID: {l.GetSlayerGroupId()}");

            sb.AppendLine($"  CharID: {l.CharacterObjectId ?? "NULL"}");
            sb.AppendLine($"  VoiceID: {l.VoiceObjectId ?? "NULL"}");
            sb.AppendLine($"  FileName: {l.VoiceFileName ?? "NULL"}");
            sb.AppendLine($"  Path: {l.Path}");

            if (!string.IsNullOrEmpty(l.ActorLine))
                sb.AppendLine($"  Text: {l.ActorLine}");

            // Información del bloque si existe
            if (l.SlayerBlock != null)
            {
                sb.AppendLine($"  >>> SLAYER BLOCK: {l.SlayerBlock.blockId}");
                sb.AppendLine($"      Slayers count: {l.SlayerBlock.GetCount()}/6");
                // Un pequeño detalle de quiénes están dentro
                sb.AppendLine($"      Contenido: {(l.SlayerBlock.amy != null ? "[AMY] " : "")}{(l.SlayerBlock.ryan != null ? "[RYAN] " : "")}{(l.SlayerBlock.dani != null ? "[DANI] " : "")}...");
            }

            sb.AppendLine("-------------------------------------------------");
        }

        string fullPath = Path.Combine(targetFolder, baseName + "_DEBUG.txt");
        File.WriteAllText(fullPath, sb.ToString());
        AssetDatabase.ImportAsset(fullPath);
        UnityEngine.Debug.Log($"<color=cyan>Debug Dump generado: {fullPath}</color>");
    }
}
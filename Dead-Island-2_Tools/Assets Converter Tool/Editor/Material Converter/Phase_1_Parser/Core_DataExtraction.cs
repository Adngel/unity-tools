using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class Core_DataExtraction
{
    public static void ExtractRawData(DefaultAsset targetFolder, Database_ParsedMaterials parsedDb, bool purge)
    {
        if (purge) 
            parsedDb.Clear();

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        List<string> txtFiles = Utils_StringsConverter.GetPathFilesList(folderPath, "*.txt");

        if (txtFiles.Count == 0) 
            return;

        parsedDb.InitializeCache();
        int count = 0;

        try
        {
            for (int i = 0; i < txtFiles.Count; i++)
            {
                if (i % 10 == 0)
                {
                    // Actualizar barra de progreso
                    float progress = (float)i / txtFiles.Count;
                    if (EditorUtility.DisplayCancelableProgressBar("Fase 1: Parsing", $"Extrayendo datos {i}/{txtFiles.Count}...", progress))
                    {
                        Debug.Log("<color=orange>[Core]</color> Operación cancelada por el usuario.");
                        break;
                    }
                }

                string currentFilePath = txtFiles[i];
                if (!File.Exists(currentFilePath)) 
                    continue;

                string content = File.ReadAllText(currentFilePath);
                Data_ParsedMaterial mat = new Data_ParsedMaterial(currentFilePath);

                if (Module_ParserTXT.ParseData(content,mat))
                {
                    parsedDb.Add(mat);
                    count++;
                }

                // Liberador de RAM
                if (i % 500 == 0)
                {
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    System.GC.Collect();
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();

            parsedDb.totalMaterials = parsedDb.Materials.Count;
            parsedDb.lastAnalysisDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            EditorUtility.SetDirty(parsedDb);
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green><b>[Core]</b></color> Parsing completado. " +
                  $"Total en DB: {parsedDb.Materials.Count} (Procesados en esta tanda: {count})");
        }
    }

    public static void ProcessData(Database_ParsedMaterials parsedDb, Database_ProcessedMaterials processedDb, bool purge)
    {
        if (purge) 
            processedDb.Clear();

        processedDb.InitializeCache();

        try
        {
            for (int i = 0; i < parsedDb.Materials.Count; i++)
            {
                float progress = (float)i / parsedDb.Materials.Count;
                var rawData = parsedDb.Materials[i];

                if (EditorUtility.DisplayCancelableProgressBar("Fase 2: Processing", $"Procesando instrucciones para {rawData.Name}...", progress))
                    break;

                var instruction = Module_ConvertData.Convert(rawData);
                processedDb.Add(instruction);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(processedDb);
            AssetDatabase.SaveAssets();
        }
    }
}
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;

public static class DI_LocFileProcessor
{
    // FUNCIÓN PARA EL BOTÓN 1
    public static Data_LocalizationFile LoadOnly(TextAsset sourceFile)
    {
        if (sourceFile == null) 
            return null;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(sourceFile.text);

        Debug.Log("<color=cyan><b>DI2 Tool:</b></color> Archivo leído correctamente.");
        return new Data_LocalizationFile(xmlDoc);
    }

    public static void ExecuteProcessors(TextAsset sourceFile, Data_LocalizationFile data)
    {
        string assetPath = AssetDatabase.GetAssetPath(sourceFile);
        string targetFolder = PrepareTargetFolder(assetPath);
        string baseName = Path.GetFileNameWithoutExtension(assetPath);
        Character_Database charDb = DI_CharacterManager.GetCharDatabase();

        // Procesador de Escenas
        var sceneProc = new DI_SceneProcessor();
        sceneProc.Execute(targetFolder, baseName, data, charDb);

        // Aquí añadiremos el de Conversaciones luego
        var convProc = new DI_ConversationProcessor();
        convProc.Execute(targetFolder, baseName, data, charDb);

        DI_DebugProcessor.DumpEverything(targetFolder, baseName, data);

        AssetDatabase.Refresh();
        Debug.Log("<color=green><b>DI2 Tool:</b></color> Generación de archivos finalizada.");
    }

    private static string PrepareTargetFolder(string assetPath)
    {
        string directory = Path.GetDirectoryName(assetPath);
        string folderName = Path.GetFileNameWithoutExtension(assetPath);
        string fullPath = Path.Combine(directory, folderName).Replace("\\", "/");

        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(directory, folderName);
        }

        return fullPath;
    }
}
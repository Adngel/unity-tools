using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.WSA;

public static class Utils_StringsConverter
{
    #region Unity Strings
    // --- LÓGICA PARA RUTA DE ARCHIVO (MATERIAL) ---
    // Entrada: Assets/.../MI_ARC_AirlinerA_AquilaAirlines_02.props.txt
    
    /// <summary>
    /// Extrae el nombre del material eliminando las extensiones del parser.
    /// Entrada: Assets/Materials/Airliner/MI_Airlines_02.props.txt
    /// Salida: MI_Airlines_02
    /// </summary>
    public static string GetNameFromUnityPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) 
            return string.Empty;

        string fileName = Path.GetFileName(fullPath);
        return fileName.Replace(".props.txt", "").Replace(".txt", "");
    }

    /// <summary>
    /// Obtiene la ruta de la carpeta normalizada para Unity.
    /// Entrada: Assets/Materials/Airliner/MI_Airlines_02.props.txt
    /// Salida: Assets/Materials/Airliner/
    /// </summary>
    public static string GetFolderFromUnityPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) 
            return string.Empty;

        string folder = Path.GetDirectoryName(fullPath);
        folder = folder.Replace("\\", "/");

        if (!folder.EndsWith("/"))
            folder += "/";

        return folder;
    }
    #endregion

    #region Unreal Strings
    // --- LÓGICA PARA FORMATO UNREAL (TEXTURAS) ---
    // Entrada: Texture2D'/Game/.../T_Name.T_Name'

    /// <summary>
    /// Extrae el nombre del asset de una ruta de Unreal.
    /// Entrada: Texture2D'/Game/Path/T_Texture.T_Texture'
    /// Salida: T_Texture
    /// </summary>
    public static string GetNameFromUnrealPath(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue)) 
            return string.Empty;

        int lastDot = rawValue.LastIndexOf('.');
        if (lastDot == -1)
        {
            Debug.LogError($"[Utils_Strings] Asset name not found in: {rawValue}");
            return "Unknown";
        }

        return rawValue.Substring(lastDot + 1).Replace("'", "");
    }

    /// <summary>
    /// Convierte una ruta interna de Unreal a una ruta de carpeta de Unity.
    /// Entrada: Texture2D'/Game/Environment/T_Texture.T_Texture'
    /// Salida: Assets/Environment/
    /// </summary>
    public static string GetFolderFromUnrealPath(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue)) 
            return string.Empty;

        // 1. Extraer el contenido entre las comillas simples
        int firstSlash = rawValue.IndexOf('/');
        int lastSlash = rawValue.LastIndexOf('/');

        if (firstSlash == -1 || lastSlash == -1 || firstSlash >= lastSlash)
            return string.Empty;

        string path = rawValue.Substring(firstSlash, (lastSlash - firstSlash) + 1);

        if (path.StartsWith("/Game/"))
        {
            path = "Assets/" + path.Substring(6);
        }

        if (!path.EndsWith("/"))
            path += "/";

        return path;
    }
    #endregion

    #region Unity File Search

    /// <summary>
    /// Recoge todos los archivos de una extensión específica dentro de una carpeta de Unity,
    /// devolviendo rutas normalizadas (con barras forward /).
    /// </summary>
    public static List<string> GetPathFilesList(string unityFolderPath, string extension = "*.txt")
    {
        if (string.IsNullOrEmpty(unityFolderPath)) 
            return new List<string>();

        List<string> list = Directory.EnumerateFiles(unityFolderPath, extension, SearchOption.AllDirectories).ToList();
        list.Select(x => x.Replace("\\", "/"));

        return list;            
    }

    /// <summary>
    /// Intenta localizar un archivo específico (como una textura _ATX.tga) 
    /// en la misma carpeta que el material o en sus subcarpetas.
    /// </summary>
    public static string FindRelatedFile(string folderPath, string fileNameWithExtension)
    {
        string[] files = Directory.GetFiles(folderPath, fileNameWithExtension, SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            return files[0].Replace("\\", "/");
        }
        return string.Empty;
    }

    #endregion


    /// <summary>
    /// Reemplaza un sufijo específico al final de un nombre de textura.
    /// Ejemplo: T_Demon_D -> T_Demon_DA
    /// </summary>
    public static string ReplaceSuffix(string fileName, string oldSuffix, string newSuffix)
    {
        if (string.IsNullOrEmpty(fileName)) 
            return string.Empty;

        if (fileName.EndsWith(oldSuffix))
        {
            return fileName.Substring(0, fileName.Length - oldSuffix.Length) + newSuffix;
        }

        return fileName + newSuffix;
    }
}

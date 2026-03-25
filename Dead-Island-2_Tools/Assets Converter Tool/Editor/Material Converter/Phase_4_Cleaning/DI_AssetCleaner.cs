using System.IO;
using UnityEngine;
using UnityEditor;

public static class DI_AssetCleaner
{
    // --- LIMPIEZA DE MATERIALES (Tu función actual mejorada) ---
    public static void CleanCorruptMaterials(DefaultAsset targetFolder)
    {
        if (targetFolder == null) 
            return;

        ProcessCleanup(targetFolder, "*.mat", "Materiales");
    }

    // --- NUEVA: LIMPIEZA DE TEXTURAS VIEJAS ---
    public static void CleanOldTextures(DefaultAsset targetFolder)
    {
        if (targetFolder == null) return;

        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        // Obtenemos todos los archivos de imagen comunes
        string[] searchPatterns = { "*.tga", "*.png", "*.jpg", "*.tif" };

        int deletedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var pattern in searchPatterns)
            {
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
                string[] files = Directory.GetFiles(absolutePath, pattern, SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // REGLA DE BORRADO: 
                    // Borramos si termina en _N o _MRO (las "viejas" de Unreal)
                    // Puedes añadir aquí _D si ya procesaste los Albedos
                    if (fileName.EndsWith("_N") || fileName.EndsWith("_MRO"))
                    {
                        string relativePath = ConvertToRelativePath(file);
                        if (AssetDatabase.DeleteAsset(relativePath))
                        {
                            deletedCount++;
                        }
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Debug.Log($"<b>[Cleaner]</b> Limpieza de texturas terminada. <color=red>Archivos eliminados: {deletedCount}</color>");
    }

    // --- UTILS ---
    private static string ConvertToRelativePath(string absolutePath)
    {
        string normalizedFile = absolutePath.Replace('\\', '/');
        string normalizedDataPath = Application.dataPath.Replace('\\', '/');

        if (normalizedFile.Contains(normalizedDataPath))
        {
            return "Assets" + normalizedFile.Replace(normalizedDataPath, "");
        }
        return null;
    }

    private static void ProcessCleanup(DefaultAsset targetFolder, string pattern, string label)
    {
        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
        string[] files = Directory.GetFiles(absolutePath, pattern, SearchOption.AllDirectories);

        int deletedCount = 0;
        int batchSize = 100;

        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (i % batchSize == 0)
                    AssetDatabase.StartAssetEditing();

                string relativePath = ConvertToRelativePath(files[i]);
                if (string.IsNullOrEmpty(relativePath)) 
                    continue;

                EditorUtility.DisplayProgressBar("Limpiando Assets", $"Procesando {label}: {i}/{files.Length}", (float)i / files.Length);

                // Si intentamos cargar y da null, el archivo está corrupto o no es un asset válido de Unity
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                if (asset == null)
                {
                    AssetDatabase.DeleteAsset(relativePath);
                    deletedCount++;
                }

                if (i % batchSize == batchSize - 1 || i == files.Length - 1)
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.UnloadUnusedAssetsImmediate();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        Debug.Log($"<b>[Cleaner]</b> {label} procesados. Borrados: {deletedCount}");
    }
}

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Core_ModelsRemap
{
    public static void RunReassign(DefaultAsset targetFolder)
    {
        if (targetFolder == null) 
            return;

        string rootPath = AssetDatabase.GetAssetPath(targetFolder);
        string[] modelPaths = Directory.GetFiles(rootPath, "*.fbx", SearchOption.AllDirectories)
                                       .Select(p => p.Replace("\\", "/"))
                                       .ToArray();

        int total = modelPaths.Length;
        int current = 0;
        int remappedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            int index = 0;
            foreach (string fullPath in modelPaths)
            {
                current++;
                index++;

                // UI: Barra de progreso clásica
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Fase 3: Reasignando Materiales",
                    $"[{current}/{total}] {Path.GetFileName(fullPath)}",
                    (float)current / total))
                {
                    Debug.LogWarning("[Core_ModelProcessor] Proceso cancelado por el usuario.");
                    break;
                }

                // 2. EJECUCIÓN DEL MÓDULO
                if (Module_MatRemapper.Reassign(fullPath))
                {
                    remappedCount++;
                }

                // 3. MANTENIMIENTO DE MEMORIA Y DISCO
                if (current % 200 == 0)
                {
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.SaveAssets();

                    EditorUtility.UnloadUnusedAssetsImmediate();
                    System.GC.Collect();

                    AssetDatabase.StartAssetEditing();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"<color=green><b>[Core_ModelProcessor]</b></color> Finalizado. {remappedCount} modelos actualizados.");
    }
}

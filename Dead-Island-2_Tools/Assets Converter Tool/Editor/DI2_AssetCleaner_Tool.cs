using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DI2_AssetCleaner_Tool : EditorWindow
{
    [MenuItem("DI2 Tools/Limpieza/Borrar Materiales .props Viejos")]
    public static void CleanOldMaterials()
    {
        // 1. Buscamos todos los materiales en el proyecto
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        int deletedCount = 0;

        // 2. Filtramos los que tengan el sufijo ".props"
        var assetsToDelete = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => path.Contains(".props.mat"))
            .ToList();

        if (assetsToDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("Limpieza", "No se encontraron materiales con el sufijo '.props'.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Confirmar Borrado",
            $"Se han encontrado {assetsToDelete.Count} materiales antiguos (.props.mat).\n\n¿Estás seguro de que quieres borrarlos?",
            "Sí, limpiar", "Cancelar");

        if (confirm)
        {
            foreach (string path in assetsToDelete)
            {
                count++;
                EditorUtility.DisplayProgressBar("Limpiando", $"Borrando: {Path.GetFileName(path)}", (float)count / assetsToDelete.Count);

                if (AssetDatabase.DeleteAsset(path))
                {
                    deletedCount++;
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Debug.Log($"<color=orange><b>[Cleaner]</b></color> Se han borrado {deletedCount} materiales antiguos correctamente.");
        }
    }
}

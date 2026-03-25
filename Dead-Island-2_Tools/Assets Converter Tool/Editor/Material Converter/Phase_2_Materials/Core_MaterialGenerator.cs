using UnityEditor;
using UnityEngine;

public class Core_MaterialGenerator
{
    // --- PUNTOS DE ENTRADA PÚBLICOS ---

    public static void RunPhaseA(Database_ProcessedMaterials db, bool overwrite)
    {
        // Llamamos al motor genérico pasando la referencia a la función privada
        ExecuteBatch(db, "Phase A: Shells", true, (data) => LogicPhaseA(data, overwrite));
    }

    public static void RunPhaseB(Database_ProcessedMaterials db, bool forceRecreation)
    {
        ExecuteBatch(db, "Phase B: Textures", false, (data) => LogicPhaseB(data, forceRecreation));
    }

    public static void RunPhaseC(Database_ProcessedMaterials db, bool overwrite)
    {
        // Fase C: lockAssetEditing en false para evitar problemas de Keywords
        ExecuteBatch(db, "Phase C: Link", true, (data) => LogicPhaseC(data, overwrite));
    }

    // --- LÓGICA DE CADA FASE (Objetos para los parámetros lambdas) ---
    private static bool LogicPhaseA(Data_ProcessedMaterial data, bool overwrite)
    {
        return Module_MaterialShells.CreateMaterialAssets(data, overwrite);
    }

    private static bool LogicPhaseB(Data_ProcessedMaterial data, bool forceRecreation)
    {
        return Module_TextureProcessor.PrepareTextures(data, forceRecreation);
    }

    private static bool LogicPhaseC(Data_ProcessedMaterial data, bool overwrite)
    {
        return Module_MaterialLinker.LinkMaterial(data, overwrite);
    }

    // --- EL MOTOR (BATCH PROCESSOR) ---

    /// <summary>
    /// Motor genérico para ejecutar tareas pesadas sobre la base de datos de materiales.
    /// Gestiona barras de progreso, bloqueos de AssetDatabase y limpieza de RAM.
    /// </summary>
    /// <param name="db">La base de datos a procesar.</param>
    /// <param name="phaseName">Nombre de la fase para mostrar en la UI.</param>
    /// <param name="lockAssetEditing">True para operaciones de disco (2A, 2C). False para operaciones de compilación de Shaders/Keywords (2B).</param>
    /// <param name="action">La lógica inyectada a ejecutar por cada material. Debe devolver true si tuvo éxito.</param>
    private static void ExecuteBatch( Database_ProcessedMaterials db, string phaseName, bool lockAssetEditing, System.Func<Data_ProcessedMaterial, bool> action)
    {
        if (db == null || db.instructions.Count == 0)
            return;

        int successCount = 0;
        int total = db.instructions.Count;

        int progressId = Progress.Start("DI2: Materials Tool", $"{phaseName}...", Progress.Options.Sticky);

        try
        {
            for (int i = 0; i < total; i++)
            {
                // BLOQUEO DE EDICIÓN: Condicional según la fase
                if (lockAssetEditing && i % 250 == 0)
                {
                    if (i > 0) 
                        AssetDatabase.StopAssetEditing();

                    AssetDatabase.StartAssetEditing();
                }


                Data_ProcessedMaterial data = db.instructions[i];
                int current = i + 1;
                float progress = (float)current / total;

                // 1. Gestión de la UI de progreso
                Progress.Report(progressId, progress, $"Building: {current}/{total} : {data.materialName}");

                if (EditorUtility.DisplayCancelableProgressBar(
                        "DI2 Builder",
                        $"[{phaseName}] Processing {current}/{total} : {data.materialName}",
                        progress))
                {
                    Debug.LogWarning($"[Core_MaterialGenerator] Fase '{phaseName}' cancelada por el usuario.");
                    break;
                }

                // 2. Acción Principal (Inyectada desde el exterior)
                // Ejecuta la función lamda

                if (action != null)
                {
                    bool success = action.Invoke(data);

                    if (success)
                        successCount++;
                }

                // 3. Optimización de Memoria RAM
                if (i % 50 == 0)
                {
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    System.GC.Collect();
                    System.Threading.Thread.Sleep(10);
                    if (i % 500 == 0) 
                        AssetDatabase.SaveAssets();
                }
            }
        }
        finally
        {
            // FINALIZAR EDICIÓN
            if (lockAssetEditing)
                AssetDatabase.StopAssetEditing();

            // Limpieza visual
            Progress.Remove(progressId);
            EditorUtility.ClearProgressBar();

            // Guardado final persistente
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"<color=green><b>[{phaseName}]</b></color> Finalizado. Éxitos: {successCount}/{total}.");
    }
}

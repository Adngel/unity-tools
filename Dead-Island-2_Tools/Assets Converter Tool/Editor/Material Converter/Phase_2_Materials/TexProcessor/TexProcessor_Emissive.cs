using UnityEditor;
using UnityEngine;

public class TexProcessor_Emissive : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // 1. EVALUACIÓN DE REEMPLAZO
        if (!EvaluatePreexisting(inst, force))
            return;

        // 2. RECOLECCIÓN DE PATH
        string originalPath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);
        if (string.IsNullOrEmpty(originalPath))
            return;

        // 3. LÓGICA DE BYPASS (No creamos archivo nuevo)
        inst.unityProcessedTextureGUID = inst.unityOriginalTextureGUID;
        inst.textureProcessedName = inst.textureOriginalName;

        // 4. CONFIGURACIÓN DEL IMPORTADOR
        Utils_TexProcessor.ConfigureImporter(originalPath, isNormal: false, isLinear: false);
    }
}
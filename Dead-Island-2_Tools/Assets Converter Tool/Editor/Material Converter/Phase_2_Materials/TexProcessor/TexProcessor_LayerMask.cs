using UnityEditor;
using UnityEngine;

public class TexProcessor_LayerMask : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // 1. EVALUACIÓN
        if (!EvaluatePreexisting(inst, force)) 
            return;

        // 2. PATHS
        string originalPath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);
        if (string.IsNullOrEmpty(originalPath)) 
            return;

        string newTexPath = "";
        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID) && inst.unityProcessedTextureGUID != inst.unityOriginalTextureGUID)
        {
            newTexPath = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
        }

        if (string.IsNullOrEmpty(newTexPath))
        {
            // Forzamos nuestro estándar _LMC (Layer Mask Canvas)
            string cleanName = inst.textureOriginalName.Replace("_AHA", "").Replace("_MRO", "").Replace("_ARA", "");
            inst.textureProcessedName = cleanName + "_LMC";
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // 3. PROCESADO DE EXTRACCIÓN
        Texture2D srcTex = Utils_TexProcessor.LoadTextureRaw(originalPath);
        if (srcTex == null) 
            return;

        Texture2D finalTex = new Texture2D(srcTex.width, srcTex.height, TextureFormat.RGBA32, false);
        Color[] srcPixels = srcTex.GetPixels();
        Color[] res = new Color[srcPixels.Length];

        for (int i = 0; i < srcPixels.Length; i++)
        {
            float maskValue = srcPixels[i].g;

            // Unity LayeredLit: Canal R mezcla Capa 1 y 2.
            // Ponemos G y B a 0 para que no activen Capas 3 o 4 accidentalmente.
            res[i] = new Color(maskValue, 0.0f, 0.0f, 1.0f);
        }

        finalTex.SetPixels(res);
        finalTex.Apply();

        // 4. GUARDADO Y CONFIGURACIÓN
        Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
        AssetDatabase.ImportAsset(newTexPath);
        Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: false, isLinear: true);

        inst.unityProcessedTextureGUID = AssetDatabase.AssetPathToGUID(newTexPath);

        // 5. LIMPIEZA
        Object.DestroyImmediate(srcTex);
        Object.DestroyImmediate(finalTex);
    }
}
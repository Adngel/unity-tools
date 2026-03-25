using UnityEditor;
using UnityEngine;

public class TexProcessor_Normal : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // 1. EVALUACIÓN DE REEMPLAZO
        if (!EvaluatePreexisting(inst, force))
            return;

        // 2. RECOLECCIÓN DE PATHS
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
            inst.textureProcessedName = Utils_StringsConverter.ReplaceSuffix(inst.textureOriginalName, "_N", "_NM");
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // 3. PROCESO DE INVERSIÓN (Y/Green Channel)
        Texture2D normalTex = Utils_TexProcessor.LoadTextureRaw(originalPath);
        if (normalTex == null)
            return;

        Texture2D finalTex = new Texture2D(normalTex.width, normalTex.height, TextureFormat.RGBA32, false);

        Color[] pixels = normalTex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Inversión DirectX -> OpenGL
            pixels[i].g = 1.0f - pixels[i].g;
        }

        finalTex.SetPixels(pixels);
        finalTex.Apply();

        // 4. GUARDADO Y CONFIGURACIÓN
        Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
        AssetDatabase.ImportAsset(newTexPath);
        Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: true, isLinear: true);

        inst.unityProcessedTextureGUID = AssetDatabase.AssetPathToGUID(newTexPath);

        // 5. LIMPIEZA DE MEMORIA
        Object.DestroyImmediate(normalTex);
        Object.DestroyImmediate(finalTex);
    }
}
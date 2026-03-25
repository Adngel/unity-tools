using System.Linq;
using UnityEditor;
using UnityEngine;

public class TexProcessor_Detail : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        if (!EvaluatePreexisting(inst, force)) return;

        // 1. RECOLECCIÓN DE COMPONENTES
        string colorPath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);

        var normalInst = mat.textureInstructions.FirstOrDefault(x =>
            x.textureProperty == TextureProperties.Detail_normal && x.layerIndex == inst.layerIndex);
        string normalPath = (normalInst != null) ? AssetDatabase.GUIDToAssetPath(normalInst.unityOriginalTextureGUID) : null;

        if (string.IsNullOrEmpty(colorPath)) 
            return;

        // 2. DEFINICIÓN DE DESTINO (_DM)
        string newTexPath = "";
        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID) && inst.unityProcessedTextureGUID != inst.unityOriginalTextureGUID)
            newTexPath = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);

        if (string.IsNullOrEmpty(newTexPath))
        {
            inst.textureProcessedName = Utils_StringsConverter.ReplaceSuffix(inst.textureOriginalName, "_DRO", "_DM");
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // 3. CARGA DE TEXTURAS VIRTUALES
        Texture2D colorTex = Utils_TexProcessor.LoadTextureRaw(colorPath);
        Texture2D normTex = (normalPath != null) ? Utils_TexProcessor.LoadTextureRaw(normalPath) : null;

        if (colorTex == null) 
            return;

        // 4. ENSAMBLAJE (Standard Detail Map de Unity)
        // R: Albedo Detail | G: Normal Y | B: Smoothness Detail | A: Normal X
        Vector2Int size = (normTex != null) ? Utils_TexProcessor.GetMaxDimensions(colorTex, normTex) : new Vector2Int(colorTex.width, colorTex.height);

        bool colorScaled = false;
        Texture2D rColor = Utils_TexProcessor.EnsureSize(colorTex, size.x, size.y, out colorScaled);
        
        bool normScaled = false;
        Texture2D rNorm = (normTex != null) ? Utils_TexProcessor.EnsureSize(normTex, size.x, size.y, out normScaled) : null;

        Texture2D finalTex = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
        Color[] cP = rColor.GetPixels();
        Color[] nP = (rNorm != null) ? rNorm.GetPixels() : null;
        Color[] res = new Color[cP.Length];

        for (int i = 0; i < res.Length; i++)
        {
            float albedoDetail = cP[i].r;
            float smoothnessDetail = cP[i].a; // O el canal que use Unreal para el smoothness de detalle

            // En Unity, para las normales de detalle en el Mask:
            // El canal G de la textura final es el Verde de la normal (Y)
            // El canal A de la textura final es el Rojo de la normal (X)
            float nX = (nP != null) ? nP[i].r : 0.5f;
            float nY = (nP != null) ? (1.0f - nP[i].g) : 0.5f; // Inversión Y

            res[i] = new Color(albedoDetail, nY, smoothnessDetail, nX);
        }

        finalTex.SetPixels(res);
        finalTex.Apply();

        // 5. GUARDADO Y REGISTRO
        Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
        AssetDatabase.ImportAsset(newTexPath);
        Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: false, isLinear: true);

        string finalGUID = AssetDatabase.AssetPathToGUID(newTexPath);
        inst.unityProcessedTextureGUID = finalGUID;

        if (normalInst != null)
        {
            normalInst.unityProcessedTextureGUID = finalGUID;
            normalInst.textureProcessedName = inst.textureProcessedName;
        }

        // 6. LIMPIEZA
        if (colorScaled) Object.DestroyImmediate(rColor);
        Object.DestroyImmediate(colorTex);

        if (normScaled && rNorm != null) Object.DestroyImmediate(rNorm);
        if (normTex != null) Object.DestroyImmediate(normTex);

        Object.DestroyImmediate(finalTex);
    }
}
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TexProcessor_Diffuse : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // --- EVALUACIÓN DE REEMPLAZO ---
        if (!EvaluatePreexisting(inst, force))
            return;

        // --- RECOLECCIÓN DE PATHS ---
        string diffusePath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);
        if (string.IsNullOrEmpty(diffusePath))
            return;

        //Búsqueda de _ATX
        Data_TextureInstruction alphaInst = mat.textureInstructions.FirstOrDefault(tex => tex.textureProperty == TextureProperties.AlphaMask);

        if (alphaInst == null)
        {
            //Búsqueda de _ARA
            alphaInst = mat.textureInstructions.FirstOrDefault(tex => tex.textureProperty == TextureProperties.DecalMask);
        }
        string alphaPath = (alphaInst != null) ? AssetDatabase.GUIDToAssetPath(alphaInst.unityOriginalTextureGUID) : null;

        // --- CASO A: NO HAY ALPHA ---
        if (string.IsNullOrEmpty(alphaPath))
        {
            // No creamos archivo nuevo. Pasamos la textura original (_D) como la procesada para su uso posterior.
            inst.unityProcessedTextureGUID = inst.unityOriginalTextureGUID;
            inst.textureProcessedName = inst.textureOriginalName;
            Utils_TexProcessor.ConfigureImporter(diffusePath, isNormal: false, isLinear: false);

            return;
        }

        // --- CASO B: HAY ALPHA (Fusión) ---
        
        string newTexPath = "";
        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID) && inst.unityProcessedTextureGUID != inst.unityOriginalTextureGUID)
        {
            newTexPath = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
        }

        if (string.IsNullOrEmpty(newTexPath))
        {
            inst.textureProcessedName = Utils_StringsConverter.ReplaceSuffix(inst.textureOriginalName, "_D", "_DA");
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // --- PROCESO DE PÍXELES ---
        Texture2D diffuseTex = Utils_TexProcessor.LoadTextureRaw(diffusePath);
        Texture2D alphaTex = Utils_TexProcessor.LoadTextureRaw(alphaPath);

        if (diffuseTex != null && alphaTex != null)
        {
            Vector2Int targetSize = Utils_TexProcessor.GetMaxDimensions(diffuseTex, alphaTex);

            bool diffScaled = false;
            Texture2D rDiff = Utils_TexProcessor.EnsureSize(diffuseTex, targetSize.x, targetSize.y, out diffScaled);

            bool alphaScaled = false;
            Texture2D rAlpha = Utils_TexProcessor.EnsureSize(alphaTex, targetSize.x, targetSize.y, out alphaScaled);

            Texture2D finalTex = new Texture2D(targetSize.x, targetSize.y, TextureFormat.RGBA32, false);

            // Mezcla de canales
            Color[] colorPixel = rDiff.GetPixels();
            Color[] alphaPixel = rAlpha.GetPixels();
            Color[] res = new Color[colorPixel.Length];

            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new Color(colorPixel[i].r, colorPixel[i].g, colorPixel[i].b, alphaPixel[i].r);
            }

            finalTex.SetPixels(res);
            finalTex.Apply();

            // Guardado e Importación
            Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
            AssetDatabase.ImportAsset(newTexPath);
            Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: false, isLinear: false);

            inst.unityProcessedTextureGUID = AssetDatabase.AssetPathToGUID(newTexPath);

            // 5. LIMPIEZA DE TEXTURAS VIRTUALES
            Object.DestroyImmediate(diffuseTex);
            if (diffScaled) Object.DestroyImmediate(rDiff);

            Object.DestroyImmediate(alphaTex);
            if (alphaScaled) Object.DestroyImmediate(rAlpha);

            Object.DestroyImmediate(finalTex);

        }
    }
}

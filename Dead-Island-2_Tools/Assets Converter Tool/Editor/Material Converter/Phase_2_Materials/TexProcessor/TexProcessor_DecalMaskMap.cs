using UnityEditor;
using UnityEngine;

public class TexProcessor_DecalMaskMap : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // 1. EVALUACIÓN
        if (!EvaluatePreexisting(inst, force)) 
            return;

        // 2. PATHS
        string araPath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);
        if (string.IsNullOrEmpty(araPath)) 
            return;

        string newTexPath = "";
        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID) && inst.unityProcessedTextureGUID != inst.unityOriginalTextureGUID)
        {
            newTexPath = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
        }

        if (string.IsNullOrEmpty(newTexPath))
        {
            // Convertimos _ARA -> _MM (Decal Version)
            inst.textureProcessedName = Utils_StringsConverter.ReplaceSuffix(inst.textureOriginalName, "_ARA", "_MM");
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // 3. CARGA Y TRABAJO DE PÍXELES
        Texture2D araTex = Utils_TexProcessor.LoadTextureRaw(araPath);
        if (araTex == null) return;

        Texture2D finalTex = new Texture2D(araTex.width, araTex.height, TextureFormat.RGBA32, false);
        Color[] araPixels = araTex.GetPixels();
        Color[] res = new Color[araPixels.Length];

        for (int i = 0; i < araPixels.Length; i++)
        {
            // ESTRUCTURA _ARA (Unreal Decal): 
            // R = Alpha/Opacity (normalmente)
            // G = Roughness
            // B = Datos extra (Blanco/AO)

            // MAPEO A MASKMAP UNITY (HDRP Decal):
            // R = Metallic (0.0, nuestras texturas _ARA no tienen información Metallic).
            // G = Ambient Occlusion
            // B = Opacity (En HDRP/Decals blue es opacity).
            // A = Smoothness (1.0 - Roughness)

            float opacity = araPixels[i].r;
            float roughness = araPixels[i].g;
            float ao = araPixels[i].b;

            res[i] = new Color(
                0f,                    // R: Metallic (Lo dejamos a 0 para decals estándar)
                ao,                    // G: AO
                opacity,               // B: Opacity (En HDRP/Decals blue es opacity).
                1.0f - roughness       // A: Smoothness
            );
        }

        finalTex.SetPixels(res);
        finalTex.Apply();

        // 4. GUARDADO Y CONFIGURACIÓN
        Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
        AssetDatabase.ImportAsset(newTexPath);
        Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: false, isLinear: true);

        inst.unityProcessedTextureGUID = AssetDatabase.AssetPathToGUID(newTexPath);

        // 5. LIMPIEZA
        Object.DestroyImmediate(araTex);
        Object.DestroyImmediate(finalTex);
    }
}
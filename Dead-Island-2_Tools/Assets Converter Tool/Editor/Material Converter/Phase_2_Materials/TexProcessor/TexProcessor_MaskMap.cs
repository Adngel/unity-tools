using System.Linq;
using UnityEditor;
using UnityEngine;

public class TexProcessor_MaskMap : TexProcessor_Base
{
    public override void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force)
    {
        // 1. EVALUACIÓN
        if (!EvaluatePreexisting(inst, force)) 
            return;

        // 2. RECOLECCIÓN DE COMPONENTES
        string mroPath = AssetDatabase.GUIDToAssetPath(inst.unityOriginalTextureGUID);
        if (string.IsNullOrEmpty(mroPath)) 
            return;

        // 3. DEFINICIÓN DE DESTINO (_MM)
        string newTexPath = "";
        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID) && inst.unityProcessedTextureGUID != inst.unityOriginalTextureGUID)
        {
            newTexPath = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
        }

        if (string.IsNullOrEmpty(newTexPath))
        {
            inst.textureProcessedName = Utils_StringsConverter.ReplaceSuffix(inst.textureOriginalName, "_MRO", "_MM");
            newTexPath = inst.textureFolderPath + inst.textureProcessedName + ".tga";
        }

        // 4. CARGA Y ENSAMBLAJE
        Texture2D mroTex = Utils_TexProcessor.LoadTextureRaw(mroPath);
        if (mroTex == null) return;

        Texture2D finalTex = new Texture2D(mroTex.width, mroTex.height, TextureFormat.RGBA32, false);
        Color[] mroPixels = mroTex.GetPixels();
        Color[] res = new Color[mroPixels.Length];

        for (int i = 0; i < mroPixels.Length; i++)
        {
            // ESTRUCTURA MRO (Unreal): R=Metallic, G=Roughness, B=Occlusion
            // ESTRUCTURA MASKMAP (Unity): R=Metallic, G=Occlusion, B=DetailMask, A=Smoothness
            float metallic = mroPixels[i].r;
            float roughness = mroPixels[i].g;
            float occlusion = mroPixels[i].b;

            res[i] = new Color(
                metallic,              // R -> Metallic
                occlusion,             // G -> Occlusion
                1.0f,                  // B -> Detail Mask (Por defecto blanco/lleno)
                1.0f - roughness       // A -> Smoothness (Invertimos Roughness)
            );
        }

        finalTex.SetPixels(res);
        finalTex.Apply();

        // 5. GUARDADO Y CONFIGURACIÓN
        Utils_TexProcessor.SaveToDiskRaw(newTexPath, finalTex);
        AssetDatabase.ImportAsset(newTexPath);
        Utils_TexProcessor.ConfigureImporter(newTexPath, isNormal: false, isLinear: true);

        inst.unityProcessedTextureGUID = AssetDatabase.AssetPathToGUID(newTexPath);

        // 6. LIMPIEZA
        Object.DestroyImmediate(mroTex);
        Object.DestroyImmediate(finalTex);
    }
}
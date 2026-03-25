using UnityEditor;
using UnityEngine;

public class MatBuilder_Decal : MatBuilder_Base
{
    public override void Apply(Material mat, Data_ProcessedMaterial data)
    {
        // 1. Configuración base (Shader GUI y Render States específicos de Decals)
        SetupSurfaceOptions(mat, data);
        mat.SetFloat("_DecalBlend", 1.0f);

        // Por defecto, asumimos que no afectan a nada hasta encontrar la textura
        mat.SetFloat("_AffectAlbedo", 0.0f);
        mat.SetFloat("_AffectNormal", 0.0f);
        mat.SetFloat("_AffectAO", 0.0f);
        mat.SetFloat("_AffectMetal", 0.0f);
        mat.SetFloat("_AffectSmoothness", 0.0f);

        // 2. Tinte global del Decal
        mat.SetColor("_BaseColor", data.baseColor);

        // 3. Procesar instrucciones
        foreach (var inst in data.textureInstructions)
        {
            if (string.IsNullOrEmpty(inst.unityProcessedTextureGUID)) 
                continue;

            string path = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null) continue;

            switch (inst.textureProperty)
            {
                case TextureProperties.Diffuse:
                    mat.SetTexture("_BaseColorMap", tex);
                    mat.SetFloat("_AffectAlbedo", 1.0f);
                    mat.EnableKeyword("_MATERIAL_AFFECTS_ALBEDO");
                    break;

                case TextureProperties.Normal:
                    mat.SetTexture("_NormalMap", tex);
                    mat.SetFloat("_NormalRescale", inst.strength);
                    mat.SetFloat("_AffectNormal", 1.0f);
                    mat.EnableKeyword("_MATERIAL_AFFECTS_NORMAL");
                    break;

                case TextureProperties.DecalMask: // El "Packed ARA Map" de Unreal
                    // HDRP Decals usan el Mask Map para AO, Metal y Smoothness
                    mat.SetTexture("_MaskMap", tex);

                    // Activamos las influencias (En DI2 los ARA suelen llevar los 3)
                    mat.SetFloat("_AffectAO", 1.0f);
                    mat.SetFloat("_AffectSmoothness", 1.0f);

                    mat.EnableKeyword("_MATERIAL_AFFECTS_MASKMAP");

                    // Remaps para asegurar que lea los canales correctamente
                    mat.SetFloat("_MetallicRemapMin", 0.0f);
                    mat.SetFloat("_MetallicRemapMax", 1.0f);
                    mat.SetFloat("_SmoothnessRemapMin", 0.0f);
                    mat.SetFloat("_SmoothnessRemapMax", 1.0f);
                    break;

                case TextureProperties.Emissive:
                    mat.SetTexture("_EmissiveColorMap", tex);
                    mat.SetColor("_EmissiveColor", data.emissiveColor * inst.strength);
                    mat.SetFloat("_AffectEmission", 1.0f);
                    mat.EnableKeyword("_MATERIAL_AFFECTS_EMISSION");
                    break;
            }
        }

        // 4. Alpha / Opacity (Fundamental en Decals)
        // El Alpha suele venir en el canal A del Albedo o del MaskMap (ARA)
        mat.SetFloat("_AlphaCutoffEnable", data.isAlphaClip ? 1.0f : 0.0f);
    }
}
using UnityEditor;
using UnityEngine;

public class MatBuilder_Lit : MatBuilder_Base
{
    public override void Apply(Material mat, Data_ProcessedMaterial data)
    {
        SetupSurfaceOptions(mat, data);
        mat.SetColor("_BaseColor", data.baseColor);

        foreach (var inst in data.textureInstructions)
        {
            if (string.IsNullOrEmpty(inst.unityProcessedTextureGUID)) 
                continue;

            string path = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null) 
                continue;

            switch (inst.textureProperty)
            {
                case TextureProperties.Diffuse:
                    mat.SetTexture("_BaseColorMap", tex);
                    break;

                case TextureProperties.Normal:
                    mat.SetTexture("_NormalMap", tex);
                    mat.EnableKeyword("_NORMALMAP");
                    mat.SetFloat("_NormalScale", inst.strength);
                    break;

                case TextureProperties.Mask:
                    mat.SetTexture("_MaskMap", tex);
                    mat.EnableKeyword("_MASKMAP");
                    
                    mat.SetFloat("_Metallic", 1.0f);
                    mat.SetFloat("_Smoothness", 1.0f);

                    mat.SetFloat("_MetallicRemapMin", 0.0f);
                    mat.SetFloat("_MetallicRemapMax", 1.0f);
                    mat.SetFloat("_AORemapMin", 0.0f);
                    mat.SetFloat("_AORemapMax", 1.0f);
                    mat.SetFloat("_SmoothnessRemapMin", 0.0f);
                    mat.SetFloat("_SmoothnessRemapMax", 1.0f);
                    break;

                case TextureProperties.Emissive:
                    mat.SetTexture("_EmissiveColorMap", tex);
                    mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                    mat.EnableKeyword("_EMISSIVE_COLOR");

                    mat.SetColor("_EmissiveColor", data.emissiveColor);
                    mat.SetFloat("_EmissiveIntensity", inst.strength);

                    mat.SetFloat("_UseEmissiveIntensity", 1.0f);
                    mat.SetInt("_EmissiveIntensityUnit", 0);
                    mat.SetFloat("_EmissiveExposureWeight", 0.0f);
                    break;

                case TextureProperties.Heightmap:
                    mat.SetTexture("_HeightMap", tex);
                    
                    mat.SetFloat("_DisplacementMode", 2.0f);
                    mat.EnableKeyword("_PIXEL_DISPLACEMENT");
                    mat.EnableKeyword("_DISPLACEMENT_LOCK_DEVICE_TILING");

                    mat.SetFloat("_HeightPoMAmplitude", inst.strength * 100f);
                    mat.SetFloat("_HeightAmplitude", 1.0f);
                    mat.SetFloat("_HeightCenter", 0.5f);

                    mat.SetFloat("_PPDMinSamples", 8.0f);
                    mat.SetFloat("_PPDMaxSamples", 32.0f);
                    break;

                case TextureProperties.Detail_color:
                    mat.SetTexture("_DetailMap", tex);
                    mat.EnableKeyword("_DETAIL_MAP");

                    mat.SetTextureScale("_DetailMap", inst.tileScale);

                    mat.SetFloat("_DetailAlbedoScale", inst.detailAlbedoStrength);
                    mat.SetFloat("_DetailNormalScale", inst.detailNormalStrength);
                    mat.SetFloat("_DetailSmoothnessScale", inst.detailSmoothnessStrength);
                    break;
            }
        }
    }
}
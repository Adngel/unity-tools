using UnityEditor;
using UnityEngine;

public class MatBuilder_LayeredLit : MatBuilder_Base
{
    public override void Apply(Material mat, Data_ProcessedMaterial data)
    {
        // 1. Configuración de superficie y capas
        SetupSurfaceOptions(mat, data);
        ConfigureLayerCount(mat, data.layersCount);

        // 2. Color base global
        mat.SetColor("_BaseColor0", data.baseColor);
        mat.SetColor("_BaseColor1", data.layer2Color);

        // 3. Procesar todas las instrucciones
        foreach (var inst in data.textureInstructions)
        {
            if (string.IsNullOrEmpty(inst.unityProcessedTextureGUID)) 
                continue;

            string path = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null) continue;

            // HDRP LayeredLit Naming Convention:
            // Layer 0 (Base) -> Sufijo "0" (ej: _BaseColorMap0)
            // Layer 1 -> Sufijo "1" (ej: _BaseColorMap1)
            // Layer 2 -> Sufijo "2" (ej: _BaseColorMap2)
            string suffix = inst.layerIndex.ToString();

            switch (inst.textureProperty)
            {
                case TextureProperties.Diffuse:
                    mat.SetTexture("_BaseColorMap" + suffix, tex);
                    break;

                case TextureProperties.Normal:
                    mat.SetTexture("_NormalMap" + suffix, tex);
                    mat.EnableKeyword(inst.layerIndex == 0 ? "_NORMALMAP" : "_NORMALMAP" + suffix);
                    mat.SetFloat("_NormalScale" + suffix, inst.strength);
                    break;

                case TextureProperties.Mask:
                    mat.SetTexture("_MaskMap" + suffix, tex);
                    mat.EnableKeyword(inst.layerIndex == 0 ? "_MASKMAP" : "_MASKMAP" + suffix);

                    mat.SetFloat("_Metallic" + suffix, 1.0f);
                    mat.SetFloat("_Smoothness" + suffix, 1.0f);
                    SetMaskRemaps(mat, suffix);
                    break;

                case TextureProperties.LayerMask:
                    // La máscara que mezcla las capas (Layer Mask Control)
                    // HDRP la llama _LayerMaskMap
                    mat.SetTexture("_LayerMaskMap", tex);
                    break;

                case TextureProperties.Heightmap:
                    mat.SetTexture("_HeightMap" + suffix, tex);
                    // En LayeredLit, el desplazamiento se configura por capa
                    // pero suele activarse globalmente en el shader.
                    mat.SetFloat("_HeightPoMAmplitude" + suffix, inst.strength * 100f);
                    break;

                case TextureProperties.Detail_color:
                    mat.SetTexture("_DetailMap" + suffix, tex);
                    mat.EnableKeyword(inst.layerIndex == 0 ? "_DETAIL_MAP" : "_DETAIL_MAP" + suffix);
                    mat.SetTextureScale("_DetailMap" + suffix, inst.tileScale);
                    mat.SetFloat("_DetailAlbedoScale" + suffix, inst.detailAlbedoStrength);
                    mat.SetFloat("_DetailNormalScale" + suffix, inst.detailNormalStrength);
                    mat.SetFloat("_DetailSmoothnessScale" + suffix, inst.detailSmoothnessStrength);
                    break;

                case TextureProperties.Emissive:
                    // La emisión suele ser global en HDRP Layered, no por capa.
                    mat.SetTexture("_EmissiveColorMap", tex);
                    mat.SetColor("_EmissiveColor", data.emissiveColor);
                    mat.SetFloat("_EmissiveIntensity", inst.strength);
                    mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                    break;
            }
        }
    }

    private void ConfigureLayerCount(Material mat, int count)
    {
        // HDRP LayeredLit requiere mínimo 2 capas y máximo 4.
        int finalCount = Mathf.Clamp(count, 2, 4);
        mat.SetFloat("_LayerCount", (float)finalCount);

        // Limpieza de Keywords
        mat.DisableKeyword("_LAYEREDLIT_2LAYERS");
        mat.DisableKeyword("_LAYEREDLIT_3LAYERS");
        mat.DisableKeyword("_LAYEREDLIT_4LAYERS");

        // Activación según conteo
        string key = $"_LAYEREDLIT_{finalCount}LAYERS";
        mat.EnableKeyword(key);
    }

    private void SetMaskRemaps(Material mat, string suffix)
    {
        mat.SetFloat("_MetallicRemapMin" + suffix, 0.0f);
        mat.SetFloat("_MetallicRemapMax" + suffix, 1.0f);
        mat.SetFloat("_AORemapMin" + suffix, 0.0f);
        mat.SetFloat("_AORemapMax" + suffix, 1.0f);
        mat.SetFloat("_SmoothnessRemapMin" + suffix, 0.0f);
        mat.SetFloat("_SmoothnessRemapMax" + suffix, 1.0f);
    }
}
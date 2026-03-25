using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.Port;

public static class Module_ConvertData
{
    // --- 1. DICCIONARIO DE REGLAS (Data-Driven) ---
    private static readonly List<(string PName, string TName, string Exclude, TextureProperties Prop)> TextureRules = new()
    {
        // --- Capa de Detalles (Prioritarias) ---
        ("Detail Packed DRO Map",   "_DRO",         null,       TextureProperties.Detail_color),
        ("Detail Normal Map",       "_N",           null,       TextureProperties.Detail_normal),

        // --- Capa General ---
        ("Diffuse Colour Map",      "_D",           null,       TextureProperties.Diffuse),
        ("Normal Map",              "_N",           "Detail",   TextureProperties.Normal),
        ("Packed MRO Map",          "_MRO",         null,       TextureProperties.Mask),
        ("Packed ARA Map",          "_ARA",         null,       TextureProperties.DecalMask),
        ("Emissive Map",            "_E",           null,       TextureProperties.Emissive),
        ("Heightmap",               "_H",           null,       TextureProperties.Heightmap),
        ("Packed Masks Map",        "_AHA",         null,       TextureProperties.LayerMask),
        
        // --- Capa de Transparencia/Alpha ---
        ("Packed Translucency Map", null,           null,       TextureProperties.AlphaMask),
        (null,                      "_ATX",         null,       TextureProperties.AlphaMask),
        ("Opacity Map",             "_A",           null,       TextureProperties.AlphaMask)
    };

    // --- 2. CLASE DE RESULTADO DE ANÁLISIS ---
    private class TextureAnalysis
    {
        public List<Data_TextureInstruction> Instructions = new();
        public bool HasAlpha = false;
        public bool HasLayered = false;
    }

    public static Data_ProcessedMaterial Convert(Data_ParsedMaterial rawData)
    {
        var inst = new Data_ProcessedMaterial(rawData.Name, rawData.FolderPath);

        // 1. Extraer y Analizar Texturas
        TextureAnalysis analysis = AnalyzeTextures(rawData, inst);
        inst.textureInstructions = analysis.Instructions;

        // 2. Extraer Colores Globales (Diffuse Tint y Emissive Color)
        inst.baseColor = FindVector(rawData, "Colour", "Tint", "");
        if (analysis.HasLayered)
            inst.layer2Color = FindVector(rawData, "Colour", "Tint", "2 - ");
        inst.emissiveColor = FindVector(rawData, "Emissive", "Emis", "");

        // 3. Determinar Shader
        inst.shaderType = DetermineShaderType(rawData.ParentShader, analysis.HasLayered);
        if (inst.shaderType == HDRPShaderType.LayeredLit)
            inst.layersCount = 2;

        // 4. Configurar Superficie
        inst.isAlphaClip = DetermineAlphaClip(rawData, analysis.HasAlpha);
        inst.alphaCutoff = rawData.OpacityMaskClipValue;
        inst.isTransparent = rawData.ParentShader.ToLower().Contains("m_glass");
        inst.isTwoSide = rawData.TwoSided;

        return inst;
    }

    // --- 3. MÉTODOS ESPECIALIZADOS ---
    private static TextureAnalysis AnalyzeTextures(Data_ParsedMaterial rawData, Data_ProcessedMaterial inst)
    {
        TextureAnalysis result = new TextureAnalysis();

        foreach (var rawTex in rawData.TextureParameters.Where(t => !string.IsNullOrEmpty(t.Value)))
        {
            string texName = Utils_StringsConverter.GetNameFromUnrealPath(rawTex.Value);
            string texPath = Utils_StringsConverter.GetFolderFromUnrealPath(rawTex.Value);

            if (texPath.Contains("Engine/"))
                continue;

            bool isLayer2 = rawTex.Name.Contains("2 - ");
            string prefix = isLayer2 ? "2 - " : "";

            string cleanParamName = isLayer2? rawTex.Name.Replace ("2 - ", ""): rawTex.Name;
            TextureProperties texType = MapToTextureProperty(cleanParamName, rawTex.Value);

            var texInst = new Data_TextureInstruction(texName, texPath, texType)
            {
                layerIndex = isLayer2 ? 1 : 0
            };

            FillInstructionData(texInst, rawData, prefix);

            // Recopilar evidencias
            if (texType == TextureProperties.AlphaMask) 
                result.HasAlpha = true;

            if (texInst.layerIndex > 0) 
                result.HasLayered = true;

            if (!texInst.UpdateGUID())
                inst.lastError = $"Alerta: No se encontró {texName}";

            result.Instructions.Add(texInst);
        }

        return result;
    }

    private static void FillInstructionData(Data_TextureInstruction texInst, Data_ParsedMaterial rawData, string prefix)
    {
        float detailTiling = FindScalar(rawData, "Detail Tiling", "Detail Scale", prefix, 1.0f);

        if (texInst.textureProperty == TextureProperties.Detail_color || texInst.textureProperty == TextureProperties.Detail_normal)
        {
            texInst.tileScale = new Vector2(detailTiling, detailTiling);
            texInst.uvChannel = 2;
        }

        switch (texInst.textureProperty)
        {
            case TextureProperties.Normal:
                texInst.strength = FindScalar(rawData, "Normal", "Bump", prefix, 1.0f);
                break;

            case TextureProperties.Emissive:
                texInst.strength = FindScalar(rawData, "Emissive Strength", "Intensity", prefix, 1.0f);
                break;

            case TextureProperties.Heightmap:
                // Guardamos el valor de Unreal (ej: 0.02). El Builder hará el * 100.
                texInst.strength = FindScalar(rawData, "Height", "POM Depth", prefix, 0.02f);
                break;

            case TextureProperties.Detail_color:
                // Buscamos las 3 intensidades que HDRP pide para el Detail Map
                texInst.detailAlbedoStrength = FindScalar(rawData, "Diffuse Colour Strength", "Albedo", prefix, 1.0f);
                texInst.detailNormalStrength = FindScalar(rawData, "Normals Strength", "Normal", prefix, 1.0f);
                texInst.detailSmoothnessStrength = FindScalar(rawData, "Roughness Strength", "Smooth", prefix, 1.0f);
                break;
        }
    }

    private static float FindScalar(Data_ParsedMaterial data, string primary, string secondary, string prefix, float @default)
    {
        var param = data.ScalarParameters.Find(s =>
            s.Name.StartsWith(prefix) && (s.Name.Contains(primary) || s.Name.Contains(secondary)));
        return param != null ? param.Value : @default;
    }

    private static Color FindVector(Data_ParsedMaterial data, string primary, string secondary, string prefix)
    {
        var param = data.VectorParameters.Find(v => {
            bool matchPrefix = string.IsNullOrEmpty(prefix)
                ? !v.Name.Contains("2 - ") // Si no hay prefijo, evita pillar cosas de la capa 2
                : v.Name.Contains(prefix);  // Si hay prefijo, búscalo

            bool matchName = v.Name.Contains(primary) || v.Name.Contains(secondary);

            return matchPrefix && matchName;
        });

        return param != null ? param.Value : Color.white;
    }

    private static TextureProperties MapToTextureProperty(string pName, string tName)
    {
        foreach (var rule in TextureRules)
        {
            bool matchPName = string.IsNullOrEmpty(rule.PName) || pName.Contains(rule.PName);
            bool matchTName = string.IsNullOrEmpty(rule.TName) || tName.Contains(rule.TName);
            bool passExclude = string.IsNullOrEmpty(rule.Exclude) || !pName.Contains(rule.Exclude);

            if (matchPName && matchTName && passExclude)
                return rule.Prop;
        }

        return TextureProperties.Unknown;
    }

    private static HDRPShaderType DetermineShaderType(string parentShader, bool isLayered)
    {
        if (string.IsNullOrEmpty(parentShader)) 
            return HDRPShaderType.Lit;

        string parent = parentShader.ToLower();
        if (parent.Contains("m_decal")) 
            return HDRPShaderType.Decal;

        if (isLayered) 
            return HDRPShaderType.LayeredLit;

        return HDRPShaderType.Lit;
    }

    private static bool DetermineAlphaClip(Data_ParsedMaterial rawData, bool hasAlphaTexture)
    {
        bool isMaskedBlend = rawData.BlendMode != null && rawData.BlendMode.Contains("BLEND_Masked");
        bool hasMaskedSwitch = rawData.StaticSwitches.Any(s => s.Name.Contains("Enable Masked Transparency") && s.Value);

        return isMaskedBlend || hasAlphaTexture || hasMaskedSwitch;
    }
}
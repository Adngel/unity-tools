using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class Module_MaterialLinker
{
    private static readonly Dictionary<HDRPShaderType, MatBuilder_Base> Builders = new()
    {
        { HDRPShaderType.Lit,        new MatBuilder_Lit() },
        { HDRPShaderType.LayeredLit, new MatBuilder_LayeredLit() },
        { HDRPShaderType.Decal,      new MatBuilder_Decal() }
    };

    public static bool LinkMaterial(Data_ProcessedMaterial data, bool overwrite)
    {
        if (data.isTextured && !overwrite)
        {
            return true;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(data.unityMaterialGUID);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError($"[Linker] ERROR: Material GUID no encontrado en {data.materialName}. Saltando...");
            return false;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (mat == null)
        {
            Debug.LogError($"[Linker] ERROR: Material no encontrado en {assetPath}. Saltando...");
            return false;
        }

        if (!overwrite)
        {
            if (IsMaterialAlreadyConfigured(mat))
            {
                data.isTextured = true;
                return true;
            }
        }

        if (Builders.TryGetValue(data.shaderType, out MatBuilder_Base builder))
        {
            builder.Apply(mat, data);

            HDMaterial.ValidateMaterial(mat);
            EditorUtility.SetDirty(mat);
            data.isTextured = true;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Revisa si el material tiene alguna textura asignada que no sea la de por defecto.
    /// </summary>
    private static bool IsMaterialAlreadyConfigured(Material mat)
    {
        Shader shader = mat.shader;

        // 1. Usamos Shader.GetPropertyCount directamente
        int propertyCount = shader.GetPropertyCount();

        for (int i = 0; i < propertyCount; i++)
        {
            // 2. Usamos shader.GetPropertyType y el enum moderno ShaderPropertyType.Texture
            if (shader.GetPropertyType(i) == ShaderPropertyType.Texture)
            {
                // 3. Usamos shader.GetPropertyName
                string propName = shader.GetPropertyName(i);
                Texture tex = mat.GetTexture(propName);

                if (tex != null)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Module_MaterialShells
{
    public static bool CreateMaterialAssets(Data_ProcessedMaterial data, bool overwrite)
    {
        if (data.isCreated && !overwrite)
        {
            return true;
        }

        data.lastError = string.Empty;

        string assetPath = $"{data.materialFolderPath}{data.materialName}.mat";

        try
        {
            string shaderName = GetHDRPShaderPath(data.shaderType);
            Shader shader = Shader.Find(shaderName);

            if (shader == null)
            {
                data.lastError = $"Shader no encontrado: {shaderName}";
                return false;
            }

            if (!Directory.Exists(data.materialFolderPath))
            {
                Directory.CreateDirectory(data.materialFolderPath);
                AssetDatabase.ImportAsset(data.materialFolderPath);
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (mat == null)
            {
                // CASO A: Nuevo material
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, assetPath);
                HDMaterial.ValidateMaterial(mat);
            }
            else if (overwrite)
            {
                // CASO B: Existe y permitimos reemplazarlo
                if (mat.shader != shader)
                {
                    mat.shader = shader;
                    HDMaterial.ValidateMaterial(mat);
                }
            }

            if (string.IsNullOrEmpty(data.unityMaterialGUID))
                data.unityMaterialGUID = AssetDatabase.AssetPathToGUID(assetPath);

            data.isCreated = true;

            return true;
        }
        catch (System.Exception e)
        {
            data.lastError = $"Excepción en Fase 2A: {e.Message}";
            return false;
        }
    }

    private static string GetHDRPShaderPath(HDRPShaderType type)
    {
        string shaderName;
        switch (type)
        {
            case HDRPShaderType.Lit:
                shaderName = "HDRP/Lit";
                break;
            case HDRPShaderType.LayeredLit:
                shaderName = "HDRP/LayeredLit";
                break;
            case HDRPShaderType.Decal:
                shaderName = "HDRP/Decal";
                break;
            case HDRPShaderType.Unknown:
            default:
                shaderName = "Unknown";
                break;
        }

        return shaderName;
    }
}

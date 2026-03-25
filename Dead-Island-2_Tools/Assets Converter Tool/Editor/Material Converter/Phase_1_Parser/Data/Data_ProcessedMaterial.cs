using System.Collections.Generic;
using UnityEngine;

public enum HDRPShaderType
{
    Lit,
    LayeredLit,
    Decal,
    Unknown
}

[System.Serializable]
public class Data_ProcessedMaterial
{
    [Header("Localización")]
    public string materialName;
    public string materialFolderPath;
    public string unityMaterialGUID;
    public int debugIndex;

    [Header("Definición")]
    public HDRPShaderType shaderType = HDRPShaderType.Lit;
    public int layersCount = 1;
    public bool isTransparent = false;
    public bool isAlphaClip = false;
    public float alphaCutoff = 0.333f;
    public bool isTwoSide = false;

    [Header("Colores Globales")]
    public Color baseColor = Color.white;
    public Color layer2Color = Color.white;
    public Color emissiveColor = Color.black;

    [Space(20)]
    [Header("Texturas")]
    public List<Data_TextureInstruction> textureInstructions;

    [Space(20)]
    [Header("Limpieza y Debug")]
    [Tooltip("GUIDs de assets que podrían borrarse si nadie más los usa al final del proceso.")]
    public List<string> disposalCandidateGUIDs = new List<string>();
    public string lastError;

    [Space(20)]
    [Header("Estado")]
    public bool isCreated;           // Fase 2A completada
    public bool isPrepared;          // Fase 2B completada
    public bool isTextured;          // Fase 2C completada


    public Data_ProcessedMaterial()
    {
        // Inicialización de listas para evitar sustos
        textureInstructions = new List<Data_TextureInstruction>();
        disposalCandidateGUIDs = new List<string>();

        baseColor = Color.white;
        layer2Color = Color.white;
        emissiveColor = Color.black;

        shaderType = HDRPShaderType.Lit;
        layersCount = 1;
        alphaCutoff = 0.333f;
    }

    public Data_ProcessedMaterial(string name, string path)
    {
        materialName = name;
        materialFolderPath = path;

        textureInstructions = new List<Data_TextureInstruction>();
        disposalCandidateGUIDs = new List<string>();

        baseColor = Color.white;
        layer2Color = Color.white;
        emissiveColor = Color.black;

        shaderType = HDRPShaderType.Lit;
        layersCount = 1;
        alphaCutoff = 0.333f;
    }

    /// <summary>
    /// Limpia las referencias a un GUID que ha sido borrado del proyecto.
    /// </summary>
    public void NotifyAssetDeleted(string deletedGuid)
    {
        // 1. Limpiar de la lista de candidatos a borrar
        if (disposalCandidateGUIDs.Contains(deletedGuid))
        {
            disposalCandidateGUIDs.Remove(deletedGuid);
        }


        // 2. Limpiar de las instrucciones de textura
        foreach (var tex in textureInstructions)
        {
            if (tex.unityOriginalTextureGUID == deletedGuid)
            {
                tex.unityOriginalTextureGUID = string.Empty;
                tex.textureOriginalName += " (DELETED)";
            }

            if (tex.unityProcessedTextureGUID == deletedGuid)
            {
                tex.unityProcessedTextureGUID = string.Empty;
            }
        }
    }
}

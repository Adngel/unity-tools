using UnityEngine;
using UnityEditor;

public enum TextureProperties
{
    Diffuse,
    Mask,
    DecalMask,
    Normal,
    Heightmap,
    Emissive,
    Detail_color,
    Detail_normal,
    LayerMask,
    AlphaMask,
    Unknown
}

[System.Serializable]
public class Data_TextureInstruction
{
    public string textureOriginalName;
    public string unityOriginalTextureGUID;
    [Space(5)]
    public string textureProcessedName;
    public string unityProcessedTextureGUID;
    [Space(5)]
    public string textureFolderPath;
    [Space(10)]

    [Header("Configuración de Importación")]
    public TextureProperties textureProperty;
    public int layerIndex;
    public int uvChannel;
    public Vector2 tileScale;

    [Header("Ajustes de Intensidad (Opcional)")]
    [Tooltip("Usado para Normal Strength, Emissive Intensity, Height Amplitude, etc.")]
    public float strength = 1.0f;
    public float detailAlbedoStrength = 1.0f;
    public float detailNormalStrength = 1.0f;
    public float detailSmoothnessStrength = 1.0f;

    public Data_TextureInstruction() { }

    public Data_TextureInstruction(string originalName, string folderPath, TextureProperties property)
    {
        textureOriginalName = originalName;
        textureFolderPath = folderPath;
        textureProperty = property;

        layerIndex = 0;
        uvChannel = 0;
        tileScale = new Vector2(1.0f, 1.0f);
        strength = 1.0f;
    }

    /// <summary>
    /// Intenta encontrar el GUID de la textura original en el proyecto.
    /// </summary>
    public bool UpdateGUID()
    {
        #if UNITY_EDITOR
        if (string.IsNullOrEmpty(textureOriginalName)) 
            return false;

        string searchQuery = $"{textureOriginalName} t:Texture2D";
        string[] searchFolder = new string[] { textureFolderPath };

        string[] guidsInFolder = AssetDatabase.FindAssets(searchQuery, searchFolder);
        
        if (guidsInFolder.Length > 0)
        {
            unityOriginalTextureGUID = guidsInFolder[0];
            return true;
        }
        #endif
        return false;
    }

} 

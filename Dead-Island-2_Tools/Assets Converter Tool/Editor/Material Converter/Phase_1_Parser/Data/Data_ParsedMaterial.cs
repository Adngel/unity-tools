using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Data_ParsedMaterial
{
    // Identificación
    public string Name;
    public string FolderPath;

    // Configuración de Renderizado
    public string ParentShader;
    public string BlendMode = "BLEND_Opaque";
    public bool TwoSided = false;
    public float OpacityMaskClipValue = 0.3333f;

    public List<TextureParam> TextureParameters;
    public List<ScalarParam> ScalarParameters;
    public List<VectorParam> VectorParameters;
    public List<SwitchParam> StaticSwitches;

    public Data_ParsedMaterial() { }
    public Data_ParsedMaterial(string fullFilePath) 
    {
        Name = Utils_StringsConverter.GetNameFromUnityPath(fullFilePath);
        FolderPath = Utils_StringsConverter.GetFolderFromUnityPath(fullFilePath);

        TextureParameters = new List<TextureParam>();
        ScalarParameters = new List<ScalarParam>();
        VectorParameters = new List<VectorParam>();
        StaticSwitches = new List<SwitchParam>();
    }
}

[System.Serializable]
public class TextureParam
{
    public string Name;  // Tomado de ParameterInfo -> Name
    public string Value; // La ruta Texture2D'...'

    public TextureParam() { }
    public TextureParam(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

[System.Serializable]
public class ScalarParam
{
    public string Name;
    public float Value;

    public ScalarParam() { }
    public ScalarParam(string name, float value)
    {
        Name = name;
        Value = value;
    }
}

[System.Serializable]
public class VectorParam
{
    public string Name;
    public Color Value;

    public VectorParam() { }
    public VectorParam(string name, Color value)
    {
        Name = name;
        Value = value;
    }
}

[System.Serializable]
public class SwitchParam
{
    public string Name;
    public bool Value;

    public SwitchParam() { }
    public SwitchParam(string name, bool value)
    {
        Name = name;
        Value = value;
    }
}
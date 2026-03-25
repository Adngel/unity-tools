using UnityEngine;
public static class Module_ParserTXT
{
    public enum ParamType 
    { 
        Scalar, 
        Texture, 
        Vector, 
        Switch 
    }

    public static bool ParseData(string content, Data_ParsedMaterial mat)
    {
        if (string.IsNullOrEmpty(content) || mat == null)
            return false;

        string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // --- PARENT ---
            if (line.StartsWith("Parent"))
            {
                mat.ParentShader = Utils_txtParser.ExtractPathParameter(line);

                if (mat.ParentShader == null)
                    Debug.LogWarning("No se encontró ningún Parent Path en la línea: " + line);

                continue;
            }

            // --- BASE PROPERTIES ---
            if (line.Contains("BasePropertyOverrides ="))
            {
                i = ParseBaseProperties(lines, i, mat);
                continue;
            }

            // --- DETECCIÓN DE BLOQUES ---
            // - ScalarParameterValues
            // - TextureParameterValues
            // - VectorParameterValues
            // - StaticSwitchParameters

            foreach (ParamType pType in System.Enum.GetValues(typeof(ParamType)))
            {
                string header = GetHeaderName(pType);
                if (line.Contains(header))
                {
                    int count = Utils_txtParser.ExtractIndexParameter(line);
                    if (count > 0)
                    {
                        i = ParseBlock(lines, i, count, mat, pType);
                    }
                    break;
                }
            }
        }
        return true;
    }

    private static int ParseBaseProperties(string[] lines, int currentIndex, Data_ParsedMaterial mat)
    {
        int i = currentIndex + 1;
        while (i < lines.Length)
        {
            string line = lines[i].Trim();
            string compactLine = line.Replace(" ", "");

            if (!compactLine.Contains("bOverride_"))
            {
                if (compactLine.Contains("BlendMode="))
                {
                    mat.BlendMode = Utils_txtParser.ExtractStringParameter(line, "BlendMode");
                }
                else if (compactLine.Contains("TwoSided="))
                {
                    mat.TwoSided = line.ToLower().Contains("true");
                }
                else if (compactLine.Contains("OpacityMaskClipValue="))
                {
                    mat.OpacityMaskClipValue = Utils_txtParser.ExtractFloatParameter(line);
                }
            }

            if (line.StartsWith("}")) 
                break;

            i++;
        }
        return i;
    }

    private static int ParseBlock(string[] lines, int currentIndex, int count, Data_ParsedMaterial mat, ParamType type)
    {
        int i = currentIndex;
        string headerBase = GetHeaderName(type);

        for (int n = 0; n < count; n++)
        {
            // Localizar cabecera
            // --------------------
            string targetHeader = $"{headerBase}[{n}]";

            while (i < lines.Length && !lines[i].Contains(targetHeader))
            {
                i++;
            }

            if (i >= lines.Length) 
                break;

            // Escanear datos
            // --------------------
            bool isParamInfo = false;
            string pName = "";
            bool pNameFound = false;
            string pValLine = "";
            bool pValFound = false;

            while (i < lines.Length)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("ParameterInfo ="))
                {
                    isParamInfo = true;
                }
                if (isParamInfo)
                {
                    if (line.Replace(" ", "").Contains("Name="))
                    {
                        pNameFound = true;
                        pName = Utils_txtParser.ExtractStringParameter(line, "Name");
                        isParamInfo = false;
                    }
                }
                if (line.StartsWith("ParameterValue ="))
                {
                    pValFound = true;
                    pValLine = line;
                }
                if (line.StartsWith("}") && pNameFound && pValFound)
                {
                    SendParameterValues(pName, pValLine, mat, type);
                    break;
                }
                i++;
            }
        }

        return i;
    }

    private static void SendParameterValues(string pName, string pValLine, Data_ParsedMaterial mat, ParamType type)
    {
        switch (type)
        {
            case ParamType.Scalar:
                float sVal = Utils_txtParser.ExtractFloatParameter(pValLine);
                mat.ScalarParameters.Add(new ScalarParam(pName, sVal));
                break;
            case ParamType.Texture:
                string tPath = Utils_txtParser.ExtractPathParameter(pValLine);
                mat.TextureParameters.Add(new TextureParam(pName, tPath));
                break;
            case ParamType.Vector:
                Color col = Utils_txtParser.ExtractColorParameter(pValLine);
                mat.VectorParameters.Add(new VectorParam(pName, col));
                break;
            case ParamType.Switch:
                bool bVal = pValLine.ToLower().Contains("true");
                mat.StaticSwitches.Add(new SwitchParam(pName, bVal));
                break;
            default:
                break;
        }
    }

    private static string GetHeaderName(ParamType type)
    {
        return type switch
        {
            ParamType.Scalar => "ScalarParameterValues",
            ParamType.Texture => "TextureParameterValues",
            ParamType.Vector => "VectorParameterValues",
            ParamType.Switch => "StaticSwitchParameters",
            _ => ""
        };
    }
}
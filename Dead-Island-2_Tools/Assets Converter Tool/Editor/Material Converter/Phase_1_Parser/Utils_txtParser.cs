using System.Globalization;
using UnityEngine;

public class Utils_txtParser
{
    // Game Source: Dead Island 2 / Unreal Engine General Export

    /// <summary>
    /// Extrae el nombre dentro de ParameterInfo = { Name=Nombre }
    /// </summary>
    public static string ExtractStringParameter(string line, string key)
    {
        int keyIndex = line.IndexOf(key);
        
        if (keyIndex == -1) 
            return "";

        string rawValue = line.Substring(keyIndex + key.Length);

                                                     //                    " = MiNombre } "
        string cleanName = rawValue .Trim()          // Quitamos espacios: "= MiNombre }"
                                    .TrimStart('=')  // Quitamos el igual: " MiNombre }"
                                    .TrimEnd('}')    // Quitamos la llave: " MiNombre "
                                    .Trim();         // Quitamos los espacios que quedaron: "MiNombre"
        return cleanName;
    }

    public static string ExtractPathParameter(string line)
    {
        int firstQuote = line.IndexOf('\'');
        int lastQuote = line.LastIndexOf('\'');
        
        if (firstQuote == -1 || lastQuote == -1 || firstQuote >= lastQuote)
            return string.Empty;

        return line.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
    }

    /// <summary>
    /// Extrae el número entre corchetes, que representa la cantidad o el índice del bloque.
    /// Ejemplo: ScalarParameterValues[12] -> devuelve 12
    /// </summary>
    public static int ExtractIndexParameter(string line)
    {
        int start = line.IndexOf('[') + 1;
        int end = line.IndexOf(']', start);

        if (start <= 0 || end == -1 || start >= end)
            return -1;

        string textValue = line.Substring(start, end - start);

        if (int.TryParse(textValue, out int result))
        {
            return result;
        }

        return -1;
    }

    /// <summary>
    /// Extrae el valor numérico después del símbolo '='
    /// </summary>
    public static float ExtractFloatParameter(string line)
    {
        int separator = line.IndexOf('=');

        if (separator == -1) 
            return 0.0f;

        string textValue = line.Substring(separator + 1).Trim();

        // Usamos InvariantCulture para asegurar que el punto '.' se lea siempre como decimal
        if (float.TryParse(textValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }

        return 0.0f;
    }

    public static Color ExtractColorParameter(string line)
    {
        Color col = Color.white;

        string content = line.Replace(" ", "");

        if (content.Contains("{"))
        {
            content = content.Split('{')[1].Split('}')[0];
        }
        
        string[] components = content.Split(',');
        foreach (string comp in components)
        {
            if (!comp.Contains("=")) 
                continue;

            string[] kvp = comp.Split('=');
            string key = kvp[0].ToUpper();
            float val = 0f;

            if (float.TryParse(kvp[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val))
            {
                switch (key)
                {
                    case "R": 
                        col.r = val; 
                        break;
                    case "G": 
                        col.g = val; 
                        break;
                    case "B": 
                        col.b = val; 
                        break;
                    case "A": 
                        col.a = val; 
                        break;
                }
            }

        }
        return col;
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

public class Utils_TexProcessor
{
    /// <summary>
    /// Lee una textura del disco saltándose el sistema de importación de Unity.
    /// Evita errores de "Texture not readable".
    /// </summary>
    public static Texture2D LoadTextureRaw(string assetPath)
    {
        string fullPath = Path.GetFullPath(assetPath).Replace("\\", "/");

        if (!File.Exists(fullPath))
        {
            return null;
        }

        if (assetPath.ToLower().EndsWith(".tga"))
        {
            return LoadTGA(fullPath);
        }

        // Fallback para PNG/JPG
        byte[] data = File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(data))
        {
            return tex;
        }

        Object.DestroyImmediate(tex);
        return null;
    }

    public static Texture2D LoadTGA(string fileName)
    {
        using (var fs = System.IO.File.OpenRead(fileName))
        using (var reader = new System.IO.BinaryReader(fs))
        {
            reader.ReadByte(); // ID Length
            reader.ReadByte(); // Color Map Type
            byte imageType = reader.ReadByte(); // 2 = Raw, 10 = RLE

            for (int i = 0; i < 9; i++) reader.ReadByte(); // Skip info

            short width = reader.ReadInt16();
            short height = reader.ReadInt16();
            byte bitDepth = reader.ReadByte();
            reader.ReadByte(); // Descriptor

            int bytesPerPixel = bitDepth / 8;
            int pixelCount = width * height;
            Color32[] pixels = new Color32[pixelCount];

            if (imageType == 2) // --- TGA SIN COMPRIMIR ---
            {
                byte[] data = reader.ReadBytes(pixelCount * bytesPerPixel);
                for (int i = 0; i < pixelCount; i++)
                {
                    int bIndex = i * bytesPerPixel;
                    pixels[i] = ExtractColor(data, bIndex, bytesPerPixel);
                }
            }
            else if (imageType == 10) // --- TGA COMPRIMIDO (RLE) ---
            {
                int currentPixel = 0;
                while (currentPixel < pixelCount)
                {
                    byte chunkHeader = reader.ReadByte();
                    int count = (chunkHeader & 0x7F) + 1;

                    if ((chunkHeader & 0x80) != 0) // RLE Chunk (Píxel repetido)
                    {
                        byte[] colorBuf = reader.ReadBytes(bytesPerPixel);
                        Color32 color = ExtractColor(colorBuf, 0, bytesPerPixel);
                        for (int i = 0; i < count; i++)
                        {
                            pixels[currentPixel++] = color;
                        }
                    }
                    else // Raw Chunk (Píxeles individuales)
                    {
                        byte[] colorBuf = reader.ReadBytes(count * bytesPerPixel);
                        for (int i = 0; i < count; i++)
                        {
                            pixels[currentPixel++] = ExtractColor(colorBuf, i * bytesPerPixel, bytesPerPixel);
                        }
                    }
                }
            }

            Texture2D tex = new Texture2D(width, height, bitDepth == 32 ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }
    }

    private static Color32 ExtractColor(byte[] d, int startIndex, int bpp)
    {
        //BGRA -> RGBA
        byte b = d[startIndex];
        byte g = d[startIndex + 1];
        byte r = d[startIndex + 2];
        byte a = (bpp == 4) ? d[startIndex + 3] : (byte)255;
        return new Color32(r, g, b, a);
    }


    /// <summary>
    /// Guarda los bytes en el disco y solo entonces notifica a Unity.
    /// </summary>
    public static void SaveToDiskRaw(string assetPath, Texture2D tex)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        byte[] bytes = tex.EncodeToTGA();
        File.WriteAllBytes(fullPath, bytes);
    }

    public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    /// <summary>
    /// Configura el importador de forma atómica.
    /// </summary>
    public static void ConfigureImporter(string assetPath, bool isNormal, bool isLinear, bool force = false)
    {
        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (ti == null) 
            return;

        bool changed = false;

        // Settings de espacio de color
        bool targetSRGB = !isLinear;
        if (ti.sRGBTexture != targetSRGB) 
        { 
            ti.sRGBTexture = targetSRGB; 
            changed = true; 
        }

        // Settings de tipo
        TextureImporterType targetType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (ti.textureType != targetType) 
        { 
            ti.textureType = targetType; 
            changed = true; 
        }

        // Forzar no readable para ahorrar memoria en el build final
        if (ti.isReadable) 
        { 
            ti.isReadable = false; 
            changed = true; 
        }

        if (changed || force)
        {
            ti.SaveAndReimport();
        }
    }

    /// <summary>
    /// Compara dos texturas y devuelve el tamaño máximo entre ambas.
    /// </summary>
    public static Vector2Int GetMaxDimensions(Texture2D a, Texture2D b)
    {
        int width = Mathf.Max(a.width, b != null ? b.width : 0);
        int height = Mathf.Max(a.height, b != null ? b.height : 0);
        return new Vector2Int(width, height);
    }

    /// <summary>
    /// Asegura que una textura tenga el tamaño objetivo. 
    /// Si ya lo tiene, devuelve la original. Si no, crea una escalada.
    /// IMPORTANTE: Si crea una nueva, el llamador debe destruirla.
    /// </summary>
    public static Texture2D EnsureSize(Texture2D source, int targetWidth, int targetHeight, out bool wasScaled)
    {
        if (source.width == targetWidth && source.height == targetHeight)
        {
            wasScaled = false;
            return source;
        }

        wasScaled = true;
        return ScaleTexture(source, targetWidth, targetHeight);
    }

}

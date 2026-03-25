using System.Collections.Generic;
using UnityEngine;

public class Module_TextureProcessor
{
    private static readonly Dictionary<TextureProperties, TexProcessor_Base> Processors = new()
    {
        { TextureProperties.Diffuse,       new TexProcessor_Diffuse() },
        { TextureProperties.Normal,        new TexProcessor_Normal() },
        { TextureProperties.Mask,          new TexProcessor_MaskMap() },
        { TextureProperties.DecalMask,     new TexProcessor_DecalMaskMap() },
        { TextureProperties.Emissive,      new TexProcessor_Emissive() },
        { TextureProperties.Heightmap,     new TexProcessor_Heightmap() },
        { TextureProperties.Detail_color,  new TexProcessor_Detail() },
        { TextureProperties.LayerMask,     new TexProcessor_LayerMask() }
    };

    public static bool PrepareTextures(Data_ProcessedMaterial data, bool forceRecreation)
    {
        if (data.isPrepared && !forceRecreation)
        {
            return true;
        }

        if (data.textureInstructions == null || data.textureInstructions.Count == 0)
            return true;

        bool allOk = true;
        foreach (Data_TextureInstruction texInst in data.textureInstructions)
        {
            if (Processors.TryGetValue(texInst.textureProperty, out TexProcessor_Base processor))
            {
                try
                {
                    processor.ProcessFile(texInst, data, forceRecreation);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TexProcessor] Error procesando {texInst.textureOriginalName}: {e.Message}");
                    allOk = false;
                }
            }
            else
            {
                Debug.LogWarning($"[TexProcessor] No hay procesador para la propiedad: {texInst.textureProperty} en la textura {texInst.textureOriginalName}");
                continue;
            }
        }

        return allOk;
    }
}

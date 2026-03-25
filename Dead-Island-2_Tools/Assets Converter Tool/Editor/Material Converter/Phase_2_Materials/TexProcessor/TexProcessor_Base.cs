using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class TexProcessor_Base
{
    public abstract void ProcessFile(Data_TextureInstruction inst, Data_ProcessedMaterial mat, bool force = false);


    /// <summary>
    /// Lógica común para decidir si una textura necesita ser procesada o si podemos saltarla.
    /// </summary>
    protected bool EvaluatePreexisting(Data_TextureInstruction inst, bool force)
    {
        if (force) 
            return true;

        if (!string.IsNullOrEmpty(inst.unityProcessedTextureGUID))
        {
            string path = AssetDatabase.GUIDToAssetPath(inst.unityProcessedTextureGUID);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                return false;
            }
        }

        return true;
    }
}

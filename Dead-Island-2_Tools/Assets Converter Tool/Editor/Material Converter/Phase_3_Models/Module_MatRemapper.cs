using System.IO;
using UnityEditor;
using UnityEngine;

public static class Module_MatRemapper
{
    public static bool Reassign(string modelPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null)
            return false;

        importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();


        Debug.Log($"<color=lime>[Remapper]</color> Auto-Remap completado para: {Path.GetFileName(modelPath)}");
        return true;
    }
}

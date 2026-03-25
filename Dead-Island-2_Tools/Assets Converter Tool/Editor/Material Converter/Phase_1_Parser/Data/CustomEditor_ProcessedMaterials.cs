using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(Database_ProcessedMaterials))]
public class CustomEditor_ProcessedMaterials : Editor
{
    private string searchString = "";
    private int foundIndex = -1;
    private string foundName = "";

    public override void OnInspectorGUI()
    {
        Database_ProcessedMaterials db = (Database_ProcessedMaterials)target;

        // 1. BUSCADOR MINIMALISTA
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("?? LOCALIZADOR DE ÍNDICE", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        searchString = EditorGUILayout.TextField(searchString);

        if (GUILayout.Button("Localizar", GUILayout.Width(80)))
        {
            // Buscamos el índice exacto
            foundIndex = db.instructions.FindIndex(x =>
                x.materialName.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (foundIndex != -1)
                foundName = db.instructions[foundIndex].materialName;
            else
                EditorUtility.DisplayDialog("Buscador", "No se encontró ningún material con ese nombre.", "Ok");
        }
        EditorGUILayout.EndHorizontal();

        if (foundIndex != -1)
        {
            EditorGUILayout.HelpBox($"MATERIAL: {foundName}\nESTÁ EN EL ÍNDICE: {foundIndex}", MessageType.Info);
            if (GUILayout.Button("Limpiar Marcador")) foundIndex = -1;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // 2. LA LISTA ORIGINAL (Sin filtros verdes, sin formatos raros)
        // Esto dibujará la lista 'instructions' tal cual la conoces.
        DrawDefaultInspector();
    }
}
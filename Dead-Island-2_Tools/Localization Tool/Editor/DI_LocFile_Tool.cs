using UnityEditor;
using UnityEngine;
using System.IO;

public class DI_LocFile_Tool : EditorWindow
{
    private TextAsset sourceFile;
    private Vector2 scrollPos;

    private Data_LocalizationFile extractedData;

    [MenuItem("Tools/DI2/Localization Text Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<DI_LocFile_Tool>("DI Localization Text Tool");
        window.minSize = new Vector2(350, 450);
    }

    void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.margin = new RectOffset(0, 0, 10, 10);

        GUILayout.Label("DI2 LOCALIZATION TOOL", titleStyle);
        EditorGUILayout.Space(5);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // SECCIÓN 1: CARGA
        BeginSection("1. Input File", Color.cyan);
        sourceFile = (TextAsset)EditorGUILayout.ObjectField("Dialogue File", sourceFile, typeof(TextAsset), false);

        if (DrawButton("Read File", "Extrae la información del XML a la memoria.", Color.cyan))
        {
            extractedData = DI_LocFileProcessor.LoadOnly(sourceFile);
        }

        if (extractedData != null)
            EditorGUILayout.HelpBox($"Datos cargados: {extractedData.DialogueLines.Count} líneas.", MessageType.Info);
        EndSection();

        // BLOQUEO DE SEGURIDAD: Si no hay datos, no hay proceso
        GUI.enabled = (sourceFile != null && extractedData != null);

        // SECCIÓN 2: BASE DE DATOS DE PERSONAJES
        BeginSection("2. Characters Database", Color.magenta);
        if (DrawButton("Sync Characters Data", "Actualiza el ScriptableObject con los nombres del archivo cargado.", Color.magenta))
        {
            var db = DI_CharacterManager.LoadDatabase();
            DI_CharacterManager.SyncFromParsedData(extractedData, db);
        }

        if (GUILayout.Button(new GUIContent("Open Database File", "Selecciona el ScriptableObject en el proyecto."), GUILayout.Height(20)))
        {
            var db = DI_CharacterManager.GetCharDatabase();
            if (db != null)
            {
                Selection.activeObject = db;
                EditorGUIUtility.PingObject(db);
            }
            else
            {
                // Si el usuario le da a Open pero aún no existe la DB, 
                // usamos LoadDatabase para que la cree y la abra.
                var newDb = DI_CharacterManager.LoadDatabase();
                Selection.activeObject = newDb;
                EditorGUIUtility.PingObject(newDb);
            }
        }

        EditorGUILayout.HelpBox("Gestiona perfiles y prioridades para los Slayers.", MessageType.None);
        EndSection();

        // SECCIÓN 3: GENERACIÓN
        BeginSection("3. Generate Scripts", Color.green);
        if (DrawButton("Generate Text Files", "Crea los archivos .txt (Escenas, Conversaciones, etc.)", Color.green))
        {
            DI_LocFileProcessor.ExecuteProcessors(sourceFile, extractedData);
        }
        EndSection();

        GUI.enabled = true;
        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Status: " + (sourceFile != null ? "Ready" : "Waiting..."), EditorStyles.miniLabel);
    }

    private void BeginSection(string title, Color color)
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.helpBox);
        sectionStyle.margin = new RectOffset(10, 10, 5, 5);
        sectionStyle.padding = new RectOffset(10, 10, 10, 10);
        EditorGUILayout.BeginVertical(sectionStyle);
        GUI.color = color;
        GUILayout.Label(title.ToUpper(), EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.Space(2);
    }

    private void EndSection() => EditorGUILayout.EndVertical();

    private bool DrawButton(string label, string tooltip, Color? bgColor = null)
    {
        Color originalColor = GUI.backgroundColor;
        if (bgColor.HasValue) GUI.backgroundColor = bgColor.Value;
        bool clicked = GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(35));
        GUI.backgroundColor = originalColor;
        return clicked;
    }
}
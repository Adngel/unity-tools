using UnityEngine;
using UnityEditor;

public class DI_Converter_Tool : EditorWindow
{
    private DefaultAsset targetFolder;
    private Database_ParsedMaterials parsedDb;
    private Database_ProcessedMaterials processedDb;

    private Vector2 scrollPos;

    // --- FLAGS DE CONTROL ---
    [Header("Phase 2 Flags")]
    private bool cleanParsedDb = false;    // Borrar SO de analizados antes de empezar
    private bool cleanProcessedDb = false; // Borrar SO de instrucciones antes de empezar

    [Header("Phase 3 Flags")]
    private bool overwriteMaterials = false; // ¿Pisar el .mat si ya existe?
    private bool forceTextureReprocess = false; // ¿Volver a generar TGAs aunque existan?

    [MenuItem("Tools/DI2/Material Converter Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<DI_Converter_Tool>("DI2 Converter Tool");
        window.minSize = new Vector2(400, 700);
    }

    void OnEnable()
    {
        EnsureAllDatabases();
    }

    void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 0, 10, 10)
        };

        GUILayout.Label("DEAD ISLAND 2 PIPELINE", titleStyle);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // --- SECCIÓN 1: CONFIGURACIÓN ---
        BeginSection("1. Setup & Databases", Color.cyan);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder", targetFolder, typeof(DefaultAsset), false);
        parsedDb = (Database_ParsedMaterials)EditorGUILayout.ObjectField("Parsed DB (Raw)", parsedDb, typeof(Database_ParsedMaterials), false);
        processedDb = (Database_ProcessedMaterials)EditorGUILayout.ObjectField("Processed DB (Baker)", processedDb, typeof(Database_ProcessedMaterials), false);
        EndSection();

        GUI.enabled = targetFolder != null && parsedDb != null && processedDb != null;

        // --- SECCIÓN 2: EL BAKER (INTERPRETACIÓN) ---
        BeginSection("2. Parser & Instruction Baking", Color.yellow);

        // Controles de la Fase 2
        EditorGUILayout.BeginHorizontal();
        cleanParsedDb = EditorGUILayout.ToggleLeft("Purge Parsed", cleanParsedDb, GUILayout.Width(110));
        cleanProcessedDb = EditorGUILayout.ToggleLeft("Purge Instructions", cleanProcessedDb, GUILayout.Width(130));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        if (DrawButton("1. Run Analysis (Parse TXT)", "Lee los archivos .txt originales."))
        {
            Core_DataExtraction.ExtractRawData(targetFolder, parsedDb, cleanParsedDb);
        }

        if (DrawButton("2. Bake Instructions", "Transforma los datos parseados en instrucciones.", Color.Lerp(Color.yellow, Color.white, 0.5f)))
        {
            Core_DataExtraction.ProcessData(parsedDb, processedDb, cleanProcessedDb);
        }
        EndSection();

        // --- SECCIÓN 3: GENERACIÓN ---
        BeginSection("3. Materials & Textures Generation", Color.green);

        // Controles de la Fase 3
        EditorGUILayout.BeginHorizontal();
        overwriteMaterials = EditorGUILayout.ToggleLeft("Overwrite .mat", overwriteMaterials, GUILayout.Width(110));
        forceTextureReprocess = EditorGUILayout.ToggleLeft("Force Re-Texture", forceTextureReprocess, GUILayout.Width(130));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

        if (DrawButton("Phase A: Create Material Shells", "Crea los archivos .mat vacíos."))
        {
            EnsureAllDatabases();
            Core_MaterialGenerator.RunPhaseA(processedDb, overwriteMaterials);
        }

        if (DrawButton("Phase B: Process Textures", "Genera derivados y corrige normales."))
        {
            Core_MaterialGenerator.RunPhaseB(processedDb, forceTextureReprocess);
        }

        if (DrawButton("Phase C: Final Assignment", "Conecta texturas con materiales."))
        {
            Core_MaterialGenerator.RunPhaseC(processedDb, overwriteMaterials);
        }
        EndSection();

        // --- SECCIÓN 4: ASIGNACIÓN Y ESCENA ---
        BeginSection("4. Models Materials Remap", Color.pink);
        if (DrawButton("Reassign To Meshes", "Vincula materiales a los modelos."))
        {
            Core_ModelsRemap.RunReassign(targetFolder);
        }

        if (DrawButton("Instantiate Models in Grid", "Previsualización en escena."))
        {
            DI_SceneHelper.LayoutModelsInGrid(targetFolder);
        }
        EndSection();

        // --- SECCIÓN 5: DANGER ZONE ---
        EditorGUILayout.Space(20);
        BeginSection("Danger Zone / Maintenance", Color.grey);

        if (DrawButton("Clean Corrupt Assets", "Elimina materiales rotos en la carpeta destino.", new Color(0.8f, 0.4f, 0.4f)))
        {
            if (EditorUtility.DisplayDialog("Limpieza", "¿Borrar assets corruptos?", "Sí", "No"))
                DI_AssetCleaner.CleanCorruptMaterials(targetFolder);
        }

        if (DrawButton("Reset DB Flags", "Pone todos los isCreated/isTextured a false para re-procesar todo.", Color.gray))
        {
            // processedDb.ResetAllFlags();
        }
        EndSection();

        EditorGUILayout.EndScrollView();
        DrawFooter();
    }

    private void DrawFooter()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        //string pCount = parsedDb != null ? parsedDb.materials.Count.ToString() : "0";
        string pCount = "0";
        string iCount = processedDb != null ? processedDb.instructions.Count.ToString() : "0";
        EditorGUILayout.LabelField($"Parsed: {pCount} | Instructions: {iCount}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    // --- MÉTODOS DE DECORACIÓN ---
    private void BeginSection(string title, Color color)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
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
        bool clicked = GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(30));
        GUI.backgroundColor = originalColor;
        return clicked;
    }

    private void EnsureAllDatabases()
    {
        string dbFolder = "Assets/Scripts/Databases"; // O la ruta que prefieras

        // Aseguramos que la carpeta exista en el proyecto
        if (!AssetDatabase.IsValidFolder(dbFolder))
        {
            // Creamos las subcarpetas necesarias
            string[] folders = dbFolder.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath += "/" + folders[i];
            }
        }

        // Auto-creación de cada una si son null
        parsedDb = EnsureAsset<Database_ParsedMaterials>(parsedDb, dbFolder, "Database_Parsed_Raw.asset");
        processedDb = EnsureAsset<Database_ProcessedMaterials>(processedDb, dbFolder, "Database_Processed_Baker.asset");
    }

    private T EnsureAsset<T>(T currentAsset, string folder, string fileName) where T : ScriptableObject
    {
        if (currentAsset != null) return currentAsset;

        string fullPath = $"{folder}/{fileName}";
        T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=white><b>[Setup]</b></color> Creado asset ausente: {fullPath}");
        }
        return asset;
    }
}
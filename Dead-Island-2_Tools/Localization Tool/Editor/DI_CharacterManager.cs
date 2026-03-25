using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DI_CharacterManager
{
    // Ruta donde se guardará tu base de datos de personajes
    private const string DB_PATH = "Assets/Editor/Localization Text/CharacterDatabase.asset";

    public static Character_Database GetCharDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<Character_Database>(DB_PATH);
        if (db != null)
        {
            db.BuildCache();
        }
        return db;
    }

    public static Character_Database LoadDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<Character_Database>(DB_PATH);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<Character_Database>();

            string directory = System.IO.Path.GetDirectoryName(DB_PATH);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(db, DB_PATH);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=magenta><b>DI2 Tool:</b></color> Nueva base de datos creada en {DB_PATH}");
        }

        db.BuildCache();
        return db;
    }

    public static void SyncFromParsedData(Data_LocalizationFile extractedData, Character_Database db)
    {
        if (extractedData == null || db == null) 
            return;

        db.BuildCache();
        int addedCount = 0;

        // 1. PROCESAR LA TABLA DE PERSONAJES
        foreach (var charEntry in extractedData.Characters.Values)
        {
            if (db.GetCharacter(charEntry.ObjectId) == null)
            {
                AddCharacterToDb(db, charEntry.ObjectId, "", charEntry.Name, "", charEntry.IsPlayer);
                addedCount++;
            }
        }

        // 2. PROCESAR TABLA DE VOCES (Búsqueda de personajes "huérfanos" o variantes)
        foreach (var voiceEntry in extractedData.Voices.Values)
        {
            Data_Character targetCharSlot = CheckCharacterNode(db, voiceEntry);

            if (!targetCharSlot.voiceIds.Contains(voiceEntry.ObjectId))
            {
                targetCharSlot.voiceIds.Add(voiceEntry.ObjectId);
            }

            targetCharSlot.voiceName = voiceEntry.Name;
            
            if (string.IsNullOrWhiteSpace (targetCharSlot.displayName))
            {
                bool hasDisplayName = !string.IsNullOrWhiteSpace(voiceEntry.DisplayName);
                targetCharSlot.displayName = hasDisplayName? voiceEntry.DisplayName.Trim() : voiceEntry.Name;
            }
        }

        // 3. FINALIZAR Y ORDENAR
        if (addedCount > 0)
        {
            db.characters = db.characters.OrderBy(c => c.displayName).ToList();
            db.BuildCache();
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log($"<color=magenta><b>DI2 Tool:</b></color> Sincronización limpia finalizada. {addedCount} personajes únicos detectados.");
        }
    }

    // Método auxiliar para evitar repetir código de creación
    private static void AddCharacterToDb(Character_Database db, string charId, string voiceId, string displayName, string vName, bool isPlayer)
    {
        string cleanName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : "Unknown";

        var newChar = new Data_Character
        {
            displayName = cleanName,
            characterId = charId ?? "",
            voiceName = vName ?? "",
            isPlayer = isPlayer,
            priority = isPlayer ? 10 : 99
        };

        newChar.voiceIds = new List<string>();
        if (!string.IsNullOrEmpty(voiceId))
        {
            newChar.voiceIds.Add(voiceId);
        }

        db.characters.Add(newChar);

        // Reconstruimos el cache temporalmente para que el siguiente "GetCharacter" funcione
        db.BuildCache();
    }

    private static Data_Character CheckCharacterNode (Character_Database db, FileData_Voice voiceEntry)
    {
        // 1. Buscar por voiceEntry.CharacterObjectId (el vínculo más fuerte: ID de Personaje en tabla Voices)
        if (!string.IsNullOrEmpty(voiceEntry.CharacterObjectId))
        {
            Data_Character byCharId = db.GetCharacter(voiceEntry.CharacterObjectId);
            if (byCharId != null) 
                return byCharId;
        }

        // 2. Buscar por el propio VoiceObjectId
        Data_Character byVoiceId = db.GetCharacter(voiceEntry.ObjectId);
        if (byVoiceId != null)
        {
            return byVoiceId;
        }

        // 3. Buscar por Nombre (Smart Match para Alex vs AlexN, etc.)
        string vName = voiceEntry.Name;
        string vDisplayName = !string.IsNullOrWhiteSpace(voiceEntry.DisplayName) ? voiceEntry.DisplayName.Trim() : "";

        // Regla del nombre recortado (AlexN -> Alex)
        string vNameTrimmed = !string.IsNullOrEmpty(vName) && vName.Length > 1
            ? vName.Substring(0, vName.Length - 1)
            : vName;

        Data_Character byName = db.characters.FirstOrDefault(c =>
        (!string.IsNullOrEmpty(vDisplayName) && c.displayName == vDisplayName) ||
        c.displayName == vName ||
        c.displayName == vNameTrimmed);

        if (byName != null) 
            return byName;


        // 4. Si no hay rastro, creamos un nuevo slot
        // Se añade a la lista pero el bucle principal terminará de ponerle los datos
        Data_Character newSlot = new Data_Character
        {
            priority = 99,
            voiceIds = new List<string>()
        };

        db.characters.Add(newSlot);

        // IMPORTANTE: Hacemos BuildCache para que si otra voz apunta al mismo
        // personaje en este mismo ciclo de Sync, no lo duplique.
        db.BuildCache();

        return newSlot;
    }
}

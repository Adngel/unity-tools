using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcessedMaterialDatabase", menuName = "DI2/Processed Database")]
public class Database_ProcessedMaterials : ScriptableObject
{
    public List<Data_ProcessedMaterial> instructions = new List<Data_ProcessedMaterial>();
    
    [System.NonSerialized]
    private Dictionary<string, int> _cache;

    public void InitializeCache()
    {
        _cache = new Dictionary<string, int>();
        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i] != null)
            {
                string kvp = instructions[i].materialName;
                _cache[kvp] = i;
            }
        }
    }

    public void Add(Data_ProcessedMaterial newInst)
    {
        if (_cache == null) 
            InitializeCache();

        int index = -1;
        if (_cache.TryGetValue(newInst.materialName, out index))
        {
            // Si ya existe, lo reemplazamos en la lista
            instructions[index] = newInst;
        }
        else
        {
            // Si es nuevo, lo añadimos a ambos
            instructions.Add(newInst);
            _cache.Add(newInst.materialName, instructions.Count-1);
        }
    }

    public void Clear()
    {
        instructions.Clear();

        if (_cache != null) 
            _cache.Clear();

        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public Data_ProcessedMaterial GetByUnityGUID(string guid)
    {
        return instructions.Find(x => x.unityMaterialGUID == guid);
    }

    private void OnValidate()
    {
        if (instructions == null || instructions.Count == 0) return;

        for (int i = 0; i < instructions.Count; i++)
        {
            if (instructions[i] != null)
            {
                instructions[i].debugIndex = i;
            }
        }
    }
}

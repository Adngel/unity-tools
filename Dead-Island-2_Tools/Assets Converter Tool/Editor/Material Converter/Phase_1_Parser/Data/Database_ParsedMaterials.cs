using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ParsedMaterialDatabase", menuName = "DI2/Parsed Database")]
public class Database_ParsedMaterials : ScriptableObject
{
    [Header("Stats")]
    public int totalMaterials;
    public string lastAnalysisDate;

    [Space(10)]
    public List<Data_ParsedMaterial> Materials = new List<Data_ParsedMaterial>();

    [System.NonSerialized]
    private Dictionary<string, int> _cache;

    public void InitializeCache()
    {
        _cache = new Dictionary<string, int>();

        for (int i = 0; i < Materials.Count; i++)
        {
            if (Materials[i] != null)
            {
                string kvp = Materials[i].Name;
                _cache[kvp] = i;
            }
        }
    }

    public void Add(Data_ParsedMaterial mat)
    {
        if (_cache == null) 
            InitializeCache();

        if (_cache.TryGetValue(mat.Name, out int index))
        {
            Materials[index] = mat;
        }
        else
        {
            Materials.Add(mat);
            _cache.Add(mat.Name, Materials.Count - 1);
        }
    }

    public void Clear()
    {
        Materials.Clear();
        totalMaterials = 0;
        lastAnalysisDate = "Never";
    }
}
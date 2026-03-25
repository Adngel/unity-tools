using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "DI2/Character Database")]
public class Character_Database : ScriptableObject
{
    public List<Data_Character> characters = new List<Data_Character>();
    private Dictionary<string, Data_Character> _cache;

    public void BuildCache()
    {
        if (_cache != null && _cache.Count > 0) 
            return;

        _cache = new Dictionary<string, Data_Character>();

        foreach (var c in characters)
        {
            if (c == null) 
                continue;

            if (!string.IsNullOrEmpty(c.characterId) && !_cache.ContainsKey(c.characterId))
                _cache.Add(c.characterId, c);

            foreach (var vId in c.voiceIds)
            {
                if (!string.IsNullOrEmpty(vId) && !_cache.ContainsKey(vId))
                    _cache.Add(vId, c);
            }
        }
    }

    public Data_Character GetCharacter(string id)
    {
        if (_cache == null || _cache.Count == 0) 
            BuildCache();

        if (string.IsNullOrEmpty(id))
            return null;

        return _cache.TryGetValue(id, out var data) ? data : null;
    }
}

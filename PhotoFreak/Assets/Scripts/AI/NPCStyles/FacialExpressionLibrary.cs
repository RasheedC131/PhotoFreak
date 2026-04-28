using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacialExpressionLibrary", menuName = "NPC/Facial Expression Library")]
public class FacialExpressionLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public FacialExpression expression;
        public Texture2D texture;     
        public Texture2D maskTexture;  
    }

    [SerializeField] private Entry[] entries;
    private Dictionary<FacialExpression, Entry> lookup;

    public Texture2D GetTexture(FacialExpression expression)
    {
        EnsureLookup();
        return lookup.TryGetValue(expression, out var e) ? e.texture : null;
    }

    public Texture2D GetMask(FacialExpression expression)
    {
        EnsureLookup();
        return lookup.TryGetValue(expression, out var e) ? e.maskTexture : null;
    }
    public bool TryGet(FacialExpression expression, out Entry entry)
    {
        EnsureLookup();
        return lookup.TryGetValue(expression, out entry);
    }

    public bool Has(FacialExpression expression)
    {
        EnsureLookup();
        return lookup.ContainsKey(expression);
    }

    private void EnsureLookup()
    {
        if (lookup != null) return;
        lookup = new Dictionary<FacialExpression, Entry>(entries?.Length ?? 0);
        if (entries == null) return;
        for (int i = 0; i < entries.Length; i++)
            lookup[entries[i].expression] = entries[i];
    }

    public void InvalidateCache() => lookup = null;
}

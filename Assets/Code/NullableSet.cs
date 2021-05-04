using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NullableSet<T> where T : class
{
    Dictionary<T, T> dictionary = new Dictionary<T, T>();

    public IEnumerable<T> Elements { get { return dictionary.Keys; } }

    public NullableSet(IEqualityComparer<T> equality_comparer)
    {
        dictionary = new Dictionary<T, T>(equality_comparer);
    }

    public NullableSet()
    {
        
    }

    public void Add(T element)
    {
        if (!Contains(element))
            dictionary[element] = element;
    }

    public void Remove(T element)
    {
        dictionary.Remove(element);
    }

    public bool Contains(T element)
    {
        return dictionary.ContainsKey(element);
    }

    public T Get(T element)
    {
        if(!Contains(element))
            return null;

        return dictionary[element];
    }
}

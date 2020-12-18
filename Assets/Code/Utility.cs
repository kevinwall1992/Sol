using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;


public static class Utility
{
    public static List<T> List<T>(params T[] elements)
    {
        return new List<T>(elements);
    }

    public static List<T> List<T>(T first, Func<T, T> next_function)
    {
        List<T> list = new List<T>();

        T next = first;
        while(next != null)
        {
            list.Add(next);

            next = next_function(next);
        }

        return list;
    }

    public static List<T> List<T>(System.Func<int, T> function, int count)
    {
        List<T> list = new List<T>();

        for (int i = 0; i < count; i++)
            list.Add(function(i));

        return list;
    }

    public static Dictionary<T, U> Dictionary<T, U>(params object[] keys_and_values)
    {
        Dictionary<T, U> dictionary = new Dictionary<T, U>();

        for (int i = 0; i < keys_and_values.Length / 2; i++)
        {
            T t = (T)keys_and_values[i * 2 + 0];
            U u = (U)keys_and_values[i * 2 + 1];

            if (t != null && u != null)
                dictionary[t] = u;
        }

        return dictionary;
    }

    public static List<T> CreateNullList<T>(int size) where T : class
    {
        List<T> list = new List<T>();

        for (int i = 0; i < size; i++)
            list.Add(null);

        return list;
    }

    public static bool SetEquality<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return a.Count() == b.Count() && a.Except(b).Count() == 0;
    }

    public static bool SetEquality<T, U>(this Dictionary<T, U> a, Dictionary<T, U> b)
    {
        return a.Count == b.Count && a.Except(b).Count() == 0;
    }

    public static T ElementAtRelativeIndex<T>(this List<T> list, T element, int relative_index)
    {
        return list[list.IndexOf(element) + relative_index];
    }

    public static T PreviousElement<T>(this List<T> list, T element)
    {
        return list.ElementAtRelativeIndex(element, -1);
    }

    public static T NextElement<T>(this List<T> list, T element)
    {
        return list.ElementAtRelativeIndex(element, +1);
    }

    public static T Take<T>(this List<T> list, T element)
    {
        list.Remove(element);

        return element;
    }

    public static T TakeFirst<T>(this List<T> list)
    {
        T element = list.First();

        return list.Take(element);
    }

    public static T TakeAt<T>(this List<T> list, int index)
    {
        T element = list[index];
        list.RemoveAt(index);

        return element;
    }

    public static List<T> GetRange_NoExcuses<T>(this List<T> list, int index, int count = -1)
    {
        if (index < 0)
            index = 0;

        if (count < 0)
            count = list.Count - index;

        if (count <= 0)
            return new List<T>();

        return list.GetRange(index, Mathf.Min(count, list.Count - index));
    }

    public static List<T> Reversed<T>(this List<T> list)
    {
        List<T> reversed = new List<T>(list);
        reversed.Reverse();

        return reversed;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T element in enumerable)
            action(element);
    }

    public static IEnumerable<T> Merged<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
    {
        List<T> merged = new List<T>(enumerable);
        merged.AddRange(other);

        return merged;
    }

    public static List<T> Merged<T>(this List<T> list, IEnumerable<T> other)
    {
        return (list as IEnumerable<T>).Merged(other) as List<T>;
    }

    public static Dictionary<T, U> Merge<T, U>(this Dictionary<T, U> a, Dictionary<T, U> b)
    {
        foreach (T key in b.Keys)
            a[key] = b[key];

        return a;
    }

    public static Dictionary<T, U> Merged<T, U>(this Dictionary<T, U> a, Dictionary<T, U> b)
    {
        Dictionary<T, U> merged = new Dictionary<T, U>(a);

        foreach (T key in b.Keys)
            merged[key] = b[key];

        return merged;
    }

    public static IEnumerable<T> DuplicatesRemoved<T>(this IEnumerable<T> enumerable)
    {
        List<T> duplicates_removed = new List<T>(enumerable);
        duplicates_removed.RemoveDuplicates();

        return duplicates_removed;
    }

    public static Dictionary<T, U> Inverted<T, U>(this Dictionary<U, T> dictionary)
    {
        Dictionary<T, U> inverted = new Dictionary<T, U>();

        foreach (U key in dictionary.Keys)
            inverted[dictionary[key]] = key;

        return inverted;
    }

    public static string Trim(this string string_, int trim_count)
    {
        return string_.Substring(0, string_.Length - trim_count);
    }

    public static List<T> Sorted<T, U>(this List<T> list, 
                                       Func<T, U> comparable_fetcher) where U : IComparable
    {
        List<T> sorted = new List<T>(list);
        sorted.Sort((a, b) => (comparable_fetcher(a).CompareTo(comparable_fetcher(b))));

        return sorted;
    }

    public static List<T> Sorted<T, U>(this IEnumerable<T> enumerable, 
                                       Func<T, U> comparable_fetcher) where U : IComparable
    {
        return Sorted(new List<T>(enumerable), comparable_fetcher);
    }

    public static T MinElement<T, U>(this IEnumerable<T> enumerable, 
                                     Func<T, U> comparable_fetcher) where U : IComparable
    {
        if (enumerable.Count() == 0)
            return default(T);

        return enumerable.Sorted(comparable_fetcher).First();
    }

    public static T MaxElement<T, U>(this IEnumerable<T> enumerable, 
                                     Func<T, U> comparable_fetcher) where U : IComparable
    {
        if (enumerable.Count() == 0)
            return default(T);

        return enumerable.Sorted(comparable_fetcher).Last();
    }

    public static int DuplicateCountOf<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.Sum(other_element => (EqualityComparer<T>.Default.Equals(other_element, element) ? 1 : 0));
    }

    public static List<T> RemoveDuplicates<T>(this IEnumerable<T> enumerable)
    {
        List<T> without_duplicates = new List<T>();

        foreach (T element in enumerable)
            if (!without_duplicates.Contains(element))
                without_duplicates.Add(element);

        return without_duplicates;
    }

    public static System.Func<T, U> CreateLookup<T, U>(this System.Func<T, U> Function, IEnumerable<T> domain)
    {
        Dictionary<T, U> dictionary = new Dictionary<T, U>();

        foreach (T t in domain)
            dictionary[t] = Function(t);

        return (t) => (dictionary[t]);
    }

    public static System.Func<U, T> CreateInverseLookup<T, U>(this System.Func<T, U> Function, IEnumerable<T> domain)
    {
        Dictionary<U, T> inverse_dictionary = new Dictionary<U, T>();

        foreach (T t in domain) 
            inverse_dictionary[Function(t)] = t;

        return (u) => (inverse_dictionary[u]);
    }

    public static List<U> ToParentType<T, U>(this IEnumerable<T> enumerable) where T : U
    {
        return (new List<T>(enumerable)).ConvertAll(t => (U)t);
    }

    public static List<U> ToChildType<T, U>(this IEnumerable<T> enumerable) where U : T
    {
        return (new List<T>(enumerable)).ConvertAll(t => (U)t);
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    public static bool AnyTrue<T>(this IEnumerable<T> enumerable, System.Func<T, bool> predicate)
    {
        foreach (T element in enumerable)
            if (predicate(element))
                return true;

        return false;
    }

    public static bool AllTrue<T>(this IEnumerable<T> enumerable, System.Func<T, bool> predicate)
    {
        foreach (T element in enumerable)
            if (!predicate(element))
                return false;

        return true;
    }

    public static IEnumerable<U> SelectComponents<T, U>(this IEnumerable<T> enumerable)
        where T : Component
        where U : Component
    {
        return enumerable.Select(component => component.GetComponent<U>())
                         .Where(component => component != null);
    }

    public static IEnumerable<U> SelectComponents<U>(this IEnumerable<Transform> transforms)
        where U : Component
    {
        return transforms.SelectComponents<Transform, U>();
    }

    public static IEnumerable<T> GetEnumValues<T>()
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("Type T must be an Enum");

        return (T[])System.Enum.GetValues(typeof(T));
    }

    public static int GetEnumSize<T>()
    {
        return GetEnumValues<T>().Count();
    }


    public static void SaveAsPNG(this RenderTexture render_texture, 
                                 string filepath = "Other/render_texture.png")
    {
        RenderTexture active_render_texture = RenderTexture.active;
        RenderTexture.active = render_texture;

        Texture2D temporary_texture = 
            new Texture2D(render_texture.width, render_texture.height);
        temporary_texture.hideFlags = HideFlags.HideAndDontSave;

        temporary_texture.ReadPixels(
            new Rect(0, 0, render_texture.width, render_texture.height), 0, 0);
        System.IO.File.WriteAllBytes(filepath, temporary_texture.EncodeToPNG());
        Texture2D.Destroy(temporary_texture);

        RenderTexture.active = active_render_texture;
    }
}

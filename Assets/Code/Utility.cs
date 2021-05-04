﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using RotaryHeart.Lib.SerializableDictionary;


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
        while (next != null)
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

    public static Dictionary<T, U> ToDictionary<T, U>(this IEnumerable<Tuple<T, U>> tuples)
    {
        return tuples.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
    }

    public static Dictionary<T, U> ToDictionary<T, U>(this IEnumerable<ValueTuple<T, U>> value_tuples)
    {
        return value_tuples.ToTuples().ToDictionary();
    }

    public static IEnumerable<Tuple<T, U>> ToTuples<T, U>(this IEnumerable<ValueTuple<T, U>> value_tuples)
    {
        return value_tuples.ToTuples();
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

    public static T PreviousElement<T>(this List<T> list, T element) where T : class
    {
        if (list.First() == element)
            return null;

        return list.ElementAtRelativeIndex(element, -1);
    }

    public static T PreviousElement<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.ToList().PreviousElement(element);
    }

    public static T NextElement<T>(this List<T> list, T element) where T : class
    {
        if (list.Last() == element)
            return null;

        return list.ElementAtRelativeIndex(element, +1);
    }

    public static T NextElement<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.ToList().NextElement(element);
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

    public static List<T> GetRangeOrGetClose<T>(this List<T> list, int index, int count = -1)
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

    public static List<T> Sort<T, U>(this List<T> list, 
                                     Func<T, U> comparable_fetcher) where U : IComparable
    {
        list.Sort((a, b) => (comparable_fetcher(a).CompareTo(comparable_fetcher(b))));
        return list;
    }

    public static List<T> Sorted<T, U>(this List<T> list, 
                                       Func<T, U> comparable_fetcher) where U : IComparable
    {
        List<T> sorted = new List<T>(list);
        sorted.Sort(comparable_fetcher);

        return sorted;
    }

    public static List<T> Sorted<T, U>(this IEnumerable<T> enumerable, 
                                       Func<T, U> comparable_fetcher) where U : IComparable
    {
        return Sorted(new List<T>(enumerable), comparable_fetcher);
    }

    public static List<T> Sorted<T>(this IEnumerable<T> enumerable) where T : IComparable
    {
        return enumerable.Sorted(element => element);
    }

    public static T MinElement<T, U>(
        this IEnumerable<T> enumerable, 
        Func<T, U> comparable_fetcher) where U : IComparable
    {
        if (enumerable.Count() == 0)
            return default(T);

        return enumerable.Sorted(comparable_fetcher).First();
    }

    public static T MaxElement<T, U>(
        this IEnumerable<T> enumerable, 
        Func<T, U> comparable_fetcher) where U : IComparable
    {
        if (enumerable.Count() == 0)
            return default(T);

        return enumerable.Sorted(comparable_fetcher).Last();
    }

    public static IEnumerable<T> PreviousElements<T>(this IEnumerable<T> enumerable, T element)
    {
        List<T> list = enumerable.ToList();

        return list.GetRange(0, list.IndexOf(element));
    }

    public static IEnumerable<T> SucceedingElements<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.Reverse().PreviousElements(element);
    }

    public static int DuplicateCountOf<T>(this IEnumerable<T> enumerable, T element)
    {
        return enumerable.Sum(other_element => (EqualityComparer<T>.Default.Equals(other_element, element) ? 1 : 0));
    }

    public static IEnumerable<T> Distinct<T, U>(this IEnumerable<T> enumerable, 
                                                Func<T, U> comparable_fetcher) where U : IComparable
    {
        return enumerable.GroupBy(comparable_fetcher).Select(group => group.First());
    }

    public static IEnumerable<T> Union<T, U>(this IEnumerable<T> a, IEnumerable<T> b, Func<T, U> comparable_fetcher) where U : IComparable
    {
        return a.Concat(b).Distinct(comparable_fetcher);
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

    public static float SumDictionary<T, U>(this Dictionary<T, U> dictionary, Func<T, U, float> GetValue)
    {
        return dictionary.Sum(pair => GetValue(pair.Key, pair.Value));
    }

    public static IEnumerable<V> Select<T, U, V>(
        this Dictionary<T, U> dictionary, 
        Func<T, U, V> selector)
    {
        return dictionary.Select(pair => selector(pair.Key, pair.Value));
    }

    public static IEnumerable<V> Select<T, U, V>(
        this SerializableDictionaryBase<T, U> dictionary,
        Func<T, U, V> selector)
    {
        return dictionary.Select(pair => selector(pair.Key, pair.Value));
    }

    public static IEnumerable<V> Select<T, U, V>(
       this IEnumerable<ValueTuple<T, U>> pairs,
       Func<T, U, V> selector)
    {
        return pairs.Select(pair => selector(pair.Item1, pair.Item2));
    }

    public static IEnumerable<W> Select<T, U, V, W>(
        this IEnumerable<ValueTuple<T, U, V>> tuples,
        Func<T, U, V, W> selector)
    {
        return tuples.Select(tuple => selector(tuple.Item1, tuple.Item2, tuple.Item3));
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

    public static V Select<T, U, V>(this ValueTuple<T, U> tuple, Func<T, U, V> GetValue_)
    {
        return GetValue_(tuple.Item1, tuple.Item2);
    }

    public static W Select<T, U, V, W>(this ValueTuple<T, U, V> tuple, Func<T, U, V, W> GetValue_)
    {
        return GetValue_(tuple.Item1, tuple.Item2, tuple.Item3);
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

    public static bool IsOverride(this MethodInfo method_info)
    {
        return method_info.GetBaseDefinition().DeclaringType != 
               method_info.DeclaringType;
    }

    //Formats a float to 5 or less characters
    //and sign symbol (if negative)
    public static string ToShortString(this float value)
    {
        float absolute_value = Mathf.Abs(value);

        if (absolute_value < 100)
        {
            if (absolute_value >= 0.3f)
                return value.ToString("F1");
            else if (absolute_value >= 0.03f)
                return value.ToString("F2");
            else
                return value.ToString("F3");
        }
        else if (absolute_value < 1000)
            return ((int)value).ToString();

        string short_string = ((int)value).ToString();
        int digit_count = short_string.Length;

        return short_string.Substring(0, 3) +
               "E" + (digit_count - 3).ToString();
    }

    //Formats a float to a currency string with up to 5 characters,
    //currency symbol, and sign symbol (if negative)
    public static string ToShortMoneyString(this float value)
    {
        if (float.IsNaN(value))
            return "$???";

        if (Mathf.Abs(value) < 1)
            return value.ToString("C2");

        List<string> scale_suffixes = Utility.List("", "K", "M", "B", "T");

        int power = Mathf.Log(Mathf.Abs(value), 1000).RoundDown();
        if (power >= scale_suffixes.Count)
            return "$" + value.ToShortString();

        float scaled_value = 
            value / Mathf.Pow(1000, power);

        int decimal_count = 
            2 - Mathf.Log(Mathf.Abs(scaled_value), 10).RoundDown();

        return scaled_value.ToString("C" + decimal_count.ToString()) + 
               scale_suffixes[power];
    }


    public static bool IsInFrustrum(this Camera camera, Vector3 position)
    {
        Vector3 viewport_position = camera.WorldToViewportPoint(position);

        return viewport_position.x > 0 && viewport_position.x < 1 &&
            viewport_position.y > 0 && viewport_position.y < 1 &&
            viewport_position.z > 0;
    }

    public static bool IsInFrustrum(this Camera camera, Vector3 position, float radius)
    {
        return camera.IsInFrustrum(position) ||
               camera.IsInFrustrum(position + new Vector3(-radius, 0)) ||
               camera.IsInFrustrum(position + new Vector3(radius, 0)) ||
               camera.IsInFrustrum(position + new Vector3(0, -radius)) ||
               camera.IsInFrustrum(position + new Vector3(0, radius));
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
